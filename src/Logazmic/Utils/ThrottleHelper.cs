using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace Logazmic.Utils
{
    using Timer = System.Timers.Timer;

    public class ThrottleHelper : IDisposable
    {
        private readonly HashSet<object> objects = new HashSet<object>();
        private readonly Action<List<object>> action;

        private readonly object dispObject = new object();
        private readonly object syncObject = new object();

        private Timer timer;
        
        public ThrottleHelper(int timeInMs, Action<List<object>> action)
        {
            this.action = action;
            timer = new Timer(timeInMs);
            timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (dispObject)
            {
                if (timer == null)
                    return;

                timer.Stop();
            }

            if (!Monitor.TryEnter(syncObject))
            {
                Do();
                return;
            }
            try
            {
                List<object> data = null;
                lock (objects)
                {
                    if (objects.Any())
                    {
                        data = objects.ToList();
                        objects.Clear();
                    }
                }

                action(data);
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                Monitor.Exit(syncObject);
            }
        }

        public void Do(object obj = null)
        {
            lock (dispObject)
            {
                if (timer == null)
                    return;
            }

            if (obj != null)
            {
                lock (objects)
                {
                    objects.Add(obj);
                }
            }
            lock (dispObject)
            {
                if (timer == null)
                    return;

                timer.Stop();
                timer.Start();
            }
        }

        public event EventHandler<Exception> Error;

        protected virtual void OnError(Exception e)
        {
            var handler = Error;
            handler?.Invoke(this, e);
        }

        public void Dispose()
        {
            lock (dispObject)
            {
                if (timer != null)
                {
                    timer.Elapsed -= TimerOnElapsed;
                    timer?.Dispose();
                    timer = null;
                }
            }
        }
    }
}
