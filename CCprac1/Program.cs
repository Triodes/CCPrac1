using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace CCprac1
{
    class Program
    {
        //program input params
        int lockType,
            lowInc,
            highEx,
            nThreads,
            modulus,
            mode;

        //hash for search mode
        string hash;

        //list of intervals to be use by the individual threads
        List<int> intervals;

        //counter indicating number of correct values found
        int counter;

        //the locking instance
        ILock locker;

        //specifies if a value has been found for the specified hash so other threads terminate
        bool foundIt = false;

        static void Main(string[] args)
        {
            //read and split input
            string inputRaw = Console.ReadLine();
            string[] input = inputRaw.Split();

            Program p = new Program(input);

            //Console.ReadLine();
        }

        public Program(string[] input)
        {
            //assign input params
            lockType = int.Parse(input[0]);
            lowInc = int.Parse(input[1]);
            highEx = int.Parse(input[2]);
            nThreads = int.Parse(input[4]);
            modulus = int.Parse(input[3]);
            mode = int.Parse(input[5]);

            //calcualte intervals
            intervals = CalcIntervals();

            //select lock type
            if (lockType == 0)
                locker = new TaSLock();
            else if (lockType == 1)
                locker = new TTaSLock();
            else
                locker = new mySpinLock();

            //select mode
            if (mode == 0)
            {
                StartThreads(CountMode);
                Console.WriteLine(counter);
            }
            else if (mode == 1)
            {
                StartThreads(ListMode);
            }
            else if (mode == 2)
            {
                hash = input[6];
                StartThreads(SearchMode);
                if (!foundIt) Console.WriteLine(-1);
            }
        }

        //creates and starts threads in proper mode
        private void StartThreads(Action<object> mode)
        {
            Thread[] t = new Thread[nThreads];

            //create threads
            for (int i = 0; i < nThreads; i++)
            {
                t[i] = new Thread(new ParameterizedThreadStart(mode));
            }

            //start threads
            for (int i = 0; i < nThreads; i++)
            {
                t[i].Start(i);
            }

            //join threads
            for (int i = 0; i < nThreads; i++)
            {
                t[i].Join();
            }
        }

        //runs program in search mode
        public void SearchMode(object id)
        {
            int i = intervals[(int)id];
            while (i < intervals[(int)id + 1] && !foundIt)
            {
                if (MTest(i) && ShaTest(i))
                {
                    foundIt = true;
                    Console.WriteLine(i);
                }
                i++;
            }
        }

        //runs program in count mode
        public void CountMode(object id)
        {
            int i = intervals[(int)id];
            while (i < intervals[(int)id + 1])
            {
                if (MTest(i))
                {
                    locker.Lock();
                    counter++;
                    locker.Unlock();
                }
                i++;
            }
        }

        //runs program in list mode
        public void ListMode(object id)
        {
            int i = intervals[(int)id];
            while (i < intervals[(int)id + 1])
            {
                if (MTest(i))
                {
                    locker.Lock();
                    counter++;
                    Console.WriteLine(counter + " " + i);
                    locker.Unlock();
                }
                i++;
            }
        }

        //compares SHA1 of i with global hash
        public bool ShaTest(int i)
        {
            SHA1 sha = SHA1.Create();
            byte[] hashArray = sha.ComputeHash(Encoding.ASCII.GetBytes(i.ToString()));
            string newHash = "";
            for (int hashPos = 0; hashPos < hashArray.Length; hashPos++)
                newHash += hashArray[hashPos].ToString("x2");
            return newHash == hash;
        }

        //the m-test
        public bool MTest(int obj)
        {
            int result = 0, counter = 1;
            while (obj != 0)
            {
                result += (obj % 10) * counter;
                obj /= 10;
                counter++;
            }

            return result % modulus == 0;
        }

        //calculate intervals threads need to use 
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
