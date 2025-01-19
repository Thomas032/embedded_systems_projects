using System;
using Trezor.Essentials;

namespace Trezor
{
    public class Program
    {
        public static Program prg = new Program();
        public VaultModel vaultModel { get; set; }

        public void Initialize()
        {
            // initialize the vault model
            vaultModel = new VaultModel();
        }
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.WriteLine("Init start");
            // run the init
            prg.Initialize();

            Console.WriteLine("Init complete");

            while (true)
            {
                prg.vaultModel.DisplayCode();
            }
        }


        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            prg.vaultModel.ResetPorts();
            Console.WriteLine("exit");
        }
    }
}