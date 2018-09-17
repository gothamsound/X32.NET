using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace x32lib
{
    public static class EncodingHelper
    {
        //TODO: Find solution for this wiert dictonary
        public static Dictionary<Type, DataType> types = new Dictionary<Type, DataType>
        {
            { typeof(float), DataType.Float},
            { typeof(int), DataType.Int},
            { typeof(string), DataType.String },
            { typeof(bool), DataType.Bool }
        };

        public static string PadString(object str)
        {
            /* OSC strings must be followed by a null and padded to a 
			   multiple of four bytes, so we calculate how many nulls to add */

            string padded = Convert.ToString(str) + "\0";
            int pad_size = 4 - (padded.Length % 4);
            while (pad_size > 0 && pad_size < 4)
            {
                padded += "\0";    // May redo with StringBuilder but the speed boost may be negligable
                pad_size -= 1;
            }
            return padded;
        }

        public static byte[] CombineBytes(byte[] a, byte[] b)
        {
            // Creates new combined byte array from 2 existing ones
            byte[] c = new byte[a.Length + b.Length];
            a.CopyTo(c, 0);
            b.CopyTo(c, a.Length);
            return c;
        }

        public static byte[] EncodeValue(object val)
        {


            switch (types[val.GetType()])
            {
                case DataType.Float:
                    {
                        // Float
                        byte[] typetag = Encoding.ASCII.GetBytes(",f\0\0");
                        byte[] fbytes = BitConverter.GetBytes(Convert.ToSingle(val));
                        if (BitConverter.IsLittleEndian == true)
                            Array.Reverse(fbytes); // Ensure big endian

                        return CombineBytes(typetag, fbytes);
                    }
                case DataType.Int:
                    {
                        // Integer
                        byte[] typetag = Encoding.ASCII.GetBytes(",i\0\0");
                        byte[] ibytes = BitConverter.GetBytes(Convert.ToInt32(val));
                        if (BitConverter.IsLittleEndian == true)
                            Array.Reverse(ibytes); // Ensure big endian

                        return CombineBytes(typetag, ibytes);
                    }
                case DataType.String:
                    {
                        // String
                        byte[] sbytes = Encoding.ASCII.GetBytes(",s\0\0" + PadString(val));
                        return sbytes;
                    }
                case DataType.Bool:
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

        private static string hexDump(string input)
        {
            byte[] tempBytes = Encoding.ASCII.GetBytes(input);
            StringBuilder hex = new StringBuilder((tempBytes.Length) * 2);
            foreach (byte b in tempBytes)
                hex.AppendFormat("{0:x2} ", b);

            return hex.ToString();
        }
    }
}
