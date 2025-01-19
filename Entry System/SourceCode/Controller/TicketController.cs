using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeScanner.Controller
{
    internal class TicketController
    {
        private DatabaseController dbController { get; set; }
        private PrintController printController { get; set; }

        public float totalTicketPrice = 0;
        public List<TicketModel> tickets = new List<TicketModel>();
        internal Random random = new Random();


        public TicketController(DatabaseController databaseController)
        {
            printController = new PrintController();
            dbController = databaseController;
        }

        public void GenerateTickets(int n, EventModel ticketEvent, SaleModel sale)
        {
            // function for generating tickets and saving them into the memory as models
            for(int i = 0; i < n; i++)
            {
                TicketModel ticket = new TicketModel();
                ticket.eventID = ticketEvent.eventID;
                ticket.saleID = sale.saleID;
                ticket.used = false;
                totalTicketPrice += ticketEvent.eventTicketPrice;
                ticket.ticketID = GetTicketID(ticket);
                ticket.ticketCode = GetTicketCode(ticket.ticketID);
                tickets.Add(ticket);
            }
        }

        private string GetTicketCode(int ticketID)
        {
            // function fore generating a unique ticket code

            string code = (random.Next(100_000, 999_999) | ticketID).ToString(); // generate a random number from <100_000, 999_999> and OR it with the ticketID
            string final = "";

            foreach(char c in code)
            {
                final += ((int)c).ToString(); // add a ASCII value of each digit in the code int into the final string
            }

            return final;
        }

        public void SaveTicketPdfs()
        {
            // function that iterates through all of the tickets in memory and saves them as PDF files 
            for(int i=0; i< tickets.Count; i++)
            {
                TicketModel ticket = tickets[i];
                EventModel ticketEvent = dbController.GetEventFromTicket(ticket);
                
                if(ticketEvent != null)
                {
                    printController.GenerateTicketPDF($"{ticketEvent.eventName}_{ticket.ticketID}.pdf",ticket.ticketCode, ticketEvent.eventName, ticketEvent.eventDate, ticketEvent.eventTicketPrice);
                }
            }
        }

        private int GetTicketID(TicketModel ticket)
        {
            // function for getting the next possible unique ID

            int maxID = -1;
            foreach(TicketModel loopTicket in tickets)
            {
                // query the current list of tickets
                if(loopTicket.eventID == ticket.eventID)
                {
                    if(loopTicket.ticketID > maxID)
                    {
                        maxID = loopTicket.ticketID;
                    }
                }
            }

            if(maxID == -1)
            {
                // no tickets in memory -> get the ID from database
                return dbController.GetMaxTicketID(ticket.eventID) + 1;
            }

            // return the next USABLE ID -> that's why there is  a + 1
            return maxID + 1;
        }
    }
}
    