using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Benchmarks.Misc
{
    using KvInternalType = System.UInt16;
    using KvLongLength = System.UInt64;

    /// <summary>
    /// a poor man's type definition
    /// </summary>
    public readonly struct TestType : IComparable, IComparable<TestType>
    {
        public static readonly int BinarySizeOf = sizeof(KvInternalType);

        public static readonly TestType MinValue = new TestType(KvInternalType.MinValue);

        public static readonly TestType MaxValue = new TestType(KvInternalType.MaxValue);

        public static readonly TestType Zero = default;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TestType(KvInternalType value)
        {
            Value = value;
        }

        public readonly KvInternalType Value;

        #region Reading and Writing in byte arrays

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static unsafe KvLongLength ReadAt(byte* p)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (*(KvInternalType*)p);
            }
            else
            {
                return BinaryPrimitives.ReverseEndianness(*(KvInternalType*)p);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public unsafe void WriteAt(byte* p)
        {
            if (BitConverter.IsLittleEndian)
            {
                *(KvInternalType*)p = Value;
            }
            else
            {
                *(KvInternalType*)p = BinaryPrimitives.ReverseEndianness(Value);
            }
        }

        #endregion

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator ==(TestType page1, TestType page2)
        {
            return page1.Value == page2.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator !=(TestType page1, TestType page2)
        {
            return page1.Value != page2.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TestType operator ++(TestType page)
        {
            return new TestType((KvInternalType)(page.Value + 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TestType operator --(TestType page)
        {
            return new TestType((KvInternalType)(page.Value - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator <(TestType page1, TestType page2)
        {
            return page1.Value < page2.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator >(TestType page1, TestType page2)
        {
            return page1.Value > page2.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator <=(TestType page1, TestType page2)
        {
            return page1.Value <= page2.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static bool operator >=(TestType page1, TestType page2)
        {
            return page1.Value >= page2.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TestType operator +(TestType page, TestType val)
        {
            return new TestType((KvInternalType)(page.Value + val.Value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static TestType operator -(TestType page, TestType val)
        {
            return new TestType((KvInternalType)(page.Value - val.Value));
        }

        #endregion

        #region Equality

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return this == (TestType)obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(string format)
        {
            return Value.ToString(format);
        }

        #endregion

        #region IComparable

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int CompareTo(object obj)
        {
            var x = (KvLongLength)obj;
            return this.CompareTo(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public int CompareTo(TestType other)
        {
            if (this.Value == other.Value)
            {
                return 0;
            }

            return this.Value < other.Value ? -1 : +1;
        }

        #endregion
    }
}
