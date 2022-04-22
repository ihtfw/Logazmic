namespace Logazmic.ViewModels
{
    using System.Threading;
    using System.Timers;

    using Caliburn.Micro;

    using Timer = System.Timers.Timer;

    /// <summary>
    /// Makes throttling on method update
    /// </summary>
    public class UpdatableScreen : Screen
    {
        private bool _needToUpdate;
        private bool _needToScrollIntoSelected;

        private bool _fullUpdate;

        private readonly object _updateLock = new object();

        private readonly Timer _timer = new Timer(150);
        private readonly Timer _forceTimer = new Timer(1500);

        protected UpdatableScreen()
        {
            _timer.Elapsed += TimerOnElapsed;
            _forceTimer.Elapsed += TimerOnElapsed;
            _forceTimer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            bool l;
            lock (_timer)
            {
                _timer.Stop();

                if (!_needToUpdate)
                {
                    return;
                }

                l = _fullUpdate;

                _needToUpdate = false;
                _fullUpdate = false;
            }
            if (Monitor.TryEnter(_updateLock))
            {
                try
                {
                    DoUpdate(l, _needToScrollIntoSelected);
                }
                finally
                {
                    Monitor.Exit(_updateLock);
                }
            }
            else
            {
                Update(l);   
            }
        }

        public void Update(bool full = false, bool scrollIntoSelected = false)
        {
            lock (_timer)
            {
                _timer.Stop();

                _needToUpdate = true;
                if (!_fullUpdate)
                {
                    _fullUpdate = full;
                }

                _needToScrollIntoSelected = scrollIntoSelected;

                _timer.Start();
            }
        }

        protected virtual void DoUpdate(bool full, bool scrollIntoSelected)
        {
        }
    }
}