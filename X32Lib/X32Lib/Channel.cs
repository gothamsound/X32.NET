using System;
using System.Collections.Generic;
using System.Text;

namespace X32Lib
{
    public class Channel
    {
        private int Number;
        private X32Server Server;

        public Channel(int number, X32Server server)
        {
            if(!(number <= 32 && number >= 1))
            {
                throw new IndexOutOfRangeException();
            }

            Number = number;
            Server = server;
        }

        public void SetChannelName(string name)
        {
            Server.Send(String.Format("/ch/{0}/config/name", Number), name);
        }

        public void SetChannelColor(Color color)
        {
            Server.Send(String.Format("/ch/{0}/config/color", Number), color);

        }
        public void SetMute(bool muted)
        {
            Server.Send(String.Format("/ch/{0}/mix/on", Number), !muted);
        }
        
        public void SetMainLevel(float val) 
        {
            Server.Send(String.Format("/ch/{0}/mix/fader", Number), val);
        }

        public void SetMuteOnBus(int bus, bool muted)
        {
            ThrowIfInvalidBus(bus);
            Server.Send(String.Format("/ch/{0}/mix/{1}/on", Number, bus), !muted);
        }
        public void SetLevelOnBus(int bus, float val)
        {
            ThrowIfInvalidBus(bus);
            Server.Send(String.Format("/ch/{0}/mix/{1}/level", Number, bus), val);
        }


        private static void ThrowIfInvalidBus(int num)
        {
            if(!(num <= 16 && num >= 1))
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}


/*
/ch/[01…32]/mix/on	enum	{OFF, ON}	
/ch/[01…32]/mix/fader	level	[0.0…1.0(+10dB), 1024]	dB
/ch/[01…32]/mix/st	enum	{OFF, ON}	
/ch/[01…32]/mix/pan	linf	[-100.000, 100.000, 2.000]	
/ch/[01…32]/mix/mono	enum	{OFF, ON}	
/ch/[01…32]/mix/mlevel	level	[0.0…1.0 (+10 dB), 161]	dB
/ch/[01…32]/mix/[01…16]/on	enum	{OFF, ON}	
/ch/[01…32]/mix/[01…16]/level	level	[0.0…1.0 (+10 dB), 161]	dB
/ch/[01…32]/mix/[01,03...15]/pan	linf	[-100.000, 100.000, 2.000]	
/ch/[01…32]/mix/[01,03...15]/type	enum	{IN/LC, <-EQ, EQ->, PRE, POST, GRP}	
*/
