using System;
using System.Text;

namespace Termors.Services.HippotronicsPilightReceiver
{
    public class PilightMessage
    {
        private StringBuilder _sb = new StringBuilder();
        private int _braces = -1;

        public bool IsComplete
        {
            get
            {
                return _braces == 0;
            }
        }

        public void AddMessageLine(string line)
        {
            char[] lineAsChars = line.ToCharArray();

            _sb.Append(line);

            foreach (char c in lineAsChars)
            {
                if (c == '{')
                {
                    if (_braces == -1) _braces = 0;
                    _braces++;
                }
                else if (c == '}')
                {
                    if (_braces > 0) _braces--;
                }
            }
        }

        public string Message
        {
            get
            {
                return _sb.ToString();
            }
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
