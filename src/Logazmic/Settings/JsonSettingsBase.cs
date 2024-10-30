﻿using JetBrains.Annotations;
using NLog;

namespace Logazmic.Settings
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public abstract class JsonSettingsBase : INotifyPropertyChanged
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [JsonIgnore]
        public readonly object SyncRoot = new();
        
        public static JsonSerializer CreateJsonSerializer()
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            serializer.Converters.Add(new StringEnumConverter());
            return serializer;
        }

        private static JsonSerializerSettings CreateSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
            
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
        
        private void SetDefaults()
        {
            lock (SyncRoot)
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
        }
            
        public abstract void Save();

        protected static T Load<T>(string path) where T : JsonSettingsBase, new()
        {
            T settings;
            if (!File.Exists(path))
            {
                settings = new T();
            }
            else
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrEmpty(json))
                {
                    settings = new T();
                }
                else
                {
                    settings = JsonConvert.DeserializeObject<T>(json, CreateSerializerSettings());
                }
            }

            settings.SetDefaults();

            return settings;
        }

        protected void Save(string path)
        {
            lock (SyncRoot)
            {
                new FileInfo(path).Directory?.Create();

                using var streamWriter = new StreamWriter(path);
                using var jsonTextWriter = new JsonTextWriter(streamWriter);
                jsonTextWriter.Formatting = Formatting.Indented;
                var jsonSerializer = CreateJsonSerializer();
                jsonSerializer.Serialize(jsonTextWriter, this);
            }

            Logger.Trace("Saved!");
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

        #region INotifyPropertyChanged implimentation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}