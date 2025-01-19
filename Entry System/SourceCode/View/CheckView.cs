using CodeScanner.Controller;
using iText.StyledXmlParser.Jsoup.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CodeScanner.View
{
    public partial class CheckView : Form
    {
        // CONTROLLERS
        DatabaseController dbController { get; set; }
        EventController eventController { get; set; }

        // GLOBAL VARIABLES
        private System.Timers.Timer notificationTimer;
        private string scannedCode = "";
        private bool readingEvent = false;
        private bool errorNotification = false; 
        Dictionary<int, String> validEventDict;


        public CheckView()
        {
            this.KeyPreview = true; // enable keyboard input
            InitializeComponent();
            GlobalMethodsSetup();
            TimerSetup();
            ViewSetup();
        }

        private void ViewSetup()
        {
            // event input settings
            event_input.Minimum = 0;

            // panel settings
            error_panel.Visible = false;
            success_panel.Visible = false;

            // other settings
            DataGridSetup();
            FillEventData();
            EnableEventInput();
        }

        private void TimerSetup()
        {
            // function that sets basic parameters of the notificationTimer
            // notification timer is used to show valid/invalid panels
            notificationTimer = new System.Timers.Timer();
            notificationTimer.Interval = 750;
            notificationTimer.Elapsed += HideNotification;
            notificationTimer.AutoReset = false;
        }

        private void DataGridSetup()
        {
            // function for the DataGridVivew component setup

            // set the column names
            event_list.Columns.Add("ID", "ID");
            event_list.Columns.Add("eventName", "Event Name");

            // read-only grid
            event_list.ReadOnly = true;

            // omit the default index column
            event_list.RowHeadersVisible = false;
        }
        private void GlobalMethodsSetup()
        {
            // function for inicialization of the global controller variables
            dbController = new DatabaseController();
            eventController = new EventController(dbController);
        }

        private void FillEventData()
        {
            // function for filling the DataGridView component with data from the database

            validEventDict = dbController.GetEventsToDate(DateTime.Now); // get events from db, that happen today or in the futurue

            if(validEventDict.Count == 0)
            {
                // if no events today -> return void;
                return;
            }

            event_list.Rows.Clear();

            foreach(var eventDataEntry in validEventDict)
            {
                // for each entry in the global dictonary validEventDict containing data for the DataGridView -> add it to the component 
                int eventID = eventDataEntry.Key;
                string eventName = eventDataEntry.Value;

                event_list.Rows.Add(eventID, eventName);
            }

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            // function for handling keydown event
            if (readingEvent)
            {
                HandleEventInput(e.KeyValue);
            }
            else
            {
                HandleBarcodeInput(e.KeyValue);
            }
        }

        private void EnableEventInput()
        {
            // function for switching the programme to event input phase
            readingEvent = true;
            event_box.Enabled = true;
            event_input.Value = 0;
            event_input.Select(0, 1);
        }

        private void DisableEventInput()
        {
            // event for blocking all of the event input related elements
            readingEvent = false;
            event_box.Enabled = false;
            event_input.Enabled = false;
        }

        private void EnableTicketCodeInput()
        {
            // function for switching the programme to ticket code input phase
            ticket_box.Enabled = true;
            ticket_input.Enabled = true;
            ticket_input.Text = "";
            ticket_input.Focus();
        }

        private void HandleEventInput(int keyVal)
        {
            // function for handling the ID of event input

            if(keyVal != 13)
            {
                // if user did not click on enter -> ignore the keypress
                return;
            }

            int selectedID = (int) event_input.Value;

            if (!validEventDict.Keys.Contains(selectedID))
            {
                // if the ID is out of range -> show error and clear out the input
                EnableTicketCodeInput();
                ShowError("ID je mimo podporavaný rozsah představení.");
                return;
            }

            eventController.AssignWithID(selectedID);

            if(eventController.eventObj == null)
            {
                // if the assignment wwas not successfull -> show error
                ShowError("Interní chyba: nezdařilé získání předstevní z indetifikátoru.");
                return;
            }

            UpdateCheckedClients();
            DisableEventInput();
            EnableTicketCodeInput();
        }

        private void HandleBarcodeInput(int keyVal)
        {
            // function for handling ticket barcode input

            if (keyVal != 13)
            {
                // enter was not pressed
                int scannedInt = keyVal - 48; // calculate the actual value of the pressed key
                if(0 <= scannedInt && scannedInt <= 9)
                {
                    // if the actual value is in the range 0-9 -> add it to the scannedCode global var
                    scannedCode += scannedInt; 
                }
            }
            else
            {
                // enter was pressed -> assess its validity
                HandleScannedCode();
            }
        }

        private void HandleScannedCode()
        {
            // check db for entries
            // if entry exists and the ticket is not used, use it and display a proper message
            // if entry is used or does not exist show error
            
            if(!dbController.CheckTicketValidity(eventController.eventObj.eventID, scannedCode))
            {
                ShowInvalidTicket();
                EnableTicketCodeInput();
                return;
            }

            dbController.SetTicketUsed(scannedCode);
            ShowValidTicket();
            UpdateCheckedClients();
            EnableTicketCodeInput(); // reset inputs
        }

        private void UpdateCheckedClients()
        {
            // function for updating the number of checked clients
            int checkedClients = dbController.GetUsedTickets(eventController.eventObj.eventID);

            checked_text.Text = $"{checkedClients} Klientů";
        }

        private void ShowInvalidTicket()
        {
            // function for showing the invalid ticket panel
            errorNotification = true;
            error_panel.Visible = true;
            notificationTimer.Start();
        }

        private void ShowValidTicket()
        {
            // function for showing the valid ticket panel
            errorNotification = false;
            success_panel.Visible = true;
            notificationTimer.Start();
        }

        private void HideNotification(object sender, ElapsedEventArgs e)
        {
            // function for hiding the valid or invalid info panel

            Invoke(new Action(() => {
                // perform a cross-thread operation to edit the form view
                if (errorNotification)
                {
                    error_panel.Visible = false;
                }
                else
                {
                    // hide success panel
                    success_panel.Visible = false;
                }
            }));
        }

        private void ShowError(string msg)
        {
            // function for showing an error message in the form of message box
            MessageBox.Show($"{msg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
