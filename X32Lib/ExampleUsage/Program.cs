using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using X32Lib;

namespace ExampleUsage
{
    public static class Programm
    {
        public static void Main()
        {
            /* When initializing an x32server, you can choose to specify 
             * a DeviceType as X32 or XAir. The X32 listens to OSC commands
             * on port 10023 whereas the XAir listens on 10024. This enum
             * is set to automate that distinction in simple language.
             * 
             * If no DeviceType is set, the server defaults to X32 (port 10023). */

            X32Server server = new X32Server("192.168.2.109", DeviceType.X32);

            // Zeroes out all track sends for a particular set of mixes

            string addressBuffer = "";
            string mix = "";

            for (int mixTrack = 1; mixTrack < 12; mixTrack++)
            {
                mix = mixTrack.ToString("00");

                for (int channel = 1; channel < 33; channel++)
                {
                    addressBuffer = String.Format("/ch/{0}/mix/{1}/level", channel.ToString("00"), mix);
                    server.Send(addressBuffer, .75f);
                    System.Threading.Thread.Sleep(1);
                }
            }
            Console.ReadLine();

        }
    }

}
