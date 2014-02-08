using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamKit2;

namespace TF2TradePriceTool
{
    //just a mapping of defindices to names and their types.
    class ItemTemplate
    {
        public String Name { get; set; }

        public ItemType Type { get; set; }
    }

    struct ItemPricingTemplate
    {
        public int DefIndex { get; set; }

        public Quality Quality { get; set; }

        //we can ignore this for most things, but it's useful to keep in anyway
        public int Effect { get; set; }
    }

    class Schema
    {
        //schema of items, mapped by index
        public Dictionary<int, ItemTemplate> ItemSchema = new Dictionary<int, ItemTemplate>();

        //mapping of particle effect ID to unusual effect names
        public Dictionary<int, String> UnusualNames = new Dictionary<int, String>();

        //mapping of strange part ID (kill eater) to their names
        public Dictionary<int, String> StrangePartNames = new Dictionary<int, String>();

        //mapping of paint colours (some form of hex code) to their defindices
        public Dictionary<int, int> PaintIDs = new Dictionary<int,int>();

        //mapping of paint defindices to (shorthand) names; e.g. Black not ...Hue.
        public Dictionary<int, string> PaintNames = new Dictionary<int, string>();

        //mapping of defindices to their default levels (for vintage weapons)
        public Dictionary<int, int> DefaultVintageLevels = new Dictionary<int, int>();

        public Dictionary<ItemPricingTemplate, Price> PriceList = new Dictionary<ItemPricingTemplate, Price>();

        public void LoadSchema()
        {
            String loaded;
            using (StreamReader reader = new StreamReader("schema.txt"))
            {
                loaded = reader.ReadToEnd();
            }

            dynamic anonObject = JsonConvert.DeserializeObject(loaded);
            this.ItemSchema = anonObject.ItemSchema;
            this.UnusualNames = anonObject.UnusualNames;
            this.StrangePartNames = anonObject.StrangePartNames;
            this.PaintIDs = anonObject.PaintIDs;
            this.PaintNames = anonObject.PaintNames;
            this.DefaultVintageLevels = anonObject.DefaultVintageLevels;
        }

        public void BuildSchema()
        {
            String steamAPI = TF2PricerMain.GetSteamAPIKey();
            using (WebAPI.Interface steamInterface = WebAPI.GetInterface("IEconItems_440", steamAPI))
            {
                Dictionary<String, String> interfaceParams = new Dictionary<string, string>() { { "language", "en" } };
                KeyValue schemaCall = steamInterface.Call("GetSchema", /* version */ 1, interfaceParams);

                KeyValue schemaItemResponse = schemaCall["items"];

                //now we build the item schema
                foreach (KeyValue item in schemaItemResponse.Children)
                {
                    int defIndex = Convert.ToInt32(item["defindex"].Value);
                    ItemTemplate template = new ItemTemplate();
                    template.Name = item.Children[4].Value;

                    //deal with types
                    String type = item["item_slot"].Value;
                    switch (type)
                    {
                        //weapons!
                        case "primary": 
                        case "secondary":
                        case "melee":
                        case "pda":
                        case "pda2":
                        case "building":
                            template.Type = ItemType.Weapon;
                            //if it's got the same values, then add it to the vintage chart (we can safely ignore basically everything else)
                            if(item["min_ilevel"].Value == item["max_ilevel"].Value)
                                DefaultVintageLevels.Add(defIndex, Convert.ToInt32(item["min_ilevel"].Value));
                            break;
                        case "head":
                        case "misc":
                            template.Type = ItemType.Hat;
                            break;
                        default:
                            template.Type = ItemType.Tool;
                                //System.Diagnostics.Debugger.Break();
                            break;
                    }

                    //now we parse for strange parts
                    if (template.Name.Contains("Strange Part:"))
                        StrangePartNames.Add(Convert.ToInt32(item["attributes"].Children[0]["value"].Value), template.Name.Substring(template.Name.IndexOf(':')+2));

                    ItemSchema.Add(defIndex, template);
                }

                //unusual names

                KeyValue unusualNames = schemaCall["attribute_controlled_attached_particles"];
                foreach (var effect in unusualNames.Children)
                {
                    UnusualNames.Add(Convert.ToInt32(effect["id"].Value), effect["name"].Value);
                }
            }
            PaintIDs = new Dictionary<int, int>()
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
                {1, 5046},//'bugged' team spirit
                {0, 0}
            };

