using System;
using System.Threading;
using TestSync.Synchronization;

namespace TestSync
{
    internal class Program
    {
        private CockpitSynchronizationContext context;

        public Program()
        {
            this.context = new CockpitSynchronizationContext();
        }

        private static void Main(string[] args)
        {
            var p = new Program();

            Thread.Sleep(1000);
            for (int i = 0; i < 10; i++)
            {
                if (i == 2)
                {
                    new Thread(p.M2).Start(i);
                    continue;
                }
                new Thread(p.M1).Start(i);
            }

            Thread.Sleep(6000);
            p.context.Dispose();
            Console.WriteLine("press a key");
            Console.Read();
        }

        private void M1(object state)
        {
            var item = new SendOrPostCallback((obj) =>
            {
                Console.WriteLine(string.Format("Begin long operaion in thread id: {0} state: {1}", Thread.CurrentThread.ManagedThreadId, obj));
                Thread.Sleep(1000);
                Console.WriteLine(string.Format("End long operaion in thread id: {0} state: {1}", Thread.CurrentThread.ManagedThreadId, obj));
            });

            this.context.Send(item, state);
        }

        private void M2(object state)
        {
            var item = new SendOrPostCallback((obj) =>
            {
                Console.WriteLine("Throw exception in thread: " + Thread.CurrentThread.ManagedThreadId);
                throw new Exception("error");
            });

            this.context.Post(item, state);
        }
    }
}