using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace phystest
{
    public class ConsoleHistory
    {
        String[] history;
        int max;
        int currentPosition;
        public ConsoleHistory(int historySize)
        {
            max = historySize;
            history = new String[max];
            for (int i = 0; i < max; i++)
            {
                history[i] = String.Empty;
            }
            currentPosition = 0;
        }
        public void Add(String command)
        {
            history[currentPosition] = command;
            currentPosition++;
            if (currentPosition == max)
                currentPosition = 0;
        }
        public String[] Get(int number)
        {
            var retstr = new String[number];

            if (number > max)
                throw new ArgumentOutOfRangeException("number", String.Format("requested {0} strings, {1} available", number, max));

            for (int i = 0; i < number; i++)
            {
                retstr[i] = history[(currentPosition + i) % max];
            }
            return retstr;
        }
    }
}
