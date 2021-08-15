using System;
using System.Runtime.CompilerServices;

namespace FluentStore.SDK.Attributes
{
    /// <summary>
    /// Indicates to the UI that this property should be shown
    /// on the <see cref="PackageBase"/>'s details page in the
    /// "Additional Information" section.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayAdditionalInformationAttribute : DisplayAttribute
    {
        /// <summary>
        /// A font icon from Segoe MDL2 Assets (or Segoe Fluent Assets).
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAdditionalInformationAttribute"/>
        /// class with the specified title.
        /// </summary>
        /// <param name="title">
        /// The title to display to the user.
        /// Defaults to the name of the target property.
        /// </param>
        /// <param name="icon">
        /// The icon to display next to the title.
        /// Defaults to <see cref="string.Empty"/>.
        /// </param>
        public DisplayAdditionalInformationAttribute([CallerMemberName] string title = null, string icon = null) : base(title)
        {
            Icon = icon ?? string.Empty;
        }
    }

    public class DisplayAdditionalInformationInfo : DisplayInfo
    {
        public DisplayAdditionalInformationInfo(string title, int rank, string icon, object value) : base(title, rank, value)
        {
            Icon = icon;
        }

        public DisplayAdditionalInformationInfo(DisplayAdditionalInformationAttribute attr, object value) : this(attr.Title, attr.Rank, attr.Icon, value)
        {

        }

        public string Icon { get; set; }
    }
}
