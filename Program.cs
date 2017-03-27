using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace TestLinqPerf
{
    class Program
    {
        private static void Main(string[] args)
        {
            var rnd = new Random(42);
            var array = Enumerable.Repeat(rnd, 100000).Select(r => r.Next()).ToArray();

            var benchmark = new Benchmark(array);
            Console.WriteLine($"{"Test",-20}\t{"Iterations",-20}\t{"Average",-20}\t{"Total",-20}");
            var results = benchmark.Run();
            foreach (var r in results)
            {
                Console.WriteLine($"{r.Name,-20}\t{r.Iterations,-20}\t{r.AverageTime.TotalSeconds,-20}\t{r.TotalTime.TotalSeconds,-20}");
            }
        }

        private class Benchmark
        {
            private readonly int[] _array;
            public Benchmark(int[] array)
            {
                _array = array;
            }

            public IEnumerable<TestResult> Run()
            {
                yield return Run(Select,                5000);
                yield return Run(SelectAndToArray,      5000);
                yield return Run(Where,                 5000);
                yield return Run(WhereAndToArray,       5000);
                yield return Run(OrderBy,               500);
                yield return Run(OrderByAndToArray,     500);
            }

            private TestResult Run(Action action, int iterations)
            {
                // JIT warmup
                action();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                var stopWatch = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    action();
                }
                stopWatch.Stop();
                return new TestResult(action.GetMethodInfo().Name, iterations, stopWatch.Elapsed);
            }

            private void Select()
            {
                _array.Select(i => i * i).Count();
            }

            private void SelectAndToArray()
            {
                _array.Select(i => i * i).ToArray();
            }

            private void Where()
            {
                _array.Where(i => i % 5 == 0).Count();
            }

            private void WhereAndToArray()
            {
                _array.Where(i => i % 5 == 0).ToArray();
            }

            private void OrderBy()
            {
                _array.OrderBy(i => i).Count();
            }

            private void OrderByAndToArray()
            {
                _array.OrderBy(i => i).ToArray();
            }
        }

        private class TestResult
        {
            public TestResult(string name, int iterations, TimeSpan totalTime)
            {
                Name = name;
                Iterations = iterations;
                TotalTime = totalTime;
                AverageTime = TimeSpan.FromTicks(totalTime.Ticks / iterations);
            }

            public string Name { get; }
            public int Iterations { get; }
            public TimeSpan TotalTime { get; }
            public TimeSpan AverageTime { get; }
        }
    }
}
