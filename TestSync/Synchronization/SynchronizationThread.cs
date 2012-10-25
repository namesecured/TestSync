using System;
using System.ComponentModel;
using System.Threading;

namespace TestSync.Synchronization
{
    internal class SynchronizationThread
    {
        private ConcurrentQueue<CallbackItem> queue;
        private ManualResetEvent canelledEvent;
        private BackgroundWorker worker;

        internal SynchronizationThread(ConcurrentQueue<CallbackItem> queue)
        {
            this.queue = queue;
            this.worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            this.worker.DoWork += Run;

            this.canelledEvent = new ManualResetEvent(false);
            this.worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunCompleted);
        }

        internal void Start()
        {            
            if (this.worker.IsBusy)
            {                
                throw new InvalidOperationException("Synchronization thread already running.");                
            }
            
            this.worker.RunWorkerAsync();
        }

        internal void Stop()
        {
            Console.WriteLine("SynchronizationThread.Stop()");
            this.worker.CancelAsync();

            Console.WriteLine("SynchronizationThread.Stop() is waiting for cancel to complete.");
            this.canelledEvent.WaitOne();
        }

        private void Run(object sender, DoWorkEventArgs args)
        {
            Console.WriteLine("SynchronizationThread.Run() worker started.");
            while (true)
            {
                if (this.worker == null)
                {
                    Console.WriteLine("SynchronizationThread.Run() worker instance was not found. Exiting.");
                    args.Cancel = true;
                    return;
                }

                if (this.worker.CancellationPending)
                {
                    Console.WriteLine("SynchronizationThread.Run() cancellation pending");
                    args.Cancel = true;
                    return;
                }

                CallbackItem callback = this.queue.Dequeue();
                if (callback != null)
                {
                    callback.Execute();
                }
            }
        }

        private void RunCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine("SynchronizationThread.RunCompleted() an error handled. " + e.Error.Message);
            }

            if (e.Cancelled)
            {
                Console.WriteLine("SynchronizationThread.RunCompleted() loop has been cancelled.");
                this.queue.CancelAsync();                
                Console.WriteLine("SynchronizationThread.RunCompleted() release cancel wait handle.");
                this.queue.Dispose();
                this.canelledEvent.Set();
                return;
            }

            Console.WriteLine("SynchronizationThread.RunCompleted() restarting.");
            this.Start();
        }
    }
}