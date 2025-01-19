using CodeScanner.Controller;
using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.Security.AccessControl;

namespace CodeScanner.View
{
    public partial class film_price_label : Form
    {
        // CONTROLLERS
        DatabaseController dbController { get; set; }
        SellerController sellerController { get; set; }
        EventController eventController { get; set; }
        TicketController ticketController { get; set; }
        SaleController saleController { get; set; }

        // GLOBAL VARIABLES
        string scannedCode = "";
        public bool readingQuantity = false;
        public bool readingReceived = false;
        public int recievedAcknowledged = -1;


        public film_price_label()
        {
            InitializeComponent();
        }

        private void SellerView_Load(object sender, EventArgs e)
        {
            GlobalMethodsSetup();
            ViewSetup();
        }

        private void GlobalMethodsSetup()
        {
            // function for setting up the Controller objects
            dbController = new DatabaseController();
            sellerController = new SellerController();
            eventController = new EventController(dbController);
            ticketController = new TicketController(dbController);
            saleController = new SaleController(dbController);
        }

        private void ViewSetup()
        {
            // function for setting up the view parameters on startup

            this.KeyPreview = true; // read input from keyboard

            // seller box init
            seller_name_input.Enabled = false;
            seller_surname_input.Visible = false;

            seller_surname_input.Enabled = false;
            seller_surname_input.Visible = false;

            seller_code_input.Focus();

            //film box init
            film_code_group.Enabled = false;

            // ticket box init
            ticket_input_box.Enabled = false;
            ticket_input.Minimum = 1;

            // price box init
            price_box.Visible = false;
            price_received_input.Minimum = 0;
            price_received_input.Maximum = decimal.MaxValue;
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            // function for handling the key press

            if (readingQuantity)
            {
                HandleQuantityInput(e.KeyValue);
            }
            else if (readingReceived)
            {
                HandleReceivedInput(e.KeyValue);
            }
            else
            {
                HandleBarcodeInput(e.KeyValue);
            }
        }

        private void HandleReceivedInput(int keyVal)
        {
            if(keyVal != 13)
            {
                // if enter received -> ignore it
                return;
            }

            if(recievedAcknowledged == 0)
            {
                // if user has ackowledged the return sum -> switch to film-reading mode
                recievedAcknowledged = -1;
                readingQuantity = false;
                readingReceived = false;
                ResetSaleData();
                DisableTotal();
                EnableFilmInput();
                return;
            }

            int received = (int)price_received_input.Value;
            float toBeReturned = received - ticketController.totalTicketPrice; // calculating the return sum

            if (toBeReturned < 0)
            {
                // if return sum incalid -> show error and clear input
                ShowError("Příliš nízká obdržená částka!");
                price_received_input.Value = 0;
                price_received_input.Select(0, 1);
                return;
            }

            // show the sum to be returned and wait for acknowledge
            price_received_input.Enabled = false;
            price_return_text.Text = $"{toBeReturned} Kč";
            recievedAcknowledged++;
        }


        private void HandleBarcodeInput(int keyVal)
        {
            // function for handeling barcode input froom the reader
            if (keyVal != 13)
            {
                // if enter not pressed -> add the ASCII code to the scannedCode string
                scannedCode += keyVal.ToString();
            }
            else
            {
                // enter pressed -> check the entered code
                HandleScannedCode();
            }
        }

        private void HandleQuantityInput(int keyVal)
        {
            // function for handeling the quantity input
            if (keyVal == 13 || keyVal == 107)
            {
                // if plus or enter key were pressed, print the ticket or continue in the sale

                int n = (int)ticket_input.Value;

                if(eventController.GetFreeCapacity() < n)
                {
                    // not enough tickets -> show error
                    ShowError("Příliš velké číslo zadaných vstupenek.");
                    ticket_input.Value = 1;
                    return;
                }

                if (saleController.saleObj == null)
                {
                    // if no sale existed, add it to memory and create a record in db
                    saleController.CreateSale(sellerController.sellerObj);
                    dbController.SaveSale(saleController);
                }

                // generate the tickets and save them to RAM
                ticketController.GenerateTickets(n, eventController.eventObj, saleController.saleObj);
                DisbaleTicketInput();
            }

            if(keyVal == 13)
            {
                // if enter clicked -> continue in the sale
                eventController.eventObj = null;
                readingQuantity = false;
                EnableFilmInput();
            }

            if (keyVal == 107)
            {
                // save the tickets and reset the controllers to accept further data
                readingReceived = true;
                readingQuantity = false;
                ticketController.SaveTicketPdfs();
                dbController.SaveTickets(ticketController);
                EnableTotal();
            }
        }

