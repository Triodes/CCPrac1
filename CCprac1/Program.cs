using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CCprac1
{
    class Program
    {
        int lockType,
            lowInc,
            highEx,
            nThreads,
            modulus,
            mode;

        string hash;

        List<int> intervals;

        int counter;

        ILock counter_L;

        static void Main(string[] args)
        {
            string inputRaw = Console.ReadLine();
            string[] input = inputRaw.Split();

            Program p = new Program(input);

            Console.ReadLine();
        }

        public Program(string[] input)
        {
            lockType = int.Parse(input[0]);
            lowInc = int.Parse(input[1]);
            highEx = int.Parse(input[2]);
            nThreads = int.Parse(input[4]);
            modulus = int.Parse(input[3]);
            mode = int.Parse(input[5]);

            intervals = CalcIntervals();

            if (lockType == 0)
                counter_L = new TaSLock();
            else if (lockType == 1)
                counter_L = new TTaSLock();
            else
                counter_L = new TaSLock();

            if (mode == 0)
            {
                Thread[] t = new Thread[nThreads];

                for (int i = 0; i < nThreads; i++)
                {
                    t[i] = new Thread(new ParameterizedThreadStart(Tel));
                }

                for (int i = 0; i < nThreads; i++)
                {
                    t[i].Start(i);
                }

                for (int i = 0; i < nThreads; i++)
                {
                    t[i].Join();
                }
                Console.WriteLine(counter);
            }
            else if (mode == 1)
            {

            }
            else if (mode == 2)
            {
                hash = input[6];
            }
        }

        public void Tel(object id)
        {
            int i = intervals[(int)id];
            while (i < intervals[(int)id + 1])
            {
                if (MTest(i))
                {
                    counter_L.Lock();
                    counter++;
                    counter_L.Unlock();
                }
                i++;
            }
        }

        public bool MTest(int obj)
        {
            int result = 0, rest = 0, tenFold;
            int length = (int)Math.Floor(Math.Log10(obj) + 1);
            for (int i = length; i > 0; i--)
            {
                tenFold = (int)Math.Pow(10, i - 1);
                rest = obj % tenFold;
                result += ((obj - rest) / tenFold) * i;
                obj = rest;
            }
            return result % modulus == 0;
        }

        public List<int> CalcIntervals()
        {
            List<int> intervals = new List<int>();
            int temp = (highEx - lowInc) / nThreads;
            intervals.Add(lowInc);
            for (int i = 0; i < nThreads; i++)
            {
                intervals.Add(intervals[i] + temp);
            }
            int index = intervals.Count - 1;
            int rem = highEx - intervals[index];
            for (int i = rem; i > 0; i--)
            {
                intervals[index] += rem;
                index--;
            }
            return intervals;
        }
    }
}
