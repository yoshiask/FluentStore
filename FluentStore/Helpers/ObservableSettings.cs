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

        public ObservableSettings(ApplicationDataContainer settings)
        {
            this.settings = settings;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (settings.Values.ContainsKey(propertyName))
            {
                var currentValue = (T)settings.Values[propertyName];
                if (EqualityComparer<T>.Default.Equals(currentValue, value))
                    return false;
            }

            settings.Values[propertyName] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (settings.Values.ContainsKey(propertyName))
                return (T)settings.Values[propertyName];

            var attributes = GetType().GetTypeInfo().GetDeclaredProperty(propertyName).CustomAttributes.Where(ca => ca.AttributeType == typeof(DefaultSettingValueAttribute)).ToList();
            if (attributes.Count == 1)
                return (T)attributes[0].NamedArguments[0].TypedValue.Value;

            return default(T);
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public sealed class DefaultSettingValueAttribute : Attribute
        {
            public DefaultSettingValueAttribute()
            {
            }

            public DefaultSettingValueAttribute(object value)
            {
                Value = value;
            }

            public object Value { get; set; }
        }
    }
}
