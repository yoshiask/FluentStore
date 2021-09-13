using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Atom.Xml
{
    [XmlRoot(ElementName = "link", Namespace = Constants.ATOM_NAMESPACE)]
    public class Link
    {
        [XmlAttribute(AttributeName = "rel")]
        public string Relation { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "updated")]
        public string Updated { get; set; }

        [XmlAttribute(AttributeName = "id")]
        [Key]
        public string Id { get; set; }
    }
}