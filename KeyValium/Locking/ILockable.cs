using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Locking
{
    internal interface ILockable : IDisposable
    {
        void Lock();

        void Unlock();

        void ValidateLock(bool expected);
    }
}