            PaintNames = new Dictionary<int, string>(){
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
            {12377523, "Mint"}//,
            //{0, "AAAANone"}
            };

            BuildPriceList();

            //now we can save it to a file
            string json = JsonConvert.SerializeObject(this);
                /*new
                {
                    ItemSchema = Schema.ItemSchema,
                    UnusualNames = Schema.UnusualNames,
                    PaintIDs = Schema.PaintIDs,
                    PaintNames = Schema.PaintNames,
                    StrangePartNames = Schema.StrangePartNames,
                    DefaultVintageLevels = Schema.DefaultVintageLevels
                });*/

            using (StreamWriter writer = new StreamWriter("schema.txt"))
                writer.Write(json);
        }

        public void BuildPriceList()
        {
            /*WebRequest request = WebRequest.Create("http://backpack.tf/api/IGetPrices/v3/?format=json&key="+TF2PricerMain.GetBackpackTFKey());
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();*/
            string returnedJSON = String.Empty;
            String data = "bptf.txt";
            using (StreamReader sr = new StreamReader(data))
            {
                returnedJSON = sr.ReadToEnd();
            }

            JObject bptfRaw = JObject.Parse(returnedJSON);
            JObject itemPriceListRaw = bptfRaw["response"]["prices"].Value<JObject>();
            foreach (KeyValuePair<String, JToken> itemEntry in itemPriceListRaw)
            {
                int defIndex = Convert.ToInt32(itemEntry.Key);
                foreach(JProperty qualityEntry in itemEntry.Value)
                {
                    if (qualityEntry.Name == "alt_defindex") //no idea what the hell this does
                        continue;
                    Quality quality = (Quality)Enum.Parse(typeof(Quality), qualityEntry.Name);

                    //now we view each effect. 
                    //normal stuff appears as 0, some weird stuff like 4 like the sparkle lugers, unusuals have their own ones
                    foreach(JProperty effectItem in qualityEntry.Value)
                    {
                        ItemPricingTemplate priceTemplate = new ItemPricingTemplate();
                        priceTemplate.DefIndex = defIndex;
                        priceTemplate.Quality = quality;
                        priceTemplate.Effect = Convert.ToInt32(effectItem.Name);
                        Price p = new Price();
                        double low = effectItem.Value["current"]["value"].Value<double>();
                        double high = low;
                        if (effectItem.Value["current"]["value_high"] != null)
                            high = effectItem.Value["current"]["value_high"].Value<double>();

                        //now convert into ref
                        String currType = effectItem.Value["current"]["currency"].Value<String>();
                        switch (currType)
                        {
                            case "keys":
                                p.LowRefinedPrice = low * Price.KeyPrice;
                                p.HighRefinedPrice = high * Price.KeyPrice;
                                break;
                            case "metal":
                                p.LowRefinedPrice = low;
                                p.HighRefinedPrice = high;
                                break;
                            case "earbuds":
                                p.LowRefinedPrice = low * Price.KeyPrice * Price.BudsPrice;
                                p.HighRefinedPrice = low * Price.KeyPrice * Price.BudsPrice;
                                break;
                            case "usd": //ignore refined
                                p.LowRefinedPrice = 1;
                                p.HighRefinedPrice = 1;
                                break;
                            default:
                                System.Diagnostics.Debugger.Break();
                                break;
                        }
                        PriceList.Add(priceTemplate, p);
                        //if (!(new List<int>(){ 0, 1, 3, 5, 6, 7, 8, 9, 11, 13, 14, 300, 600, 1100 }.Contains((int)priceTemplate.Quality)))
                        //    System.Diagnostics.Debugger.Break();
                        //int lowprice = qualityEntry.Value;
                    }
                }
                //System.Diagnostics.Debugger.Break();
            }
            //try struct
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            /*new
            {
                ItemSchema = Schema.ItemSchema,
                UnusualNames = Schema.UnusualNames,
                PaintIDs = Schema.PaintIDs,
                PaintNames = Schema.PaintNames,
                StrangePartNames = Schema.StrangePartNames,
                DefaultVintageLevels = Schema.DefaultVintageLevels
            });*/

            using (StreamWriter writer = new StreamWriter("schema.txt"))
                writer.Write(json);
        }
    }
}
