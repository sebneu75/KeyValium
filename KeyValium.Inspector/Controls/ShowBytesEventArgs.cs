using System;

namespace KeyValium.Inspector.Controls
{
    internal class ShowBytesEventArgs : EventArgs
    {
        internal ShowBytesEventArgs(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] Bytes
        {
            get;
            private set;
        }
    }
}