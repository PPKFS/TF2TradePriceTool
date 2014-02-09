using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TF2TradePriceTool
{
    class Price
    {
        //can't be bothered to get this sorted neatly
        public static double KeyPrice = 7.00;
        public static double BudsPrice = 19.50;
        public static double RefPrice = 0.295; //it makes it work nicely, okay?

        public static Price Unpriced = new Price() { LowRefinedPrice = 0, HighRefinedPrice = 0 };

        public double LowRefinedPrice { get; set; } //the price in refined

        public double HighRefinedPrice { get; set; }

        public override string ToString()
        {
            if (LowPrice == HighPrice)
                return LowPrice;
            else
                return LowPrice + " - " + HighPrice;
        }

        public String LowPrice
        {
            get
            {
                //if it's just refined, return it in refined
                //if it's a key or more, return it in keys+refined
                //if it's buds or more, return it in buds + keys
                if (LowRefinedPrice == 0)
                    return "No price";
                if (LowRefinedPrice < KeyPrice)
                    return Math.Round(LowRefinedPrice, 2) + " ref";
                else if (LowRefinedPrice == KeyPrice)
                    return "1 key";
                else if (LowRefinedPrice < 2 * KeyPrice)
                {
                    return "1 key + " + Math.Round((LowRefinedPrice % KeyPrice), 2) + " ref";
                }
                else if (LowRefinedPrice < BudsPrice * KeyPrice)
                {
                    return Math.Truncate(LowRefinedPrice / KeyPrice) + " keys + " + Math.Round((LowRefinedPrice % KeyPrice), 2) + " ref";
                }
                else
                    return Math.Round(LowRefinedPrice / (BudsPrice * KeyPrice), 1) + " buds";
            }
        }

        public String HighPrice
        {
            get
            {
                //if it's just refined, return it in refined
                //if it's a key or more, return it in keys+refined
                //if it's buds or more, return it in buds + keys
                if (LowRefinedPrice == 0)
                    return "No price";
                if (HighRefinedPrice < KeyPrice)
                    return Math.Round(HighRefinedPrice, 2) + " ref";
                else if (HighRefinedPrice == KeyPrice)
                    return "1 key";
                else if (HighRefinedPrice < 2 * KeyPrice)
                {
                    return "1 key + " + Math.Round((HighRefinedPrice % KeyPrice), 2) + " ref";
                }
                else if (HighRefinedPrice < BudsPrice * KeyPrice)
                {
                    return Math.Truncate(HighRefinedPrice / KeyPrice) + " keys + " + Math.Round((HighRefinedPrice % KeyPrice), 2) + " ref";
                }
                else
                    return Math.Round(HighRefinedPrice / (BudsPrice * KeyPrice), 1) + " buds";

            }
        }
        public static Price operator +(Price a, Price b)
        {
            return new Price()
            {
                LowRefinedPrice = a.LowRefinedPrice + b.LowRefinedPrice,
                HighRefinedPrice = a.HighRefinedPrice + b.HighRefinedPrice
            };
        }
    }

    class ItemPricingTemplate
    {
        public int DefIndex { get; set; }

        public Quality Quality { get; set; }

        //we can ignore this for most things, but it's useful to keep in anyway
        public int Effect { get; set; }

        public override string ToString()
        {
            return String.Join("|", DefIndex, Quality, Effect);
        }
    }

    class PriceSchema
    {
        //so this seems to be the easiest way, given than trying to understand the damn typeconverter
        public Dictionary<String, Price> PriceList = new Dictionary<String, Price>();

        public void LoadPriceList()
        {
            String readJSON;
            using (StreamReader reader = new StreamReader(TF2PricerMain.PriceLocation))
                readJSON = reader.ReadToEnd();

            dynamic jsonDynamic = JsonConvert.DeserializeObject<PriceSchema>(readJSON);
            this.PriceList = jsonDynamic.PriceList;
        }

        public Price GetPrice(int defindex, Quality quality, int effect = 0)
        {
            Price price;
            PriceList.TryGetValue(String.Join("|", defindex, quality, effect), out price);
            return (price == null) ? Price.Unpriced : price;
        }

        public Price GetPrice(Item i)
        {
            return GetPrice(i.DefIndex, i.Quality, Convert.ToInt32(i[Item.Effect]));
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
                foreach (JProperty qualityEntry in itemEntry.Value)
                {
                    if (qualityEntry.Name == "alt_defindex") //no idea what the hell this does
                        continue;
                    Quality quality = (Quality)Enum.Parse(typeof(Quality), qualityEntry.Name);

                    //now we view each effect. 
                    //normal stuff appears as 0, some weird stuff like 4 like the sparkle lugers, unusuals have their own ones
                    foreach (JProperty effectItem in qualityEntry.Value)
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
                                p.HighRefinedPrice = high * Price.KeyPrice * Price.BudsPrice;
                                break;
                            case "usd": //seems to be both unusuals and refined
                                p.LowRefinedPrice = low / Price.RefPrice;
                                p.HighRefinedPrice = high / Price.RefPrice;
                                break;
                            default:
                                System.Diagnostics.Debugger.Break();
                                break;
                        }
                        PriceList.Add(priceTemplate.ToString(), p);
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

            using (StreamWriter writer = new StreamWriter(TF2PricerMain.PriceLocation))
                writer.Write(json);
        }

        public Price GetPaintPrice(string p)
        {
            Price price;
            PriceList.TryGetValue(String.Join("|", TF2PricerMain.Schema.PaintIDs[Convert.ToInt32(p)], Quality.Unique, 0), out price);
            if (price == null)
                return Price.Unpriced;
            else
                return new Price() { LowRefinedPrice = price.LowRefinedPrice / 2, HighRefinedPrice = price.HighRefinedPrice / 2 };
        }
    }
}
