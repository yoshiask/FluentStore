using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace WinGetRun.Enums
{
    public class StringEnumCamelCaseConverter : StringEnumConverter
    {
        public StringEnumCamelCaseConverter()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }
    }
}
