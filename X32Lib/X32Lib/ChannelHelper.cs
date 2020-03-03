using System;
using System.Collections.Generic;
using System.Text;

namespace X32Lib
{
    public static class ChannelHelper
    {
        public static bool IsValidChannelNumber(int num)
        {
            return num <= 32 && num >= 1;
        }
        public static void ThrowIfChannelNumberInvalid(int num)
        {
            if (!IsValidChannelNumber(num))
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
