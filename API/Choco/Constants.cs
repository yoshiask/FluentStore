using System.Xml.Linq;

namespace Chocolatey
{
    public static class Constants
    {
        private const string CHOCOLATEY_API_BASE = "community.chocolatey.org/api/v2";
        public const string CHOCOLATEY_API_HOST = "https://" + CHOCOLATEY_API_BASE;

        public static readonly XNamespace XMLNS_ATOM = "http://www.w3.org/2005/Atom";
        public static readonly XNamespace XMLNS_ADO_DATASERVICES_METADATA = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        public static readonly XNamespace XMLNS_ADO_DATASERVICES = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        public static readonly XNamespace XMLNS_CHOCOLATEY = "http://" + CHOCOLATEY_API_BASE;
    }
}
