using System;
using Termors.Services.Libraries.Ev1527Lib;

namespace Termors.Services.Tools.PulseEv1527
{
    enum Action
    {
        Unknown,
        Encode,
        Decode
    }

    public class Program
    {
        static string PULSES = "464 717 458 720 1076 157 999 239 975 253 963 257 963 264 333 850 340 850 337 844 967 260 943 283 317 868 938 283 937 291 928 295 937 287 922 301 918 312 292 896 288 902 908 316 289 905 289 900 294 8997";

        static Action WhatToDo = Action.Unknown;
        static uint Node = 0;
        static uint Command = 0;

        public static void Main(string[] args)
        {
            HandleArgs(args);

            if (WhatToDo == Action.Unknown) return;

            if (WhatToDo == Action.Decode)
            {
                uint node, action;

                Ev1527Decoder.Decode(PULSES, out node, out action);

                Console.WriteLine("{{\n \"message\": {{\n    \"unitcode\": {0},\n    \"command\": {1}\n  }}\n}}", node, action);
            }

            if (WhatToDo == Action.Encode)
            {
                int[] pulses = Ev1527Decoder.Encode(Node, Command);
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


    }
}
