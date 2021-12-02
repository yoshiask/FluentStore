using System;

namespace FluentStore.Services
{
    public interface ISettingsService
    {
        [DefaultSettingValue(Value = @"(?i)(guide|manual|tutorial)(?-i)")]
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
