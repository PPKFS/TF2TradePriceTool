using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SteamKit2;

namespace TF2TradePriceTool
{
    //backpack split into a number of sections
    //each section can then manage its own printing of hats
    //also allows for titles and the like in a neater way
    class Backpack
    {
        public List<Section> Sections { get; set; }

        public Backpack()
        {
            Sections = new List<Section>();
            Sections.Add(new UnusualSection());
        }

        public void GetContents(string steamID)
        {
            KeyValue items;
            using (WebAPI.Interface steamInterface = WebAPI.GetInterface("IEconItems_440", TF2PricerMain.GetSteamAPIKey()))
            {
                Dictionary<string, string> itemArgs = new Dictionary<string, string>();
                itemArgs["SteamID"] = "76561198034183306";
                if (!File.Exists("raw_items.txt"))
                    items = steamInterface.Call("GetPlayerItems", /* version */ 1, itemArgs);
                else
                {
                    using (StreamReader reader = new StreamReader("raw_items.txt"))
                    {
                        String fromFile = reader.ReadToEnd();
                        items = JsonConvert.DeserializeObject<KeyValue>(fromFile);
                    }
                }
                //this is just for reference..
                /*string serialized = JsonConvert.SerializeObject(items, Formatting.Indented);
                using (StreamWriter writer = new StreamWriter("raw_items.txt"))
                {
                    writer.Write(serialized);
                }*/
            }

            foreach (KeyValue itemJSON in items["items"].Children)
            {
                //read in the items. if it's a property every item has, like a level, then it's a property.
                //if it's a property only some items have, like strange parts or paint, it's an attribute.
                Item newItem = new Item();
                newItem.DefIndex = Convert.ToInt32(itemJSON["defindex"].Value);
                newItem.Template = TF2PricerMain.Schema.ItemSchema[newItem.DefIndex];
                newItem.Level = Convert.ToInt32(itemJSON["level"].Value);
                newItem.Quality = (Quality)Enum.Parse(typeof(Quality), itemJSON["quality"].Value);
                //if the flag exists and it's not 0 (that is, it's not tradable)
                if (itemJSON["flag_cannot_trade"].Value != null && itemJSON["flag_cannot_trade"].Value != "0")
                    newItem.IsTradable = false;
                if (itemJSON["flag_cannot_craft"].Value != null && itemJSON["flag_cannot_craft"].Value != "0")
                    newItem.IsCraftable = false;
                if (newItem.Name.Contains("Dueling Mini") || newItem.Name.Contains("Noise"))
                    newItem[Item.Quantity] = itemJSON["quantity"].Value;

                //test line TODO remove
                if (newItem.Quality != Quality.Unusual || newItem.Type != ItemType.Hat)
                    continue;

                //now we can check for various attributes
                //since some want float value and some want value, it's easiest done as a switch
                foreach (var attr in itemJSON["attributes"].Children)
                {
                    int defindex = Convert.ToInt32(attr["defindex"].Value);
                    switch (defindex)
                    {
                        case 500: //custom name
                            newItem[Item.CustomName] = attr["value"].Value;
                            break;
                        case 142: //painted
                            newItem[Item.Paint] = attr["float_value"].Value;
                            break;
                        case 229: //lowcraft
                            newItem[Item.CraftNumber] = attr["value"].Value;
                            break;
                        case 185: //gifted flag
                            newItem.IsGifted = true;
                            break;
                        case 380: //1st part
                            newItem[Item.StrangePart1] = attr["float_value"].Value;
                            break;
                        case 382: //2nd part
                            newItem[Item.StrangePart2] = attr["float_value"].Value;
                            break;
                        case 384: //3rd part
                           newItem[Item.StrangePart3] = attr["float_value"].Value;
                            break;
                        case 134: //unusual
                            newItem[Item.Effect] = attr["float_value"].Value;
                            break;
                        case 153: //untradable
                        case 501: //description
                            break;
                        default:
                            break;
                    }
                }

                foreach (Section section in Sections)
                {
                    Quality q = (Quality)500;
                    //try to add the item to that section of the backpack
                    if (section.TryAdd(newItem))
                        break;
                }
            }
            //now we've read in all the items, we sort the sections and then we start printing them
            //the printing is twofold - we print 
        }

        public void PrintOutItems()
        {
            using (StreamWriter writer = new StreamWriter(TF2PricerMain.OutputLocation))
            {
                foreach (Section s in Sections)
                {
                    s.Print(writer);
                }
            }
        }

    }
}
