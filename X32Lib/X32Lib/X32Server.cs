using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using OscCore.LowLevel;

namespace X32Lib
{
    public class X32Server
    {
        public DeviceType type { get; private set; }


        private IPEndPoint LocalEndPoint;
        private IPEndPoint RemoteEndPoint;

        Channel[] Channels = new Channel[32];


        public X32Server(string ip, DeviceType type = DeviceType.X32)
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

            for (int i = 0; i < Channels.Length; i++)
            {
                Channels[i] = new Channel(i + 1, this);
            }
        }


        private void SetRemoteEndPoint(string ip, int port = 10023)
        {
            SetRemoteEndPoint(IPAddress.Parse(ip), port);
        }
        private void SetRemoteEndPoint(IPAddress ip, int port = 10023)
        {
            RemoteEndPoint = new IPEndPoint(ip, port);
        }


        public void Send(string address, object val)
        {
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

        }

    }
}
