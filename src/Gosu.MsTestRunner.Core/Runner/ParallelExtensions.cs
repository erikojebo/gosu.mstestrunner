using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gosu.MsTestRunner.Core.Runner
{
    public static class ParallelExtensions
    {
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source,
            Action<T> action, 
            int maxParallelism)
        {
            var parallelismLimiter = new SemaphoreSlim(initialCount: maxParallelism, maxCount: maxParallelism);

            var tasks = source.Select(
                x => Task.Run(async () =>
                {
                    await parallelismLimiter.WaitAsync();

                    try
                    {
                        action(x);
                    }
                    finally
                    {
                        parallelismLimiter.Release();
                    }
                }));

            await Task.WhenAll(tasks);
        }
    }
}