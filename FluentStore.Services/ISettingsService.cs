using System;

namespace FluentStore.Services
{
    public interface ISettingsService
    {
        [DefaultSettingValue(Value = @"(?i)(guide|manual|tutorial)(?-i)")]
        public string ExclusionFilter { get; set; }

        [DefaultSettingValue(Value = true)]
        public bool UseExclusionFilter { get; set; }

        /// <summary>
        /// Gets the enabled state of the specified package handler. Defaults to <c>true</c>.
        /// </summary>
        public bool GetPackageHandlerEnabledState(string typeName);

        /// <summary>
        /// Sets the enabled state of the specified package handler.
        /// </summary>
        public void SetPackageHandlerEnabledState(string typeName, bool enabled);
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
