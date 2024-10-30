﻿namespace Logazmic.Utils
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class PropertyChangedHelpers
    {
        private class Disposable: IDisposable
        {
            private readonly Action _action;

            public Disposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }

        public static IDisposable SubscribeToPropertyChanged<TSource, TProp>(
            this TSource source,
            Expression<Func<TSource, TProp>> propertySelector,
            Action onChanged)
            where TSource : INotifyPropertyChanged
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (propertySelector == null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }
            if (onChanged == null)
            {
                throw new ArgumentNullException(nameof(onChanged));
            }

            var subscribedPropertyName = GetPropertyName(propertySelector);

            PropertyChangedEventHandler handler = (_, e) =>
            {
                if (string.Equals(e.PropertyName, subscribedPropertyName, StringComparison.InvariantCulture))
                {
                    onChanged();
                }
            };

            source.PropertyChanged += handler;

            return new Disposable(() => source.PropertyChanged -= handler);
        }

        public static IDisposable SubscribeToPropertyChanging<TSource, TProp>(
            this TSource source, Expression<Func<TSource,
                TProp>> propertySelector,
            Action onChanging)
            where TSource : INotifyPropertyChanging
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (propertySelector == null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }
            if (onChanging == null)
            {
                throw new ArgumentNullException(nameof(onChanging));
            }

            var subscribedPropertyName = GetPropertyName(propertySelector);

            PropertyChangingEventHandler handler = (_, e) =>
            {
                if (string.Equals(e.PropertyName, subscribedPropertyName, StringComparison.InvariantCulture))
                {
                    onChanging();
                }
            };

            source.PropertyChanging += handler;

            return new Disposable(() => source.PropertyChanging -= handler);
        }

        private static string GetPropertyName<TSource, TProp>(Expression<Func<TSource, TProp>> propertySelector)
        {
            var memberExpr = propertySelector.Body as MemberExpression;

            if (memberExpr == null)
            {
                throw new ArgumentException(@"must be a member accessor", nameof(propertySelector));
            }

            var propertyInfo = memberExpr.Member as PropertyInfo;

            if (propertyInfo == null || propertyInfo.DeclaringType != typeof(TSource))
            {
                throw new ArgumentException(@"must yield a single property on the given object", nameof(propertySelector));
            }

            return propertyInfo.Name;
        }
    }
}