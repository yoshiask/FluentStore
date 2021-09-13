#if NETSTANDARD1_3_OR_GREATER

using System;
using System.Xml;
using static Atom.Constants;

namespace Atom
{
    public static class Factory
    {
        public static void SetElementValue(ref XmlElement element, string value)
        {
            element.InnerText = value;
        }

        public static void SetValueAsElement(XmlDocument doc, ref XmlElement element, string name, string value)
        {
            XmlElement valueElement;
            int sep = name.IndexOf(':');
            if (sep <= 0)
            {
                // Use namespace prefix
                string prefix = name.Substring(0, sep);
                string localName = name.Substring(sep + 1, name.Length - 1);
                valueElement = doc.CreateElement(prefix, localName, null);
            }
            else
            {
                valueElement = doc.CreateElement(name);
            }

            valueElement.InnerText = value;
            element.AppendChild(valueElement);
        }

        public static XmlElement CreateFeed(XmlDocument doc, string localNamespace = null, bool includeOpenSourceNS = false)
        {
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("a", ATOM_NAMESPACE);

            if (includeOpenSourceNS)
                nsManager.AddNamespace("os", OPENSEARCH_NAMESPACE);

            var feed = doc.CreateElement("a", "feed", null);
            if (localNamespace != null)
                feed.SetAttribute("xmlns", localNamespace);

            return feed;
        }

        public static XmlElement CreateLink(XmlDocument doc, string href, string rel = "self", string type = "application/atom+xml")
        {
            var link = doc.CreateElement("a", "link", null);
            link.SetAttribute("href", href);
            link.SetAttribute("rel", rel);
            link.SetAttribute("type", type);
            return link;
        }

        public static XmlElement CreateUpdated(XmlDocument doc, DateTimeOffset dateUpdated)
        {
            var updated = doc.CreateElement("a", "updated", null);
            updated.InnerText = dateUpdated.UtcDateTime.ToString("O");
            return updated;
        }

        public static XmlElement CreateTitle(XmlDocument doc, string title, string type = "text")
        {
            var titleElem = doc.CreateElement("a", "title", null);
            titleElem.InnerText = title;
            titleElem.SetAttribute("type", type);
            return titleElem;
        }

        public static XmlElement CreateId(XmlDocument doc, string id)
        {
            var idElem = doc.CreateElement("a", "id", null);
            idElem.InnerText = id;
            return idElem;
        }

        public static XmlElement CreateAuthor(XmlDocument doc, string name, string url = null)
        {
            var author = doc.CreateElement("a", "author", null);
            SetValueAsElement(doc, ref author, "name", name);
            if (url != null)
                SetValueAsElement(doc, ref author, "uri", url);
            return author;
        }

        public static XmlElement CreateEntry(XmlDocument doc, string title, string id, string href, DateTime updated)
        {
            var entry = doc.CreateElement("a", "entry", null);

            entry.AppendChild(CreateId(doc, id));
            entry.AppendChild(CreateLink(doc, href));
            entry.AppendChild(CreateTitle(doc, title));
            entry.AppendChild(CreateUpdated(doc, updated));

            return entry;
        }

        public static XmlElement CreateRootEntry(XmlDocument doc, string title, string id, string href, DateTime updated, string localNamespace = null, bool includeOpenSourceNS = false)
        {
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("a", ATOM_NAMESPACE);

            if (includeOpenSourceNS)
                nsManager.AddNamespace("os", OPENSEARCH_NAMESPACE);

            var entry = doc.CreateElement("a", "entry", null);
            if (localNamespace != null)
                entry.SetAttribute("xmlns", localNamespace);

            entry.AppendChild(CreateId(doc, id));
            entry.AppendChild(CreateLink(doc, href));
            entry.AppendChild(CreateTitle(doc, title));
            entry.AppendChild(CreateUpdated(doc, updated));

            return entry;
        }
    }
}

#endif
