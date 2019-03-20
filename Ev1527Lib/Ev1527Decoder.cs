using System;

namespace Termors.Services.Libraries.Ev1527Lib
{
    public static class Ev1527Decoder
    {
        static readonly int THRESHOLD = 256 * 25 / 10;

        public static void Decode(string pulseTrain, out uint node, out uint action)
        {
            string[] pulseValues = pulseTrain.Split(' ');

            int[] bits = new int[pulseValues.Length / 2 - 1];
            int bit = 0;

            for (int i = 0; i < pulseValues.Length - 2; i += 2)
            {
                bits[bit++] = Int32.Parse(pulseValues[i + 3]) > THRESHOLD ? 1 : 0;
            }


            node = BitArrayToDec(bits, 0, 20);
            action = BitArrayToDec(bits, 20, 4);
        }

        public static int[] Encode(uint node, uint action, int pulseWidthUs = 256, int low = 400, int high = 800)
        {
            int[] nodeBits = NumberToBitArray(node);
            int[] actionBits = NumberToBitArray(action);
            int[] ev1527Values = new int[50];

            ev1527Values[0] = low;
            ev1527Values[1] = high;

            for (int i = 0; i < 20; i++)
            {
                ev1527Values[2 * i + 2] = high;
                ev1527Values[2 * i + 3] = nodeBits[i] == 1 ? high : low;
            }

            for (int i = 0; i < 4; i++)
            {
                ev1527Values[2 * i + 42] = high;
                ev1527Values[2 * i + 43] = actionBits[i] == 1 ? high : low;
            }

            ev1527Values[49] = pulseWidthUs * 34;

            return ev1527Values;
        }

        // Bin array to decimal number, LSB first
        static uint BitArrayToDec(int[] bits, int start, int count)
        {
            uint result = 0;

            for (int j = 0, i = start; i < start + count; i++, j++) result += (uint)((1 << j) * bits[i]);

            return result;
        }

        // Decimal number to bit array, LSB first. Gives array of 32 ints
        static int[] NumberToBitArray(uint number)
        {
            int[] bits = new int[32];

            for (int i = 0; i < bits.Length; i++)
            {
                uint remaining = number >> i;
                bits[i] = remaining % 2 == 0 ? 0 : 1;
            }

            return bits;
        }
    }
}
