using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace x32lib
{
    public class Subscription
    {
        public static string OSCaddress { get; set; }

        public System.Timers.Timer renewTimer;

        private x32server server;

        private static int secTillResub = 3;

        public Subscription(string address, x32server s)
        {
            server = s;
            OSCaddress = EncodingHelper.PadString(address);

            renewTimer = new Timer(secTillResub * 1000);
            renewTimer.Elapsed += new ElapsedEventHandler(Renew);
            renewTimer.Start();
            Console.WriteLine("Timer started");
        }

        public void Renew(object sender, EventArgs e)
        {
            Console.WriteLine("Timer elapsed");

            //send command
            //string tempOSC = PadString ("/subscribe") + ",s\0\0" + OSCaddress;
            //Send (OSCaddress, new byte[] {0x00,0x00,0x00,0x00});

        }

    }
}
