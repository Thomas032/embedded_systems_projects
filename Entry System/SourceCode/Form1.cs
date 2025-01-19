using CodeScanner.View;
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

namespace CodeScanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private System.Timers.Timer error_timer; // timer used for displaying an error panel
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // enable handling key presses

            // view setup
            error_panel.Visible = false;

            // timer setup
            error_timer = new System.Timers.Timer();
            error_timer.Interval = 500;
            error_timer.Elapsed += hideError;
            error_timer.AutoReset = false;
        }

        private void hideError(object sender, ElapsedEventArgs e)
        {
            // hide the error message when timer stops

            Invoke(new Action(() => { error_panel.Visible = false; })); // perform a cross thread operation to edit the form view
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.NumPad1)
            {
                // open sell mode form
                film_price_label sellerView = new film_price_label();
                sellerView.Show();
                return;
            }
            else if(e.KeyCode == Keys.NumPad3)
            {
                // open check mode form
                CheckView checkView = new CheckView();
                checkView.Show();
                return;
            }
            else
            {
                // show erroe -> user clicked on a different key
                error_panel.Visible = true;
                error_timer.Start();
            }
        }
    }
}
