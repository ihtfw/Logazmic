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
        private readonly HashSet<object> _objects = new HashSet<object>();
        private readonly Action<List<object>> _action;

        private readonly object _dispObject = new object();
        private readonly object _syncObject = new object();

        private Timer _timer;
        
        public ThrottleHelper(int timeInMs, Action<List<object>> action)
        {
            _action = action;
            _timer = new Timer(timeInMs);
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_dispObject)
            {
                if (_timer == null)
                    return;

                _timer.Stop();
            }

            if (!Monitor.TryEnter(_syncObject))
            {
                Do();
                return;
            }
            try
            {
                List<object> data = null;
                lock (_objects)
                {
                    if (_objects.Any())
                    {
                        data = _objects.ToList();
                        _objects.Clear();
                    }
                }

                _action(data);
            }
            catch (Exception e)
            {
                OnError(e);
            }
            finally
            {
                Monitor.Exit(_syncObject);
            }
        }

        public void Do(object obj = null)
        {
            lock (_dispObject)
            {
                if (_timer == null)
                    return;
            }

            if (obj != null)
            {
                lock (_objects)
                {
                    _objects.Add(obj);
                }
            }
            lock (_dispObject)
            {
                if (_timer == null)
                    return;

                _timer.Stop();
                _timer.Start();
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
            lock (_dispObject)
            {
                if (_timer != null)
                {
                    _timer.Elapsed -= TimerOnElapsed;
                    _timer?.Dispose();
                    _timer = null;
                }
            }
        }
    }
}
