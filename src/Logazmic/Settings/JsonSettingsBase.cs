namespace Logazmic.Settings
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization.Formatters;

    using Caliburn.Micro;

    using Logazmic.Annotations;

    using Newtonsoft.Json;

    public abstract class JsonSettingsBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implimentation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private static readonly JsonSerializerSettings serilizerSettings = new JsonSerializerSettings
                                                                           {
                                                                               TypeNameHandling = TypeNameHandling.Auto,
                                                                               TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                                                                               
                                                                           };

        protected JsonSettingsBase()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            SetDefaults();
            SetupAutoSave();
        }

        private void SetupAutoSave()
        {
            PropertyChanged += (sender, args) => Save();
            var notifyCollectionChangedProperties = GetType().GetProperties().Where(p => typeof(INotifyCollectionChanged).IsAssignableFrom(p.PropertyType));
            foreach (var propertyInfo in notifyCollectionChangedProperties)
            {
                var collectionChanged = ((INotifyCollectionChanged)propertyInfo.GetValue(this));
                if(collectionChanged != null)
                    collectionChanged.CollectionChanged += (_,__) => Save();
            }
        }

        protected virtual void SetDefaults()
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (!propertyInfo.IsDefined(typeof(DefaultValueAttribute)))
                {
                    continue;
                }
                var value = propertyInfo.GetCustomAttributes<DefaultValueAttribute>().Single().DefaultValue;
                propertyInfo.SetValue(this, value);
            }
        }

        public abstract void Save();

        protected static T Load<T>(string path) where T : JsonSettingsBase, new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, serilizerSettings);
        }

        protected void Save(string path)
        {
            new FileInfo(path).Directory.Create();
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, serilizerSettings);
            File.WriteAllText(path, json);
        }

        [AttributeUsage(AttributeTargets.Property)]
        protected class DefaultValueAttribute : Attribute
        {
            public DefaultValueAttribute(object defaultValue)
            {
                DefaultValue = defaultValue;
            }

            public object DefaultValue { get; set; }
        }
    }
}