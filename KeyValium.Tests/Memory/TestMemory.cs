using KeyValium.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Tests.Memory
{
    public class TestMemory : IDisposable
    {
        const int BUFFERSIZE = 64;

        public TestMemory()
        {
            _source = new byte[BUFFERSIZE * 2];
            _target = new byte[BUFFERSIZE * 2];

            InitBuffer(_source);
            ClearBuffer(_target);
        }

        private byte[] _source;

        private byte[] _target;

        //[Fact]
        //public unsafe void MemCopyForward()
        //{
        //    for (var length = 0; length < BUFFERSIZE; length++)
        //    {
        //        for (var soffset = 0; soffset < BUFFERSIZE; soffset++)
        //        {
        //            for (var toffset = 0; toffset < BUFFERSIZE; toffset++)
        //            {
        //                ClearBuffer(_target);

        //                fixed (byte* src = _source)
        //                fixed (byte* trg = _target)
        //                {
        //                    MemUtils.MoveMemoryForward(trg + toffset, src + soffset, length);
        //                }

        //                Assert.True(AreBuffersEqual(_target, toffset, _source, soffset, length, true));
        //            }
        //        }
        //    }
        //}

        //[Fact]
        //public unsafe void MemCopyBackward()
        //{
        //    for (var length = 0; length < BUFFERSIZE; length++)
        //    {
        //        for (var soffset = 0; soffset < BUFFERSIZE; soffset++)
        //        {
        //            for (var toffset = 0; toffset < BUFFERSIZE; toffset++)
        //            {
        //                ClearBuffer(_target);

        //                fixed (byte* src = _source)
        //                fixed (byte* trg = _target)
        //                {
        //                    MemUtils.MoveMemoryBackward(trg + toffset, src + soffset, length);
        //                }

        //                Assert.True(AreBuffersEqual(_target, toffset, _source, soffset, length, true));
        //            }
        //        }
        //    }
        //}

        [Fact]
        public unsafe void MemCopyOverlap()
        {
            for (var length = 32; length < BUFFERSIZE; length++)
            {
                for (var soffset = 0; soffset < BUFFERSIZE; soffset++)
                {
                    for (var toffset = 0; toffset < BUFFERSIZE; toffset++)
                    {
                        InitBuffer(_target);
                        var copy = CopyBuffer(_target, soffset, length);

                        fixed (byte* trg = _target)
                        {
                            MemUtils.MemoryMove(trg + toffset, trg + soffset, length);
                        }

                        Assert.True(AreBuffersEqual(_target, toffset, copy, 0, length, false));
                    }
                }
            }
        }

        private bool AreBuffersEqual(byte[] target, int targetoffset, byte[] source, int sourceoffset, int length, bool verifytargetrange)
        {
            if (verifytargetrange)
            {
                for (int i = 0; i < targetoffset; i++)
                {
                    if (target[i] != 0)
                    {
                        // bytes below lower bound have been overwritten
                        return false;
                    }
                }

                for (int i = targetoffset + length; i < target.Length; i++)
                {
                    if (target[i] != 0)
                    {
                        // bytes after upper bound have been overwritten
                        return false;
                    }
                }
            }

            for (int i = 0; i < length; i++)
            {
                if (target[targetoffset + i] != source[sourceoffset + i])
                {
                    return false;
                }
            }

            return true;
        }

        private byte[] CopyBuffer(byte[] buffer, int offset, int length)
        {
            var copy = new byte[length];

            for (int i = 0; i < length; i++)
            {
                copy[i] = buffer[i + offset];
            }

            return copy;
        }

        private void InitBuffer(byte[] buffer)
        {
            var val = 1;

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)val;
                val++;

                // don't use 0x00 and 0xff
                if (val > 254)
                {

                    val = 1;
                }
            }
        }

        private void ClearBuffer(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }

        public void Dispose()
        {

        }
    }
}
