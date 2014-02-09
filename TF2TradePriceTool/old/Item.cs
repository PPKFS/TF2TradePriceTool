using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricerThing
{
    public enum Quality
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
        Haunted = 13
    }


    public static class Paint
    {

        //maps colour codes to colour names
        public static Dictionary<int, string> PaintNames = 
            new Dictionary<int,string>(){
            {7511618, "Indub. Green"},
            {4345659, "Greed"},
            {5322826, "Violet"},
            {14204632, "216-190-216"},
            {8208497, "Purple"},
            {13595446, "Orange"},
            {10843461, "Muskelmannbraun"},
            {12955537, "Drab"},
            {6901050, "Brown"},
            {8154199, "Rustic"},
            {15185211, "Gold"},
            {8289918, "Grey"},
            {15132390, "White"},
            {1315860, "Black"},
            {16738740, "Pink"},
            {3100495, "Slate"},
            {8421376, "Olive"},
            {3329330, "Lime"},
            {15787660, "Business Pants"},
            {15308410, "Salmon"},
            {12073019, "Team Spirit"},
            {4732984, "Operator's Overalls"},
            {11049612, "Lab Coat"},
            {3874595, "Balaclavas"},
            {6637376, "Air of Debonair"},
            {8400928, "Value of Teamwork"},
            {12807213, "Cream Spirit"},
            {2960676, "After Eight"},
            {1, "Team Spirit"},
            {12377523, "Mint"},
            {0, "AAAANone"}
            };

        public static Dictionary<int, int> PaintDefIDs = 
            new Dictionary<int,int>()
            {
                {7511618, 5027},
                {4345659, 5028},
                {5322826, 5029},
                {14204632, 5030},
                {8208497, 5031},
                {13595446, 5032},
                {10843461, 5033},
                {12955537, 5034},
                {6901050, 5035},
                {8154199, 5036},
                {15185211, 5037},
                {8289918, 5038},
                {15132390, 5039},
                {1315860, 5040},
                {16738740, 5051},
                {3100495, 5052},
                {8421376, 5053},
                {3329330, 5054},
                {15787660, 5055},
                {15308410, 5056},
                {12073019, 5046},
                {4732984, 5060},
                {11049612, 5061},
                {3874595, 5062},
                {6637376, 5063},
                {8400928, 5064},
                {12807213, 5065},
                {2960676, 5077},
                {12377523, 5076},
                {1, 1},
                {0, 0}
            };
    }

    public static class StrangePart
    {
    }

    public class Item : IEquatable<Item>
    {
        public ItemTemplate Template { private get; set; }

        public string Name
        {
            get
            {
                return Template.Name+ ((this.CraftNumber <= 100 && this.CraftNumber != 0) ?  " #"+this.CraftNumber : "");
            }
        }

        public int DefIndex
        {
            get
            {
                return Template.DefIndex;
            }
        }

        public ItemType Type
        {
            get
            {
                return Template.Type;
            }
        }

        //bunch of quantities, some relevant, others less so.
        public int Level { get; set; }
        public int Quantity { get; set; }
        public bool IsTradable { get; set; }
        public bool IsCraftable { get; set; }
        public Quality Quality { get; set; }
        public string CustomName { get; set; }
        public int PaintCol { get; set; }
        public string PaintName
        {
            get
            {
                return Paint.PaintNames[PaintCol];
            }
        }
        public int PaintDefID
        {
            get
            {
                return Paint.PaintDefIDs[PaintCol];
            }
        }
        public int CraftNumber { get; set; }
        public bool IsGifted { get; set; }

        public Item ContainedItem { get; set; }

        public List<int> StrangeParts { get; set; }

        public String Effect { get; set; }

        public Item()
        {
            IsTradable = true;
            IsCraftable = true;
            IsGifted = false;
            CustomName = null;
            ContainedItem = null;
            StrangeParts = new List<int>();
        }

        public override bool Equals(object obj)
        {
            return this.Equals((Item)obj);
        }

        public bool Equals(Item obj)
        {
            Item a = this;
            Item other = (Item)obj;
            return true;
            
        }

        public override int GetHashCode()
        {
            return this.DefIndex;
        }
        /*public int CompareTo(Item other)
        {
            //if the names are equal, sort on effect first (for unusuals) and then paint
            if (other.Name.Equals(this.Name))
            {
                if(
                //if the effects are equal, or null
                if ((this.Effect == null || other.Effect == null) || this.Effect.Equals(other.Effect))
                    return this.PaintDefID.CompareTo(other.PaintDefID);
                else
                    return this.Effect.CompareTo(other.Effect);
            }
            else
                return this.Name.CompareTo(other.Name);
        }*/
    }
}
