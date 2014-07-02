namespace Logazmic.Utils
{
    using System;

    using Caliburn.Micro;

    static class Messaging
    {
        private static IEventAggregator eventAggregator;

        public static IEventAggregator EventAggregator
        {
            get { return eventAggregator ?? (eventAggregator = EventAggregatorFactory()); }
            set
            {
                eventAggregator = value;
            }
        }

        public static Func<IEventAggregator> EventAggregatorFactory = () => new EventAggregator();

        public static void Unsubscribe(object subscriber)
        {
            EventAggregator.Unsubscribe(subscriber);
        }
        
        public static void Subscribe(object subscriber)
        {
            EventAggregator.Subscribe(subscriber);
        }

        public static bool HandlerExistsFor(Type type)
        {
            return EventAggregator.HandlerExistsFor(type);
        }

        public static void Publish(object message)
        {
            EventAggregator.Publish(message, action => action());
        }
    }
}
