using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyValium.Performance
{
    public class Counters
    {
        internal static IKvStopwatch Stopwatch;

        static Counters()
        {
            Stopwatch = new DefaultStopwatch();
        }

        private static object _lock = new object();

        #region Callcounts including caller

        internal static Dictionary<string, CallInfo> CallCounts = new Dictionary<string, CallInfo>();

        [Conditional("CALLCOUNTS")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void CallCount()
        {
            if (!KvDebugOptions.PerformanceCount) return;

            var trace = new StackTrace();

            // the called method
            var frame1 = trace.GetFrame(1);
            var called = GetMethodName(frame1);

            // the caller
            var frame2 = trace.GetFrame(2);
            var calledby = GetMethodName(frame2);

            IncCallCount(called, calledby);
        }

        private static void IncCallCount(string called, string calledby)
        {
            lock (_lock)
            {
                if (!CallCounts.ContainsKey(called))
                {
                    CallCounts.Add(called, new CallInfo());
                }

                CallCounts[called].Count++;
                CallCounts[called].AddCall(calledby);
            }
        }

        private static string GetMethodName(StackFrame frame)
        {
            var method = frame.GetMethod();
            var p = method.GetParameters().Select(x => x.ParameterType.Name[0]);

            var key = string.Format("{0}.{1}({2})", GetTypeName(method), method.Name, string.Join("", p));

            return key;
        }

        private static string GetTypeName(MethodBase method)
        {
            var type = method.DeclaringType;
            return string.Format("{0}", type.Name);
        }

        [Conditional("CALLCOUNTS")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PrintCallCounts()
        {
            foreach (var ci in CallCounts.OrderBy(x=>x.Key))
            {
                Console.WriteLine("{0,8}: {1} ", ci.Value.Count, ci.Key);

                foreach (var caller in ci.Value.Callers.OrderBy(x => x.Key))
                {
                    Console.WriteLine("{0,8}:  <- {1} ", caller.Value, caller.Key);
                }

                Console.WriteLine();
            }
        }

        [Conditional("CALLCOUNTS")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ClearCallCounts()
        {
            CallCounts.Clear();
        }

        #endregion

        #region Measure Time

        internal static Dictionary<string, PerformanceInfo> Performance = new Dictionary<string, PerformanceInfo>();

        static long _start;
        static long _stop;

        [Conditional("PERFORMANCE")]
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Start()
        {
            _start = Stopwatch.GetTicks();
        }

        [Conditional("PERFORMANCE")]
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Stop(string name, long count = 1)
        {
            _stop = Stopwatch.GetTicks();

            if (!Performance.ContainsKey(name))
            {
                Performance.Add(name, new PerformanceInfo() { Name = name });
            }

            Performance[name].AddValue((double)(_stop - _start) / (double)count);
        }

        [Conditional("PERFORMANCE")]
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ClearPerformance()
        {
            Performance.Clear();
        }

        #endregion

        #region Simple Counter

        internal static Dictionary<string, long> Counts = new Dictionary<string, long>();

        [Conditional("PERFORMANCE")]
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Count(string name)
        {
            if (!Counts.ContainsKey(name))
            {
                Counts.Add(name, 0);
            }

            Counts[name]++;
        }

        [Conditional("PERFORMANCE")]
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void CountIf(string name, bool condition)
        {
            if (condition)
            {
                Count(name);
            }
        }

        [Conditional("PERFORMANCE")]
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ClearCounts()
        {
            Counts.Clear();
        }

        #endregion
    }
}
