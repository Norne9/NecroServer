using System;
using System.Collections.Generic;
using System.Text;

namespace NecroServer
{
    public static class IntArrayExt
    {
        public static int[] SetBit(this int[] self, int index)
        {
            int byteIndex = index / 32;
            int bitIndex = index % 32;
            int mask = 1 << bitIndex;

            self[byteIndex] = self[byteIndex] | mask;
            return self;
        }

        public static bool GetBit(this int[] self, int index)
        {
            int byteIndex = index / 32;
            int bitIndex = index % 32;
            int mask = 1 << bitIndex;

            return (self[byteIndex] & mask) != 0;
        }
    }
}
