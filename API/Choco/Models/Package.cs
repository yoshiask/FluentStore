using Atom.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Chocolatey.Models
{
    [XmlRoot(ElementName = "entry", Namespace = Atom.Constants.ATOM_NAMESPACE)]
    public class Package : Entry
    {

    }
}
