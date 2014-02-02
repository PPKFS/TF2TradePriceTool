using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricerThing
{
    public enum ItemType
    {
        Promo,
        Hat,
        Weapon,
        Tool
    }

    public class ItemTemplate
    {
        //unique ID, I think
        public int DefIndex { get; set; }

        public string Name { get; set; }

        public int StrangePartID { get; set; }

        public ItemType Type { get; set; }

        public int DefaultLevel { get; set; }
    }
}
