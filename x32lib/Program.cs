using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace x32lib
{
	/*public class Subscription
	{
		public static string OSCaddress { get; set; }

		public System.Timers.Timer renewTimer;

		private static int secTillResub = 3;

		public Subscription(string address)
		{
			OSCaddress = address;

			renewTimer = new Timer (secTillResub*1000);
			renewTimer.Elapsed += new ElapsedEventHandler (Renew);
			renewTimer.Start ();
			Console.WriteLine ("Timer started");
		}

		public void Renew(object sender, EventArgs e)
		{
			Console.WriteLine ("Timer elapsed");
		}

	} */

    public enum DeviceType
    {
        X32, XAir
    }

	public class x32server
	{

		public static IPEndPoint x32Address { get; set; }

		public static IPEndPoint LocalAddress { get; set; } = initializeLocalAddress();

		private static Timer renewTimer;

		public x32server(string ip, DeviceType t = DeviceType.X32)
		{
            int port = 10023;

            switch (t)
            {
                case DeviceType.X32:
                    port = 10023;
                    break;

                case DeviceType.XAir:
                    port = 10024;
                    break;

                default:
                    break;
            }

            Setx32Address (ip, port);
		}

		public void Setx32Address(string ip, int port=10023)
		{
			IPAddress x32IP = IPAddress.Parse(ip);
			IPEndPoint x32EP = new IPEndPoint(x32IP, port);
			x32Address =  x32EP;

			System.Threading.Thread.Sleep (200);
		}

		private static IPEndPoint initializeLocalAddress()
		{
			Socket findIP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
			findIP.Connect("1.1.1.1", 1234);
			IPEndPoint localEP = findIP.LocalEndPoint as IPEndPoint;
			findIP.Close();

			return localEP;
		}

		public void SetLocalPort(int port)
		{
			LocalAddress.Port = port;
		}

		static Dictionary<Type, int> types = new Dictionary<Type, int>
		{
			{ typeof(float), 0 },
			{ typeof(int), 1 },
			{ typeof(string), 2 },
			{ typeof(bool), 3 }
		};

		static Dictionary<int, string> colors = new Dictionary<int, string>
		{
			{ 0, "Black"},
			{ 1, "Red"},
			{ 2, "Green"},
			{ 3, "Yellow"},
			{ 4, "Blue"},
			{ 5, "Magenta"},
			{ 6, "Cyan"},
			{ 7, "White"},
			{ 8, "Gray"},
			{ 9, "Red Inv"},
			{ 10, "Green Inv"},
			{ 11, "Yellow Inv"},
			{ 12, "Blue Inv"},
			{ 13, "Magenta Inv"},
			{ 14, "Cyan Inv"},
			{ 15, "White Inv"}
		};

		public static string PadString(object str)
		{
			/* OSC strings must be followed by a null and padded to a 
			   multiple of four bytes, so we calculate how many nulls to add */
			
			string padded = Convert.ToString(str)+"\0";
			int pad_size = 4-(padded.Length % 4);
			while (pad_size > 0 && pad_size < 4)
			{
				padded += "\0";    // May redo with StringBuilder but the speed boost may be negligable
				pad_size -= 1;
			}
			return padded;
		}
			
		private static byte[] CombineBytes(byte[] a, byte[] b)
		{
			// Creates new combined byte array from 2 existing ones
			byte[] c = new byte[a.Length + b.Length];
			a.CopyTo (c, 0);
			b.CopyTo (c, a.Length);
			return c;
		}

		private static byte[] EncodeValue(object val)
		{


			switch (types[val.GetType()])
			{
			case 0:
				{
					// Float
					byte[] typetag = Encoding.ASCII.GetBytes(",f\0\0");
					byte[] fbytes = BitConverter.GetBytes(Convert.ToSingle(val));
					if (BitConverter.IsLittleEndian == true)
						Array.Reverse (fbytes); // Ensure big endian
					
					return CombineBytes(typetag,fbytes);
				}
			case 1:
				{
					// Integer
					byte[] typetag = Encoding.ASCII.GetBytes(",i\0\0");
					byte[] ibytes = BitConverter.GetBytes(Convert.ToInt32(val));
					if (BitConverter.IsLittleEndian == true)
						Array.Reverse (ibytes); // Ensure big endian
					
					return CombineBytes(typetag,ibytes);
				}
			case 2:
				{
					// String
					byte[] sbytes = Encoding.ASCII.GetBytes(",s\0\0"+PadString(val));
					return sbytes;
				}
			case 3:
				{
					// Boolean
					byte[] emptybool = new byte[4];
					return emptybool;
				}
			default:
				// When all else fails, return some blank bytes
				byte[] empty = new byte[4];
				return empty;

			}

		}

		private string hexDump(string input)
		{
			byte[] tempBytes = Encoding.ASCII.GetBytes (input);
			StringBuilder hex = new StringBuilder((tempBytes.Length) * 2);
			foreach (byte b in tempBytes)
				hex.AppendFormat("{0:x2} ", b);

			return hex.ToString();
		}

		private List<Subscription> Subscriptions = new List<Subscription>();

		public void ClearSubscriptions()
		{
			foreach (Subscription s in Subscriptions) 
			{
				s.renewTimer.Stop ();
			}

			Subscriptions.Clear ();
		}
			

		public void StartForwardServer(string forward_ip, int forward_port, int updateFreq=5)
		{
			
			if (Subscriptions.Count < 1)
				return;

			renewTimer = new Timer(updateFreq*1000); // Default updateFreq value will make timer trigger every 5 seconds
			renewTimer.Elapsed += renewEvent;
			renewTimer.Enabled = true;

			bool server_running = true;

			// Create X32 socket
			Socket xsocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			xsocket.Bind (LocalAddress);


			// Create forward socket
			Socket forwardsocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint local_b = LocalAddress;
			local_b.Port = 2345;
			forwardsocket.Bind (local_b); // Probably will need to bind to a different port to listen to devices separately?
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
			forwardsocket.Close ();

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
		}

		private static float[] meters = new float[4];

		private static bool MeterServerStarted = false;


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
				Array.Reverse (value); // Ensure big endian

			byte[] OSCmessage = new byte[address.Length + value.Length];
			address.CopyTo(OSCmessage, 0);
			value.CopyTo(OSCmessage, address.Length);

			//byte[] altmessage = Encoding.ASCII.GetBytes ("/subscribe\0\0,si\0/ch/01/config/color\0");

			// Open socket
			Socket meterserver_s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			meterserver_s.Bind (LocalAddress);

			try
			{
				meterserver_s.SendTo(OSCmessage, x32Address);
				//meterserver_s.SendTo(altmessage, x32Address);
			}
			catch 
			{
				Console.WriteLine ("Failed to send message!");
			}

			byte[] buffer = new byte[56]; //40 bytes?
			//byte[] nullByte = Encoding.ASCII.GetBytes ("\0");
			//meterserver_s.Receive (buffer);
			int buffercount = 4;

			while (buffercount > 0) // Need to work in a timeout value
			{
				Console.Write ("Trying to receive...");
				meterserver_s.Receive (buffer);
				Console.WriteLine ("Done!");

				StringBuilder hex = new StringBuilder((buffer.Length) * 2);
				int count = 0;
				foreach (byte b in buffer) 
				{
					count++;
					hex.AppendFormat ("{0:x2} ", b);
					if (count == 4) {
						count = 0;
						hex.Append ("  ");
					}

				}
				
				Console.WriteLine (hex);
				string rawstring = Encoding.ASCII.GetString (buffer);
				Console.WriteLine (rawstring);

				byte[] blobbytes = { buffer [19], buffer [18], buffer [17], buffer [16] };
				int numfloats = BitConverter.ToInt32 (buffer, 20);
				int bloblength = BitConverter.ToInt32 (blobbytes, 0);

				Console.WriteLine (bloblength + " bytes in blob");
				Console.WriteLine (numfloats + " floats in blob\n");


				//byte[] colorbytes = { buffer [27], buffer [26], buffer [25], buffer [24] };
				//int color = BitConverter.ToInt32 (colorbytes, 0);

				//Console.WriteLine (colors [color] +"\n");
				byte[] levelbytes = {buffer[25],buffer[24]};
				int level = BitConverter.ToUInt16(levelbytes,0);

				Console.WriteLine (level + "db\n");

				buffercount -= 1;
			}


			meterserver_s.Close ();
			Console.WriteLine ("Closed Meter Server!");


		}

		public float GetTrackMeter(int track, int meter=0)
		{
			// Check for X32 endpoint
			if (x32Address == null) {
				Console.WriteLine ("Haven't set the x32 Address yet!");
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
				
			MeterServer (track);
			//Console.WriteLine(track);



			meters [meter] = .5f;
			return meters[meter];
		}

		public void SetTrackName(int track, string name)
		{
			/* This function is just shorthand for typing
			   out the address and using Send() */
			string track_string = String.Format ("{0:00}", track);
			string address = "/ch/"+track_string+"/config/name";
			Send(address, name);

		}

		public void Send(string address,  object val)
		{
			if (x32Address == null)
			{
				throw new Exception("Haven't set the x32 Address yet!");
				return;
			}

			byte[] OSCaddress = Encoding.ASCII.GetBytes(PadString(address));
			byte[] OSCvalue = EncodeValue(val);

			byte[] OSCmessage = CombineBytes (OSCaddress, OSCvalue);

			StringBuilder hex = new StringBuilder((OSCmessage.Length) * 2);
			foreach (byte b in OSCmessage)
				hex.AppendFormat("{0:x2} ", b);

			Console.WriteLine(hex);
			Console.WriteLine(ASCIIEncoding.ASCII.GetChars(OSCmessage));

			Socket quickSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			quickSend.Bind(LocalAddress);
			try
			{
				quickSend.SendTo(OSCmessage, x32Address);
				Console.WriteLine("Sent message!");
			}
			catch
			{
				Console.WriteLine ("Failed to send message!");
			}
			quickSend.Close();

			System.Threading.Thread.Sleep (1);

		}

		public void AddSubscription (string command)
		{
			Subscriptions.Add(new Subscription(command, this));
		}

		public class Subscription
		{
			public static string OSCaddress { get; set; }

			public System.Timers.Timer renewTimer;

			private x32server server;

			private static int secTillResub = 3;

			public Subscription(string address, x32server s)
			{
				server = s;
				OSCaddress = PadString(address);

				renewTimer = new Timer (secTillResub*1000);
				renewTimer.Elapsed += new ElapsedEventHandler (Renew);
				renewTimer.Start ();
				Console.WriteLine ("Timer started");
			}

			public void Renew(object sender, EventArgs e)
			{
				Console.WriteLine ("Timer elapsed");

				//send command
				//string tempOSC = PadString ("/subscribe") + ",s\0\0" + OSCaddress;
				//Send (OSCaddress, new byte[] {0x00,0x00,0x00,0x00});

			}

		}

		public static void Main()
		{
            /* When initializing an x32server, you can choose to specify 
             * a DeviceType as X32 or XAir. The X32 listens to OSC commands
             * on port 10023 whereas the XAir listens on 10024. This enum
             * is set to automate that distinction in simple language.
             * 
             * If no DeviceType is set, the server defaults to X32 (port 10023). */

            x32server server = new x32server("192.168.2.109", DeviceType.X32);

            // Moves faders 9-16 up and down repeatedly
            /*
            float faderLevel = 1f;

			for (int times = 0; times < 30; times++) {
				server.Send ("/ch/09/mix/fader", faderLevel);
				server.Send ("/ch/10/mix/fader", faderLevel);
				server.Send ("/ch/11/mix/fader", faderLevel);
				server.Send ("/ch/12/mix/fader", faderLevel);
				server.Send ("/ch/13/mix/fader", faderLevel);
				server.Send ("/ch/14/mix/fader", faderLevel);
				server.Send ("/ch/15/mix/fader", faderLevel);
				server.Send ("/ch/16/mix/fader", faderLevel);

				if (faderLevel == 1f)
					faderLevel = 0f;
				else
					faderLevel = 1f;

				System.Threading.Thread.Sleep (100); // Sleep for a time so as to not overload the X32 with requests, and to allow the faders to physically travel
			}

            */

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
			//Console.ReadLine();

		}    
	}

}
