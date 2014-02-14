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

        public static List<String> OnlyUncraftList = new List<string>() { "Alien Swarm Parasite" };

        [IndexerName("Attributes")]
        public String this[int key]
        {
            get
            {
                String attrib;
                attributes.TryGetValue(key, out attrib);
                return attrib;
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
                if (name == null)
                    return null;
                else
                    return TF2PricerMain.Schema.PaintNames[Convert.ToInt32(name)];
            }
        }

        public String EffectName
        {
            get
            {
                String name;
                attributes.TryGetValue(Item.Effect, out name);
                if (name == null)
                    return null;
                else
                    return TF2PricerMain.Schema.UnusualNames[Convert.ToInt32(name)];
            }

        }

        public List<String> StrangeParts
        {
            get
            {
                List<String> returnedParts = new List<string>();
                String[] parts = new String[3];
                attributes.TryGetValue(Item.StrangePart1, out parts[0]);
                attributes.TryGetValue(Item.StrangePart2, out parts[1]);
                attributes.TryGetValue(Item.StrangePart3, out parts[2]);
                foreach (String s in parts)
                {
                    if (s != null)
                        returnedParts.Add(s);
                }
                return (returnedParts.Count == 0) ? null : returnedParts;
            }
        }

        public Item()
        {
            IsTradable = true;
            IsGifted = false;
            IsCraftable = true;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            Item other = obj as Item;
            if (other == null)
                return false;
            //different items, qualities, tradabilities, craftabilities
            if (this.DefIndex != other.DefIndex || this.Quality != other.Quality || this.IsTradable != other.IsTradable || this.IsCraftable != other.IsCraftable)
                return false;
            else if (this.IsGifted != other.IsGifted || (this.PaintName != other.PaintName && this.Quality != Quality.Unusual)) //gifted, paint (if not unusual)
                return false;
            else if (new int[] { 0, 1, 42, 69, 99, 100 }.Contains(this.Level) && this.Quality == Quality.Vintage && this.Level != other.Level) //diff levs if vintage
                return false;
            else if (this.Quality == Quality.Vintage && this.Type == ItemType.Weapon && this.Level != other.Level) //odd-levelled vintages
                return false;
            //now to check attributes
            foreach (KeyValuePair<int, String> kv in this.attributes)
            {
                if (kv.Key == Item.CustomName)
                    continue;
                //if the other one doesn't have this key
                if (other[kv.Key] == null)
                    return false; //if the other one has a different value
                else if (other[kv.Key] != kv.Value)
                    return false;
            }
            return true; //then it's the same
        }
    }
}
