using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace FluentStore.Converters
{
    public class CSSColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            return new SolidColorBrush(ParseCSSColorAsWinUIColor((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static System.Drawing.Color ParseCSSColorAsDrawingColor(string cssString)
        {
            if (string.IsNullOrWhiteSpace(cssString))
            {
                return System.Drawing.Color.Transparent;
            }
            else if (cssString.StartsWith("rgb("))
            {
                cssString = cssString.Remove(0, 4);
                cssString = cssString.Remove(cssString.Length - 1, 1);
                string[] components = cssString.Split(',');
                return System.Drawing.Color.FromArgb(
                    byte.Parse(components[0].Trim()),
                    byte.Parse(components[1].Trim()),
                    byte.Parse(components[2].Trim())
                );
            }
            else if (cssString[0] == '#')
            {
                switch (cssString.Length)
                {
                    case 9:
                        {
                            var cuint = System.Convert.ToUInt32(cssString.Substring(1), 16);
                            var a = (byte)(cuint >> 24);
                            var r = (byte)((cuint >> 16) & 0xff);
                            var g = (byte)((cuint >> 8) & 0xff);
                            var b = (byte)(cuint & 0xff);

                            return System.Drawing.Color.FromArgb(a, r, g, b);
                        }

                    case 7:
                        {
                            var cuint = System.Convert.ToUInt32(cssString.Substring(1), 16);
                            var r = (byte)((cuint >> 16) & 0xff);
                            var g = (byte)((cuint >> 8) & 0xff);
                            var b = (byte)(cuint & 0xff);

                            return System.Drawing.Color.FromArgb(255, r, g, b);
                        }

                    case 5:
                        {
                            var cuint = System.Convert.ToUInt16(cssString.Substring(1), 16);
                            var a = (byte)(cuint >> 12);
                            var r = (byte)((cuint >> 8) & 0xf);
                            var g = (byte)((cuint >> 4) & 0xf);
                            var b = (byte)(cuint & 0xf);
                            a = (byte)(a << 4 | a);
                            r = (byte)(r << 4 | r);
                            g = (byte)(g << 4 | g);
                            b = (byte)(b << 4 | b);

                            return System.Drawing.Color.FromArgb(a, r, g, b);
                        }

                    case 4:
                        {
                            var cuint = System.Convert.ToUInt16(cssString.Substring(1), 16);
                            var r = (byte)((cuint >> 8) & 0xf);
                            var g = (byte)((cuint >> 4) & 0xf);
                            var b = (byte)(cuint & 0xf);
                            r = (byte)(r << 4 | r);
                            g = (byte)(g << 4 | g);
                            b = (byte)(b << 4 | b);

                            return System.Drawing.Color.FromArgb(255, r, g, b);
                        }

                    default: throw new FormatException("The string passed in the cssString argument is not a recognized Color format.");
                }
            }
            else if (NamedCSSColors.ContainsKey(cssString))
            {
                return ParseCSSColorAsDrawingColor(NamedCSSColors[cssString]);
            }
            else
            {
                return System.Drawing.Color.Transparent;
            }
        }

        public static Color ParseCSSColorAsWinUIColor(string cssString)
        {
            var drawingColor = ParseCSSColorAsDrawingColor(cssString);
            return Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public static Dictionary<string, string> NamedCSSColors = new Dictionary<string, string>()
        {
            { "AliceBlue", "#F0F8FF" },
            { "AntiqueWhite", "#FAEBD7" },
            { "Aqua", "#00FFFF" },
            { "Aquamarine", "#7FFFD4" },
            { "Azure", "#F0FFFF" },
            { "Beige", "#F5F5DC" },
            { "Bisque", "#FFE4C4" },
            { "Black", "#000000" },
            { "BlanchedAlmond", "#FFEBCD" },
            { "Blue", "#0000FF" },
            { "BlueViolet", "#8A2BE2" },
            { "Brown", "#A52A2A" },
            { "BurlyWood", "#DEB887" },
            { "CadetBlue", "#5F9EA0" },
            { "Chartreuse", "#7FFF00" },
            { "Chocolate", "#D2691E" },
            { "Coral", "#FF7F50" },
            { "CornflowerBlue", "#6495ED" },
            { "Cornsilk", "#FFF8DC" },
            { "Crimson", "#DC143C" },
            { "Cyan", "#00FFFF" },
            { "DarkBlue", "#00008B" },
            { "DarkCyan", "#008B8B" },
            { "DarkGoldenrod", "#B8860B" },
            { "DarkGray", "#A9A9A9" },
            { "DarkGreen", "#006400" },
            { "DarkGrey", "#A9A9A9" },
            { "DarkKhaki", "#BDB76B" },
            { "DarkMagenta", "#8B008B" },
            { "DarkOliveGreen", "#556B2F" },
            { "DarkOrange", "#FF8C00" },
            { "DarkOrchid", "#9932CC" },
            { "DarkRed", "#8B0000" },
            { "DarkSalmon", "#E9967A" },
            { "DarkSeaGreen", "#8FBC8F" },
            { "DarkSlateBlue", "#483D8B" },
            { "DarkSlateGray", "#2F4F4F" },
            { "DarkSlateGrey", "#2F4F4F" },
            { "DarkTurquoise", "#00CED1" },
            { "DarkViolet", "#9400D3" },
            { "DeepPink", "#FF1493" },
            { "DeepSkyBlue", "#00BFFF" },
            { "DimGray", "#696969" },
            { "DodgerBlue", "#1E90FF" },
            { "FireBrick", "#B22222" },
            { "FloralWhite", "#FFFAF0" },
            { "ForestGreen", "#228B22" },
            { "Fuchsia", "#FF00FF" },
            { "Gainsboro", "#DCDCDC" },
            { "GhostWhite", "#F8F8FF" },
            { "Gold", "#FFD700" },
            { "Goldenrod", "#DAA520" },
            { "Gray", "#808080" },
            { "Green", "#008000" },
            { "GreenYellow", "#ADFF2F" },
            { "Grey", "#808080" },
            { "Honeydew", "#F0FFF0" },
            { "HotPink", "#FF69B4" },
            { "IndianRed", "#CD5C5C" },
            { "Indigo", "#4B0082" },
            { "Ivory", "#FFFFF0" },
            { "Khaki", "#F0E68C" },
            { "Lavender", "#E6E6FA" },
            { "LavenderBlush", "#FFF0F5" },
            { "LawnGreen", "#7CFC00" },
            { "LemonChiffon", "#FFFACD" },
            { "LightBlue", "#ADD8E6" },
            { "LightCoral", "#F08080" },
            { "LightCyan", "#E0FFFF" },
            { "LightGoldenrodYellow", "#FAFAD2" },
            { "LightGray", "#D3D3D3" },
            { "LightGreen", "#90EE90" },
            { "LightGrey", "#D3D3D3" },
            { "LightPink", "#FFB6C1" },
            { "LightSalmon", "#FFA07A" },
            { "LightSeaGreen", "#20B2AA" },
            { "LightSkyBlue", "#87CEFA" },
            { "LightSlateGray", "#778899" },
            { "LightSlateGrey", "#778899" },
            { "LightSteelBlue", "#B0C4DE" },
            { "LightYellow", "#FFFFE0" },
            { "Lime", "#00FF00" },
            { "LimeGreen", "#32CD32" },
            { "Linen", "#FAF0E6" },
            { "Magenta", "#FF00FF" },
            { "Maroon", "#800000" },
            { "MediumAquamarine", "#66CDAA" },
            { "MediumBlue", "#0000CD" },
            { "MediumOrchid", "#BA55D3" },
            { "MediumPurple", "#9370DB" },
            { "MediumSeaGreen", "#3CB371" },
            { "MediumSlateBlue", "#7B68EE" },
            { "MediumSpringGreen", "#00FA9A" },
            { "MediumTurquoise", "#48D1CC" },
            { "MediumVioletRed", "#C71585" },
            { "MidnightBlue", "#191970" },
            { "MintCream", "#F5FFFA" },
            { "MistyRose", "#FFE4E1" },
            { "Moccasin", "#FFE4B5" },
            { "NavajoWhite", "#FFDEAD" },
            { "Navy", "#000080" },
            { "OldLace", "#FDF5E6" },
            { "Olive", "#808000" },
            { "OliveDrab", "#6B8E23" },
            { "Orange", "#FFA500" },
            { "OrangeRed", "#FF4500" },
            { "Orchid", "#DA70D6" },
            { "PaleGoldenrod", "#EEE8AA" },
            { "PaleGreen", "#98FB98" },
            { "PaleTurquoise", "#AFEEEE" },
            { "PaleVioletRed", "#DB7093" },
            { "PapayaWhip", "#FFEFD5" },
            { "PeachPuff", "#FFDAB9" },
            { "Peru", "#CD853F" },
            { "Pink", "#FFC0CB" },
            { "Plum", "#DDA0DD" },
            { "PowderBlue", "#B0E0E6" },
            { "Purple", "#800080" },
            { "Rebeccapurple", "#663399" },
            { "Red", "#FF0000" },
            { "RosyBrown", "#BC8F8F" },
            { "RoyalBlue", "#4169E1" },
            { "SaddleBrown", "#8B4513" },
            { "Salmon", "#FA8072" },
            { "SandyBrown", "#F4A460" },
            { "SeaGreen", "#2E8B57" },
            { "Seashell", "#FFF5EE" },
            { "Sienna", "#A0522D" },
            { "Silver", "#C0C0C0" },
            { "SkyBlue", "#87CEEB" },
            { "SlateBlue", "#6A5ACD" },
            { "SlateGray", "#708090" },
            { "SlateGrey", "#708090" },
            { "Snow", "#FFFAFA" },
            { "SpringGreen", "#00FF7F" },
            { "SteelBlue", "#4682B4" },
            { "Tan", "#D2B48C" },
            { "Teal", "#008080" },
            { "Thistle", "#D8BFD8" },
            { "Tomato", "#FF6347" },
            { "Turquoise", "#40E0D0" },
            { "Violet", "#EE82EE" },
            { "Wheat", "#F5DEB3" },
            { "White", "#FFFFFF" },
            { "WhiteSmoke", "#F5F5F5" },
            { "Yellow", "#FFFF00" },
            { "YellowGreen", "#9ACD32" },
            { "Transparent", "#00000000" },
            { "transparent", "#00000000" },
        };
    }
}
