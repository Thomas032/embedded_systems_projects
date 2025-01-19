using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeScanner.Controller
{
    internal class EventController
    {
        // Holds the current event object
        public EventModel eventObj { get; set; }

        // Database controller instance for handling database operations
        private DatabaseController dbController { get; set; }

        // Constructor to initialize the EventController with a DatabaseController instance
        public EventController(DatabaseController databaseController)
        {
            dbController = databaseController;
        }

        public void AssignModel(SQLiteDataReader reader)
        {
            // function that assigns values from a SQLiteDataReader to the eventObj property
            try
            {
                // Create a new EventModel and populate its properties with data from the reader
                eventObj = new EventModel
                {
                    eventID = Convert.ToInt32(reader["eventID"]),
                    eventName = Convert.ToString(reader["eventName"]),
                    eventDate = Convert.ToDateTime(reader["eventDate"]),
                    eventCapacity = Convert.ToInt32(reader["eventCapacity"]),
                    eventTicketPrice = Convert.ToInt32(reader["eventTicketPrice"]),
                    eventCode = Convert.ToString(reader["eventCode"])
                };
            }
            catch (Exception ex)
            {
                // Display an error message if there's an issue processing the data
                MessageBox.Show($"Chyba při zpracovávání dat prodejce: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int GetFreeCapacity()
        {
            // function that retrieves the free capacity of the associated event
            SQLiteDataReader reader = dbController.GetModelReader("Ticket", "eventID", eventObj.eventID.ToString());

            if (reader == null)
            {
                // If there are no tickets for the event, return the full capacity
                return eventObj.eventCapacity;
            }

            int eventTickets = 1; // Start at 1 to account for the current ticket
            while (reader.Read())
            {
                eventTickets++;
            }

            // Return the remaining capacity after considering the sold tickets
            return eventObj.eventCapacity - eventTickets;
        }

        public void AssignWithID(int id)
        {
            // funtion that assigns event details based on the provided event ID
            // Retrieve event data from the database using the provided event ID
            SQLiteDataReader reader = dbController.GetModelReader("Event", "eventID", id.ToString());
            // Assign the retrieved data to the eventObj property
            AssignModel(reader);
        }
    }
}
