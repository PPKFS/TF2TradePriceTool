using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricerThing
{
    class Schema
    {
        public Dictionary<int, ItemTemplate> ItemSchema { get; set; }

        public Dictionary<int, ItemAttributeTemplate> AttributeSchema { get; set; }

        public Dictionary<int, string> UnusualNames = new Dictionary<int, string>();

        public Dictionary<int, String> StrangePartNames = new Dictionary<int, String>();
    }
}
