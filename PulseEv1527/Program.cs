using System;

namespace PulseEv1527
{
    enum Action
    {
        Unknown,
        Encode,
        Decode
    }

    class Program
    {
        static string PULSES = "464 717 458 720 1076 157 999 239 975 253 963 257 963 264 333 850 340 850 337 844 967 260 943 283 317 868 938 283 937 291 928 295 937 287 922 301 918 312 292 896 288 902 908 316 289 905 289 900 294 8997";
        static readonly int THRESHOLD = 256 * 25 / 10;

        static Action WhatToDo = Action.Unknown;
        static uint Node = 0;
        static uint Command = 0;

        static void Main(string[] args)
        {
            HandleArgs(args);

            if (WhatToDo == Action.Unknown) return;

            if (WhatToDo == Action.Decode)
            {
                uint node, action;

                Decode(PULSES, out node, out action);

                Console.WriteLine("{{\n \"message\": {{\n    \"unitcode\": {0},\n    \"command\": {1}\n  }}\n}}", node, action);
            }

            if (WhatToDo == Action.Encode)
            {
                int[] pulses = Encode(Node, Command);
                for (int i = 0; i < pulses.Length; i++)
                {
                    if (i > 0) Console.Write(" ");
                    Console.Write(pulses[i]);
                }
                Console.WriteLine();
            }

        }

        private static void HandleArgs(string[] args)
        {
            if (args.Length < 2)
            {
                HandleWrongCommandLine();
                return;
            }

            string command = args[0].ToLower();
            if (command == "-h")
            {
                HandleWrongCommandLine();
                return;
            }

            if (command == "-d")
            {
                // Decode
                WhatToDo = Action.Decode;

                if (args.Length == 2) PULSES = args[1];
                else
                {
                    // Concatenate remaining args
                    var sb = new System.Text.StringBuilder();
                    for (int i = 1; i < args.Length; i++)
                    {
                        if (i > 1) sb.Append(" ");
                        sb.Append(args[i]);
                    }
                    PULSES = sb.ToString();
                }

                return;
            }

            if (command == "-e" && args.Length == 3)
            {
                // Encode
                WhatToDo = Action.Encode;
                Node = UInt32.Parse(args[1]);
                Command = UInt32.Parse(args[2]);

                return;
            }

            // Unknown command
            HandleWrongCommandLine();
        }

        private static void HandleWrongCommandLine()
        {
            Console.Error.WriteLine("Usage: PulseEv1527 {-h|-e|-d} arg1 ...");
            Console.Error.WriteLine("  decode: -d \"pulse1 pulse2 pulse3 ...\" or -d pulse1 pulse2 pulse3 ...");
            Console.Error.WriteLine("  encode: -d node command");

        }

        static void Decode(string pulseTrain, out uint node, out uint action)
        {
            string[] pulseValues = pulseTrain.Split(' ');

            int[] bits = new int[pulseValues.Length/2 - 1];
            int bit = 0;

            for (int i = 0; i < pulseValues.Length - 2; i+=2)
            {
                bits[bit++] = Int32.Parse(pulseValues[i + 3]) > THRESHOLD ? 1 : 0;
            }


            node = BitArrayToDec(bits, 0, 20);
            action = BitArrayToDec(bits, 20, 4);
        }

        static int[] Encode(uint node, uint action, int pulseWidthUs = 256, int low=400, int high=800)
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

            for (int j = 0, i = start; i < start + count; i++, j++) result += (uint) ((1 << j) * bits[i]);

            return result;
        }

        // Decimal number to bit array, LSB first. Gives array of 32 ints
        static int[] NumberToBitArray(uint number)
        {
            int[] bits = new int[32];

            for (int i=0; i<bits.Length; i++)
            {
                uint remaining = number >> i;
                bits[i] = remaining % 2 == 0 ? 0 : 1;
            }

            return bits;
        }

    }
}
