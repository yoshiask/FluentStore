using System.Xml.Serialization;

namespace Atom.Xml
{
    public class Content
    {
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}