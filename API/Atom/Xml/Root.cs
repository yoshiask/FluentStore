using System.Collections.Generic;
using System.Xml.Serialization;

namespace Atom.Xml
{
    [XmlRoot("root", Namespace = Constants.ATOM_NAMESPACE)]
    public abstract class Root
    {
        [XmlAttribute(AttributeName = "xmlns")]
        public string Namespace { get; set; }

        [XmlElement(ElementName = "link")]
        public List<Link> Links { get; set; } = new List<Link>();

        [XmlElement(ElementName = "updated")]
        public string Updated { get; set; }

        [XmlElement(ElementName = "title")]
        public Content Title { get; set; }

        [XmlElement(ElementName = "content")]
        public Content Content { get; set; }

        [XmlElement(ElementName = "id")]
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }

        [XmlElement(ElementName = "author")]
        public Author Author { get; set; }

        [XmlElement(ElementName = "rights")]
        public string Rights { get; set; }
    }
}
