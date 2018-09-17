using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Timers;

namespace x32lib
{
    public class x32server
    {
        public DeviceType type { get; private set; }
        public static IPEndPoint RemoteEndPoint { get; set; }
        public static IPEndPoint LocalEndPoint { get; set; } = initializeLocalAddress();
        private static Timer renewTimer;

        private List<Subscription> Subscriptions = new List<Subscription>();
        private static float[] meters = new float[4];
        private static bool MeterServerStarted = false;


        public x32server(string ip, DeviceType type = DeviceType.X32)
        {
            this.type = type;
            int port;
            switch (type)
            {
                case DeviceType.X32:
                    port = 10023;
                    break;

                case DeviceType.XAir:
                    port = 10024;
                    break;

                default:
                    port = 10023;
                    break;
            }

            SetRemoteEndPoint(ip, port);
        }

        public void SetRemoteEndPoint(string ip, int port = 10023)
        {
            RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ip),port);
        }

        private static IPEndPoint initializeLocalAddress()
        {
            Socket findIP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            findIP.Connect("1.1.1.1", 1234);
            IPEndPoint localEP = findIP.LocalEndPoint as IPEndPoint;
            findIP.Close();

            return localEP;
        }

        public void ClearSubscriptions()
        {
            foreach (Subscription s in Subscriptions)
            {
                s.renewTimer.Stop();
            }

            Subscriptions.Clear();
        }


        public void StartForwardServer(string forward_ip, int forward_port, int updateFreq = 5)
        {

            if (Subscriptions.Count < 1)
                return;

            renewTimer = new Timer(updateFreq * 1000); // Default updateFreq value will make timer trigger every 5 seconds
            renewTimer.Elapsed += renewEvent;
            renewTimer.Enabled = true;

            bool server_running = true;

            // Create X32 socket
            Socket xsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            xsocket.Bind(LocalEndPoint);


            // Create forward socket
            Socket forwardsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint local_b = LocalEndPoint;
            local_b.Port = 2345;
            forwardsocket.Bind(local_b); // Probably will need to bind to a different port to listen to devices separately?
            IPEndPoint forwardEP = new IPEndPoint(IPAddress.Parse(forward_ip), forward_port);


            while (server_running == true)
            {
                // Serve
                Console.WriteLine("Server running. Press enter to stop.");


                //Console.ReadLine (); // will probably replace this later, hence the while loop
                server_running = false;
            }

            // Close both sockets finally
            xsocket.Close();
            forwardsocket.Close();

        }

        private void renewEvent(Object source, ElapsedEventArgs e)
        {
            /*   This is depreciated

			// Renew OSC subscription(s)
			string tempOSC;
			Socket renewsocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			renewsocket.Bind (LocalAddress);

			foreach (string s in Subscriptions)
			{
				tempOSC = PadString ("/subscribe") + ",s\0\0" + s;
				renewsocket.SendTo (Encoding.ASCII.GetBytes (tempOSC), x32Address);
			}
			renewsocket.Close ();

			*/
            throw new NotImplementedException();
        }

        private static void MeterServer(int track)
        {
            // XAir seems to not return correct values so...fuck

            if (MeterServerStarted == true)
                return;

            // Create OSC request
            string path = "/meters\0,si\0/meters/0\0\0\0";
            byte[] address = Encoding.ASCII.GetBytes(path);
            byte[] value = BitConverter.GetBytes(1);
            if (BitConverter.IsLittleEndian == true)
                Array.Reverse(value); // Ensure big endian

            byte[] OSCmessage = new byte[address.Length + value.Length];
            address.CopyTo(OSCmessage, 0);
            value.CopyTo(OSCmessage, address.Length);

            //byte[] altmessage = Encoding.ASCII.GetBytes ("/subscribe\0\0,si\0/ch/01/config/color\0");

            // Open socket
            Socket meterserver_s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            meterserver_s.Bind(LocalEndPoint);

            try
            {
                meterserver_s.SendTo(OSCmessage, RemoteEndPoint);
                //meterserver_s.SendTo(altmessage, x32Address);
            }
            catch
            {
                Console.WriteLine("Failed to send message!");
            }

            byte[] buffer = new byte[56]; //40 bytes?
                                          //byte[] nullByte = Encoding.ASCII.GetBytes ("\0");
                                          //meterserver_s.Receive (buffer);
            int buffercount = 4;

            while (buffercount > 0) // Need to work in a timeout value
            {
                Console.Write("Trying to receive...");
                meterserver_s.Receive(buffer);
                Console.WriteLine("Done!");

                StringBuilder hex = new StringBuilder((buffer.Length) * 2);
                int count = 0;
                foreach (byte b in buffer)
                {
                    count++;
                    hex.AppendFormat("{0:x2} ", b);
                    if (count == 4)
                    {
                        count = 0;
                        hex.Append("  ");
                    }

                }

                Console.WriteLine(hex);
                string rawstring = Encoding.ASCII.GetString(buffer);
                Console.WriteLine(rawstring);

                byte[] blobbytes = { buffer[19], buffer[18], buffer[17], buffer[16] };
                int numfloats = BitConverter.ToInt32(buffer, 20);
                int bloblength = BitConverter.ToInt32(blobbytes, 0);

                Console.WriteLine(bloblength + " bytes in blob");
                Console.WriteLine(numfloats + " floats in blob\n");

                byte[] levelbytes = { buffer[25], buffer[24] };
                int level = BitConverter.ToUInt16(levelbytes, 0);

                Console.WriteLine(level + "db\n");

                buffercount -= 1;
            }


            meterserver_s.Close();
            Console.WriteLine("Closed Meter Server!");


        }

        public float GetTrackMeter(int track, int meter = 0)
        {
            // Check for X32 endpoint
            if (RemoteEndPoint == null)
            {
                Console.WriteLine("Haven't set the x32 Address yet!");
                return 0f;
            }

            // Check for valid track number
            if (track > 0 && track < 33)
            {
                track -= 1;
            }
            else
            {
                Console.WriteLine("Invalid track provided.");
                return 0f;
            }

            MeterServer(track);
            //Console.WriteLine(track);



            meters[meter] = .5f;
            return meters[meter];
        }

        public void SetTrackName(int track, string name)
        {
            /* This function is just shorthand for typing
			   out the address and using Send() */
            string track_string = String.Format("{0:00}", track);
            string address = "/ch/" + track_string + "/config/name";
            Send(address, name);

        }

        public void Send(string address, object val)
        {
            if (RemoteEndPoint == null)
            {
                throw new Exception("Haven't set the x32 Address yet!");
                return;
            }

            byte[] OSCaddress = Encoding.ASCII.GetBytes(EncodingHelper.PadString(address));
            byte[] OSCvalue = EncodingHelper.EncodeValue(val);

            byte[] OSCmessage = EncodingHelper.CombineBytes(OSCaddress, OSCvalue);

            StringBuilder hex = new StringBuilder((OSCmessage.Length) * 2);
            foreach (byte b in OSCmessage)
                hex.AppendFormat("{0:x2} ", b);

            Console.WriteLine(hex);
            Console.WriteLine(ASCIIEncoding.ASCII.GetChars(OSCmessage));

            Socket quickSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            quickSend.Bind(LocalEndPoint);
            try
            {
                quickSend.SendTo(OSCmessage, RemoteEndPoint);
                Console.WriteLine("Sent message!");
            }
            catch
            {
                Console.WriteLine("Failed to send message!");
            }
            quickSend.Close();

            System.Threading.Thread.Sleep(1);

        }

        public void AddSubscription(string command)
        {
            Subscriptions.Add(new Subscription(command, this));
        }

    }
}
