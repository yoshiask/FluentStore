using System.Collections.Generic;
using System.Xml.Serialization;

namespace Atom.Xml
{
    [XmlRoot("feed", Namespace = Constants.ATOM_NAMESPACE)]
    public class Feed : Root
    {
        [XmlElement(ElementName = "entry")]
        public List<object> Entries { get; set; } = new List<object>();
    }
}
