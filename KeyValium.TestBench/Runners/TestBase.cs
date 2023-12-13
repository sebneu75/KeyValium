using KeyValium.TestBench.Measure;
using System;
using System.Collections.Generic;

namespace KeyValium.TestBench.Runners
{
    abstract internal class TestBase : RunnerBase
    {
        /// <summary>
        /// returns a default TestDescription for Tests
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override IEnumerable<TestDescription> GetItems()
        {
            var td = new TestDescription(Name);
            td.MinKeySize = 16;
            td.MaxKeySize = 16;
            td.MinValueSize = 128;
            td.MaxValueSize = 128;
            td.KeyCount = 100000;
            td.CommitSize = 10000;
            td.GenStrategy = KeyGenStrategy.Random;
            td.OrderInsert = KeyOrder.Random;
            td.OrderRead = KeyOrder.Random;
            td.OrderDelete = KeyOrder.Ascending;

            yield return td;
        }
    }
}
