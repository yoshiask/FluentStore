using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Atom.Xml
{
    [XmlRoot(ElementName = "entry", Namespace = Constants.ATOM_NAMESPACE)]
    public class Entry
    {
        [XmlElement(ElementName = "title")]
        public Content Title { get; set; }

        [XmlElement(ElementName = "updated")]
        public DateTime Updated { get; set; }

        [Key]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "link")]
        public List<Link> Links { get; set; } = new List<Link>();

        [XmlElement(ElementName = "summary")]
        public string Summary { get; set; }
    }
}