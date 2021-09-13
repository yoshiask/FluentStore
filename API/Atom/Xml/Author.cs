using System.Xml.Serialization;

namespace Atom.Xml
{
    [XmlRoot(ElementName = "author", Namespace = Constants.ATOM_NAMESPACE)]
    public class Author
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "uri")]
        public string Url { get; set; }
    }
}