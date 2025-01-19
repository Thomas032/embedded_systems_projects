using Entra.Controller;
using Entra.Model;
using Entra.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace Entra
{
    public partial class Form1 : Form
    {
        public System.Timers.Timer pollTimer { get; set; }
        public System.Timers.Timer accessTimer { get; set; }
        public System.Timers.Timer clockTimer { get; set; }
        public DatabaseController dbController { get; set; }
        public SerialController serialController { get; set; }
        public ReaderController readerController { get; set; }
        public UserController userController { get; set; }
        public LogController logController { get; set; }
        public AdminView adminView { get; set; }

        public string code { get; set; }

        internal int lastReadStatus { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // global objects
            dbController = new DatabaseController();
            readerController = new ReaderController(dbController);
            logController = new LogController(dbController);

            // poll timer settings
            pollTimer = new System.Timers.Timer();
            pollTimer.Interval = 200;
            pollTimer.Elapsed += checkStatus;
            pollTimer.Start();

            // access timer settings
            accessTimer = new System.Timers.Timer();
            accessTimer.Interval = 5000;
            accessTimer.Elapsed += accessTimerElapsed;

            // clock timer setup
            clockTimer = new System.Timers.Timer();
            clockTimer.Interval = 1000;
            clockTimer.Elapsed += clockTimerElapsed;
            clockTimer.AutoReset = true;
            clockTimer.Start();

            serialController = new SerialController("COM13", 19200);
            serialController.Init();

            lastReadStatus = 0;

            // setup the GUI
            clearPanel0();
            clearPanel1();
            clearPanel2();
            time_label.Text = DateTime.Now.ToString("dddd, HH:mm:ss");
            
        }

        private void clockTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // method that executes every second -> get current time and display it
            DateTime time = DateTime.Now;
            time_label.Invoke((MethodInvoker)delegate
            {
                // get the current name of the day as well as the clock
                time_label.Text = DateTime.Now.ToString("dddd, HH:mm:ss");
            });
        }

        private void checkStatus(object sender, ElapsedEventArgs e)
        {
            // function for checking the status of the serial controller so that if new data apper
            // on the serial bus the GUI responds appropriately
            // should be as simple as checking the model class that the serial controller will return
            if (lastReadStatus != serialController.readCounter)
            {
                int reader_address = serialController.currentAddr;
                if(reader_address == 0)
                {
                    code = serialController.convertToDec(serialController.currentCode).ToString();
                }
                else{
                    code = serialController.currentCode;
                }

                // assign the database model from address so further details could be extracted
                readerController.AssignFromAddress(reader_address);

                if (readerController.readerObj.uuid != "")
                {
                    // the rederdata got parsed successfully
                    // now I can get the user data from the database
                    userController = new UserController(dbController);
                    userController.AssignFromCode(code);

                    if(userController.userObj != null)
                    {
                        // the user data got parsed successfully
                        // now I can show the data in the panel
                        
                        // assign the group data to the user
                        
                        userController.AssignGroup();

                        if(userController.groupController.groupObj == null)
                        {
                            return;
                        }
                        
                        // show the final data
                        showData(reader_address);

                        // log the entry
                        logController.LogEntry(userController.userObj.ID, reader_address, userController.HasAccess(reader_address));

                        // show logs
                        ShowLogs();
                    }
                    else
                    {
                        // user does not exist in the databse -> pass
                        //MessageBox.Show("User for this code does not exist");
                    }
                }
                lastReadStatus = serialController.readCounter;
            }

        }

        private void admin_btn_Click(object sender, EventArgs e)
        {
            // method for redirecting the user to the admin page
            // stop the internal timers
            pollTimer.Stop();
            accessTimer.Stop();
            clockTimer.Stop();

            // close the serial connection so that it can be used by the admin panel

            // open the admin panel
            serialController.readCounter = 0;
            adminView = new AdminView();
            adminView.FormClosed += new FormClosedEventHandler(adminViewClosed);
            adminView.Show();
        }

        private void adminViewClosed(object sender, FormClosedEventArgs e)
        {
            // method for reseting the form status after the admin panel is closed

            // reopen the serial connection
            serialController.openConnection();
            // restart timers
            pollTimer.Start();
            accessTimer.Start();
            clockTimer.Start();
        }

        private void showData(int addr)
        {
           
            if (addr == 0)
            {
                reader_panel_0.Invoke((MethodInvoker)delegate
                {
                    user_input_0.Text = $"{userController.userObj.name} {userController.userObj.surname}";
                    code_input_0.Text = code.ToString();
                    group_input_0.Text = userController.GetGroupName();

                    bool status = userController.HasAccess(addr);
                    
                    if (status)
                    {
                        granted_panel_0.Visible = true;
                        denied_panel_0.Visible = false;
                    }
                    else
                    {
                        granted_panel_0.Visible = false;
                        denied_panel_0.Visible = true;
                    }
                });

            }
            else if (addr == 1)
            {
                reader_panel_1.Invoke((MethodInvoker)delegate
                {
                    user_input_1.Text = $"{userController.userObj.name} {userController.userObj.surname}";
                    code_input_1.Text = code.ToString();
                    group_input_1.Text = userController.GetGroupName();
                    bool status = userController.HasAccess(addr);

                    if (status)
                    {
                        granted_panel_1.Visible = true;
                        denied_panel_1.Visible = false;
                    }
                    else
                    {
                        granted_panel_1.Visible = false;
                        denied_panel_1.Visible = true;
                    }
                });
            }
            else if (addr == 2)
            {
                reader_panel_1.Invoke((MethodInvoker)delegate
                {
                    user_input_2.Text = $"{userController.userObj.name} {userController.userObj.surname}";
                    code_input_2.Text = code.ToString();
                    group_input_2.Text = userController.GetGroupName();
                    bool status = userController.HasAccess(addr);

                    if (status)
                    {
                        granted_panel_2.Visible = true;
                        denied_panel_2.Visible = false;
                    }
                    else
                    {
                        granted_panel_2.Visible = false;
                        denied_panel_2.Visible = true;
                    }
                });
            }
            accessTimer.Start();
        }

        private void ShowLogs()
        {
            // method for showing the logs from the db

            // get the list of logs -> by default only first five
            List<LogModel> logs = logController.GetLatestLogs();

            if(logs == null)
            {
                return;
            }

            logs_text.Clear();
            foreach (LogModel log in logs)
            {
                logs_text.Text += $"{log.accessTime} - {log.userID} - {log.readerID} - {log.accessResult}\n";
            }
        }

        private void accessTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // method for clearing the data after user access

            reader_panel_0.Invoke((MethodInvoker)delegate
            {
                clearPanel0();
            });
            reader_panel_1.Invoke((MethodInvoker)delegate
            {
                clearPanel1();
            });
            reader_panel_2.Invoke((MethodInvoker)delegate
            {
                clearPanel2();
            });
        }

        private void clearPanel0()
        {
            user_input_0.Text = "";
            code_input_0.Text = "";
            group_input_0.Text = "";
            granted_panel_0.Visible = false;
            denied_panel_0.Visible = true;
        }

        private void clearPanel1()
        {
            user_input_1.Text = "";
            code_input_1.Text = "";
            group_input_1.Text = "";
            granted_panel_1.Visible = false;
            denied_panel_1.Visible = true;
        }
        private void clearPanel2()
        {
            user_input_2.Text = "";
            code_input_2.Text = "";
            group_input_2.Text = "";
            granted_panel_2.Visible = false;
            denied_panel_2.Visible = true;
        }
    }
}
