using System;
using System.Threading;

namespace TestSync.Synchronization
{
    public class CockpitSynchronizationContext : SynchronizationContext, IDisposable
    {
        private bool disposed;
        private bool disposing;
        private SynchronizationThread syncThread;
        private ConcurrentQueue<CallbackItem> queue;

        public CockpitSynchronizationContext()
            : base()
        {
            this.queue = new ConcurrentQueue<CallbackItem>();
            this.syncThread = new SynchronizationThread(queue);
            this.syncThread.Start();
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            var item = new CallbackItem(d, state);
            this.queue.Enqueue(item);
            Console.WriteLine("CockpitSynchronizationContext.Send() wait for execution handle. Id: " + item.Id);
            item.ExecutionCompleted.WaitOne();
            if (item.Error != null)
            {
                throw item.Error;
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            var item = new CallbackItem(d, state);
            this.queue.Enqueue(item);
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public void Dispose()
        {
            Console.WriteLine("CockpitSynchronizationContext.Dispose()");
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Console.WriteLine("CockpitSynchronizationContext.Dispose(bool)");
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.syncThread.Stop();
            }

            this.disposed = true;
        }
    }
}