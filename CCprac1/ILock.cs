using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CCprac1
{
    interface ILock
    {
        bool Lock();
        void Unlock();
    }

    class TaSLock : ILock
    {
        protected int val;
        public virtual bool Lock()
        {
            while (Interlocked.Exchange(ref val, 1) == 1);
            return true;
        }

        public void Unlock()
        {
            val = 0;
        }
    }

    class TTaSLock : TaSLock, ILock
    {
        public override bool Lock()
        {
            while (true)
            {
                while (val == 1) ;
                if (Interlocked.Exchange(ref val, 1) == 0)
                    return true;
            }
        }
    }

    class mySpinLock : ILock
    {
        SpinLock lockObj;
        public mySpinLock()
        {
            lockObj = new SpinLock();
        }

        public bool Lock()
        {
            bool temp = false;
            lockObj.Enter(ref temp);
            return temp;
        }

        public void Unlock()
        {
            lockObj.Exit();
        }
    }
}
