using System;
using System.Threading;

namespace TestSync.Synchronization
{
    internal class CallbackItem : IDisposable
    {
        private readonly ManualResetEvent executionCompleted;
        private readonly SendOrPostCallback callback;
        private readonly object state;

        private bool disposed;

        public CallbackItem(SendOrPostCallback item, object state)
        {
            this.Id = Guid.NewGuid();
            this.executionCompleted = new ManualResetEvent(false);
            this.callback = item;
            this.state = state;
        }

        internal Guid Id { get; private set; }

        internal Exception Error { get; private set; }

        internal void Execute()
        {
            try
            {
                this.callback(this.state);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CallbackItem.Execute() handled exception. Id: " + this.Id + " state: " + this.state.ToString());
                this.Error = ex;
            }
            finally
            {
                Console.WriteLine("CallbackItem.Execute() release execution handle. Id: " + this.Id);
                this.executionCompleted.Set();
            }
        }

        internal WaitHandle ExecutionCompleted
        {
            get
            {
                return this.executionCompleted;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("CallbackItem.Dispose()");
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            Console.WriteLine("CallbackItem.Dispose(bool)");
            if (!disposed)
            {
                Console.WriteLine("CallbackItem.Dispose(bool) not disposed");
                if (disposing)
                {
                    Console.WriteLine("CallbackItem.Dispose(bool) disposing");
                    Console.WriteLine("CallbackItem.Execute() release execution handle. Id: " + this.Id);
                    this.executionCompleted.Set();
                }
            }

            this.disposed = true;
        }
    }
}