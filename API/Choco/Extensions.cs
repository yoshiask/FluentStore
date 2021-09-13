using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Chocolatey
{
    public static class Extensions
    {
        public static string ToLowerString(this bool val)
        {
            return val.ToString().ToLowerInvariant();
        }

        public static XmlNamespaceManager GetPopulatedNamespaceMgr(this XmlDocument xd)
        {
            XmlNamespaceManager nmsp = new XmlNamespaceManager(xd.NameTable);
            XPathNavigator nav = xd.DocumentElement.CreateNavigator();
            foreach (var kvp in nav.GetNamespacesInScope(XmlNamespaceScope.All))
            {
                string sKey = kvp.Key;
                if (sKey == string.Empty)
                    sKey = "default";
                nmsp.AddNamespace(sKey, kvp.Value);
            }

            return nmsp;
        }

        public static T Deserialize<T>(this XmlReader xml)
        {
            if (xml == null)
                return default(T);

            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xml);
        }

        public static T Deserialize<T>(this XContainer xml)
        {
            if (xml == null)
                return default(T);
            return xml.CreateReader().Deserialize<T>();
        }
    }
}
