using System;
using System.Runtime.CompilerServices;

namespace FluentStore.SDK.Attributes
{
    /// <summary>
    /// Indicates to the Fluent Store UI that this property should be shown
    /// on the <see cref="PackageBase"/>'s details page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayAttribute : Attribute
    {
        /// <summary>
        /// The title to display to the user.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute"/> class with the
        /// specified title.
        /// </summary>
        /// <param name="title">
        /// The title to display to the user.
        /// Defaults to the name of the target property.
        /// </param>
        public DisplayAttribute([CallerMemberName] string title = null)
        {
            Title = title ?? string.Empty;
        }
    }

    public class DisplayInfo
    {
        public DisplayInfo(string title, object value)
        {
            Title = title;
            Value = value;
        }

        public DisplayInfo(DisplayAttribute attr, object value) : this(attr.Title, value)
        {

        }

        public string Title { get; set; }
        public object Value { get; set; }
    }
}
