using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TestSync.Synchronization
{
    internal class ConcurrentQueue<T> : IDisposable where T : class
    {
        private readonly object syncRoot;

        private readonly ManualResetEvent cancelEvent;
        private readonly WaitHandle[] waitHandles;
        private Semaphore hasNewItem;
        private Queue<T> queue;        

        internal ConcurrentQueue()
        {
            this.syncRoot = new object();
            this.cancelEvent = new ManualResetEvent(false);
            this.queue = new Queue<T>();
            this.hasNewItem = new Semaphore(0, int.MaxValue);
            this.waitHandles = new WaitHandle[] { this.cancelEvent, this.hasNewItem };
        }

        internal void Enqueue(T item)
        {
            lock (this.syncRoot)
            {
                if (this.queue == null)
                {
                    var i = item as IDisposable;
                    if (i != null)
                    {                        
                        i.Dispose();
                    }
                }

                this.queue.Enqueue(item);
            }
            // notify that new item has came
            this.hasNewItem.Release();
        }

        internal T Dequeue()
        {
            // waits for a new item or cancel event
            WaitHandle.WaitAny(this.waitHandles);
            T item;
            lock (this.syncRoot)
            {
                if (this.queue == null)
                {
                    return default(T);
                }

                item = this.queue.Dequeue() ?? default(T);
            }

            return item;
        }

        internal void CancelAsync()
        {
            // notify that cancel has been requested
            this.cancelEvent.Set();
        }

        public void Dispose()
        {
            // check
            if (this.hasNewItem == null)
            {
                return;
            }

            this.hasNewItem.Close();
            lock (syncRoot)
            {
                this.queue.Cast<IDisposable>().ToList().ForEach(item => item.Dispose());
                this.queue.Clear();
                this.queue = null;
            }
            this.hasNewItem = null;
        }
    }
}