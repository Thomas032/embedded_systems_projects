using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.Deployment.Application;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace CodeScanner.Controller
{
    public class DatabaseController
    {
        public string path { get; set; }
        private SQLiteConnection connection;
        private SQLiteCommand command;


        public DatabaseController(string userPath = "collosus.db")
        {
            path = userPath;
            InitDatabase();
            CreateTables();
        }

        private void InitDatabase()
        {
            // function for initialisation of the database
            if (!File.Exists(path))
            {
                try
                {
                    SQLiteConnection.CreateFile(path); // create a db file to connect to
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            try
            {
                // connect to the databse
                connection = new SQLiteConnection($"Data Source={path};Version=3;");
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // create a new SQLITE command with the connection
            command = new SQLiteCommand(connection);
        }

        private void CreateTables()
        {
            // create event table
            try
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Event (
                        eventID INTEGER PRIMARY KEY,
                        eventName TEXT,
                        eventDate DATETIME,
                        eventCapacity INTEGER,
                        eventTicketPrice REAL,
                        eventCode TEXT
                    )";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite při tvorbě tabulky události: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // create ticket table
            try
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Ticket (
                        ticketID INTEGER PRIMARY KEY,
                        eventID INTEGER,
                        ticketCode VARCHAR,
                        ticketUsed BOOL,
                        saleID INTEGER,
                        FOREIGN KEY (eventID) REFERENCES Event(eventID),
                        FOREIGN KEY (saleID) REFERENCES Sale(saleID)

                    )";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite při tvorbě tabulky lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // create a seller table
            try
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Seller (
                        sellerID INTEGER PRIMARY KEY,
                        sellerName VARCHAR,
                        sellerSurname VARCHAR,
                        sellerCode VARCHAR
                    )";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite při tvorbš tabulky prodejce: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // create a sale table
            try
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Sale (
                        saleID INTEGER PRIMARY KEY,
                        sellerID VARCHAR,
                        date DATETIME,
                        FOREIGN KEY (sellerID) REFERENCES Seller(sellerID)
                    )";
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite při tvorbě tabulky prodeje: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public SQLiteDataReader GetModelReader(string table, string column, string code)
        {
            // function for getting an SQLiteDataReader from a specified, table, column and code
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM {table} WHERE {column} = '{code}';";

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // if has data -> return the reader
                    return reader;
                }
            }
            catch(SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba při čtení kódu prodejce: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null; // no data  =  null
        }

        public int Entries(string table)
        {
            // function for returning the number of entries in a table

            int index = 0;
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = $"SELECT * FROM {table};";

                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    while(reader.Read())
                    {
                        // while has data -> increase the index count
                        index++;
                    }
                }
                reader.Close();
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba při čtení kódu prodejce: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return index;
        }
        internal void SaveSale(SaleController saleController)
        {
            // function for saving data about a sale to a database
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "INSERT INTO Sale(sellerID, sate) VALUES (@sellerID, @date)"; // a typo in date -> change and add data later
                command.Parameters.Add("@sellerID", DbType.Int32).Value = saleController.saleObj.sellerID;
                command.Parameters.Add("@date", DbType.DateTime).Value = saleController.saleObj.date;
                command.ExecuteNonQuery();
            }catch(SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite při ukládání informací o prodeji: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch(Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba systému při ukládání informací o prodeji: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } 

        internal void SaveTickets(TicketController ticketController)
        {
            // function for saving all of the tickets to the database
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = @"INSERT INTO Ticket(eventID, ticketCode, ticketUsed, saleID) VALUES 
                                                                  (@eventID, @ticketCode, @ticketUsed, @saleID);";
            }
            catch(SQLiteException ex)
            {
                MessageBox.Show($"Chyba SQLite při tvorbě SQLite příkazu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba systému při SQLite tvorbě příkaz: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            foreach (TicketModel ticket in ticketController.tickets)
            {
                // for every ticket in the memory...
                try
                {
                    command.Parameters.Clear(); // Clear previous parameters

                    // add the necessary parameters
                    command.Parameters.Add("@eventID", DbType.Int32).Value = ticket.eventID;
                    command.Parameters.Add("@ticketCode", DbType.String).Value = ticket.ticketCode;
                    command.Parameters.Add("@ticketUsed", DbType.Boolean).Value = ticket.used;
                    command.Parameters.Add("@saleID", DbType.Int32).Value = ticket.saleID;

                    command.ExecuteNonQuery();
                }
                catch(SQLiteException ex)
                {
                    // handle sqlite exceptions
                    MessageBox.Show($"Chyba SQLite při přidávání {ticket.ticketID}. lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    // handle normal exception
                    MessageBox.Show($"Nastala chyba systému při uládání lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        internal EventModel GetEventFromTicket(TicketModel ticket)
        {
            // fuction for getting an EventModel from the TicketModel
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM Event WHERE eventID = @ticketEventID;";
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Chyba SQLite při získávání dat o lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nastala chyba systému při získávání dat o lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {
                // add the ticket event parameter
                command.Parameters.Add("@ticketEventID", DbType.Int32).Value = ticket.eventID;
                SQLiteDataReader reader = command.ExecuteReader();

                if(reader != null )
                {
                    while(reader.Read())
                    {
                        // while the reader has data -> get the EventModel from the EventController and return it
                        EventController tmpEventController = new EventController(new DatabaseController());
                        tmpEventController.AssignModel(reader);
                        return tmpEventController.eventObj;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                // handle sqlite exceptions
                MessageBox.Show($"Chyba SQLite  při čtení dat o představení {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // handle normal exception
                MessageBox.Show($"Nastala chyba systému při čtení dat o představení: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        internal int GetMaxTicketID(int eventID)
        {
            // function for getting the maximum ticket ID from databse
            int maxID = 0;

            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT ticketID FROM Ticket WHERE eventID = @ticketEventID;";
                command.Parameters.Add("@ticketEventID", DbType.Int32).Value = eventID;

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // while the reader has data

                        int entryID = reader.GetInt32(0); 
                        if (entryID > maxID)
                        {
                            maxID = entryID;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"Chyba SQLite při získávání dat o lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nastala chyba systému při získávání dat o lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return maxID;
        }

        internal Dictionary<int, String> GetEventsToDate(DateTime currentDate)
        {
            // get a dictonary with ids and names of events for the current date
            Dictionary<int, string> upcomingEvents = new Dictionary<int, string>();
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT eventID, eventName, eventDate FROM Event WHERE eventDate > @currentDate;";
                command.Parameters.Add("@currentDate", DbType.DateTime).Value = currentDate;

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader != null)
                {
                    while (reader.Read())
                    {
                        // while have data
                        int eventID = reader.GetInt32(0);
                        string eventName = reader.GetString(1);

                        // Add to the dictionary
                        upcomingEvents.Add(eventID, $"{eventName}");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"Chyba SQLite při získávání dat o představení: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"Nastala chyba systému při získávání dat o představení: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return upcomingEvents;
        }

        internal bool CheckTicketValidity(int eventID, string ticketCode)
        {
            // function for getting the data related to ticket validity from a database
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT ticketUsed FROM Ticket WHERE ticketCode = @ticketCode AND eventID = @eventID;";
                command.Parameters.Add("@ticketCode", DbType.String).Value = ticketCode;
                command.Parameters.Add("@eventID", DbType.Int32).Value = eventID;

                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    bool ticketUsed = reader.GetBoolean(0);

                    if (ticketUsed)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"Chyba SQLite při ověřování lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"Nastala chyba systému při ověřování lístku: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        internal void SetTicketUsed(string ticketCode)
        {
            // function for setting a ticket as used based on the ticket code
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "UPDATE Ticket SET ticketUsed = 1 WHERE ticketCode = @ticketCode;";
                command.Parameters.Add("@ticketCode", DbType.String).Value = ticketCode;

                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"Chyba SQLite při označování lístku jako použitého: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"Nastala chyba systému při označování lístku jako použitého: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal int GetUsedTickets(int eventID)
        {
            // function for getting the numer of used tickets based on the eventID provided
            int used = 0;
            try
            {
                command = new SQLiteCommand(connection);
                command.CommandText = "SELECT * FROM Ticket WHERE eventID = @eventID AND ticketUsed = 1;";
                command.Parameters.Add("@eventID", DbType.Int32).Value = eventID;

                SQLiteDataReader reader = command.ExecuteReader();

                if(reader != null )
                {
                    while(reader.Read())
                    {
                        // while reader has data -> increase the used int variable
                        used++;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                // Handle SQLite exceptions
                MessageBox.Show($"Chyba SQLite při získávání použitých lístků: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"Nastala chyba systému při získávání použitých lístků: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return used;
        }


    }
}