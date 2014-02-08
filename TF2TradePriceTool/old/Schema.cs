using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricerThing
{
    class ItemAttributeTemplate
    {
        //unique ID
        public int DefIndex { get; set; }

        public string Name { get; set; }
    }

    class Schema
    {
        public Dictionary<int, ItemTemplate> ItemSchema { get; set; }

        public Dictionary<int, ItemAttributeTemplate> AttributeSchema { get; set; }

        public Dictionary<int, string> UnusualNames = new Dictionary<int, string>();

        public Dictionary<int, String> StrangePartNames = new Dictionary<int, String>();

        //since there's a lot of items without pricings, and a lot of stuff doesn't match to the schema directly (e.g. uncraftable strange hats)
        //I'm just going to store pricing data separately and make this whole thing a giant mess
    }
}
