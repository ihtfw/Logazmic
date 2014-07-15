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
        private bool needToUpdate;

        private bool fullUpdate;

        private readonly object updateLock = new object();

        private readonly Timer timer = new Timer(150);
        private readonly Timer forceTimer = new Timer(1500);

        public UpdatableScreen()
        {
            timer.Elapsed += TimerOnElapsed;
            forceTimer.Elapsed += TimerOnElapsed;
            forceTimer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            bool l;
            lock (timer)
            {
                timer.Stop();

                if (!needToUpdate)
                {
                    return;
                }

                l = fullUpdate;

                needToUpdate = false;
                fullUpdate = false;
            }
            if (Monitor.TryEnter(updateLock))
            {
                try
                {
                    DoUpdate(l);
                }
                finally
                {
                    Monitor.Exit(updateLock);
                }
            }
            else
            {
                Update(l);   
            }
        }

        public void Update(bool full = false)
        {
            lock (timer)
            {
                timer.Stop();

                needToUpdate = true;
                if (!fullUpdate)
                {
                    fullUpdate = full;
                }

                timer.Start();
            }
        }

        protected virtual void DoUpdate(bool full)
        {
        }
    }
}