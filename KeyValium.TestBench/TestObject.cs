using System;

namespace KeyValium.TestBench
{
    public class TestObject
    {
        public TestObject()
        {
            PropArray = new int[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            PropByte = byte.MaxValue;
            PropDateTime = DateTime.Now;
            PropDecimal = 1234567890123456789012345678m;
            PropDouble = double.MaxValue;
            PropFloat = float.MaxValue;
            PropInt = int.MaxValue;
            PropLong = long.MaxValue;
            PropShort = short.MaxValue;
            PropString = "abcdefghijklmnopqrstuvwxyz";
            PropTimeSpan = TimeSpan.MaxValue;

            _fieldDateTime = null;
            _fieldInt = null;
            _fieldString = null;
        }

        public string PropString
        {
            get;
            set;
        }

        public byte PropByte
        {
            get;
            set;
        }

        public short PropShort
        {
            get;
            set;
        }

        public int PropInt
        {
            get;
            set;
        }

        public long PropLong
        {
            get;
            set;
        }

        public decimal PropDecimal
        {
            get;
            set;
        }

        public float PropFloat
        {
            get;
            set;
        }

        public double PropDouble
        {
            get;
            set;
        }

        public DateTime PropDateTime
        {
            get;
            set;
        }

        public TimeSpan PropTimeSpan
        {
            get;
            set;
        }

        public int[] PropArray
        {
            get;
            set;
        }

        public DateTime? _fieldDateTime;

        public int? _fieldInt;

        public string _fieldString;
    }
}
