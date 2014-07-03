namespace Logazmic.ViewModels
{
    using System.Timers;

    using Caliburn.Micro;

    /// <summary>
    /// Makes throttling on method update
    /// </summary>
    public class UpdatableScreen : Screen
    {
        private bool needToUpdate = false;

        private bool fullUpdate = false;

        private bool isUpdating = false;

        private readonly Timer timer = new Timer(150);

        public UpdatableScreen()
        {
            timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            bool l;
            lock (timer)
            {
                timer.Stop();

                if (!needToUpdate)
                    return;

                l = fullUpdate;

                needToUpdate = false;
                fullUpdate = false;
            }

            isUpdating = true;
            try
            {
                DoUpdate(l);
            }
            finally
            {
                isUpdating = false;
            }
        }

        public void Update(bool full = false)
        {
            lock (timer)
            {
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