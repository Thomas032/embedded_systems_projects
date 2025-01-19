using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Security.Policy;
using System.Security.Cryptography;

namespace Entra.Controller
{
   public class SerialController
    {
        public SerialPort serialPort { get; set; }
        public string buffer { get; set; }

        public Dictionary<int, int> codes { get; set; }
        public bool dataReady { get; set; }

        public string currentCode { get; set; }
        public int currentAddr { get; set; }

        public int readCounter { get; set; }

        public SerialController(string port, int baudRate)
        {
            // set up the global objects
            serialPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);
            codes = new Dictionary<int, int>();
            dataReady = false;
            currentAddr = -1;
            currentCode = "";
            readCounter = 0;
        }

        public void Init()
        {
            // initialize the connection by assigning the venet handeler and opening the port
            serialPort.DataReceived += new SerialDataReceivedEventHandler(dataReceived);
            try
            {
                serialPort.Open();
            }
            catch { }
        }

        private void dataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // method for adding the received data to buffer
            string data = serialPort.ReadExisting();

            // add data to buffer
            buffer += data;
            // check if we have any parseable data int the buffer -> regex and capture groups
            string pattern = "\\u0002(\\u0004)?([ABCDEF1234567890]*)\\r\\n(\\u0003)(\\u0015)?";

            // Match the input against the pattern
            MatchCollection matches = Regex.Matches(buffer, pattern);

            // Process each match
            foreach (Match match in matches)
            {
                // Access the captured data using the named group "data"
                string g0 = match.Groups[0].Value; // full code
                string g1 = match.Groups[1].Value; // 0004
                string g2 = match.Groups[2].Value; // actual code
                string g3 = match.Groups[3].Value; // 0003
                string g4 = match.Groups[4].Value; //0015

                Console.Error.WriteLine("G2 = " + g2);


                if(g2 == "")
                {
                    return;
                }

                // after debug add the groups to the dict

                int reader_addr = 0;
                //int converted = 0;

                if (g1 != "")
                {
                    // chip keypad
                    if(g2.Length == 4)
                    {
                        // keypad code input was used
                        reader_addr = 0;
                        //converted = convertToDec(g2);
                    }
                    else if(g2.Length == 8)
                    {
                        // keypad reader was used
                        reader_addr = 1;
                        //converted = Convert.ToInt32(g2, 16);
                    }
                }
                else
                {
                    // chip only
                    reader_addr = 2;
                    //converted = Convert.ToInt32(g2, 16);
                }

                currentCode = g2;
                currentAddr = reader_addr;

                readCounter++;

                // remove byte from buffer
                int startIndex = buffer.IndexOf(g0);
                string newBuffer = buffer.Remove(startIndex, g0.Length);
                buffer = newBuffer;
            }

            buffer = "";
        }

        public int convertToDec(string code)
        {
            int numberRepre = int.Parse(code, System.Globalization.NumberStyles.HexNumber); ;

            return numberRepre;
        }

        public void closeConnection()
        {
            serialPort.Close();
        }
        public void openConnection()
        {
            try
            {
                serialPort.Open();
            }
            catch
            {
                
            }
        }
    }
}
