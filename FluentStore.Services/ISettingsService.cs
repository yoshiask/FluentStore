using System;
using System.Collections.Generic;

namespace FluentStore.Services
{
    public interface ISettingsService
    {
        [DefaultSettingValue(Value = false)]
        public bool UseAppInstaller { get; set; }

        [DefaultSettingValue(Value = @"\s?(?i)guide(?-i)\s?")]
        public string ExclusionFilter { get; set; }

        [DefaultSettingValue(Value = true)]
        public bool UseExclusionFilter { get; set; }
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
