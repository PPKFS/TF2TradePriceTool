using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TF2TradePriceTool
{
    enum ItemType
    {
        Hat,
        Weapon,
        Tool
    }

    enum Quality
    {
        Normal = 0,
        Genuine = 1,
        Vintage = 3,
        Unusual = 5,
        Unique = 6,
        Community = 7,
        Valve = 8,
        SelfMade = 9,
        Strange = 11,
        Haunted = 13,
        Collectors = 14,
        UncraftableVintage = 300, //apparently this exists
        Uncraftable = 600, //for backpack.tf
        UncraftableStrange = 1100
    }

    class Item
    {
        public static int Quantity = 1;
        public static int Effect = 2;
        public static int Paint = 3;
        public static int CustomName = 4;
        public static int CraftNumber = 5;
        public static int StrangePart1 = 6;
        public static int StrangePart2 = 7;
        public static int StrangePart3 = 8;

        [IndexerName("Attributes")]
        public String this[int key]
        {
            get
            {
                return attributes[key];
            }

            set
            {
                attributes[key] = value;
            }
        }

        public int DefIndex { get; set; }

        public ItemTemplate Template { get; set; }

        public int Level { get; set; }

        public bool IsGifted { get; set; }

        public bool IsTradable { get; set; }

        public bool IsCraftable { get; set; }

        public Quality Quality { get; set; }

        private Dictionary<int, String> attributes = new Dictionary<int, string>();

        public string Name
        {
            get
            {
                return Template.Name;
            }
        }

        public ItemType Type
        {
            get
            {
                return Template.Type;
            }
        }

        public String PaintName
        {
            get
            {
                String name;
                attributes.TryGetValue(Item.Paint, out name);
                return name;
            }
        }

        public Item()
        {
            IsTradable = true;
            IsGifted = false;
            IsCraftable = false;
        }
    }
}
