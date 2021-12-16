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
        /// A glyph from <see cref="FontFamily"/>.
        /// </summary>
        public string Icon { get; set; }

        private string _FontFamily;
        /// <summary>
        /// The font family to use for <see cref="Icon"/>.
        /// </summary>
        public string FontFamily
        {
            get
            {
                if (_FontFamily == null)
                {
                    if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                        _FontFamily = "Segoe Fluent Icons";
                    else
                        _FontFamily = "Segoe MDL2 Assets";
                }
                return _FontFamily;
            }
            set => _FontFamily = value;
        }

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
        public DisplayAdditionalInformationInfo(string title, int rank, string icon, string fontFamily, object value)
            : base(title, rank, value)
        {
            Icon = icon;
            FontFamily = fontFamily;
        }

        public DisplayAdditionalInformationInfo(DisplayAdditionalInformationAttribute attr, object value)
            : this(attr.Title, attr.Rank, attr.Icon, attr.FontFamily, value)
        {

        }

        public string Icon { get; set; }

        public string FontFamily { get; set; }
    }
}