        public void ResetSaleData()
        {
            // reset the controller objects so new data could be stored
            ticketController.tickets = new List<TicketModel>();
            eventController.eventObj = null;
            saleController.saleObj = null;
        }

        public void HandleScannedCode()
        {
            // function for deciding which handler function should be used based on the state of the controllers
            if (sellerController.sellerObj == null)
            {
                HandleSellerCode();
            }
            else if (eventController.eventObj == null)
            {
                HandleFilmCode();
            }
            scannedCode = "";
        }

        private void HandleSellerCode()
        {
            // function for handling the barcode of a seller

            SQLiteDataReader sellerReader = dbController.GetModelReader("Seller", "sellerCode", scannedCode);
            if (sellerReader != null)
            {
                // seller exists -> convert it to model
                sellerController.AssignModel(sellerReader);
                DisableSellerInput();
                EnableFilmInput();
            }
            else
            {
                // seller is not valid -> show error and clear input
                seller_code_input.Text = "";
                ShowError("Nesprávný kód prodejce...");
            }
        }

        private void HandleFilmCode()
        {
            // function for handeling film input
            SQLiteDataReader eventReader = dbController.GetModelReader("Event", "eventCode", scannedCode);
            if (eventReader != null)
            {
                // event exists
                eventController = new EventController(dbController);
                eventController.AssignModel(eventReader);

                if(eventController.GetFreeCapacity() == 0)
                {
                    // event capacity exceeded -> show error clear input
                    ShowError("Kapacita představení vyčeprána!");
                    film_code_input.Text = "";
                    eventController.eventObj = null;
                    return;
                }

                ShowEventData();
                EnableTicketInput();
            }
            else
            {
                // event does not exist -> clear input and show error
                film_code_input.Text = "";
                ShowError("Nesprávný kód filmu...");
            }
        }
        private void EnableTotal()
        {
            // enable the total view elements
            price_box.Visible = true;
            price_received_input.Enabled = true;
            price_received_input.Value = 0;
            price_total_text.Text = $"{Math.Round(ticketController.totalTicketPrice, 2)} Kč";
            price_return_text.Text = "";
            price_received_input.Focus();
            price_received_input.Select(0, 1);
        }

        private void DisableTotal()
        {
            // function for disablig the total
            price_box.Visible = false;
            price_total_text.Text = $"0 Kč";
        }

        private void DisableSellerInput()
        {
            // show the seller related inputs
            seller_name_input.Visible = true;
            seller_surname_input.Visible = true;

            // set seller informatuin
            seller_name_input.Text = sellerController.sellerObj.sellerName;
            seller_surname_input.Text = sellerController.sellerObj.sellerSurname;
            seller_code_input.Enabled = false;
        }

        private void EnableFilmInput()
        {
            // function for enabling the film inputs in the view
            film_name_input.Text = "";
            film_price_input.Text = "";
            free_input.Text = "";
            occupied_input.Text = "";
            film_code_input.Text = "";
            film_code_input.Enabled = true;
            film_code_group.Enabled = true;
            film_code_input.Focus();
        }


        private void EnableTicketInput()
        {
            // function for enabling the ticket inputs in the view
            readingQuantity = true;
            ticket_input_box.Enabled = true;
            ticket_input.Enabled = true;
            ticket_input.Focus();
            ticket_input.Select(0, 1);
        }
        private void DisbaleTicketInput()
        {
            // function for disabling the ticket input in the view
            ticket_input.Value = 1;
            ticket_input_box.Enabled = false;
        }
        private void ShowEventData()
        {
            // function for filling the textboxes with event related values
            film_code_input.Enabled = false;
            film_name_input.Text = eventController.eventObj.eventName;
            free_input.Text = eventController.GetFreeCapacity().ToString();
            occupied_input.Text = eventController.eventObj.eventCapacity.ToString();
            film_price_input.Text = eventController.eventObj.eventTicketPrice.ToString();
        }


        public void ShowError(string msg)
        {
            // function for showing erros as message boxes
            MessageBox.Show($"{msg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
