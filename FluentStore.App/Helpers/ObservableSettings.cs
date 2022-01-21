using CommunityToolkit.Diagnostics;
using FluentStore.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace FluentStore.Helpers
{
    public class ObservableSettings : INotifyPropertyChanged
    {
        private readonly ApplicationDataContainer settings;
        private readonly Dictionary<string, ApplicationDataContainer> tables;

        public ObservableSettings(ApplicationDataContainer settings, Dictionary<string, ApplicationDataContainer> tables)
        {
            this.settings = settings;
            this.tables = tables;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(T value, [CallerMemberName] string propertyName = null)
        {
            return Set(settings, value, propertyName);
        }

        protected bool Set<T>(string key, T value, [CallerMemberName] string propertyName = null)
        {
            Guard.IsTrue(tables.ContainsKey(key), nameof(key));

            return Set(tables[key], value, propertyName);
        }

        protected bool Set<T>(ApplicationDataContainer container, T value, [CallerMemberName] string propertyName = null)
        {
            if (container.Values.ContainsKey(propertyName))
            {
                var currentValue = (T)container.Values[propertyName];
                if (EqualityComparer<T>.Default.Equals(currentValue, value))
                    return false;
            }

            container.Values[propertyName] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected T Get<T>([CallerMemberName] string propertyName = null, T defaultValue = default)
        {
            return Get<T>(settings, propertyName, defaultValue);
        }

        protected T Get<T>(string key, [CallerMemberName] string propertyName = null, T defaultValue = default)
        {
            Guard.IsTrue(tables.ContainsKey(key), nameof(key));

            return Get<T>(tables[key], propertyName, defaultValue);
        }

        protected T Get<T>(ApplicationDataContainer container, [CallerMemberName] string propertyName = null, T defaultValue = default)
        {
            if (container.Values.ContainsKey(propertyName))
                return (T)container.Values[propertyName];

            var inf = GetType().GetInterface(nameof(ISettingsService));
            var attributes = inf.GetProperty(propertyName)?.CustomAttributes.Where(ca => ca.AttributeType == typeof(DefaultSettingValueAttribute)).ToList();
            if (attributes != null && attributes.Count == 1)
                return (T)attributes[0].NamedArguments[0].TypedValue.Value;

            return defaultValue;
        }
    }
}
