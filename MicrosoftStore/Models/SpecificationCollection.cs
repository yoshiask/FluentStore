using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftStore.Models
{
    public class SpecificationCollection
    {
        public string Title { get; set; }
        public List<SpecificationItem> Items { get; set; }
    }

    public class SpecificationItem
    {
        public string Level { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValidationHint { get; set; }
        public bool IsValidationPassed { get; set; }
    }
}
