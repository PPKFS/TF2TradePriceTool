using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PricerThing
{
    class Program
    {

        static Dictionary<Item, int> countedBP = new Dictionary<Item, int>();
        static Schema itemSchema = null;
        static void OldMain(string[] args)
        {
            Schema schema = GetSchema();
            List<Item> backpack = new List<Item>();

            using (WebAPI.Interface steamInterface = WebAPI.GetInterface("IEconItems_440", "7B35211AD579A53C622866CB86B847E8"))
            {
                KeyValue items;
                //only download if needed
                if(!File.Exists("items.txt") /*|| true*/)
                {
                    Dictionary<string, string> itemArgs = new Dictionary<string, string>();
                    itemArgs["SteamID"] = "76561198034183306";

                    items = steamInterface.Call("GetPlayerItems", /* version */ 1, itemArgs);
                    //switching from dynamic means we can edit in debug
                    //items = steamInterface.GetPlayerItems(SteamID: 76561198034183306);
                    string serialized = JsonConvert.SerializeObject(items, Formatting.Indented);
                    using (StreamWriter writer = new StreamWriter("items.txt"))
                    {
                        writer.Write(serialized);
                    }
                }
                else
                {
                    using (StreamReader reader = new StreamReader("items.txt"))
                    {
                        items = JsonConvert.DeserializeObject<KeyValue>(reader.ReadToEnd());
                    }
                }

                foreach (KeyValue item in items.Children[2].Children)
                {
                    //read them all in
                    Item newItem = new Item();

                    backpack.Add(newItem);
                }
            }
            //it kinda hacks it together in that it'll order strange parts by their sum..which is probably semi-unqiue
            IEnumerable<Item> sortedbp = backpack.OrderBy(x => x.Name).ThenBy(x => x.PaintName).ThenBy(x => x.Level).ThenBy(x => x.StrangeParts.Sum());
            //so much hacky
            foreach (Item i in sortedbp)
            {
                if (countedBP.Keys.Contains(i))
                    countedBP[i] = countedBP[i] + 1;
                else
                    countedBP[i] = 1;
            }

            dynamic priceJSON;
            //now we can read in the values for the items
            using (StreamReader priceReader = new StreamReader("bptf.txt"))
            {
                priceJSON = JsonConvert.DeserializeObject(priceReader.ReadToEnd());
                priceJSON = priceJSON.response.prices;
            }

            foreach (var itemEntry in priceJSON)
            {
                itemEntry.AfterSelf();
            }

            //now the keys are items, and the values are the counts
            //countedBP = sortedbp.GroupBy(x => x, new ItemComparer()).ToDictionary(x => x.Key, x => x.Count());
            //now we can print them all out
            PrintOutStuff(sortedbp);
            Console.ReadLine();
        }
        #region Printing
        static void PrintOutStuff(IEnumerable<Item> items)
        {
            //print stuff in this order
            //Unusuals
            //Strange Hats ???
            //Genuine
            //Vintages
            //craft numbers
            //Unique Hats
            //Strange Weapons
            //Genuine Weapons
            //Vintage Weapons
            //Unique Weapons
            //tools
            //TODO: wrapped items

            var hats = items.Where(x => x.Type == ItemType.Hat);
            var weapons = items.Where(x => x.Type == ItemType.Weapon);
            var tools = items.Where(x => x.Type == ItemType.Tool).OrderBy(x => x.Name).ThenBy(x => x.Quantity);

            //unusuals
            //Console.WriteLine(items.Count().ToString() + " " + (hats.Count() + weapons.Count() + tools.Count()).ToString());
            var unusuals = hats.Where(x => x.Quality == Quality.Unusual).OrderBy(x => x.Name).ThenBy(x => x.Effect);
            var genHats = hats.Where(x => x.Quality == Quality.Genuine);
            var vinHats = hats.Where(x => x.Quality == Quality.Vintage);
            var craftNos = items.Where(x => x.CraftNumber <= 100 && x.CraftNumber != 0);
            var uniqueHats = hats.Where(x => x.Quality == Quality.Unique && !craftNos.Contains(x));

            //now for weapons
            var strangeWeps = weapons.Where(x => x.Quality == Quality.Strange);
            var genWeps = weapons.Where(x => x.Quality == Quality.Genuine);
            var uniqueWeps = weapons.Where(x => (x.Quality == Quality.Unique || x.Quality == Quality.Unusual) && !craftNos.Contains(x));
            var vinWeps = weapons.Where(x => x.Quality == Quality.Vintage);

            using(StreamWriter w = new StreamWriter(DateTime.Now.Day.ToString() +"-"+ DateTime.Now.Month.ToString()+".txt"))
            {
                if (unusuals.Count() != 0)
                    PrintUnusuals(unusuals);
                if (genHats.Count() != 0)
                    PrintGenuineHats(genHats);
                if (vinHats.Count() != 0)
                    PrintVintageHats(vinHats);
                if (craftNos.Count() != 0)
                    PrintCraftNumbers(craftNos);
                if (uniqueHats.Count() != 0)
                    PrintUniqueHats(uniqueHats);
                Console.WriteLine("---");

                if (strangeWeps.Count() != 0)
                    PrintStrangeWeps(strangeWeps);
                if (genWeps.Count() != 0)
                    PrintGenuineWeps(genWeps);
                if (vinWeps.Count() != 0)
                    PrintVintageWeps(vinWeps);
                if (uniqueWeps.Count() != 0)
                    PrintUniqueWeps(uniqueWeps);
                if (tools.Count() != 0)
                    PrintTools(tools);
            }
        }

        private static void PrintCraftNumbers(IEnumerable<Item> craftNos)
        {
            Console.WriteLine("\n\n **Craft # Stuff**  \n");
            String lastLine = "";
            foreach (Item u in craftNos)
            {
                String currLine;
                List<string> attribs = GetAttributesForItem(u);
                currLine = FormatItem(u, false, attribs.ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintTools(IEnumerable<Item> tools)
        {
            Console.WriteLine("\n\n **Tools and Stuff**  \n");
            String lastLine = "";
            foreach (Item u in tools)
            {
                List<string> attribs = GetAttributesForItem(u);
                if (u.Name.Contains("Noise Maker") && !u.Name.Contains("Holiday") || u.Name.Contains("Dueling")) 
                    attribs.Add(String.Format("{0} uses", u.Quantity));
                String currLine = FormatItem(u, false, attribs.ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintUniqueWeps(IEnumerable<Item> uniqueWeps)
        {
            Console.WriteLine("\n\n **Normal Weapons**  \n");
            String lastLine = "";
            foreach (Item u in uniqueWeps)
            {
                String currLine;
                if(u.Quality == Quality.Unusual) //if we've got a UHHH
                    currLine = FormatItem(u, true, GetAttributesForItem(u).ToArray());
                else
                    currLine = FormatItem(u, false, GetAttributesForItem(u).ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintVintageWeps(IEnumerable<Item> vinWeps)
        {
            Console.WriteLine("\n\n **Vintage Weapons**  \n");
            String lastLine = "";
            foreach (Item v in vinWeps)
            {

                List<String> attribs = GetAttributesForItem(v);
                if (v.Level != itemSchema.ItemSchema[v.DefIndex].DefaultLevel)
                    attribs.Add("Level " + v.Level.ToString());
                String currLine = FormatItem(v, true, attribs.ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintGenuineWeps(IEnumerable<Item> genWeps)
        {
            Console.WriteLine("\n\n **Genuine Weapons**  \n");
            String lastLine = "";
            foreach (Item g in genWeps)
            {
                String currLine = FormatItem(g, true, GetAttributesForItem(g).ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintStrangeWeps(IEnumerable<Item> strangeWeps)
        {
            Console.WriteLine("\n\n **Strange Weapons**  \n");
            String lastLine = "";
            
            foreach (Item u in strangeWeps)
            {
                List<String> attribs = GetAttributesForItem(u);
                //ewwww
                foreach (int i in u.StrangeParts)
                {
                    attribs.Add(itemSchema.StrangePartNames[i]);
                }
                String currLine = FormatItem(u, true, attribs.ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintUniqueHats(IEnumerable<Item> uniqueHats)
        {
            Console.WriteLine("\n\n **Normal Hats**  \n");
            String lastLine = "";
            foreach (Item u in uniqueHats)
            {
                String currLine = FormatItem(u, false, GetAttributesForItem(u).ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        private static void PrintVintageHats(IEnumerable<Item> vinHats)
        {
            Console.WriteLine("\n\n **Vintage Hats**  \n");
            String lastLine = "";
            foreach (Item v in vinHats)
            {

                List<String> attribs = GetAttributesForItem(v);
                if (new int[] { 0, 1, 42, 69, 99, 100 }.Contains(v.Level))
                    attribs.Add("Level " + v.Level.ToString());
                String currLine = FormatItem(v, true, attribs.ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        static void PrintUnusuals(IEnumerable<Item> unusuals)
        {
            Console.WriteLine("**Unusuals**  \n");
            String lastLine = "";
            foreach (Item u in unusuals)
            {
                List<String> attribs = GetAttributesForItem(u);
                attribs.Add(u.Effect);
                String currLine = FormatItem(u, true, attribs.ToArray());
                if (currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        static void PrintGenuineHats(IEnumerable<Item> genHats)
        {
            Console.WriteLine("\n\n **Genuine Hats**  \n");
            String lastLine = "";
            foreach (Item g in genHats)
            {
                String currLine = FormatItem(g, true, GetAttributesForItem(g).ToArray());
                if(currLine != lastLine)
                    Console.WriteLine(currLine);
                lastLine = currLine;
            }
        }

        //valve quality can be none
        static String FormatItem(Item item, bool printQuality, params String[] attributes)
        {
            Quality quality = item.Quality;
            String name = item.Name;
            String qual = (!printQuality) ? "" : quality.ToString();
            if(attributes.Count() == 0)
                return String.Join(" ", "*", qual, name, (countedBP[item] != 1) ? "x" + countedBP[item] : "");
            else
                return String.Join(" ", "*", qual, name, "("+String.Join(", ", attributes)+")", (countedBP[item] != 1) ? "x" + countedBP[item] : "");
            /*StringBuilder builder = new StringBuilder();
            builder.Append("* ");
            if (quality != Quality.Valve)
                builder.Append(quality.ToString());
            builder.Append(" "*/
        }

        static List<String> GetAttributesForItem(Item item)
        {
            List<String> attribs = new List<string>();
            if (item.PaintName != "AAAANone")
                attribs.Add(item.PaintName);
            if (item.IsGifted)
                attribs.Add("Gifted");
            if (!item.IsCraftable)
                attribs.Add("Uncraftable");
            if (!item.IsTradable)
                attribs.Add("Untradable");
            return attribs;
        }

        #endregion

        #region Reading In Data (Schema)
        static Item ReadItem(KeyValue item, Schema schema)
        {
            Item newItem = new Item();
            newItem.Template = schema.ItemSchema[Convert.ToInt32(TryGetValue(item, "defindex", "", true))];
            newItem.Level = Convert.ToInt32(TryGetValue(item, "level", "5"));
            newItem.Quantity = Convert.ToInt32(TryGetValue(item, "quantity", "0"));
            newItem.IsTradable = TryGetValue(item, "flag_cannot_trade", "0") == "1" ? false : true;
            newItem.IsCraftable = TryGetValue(item, "flag_cannot_craft", "0") == "1" ? false : true;
            newItem.Quality = (Quality)Enum.Parse(typeof(Quality), TryGetValue(item, "quality", "6"));
            if (newItem.Name == "A Carefully Wrapped Gift")
                newItem.ContainedItem = ReadItem(item["contained_item"], schema);
            //379 - part 1 kills
            //380 - ??? 1106771968 31 posthumous?
            //381 -part 2 kills
            //382 ???
            //383 - part 3
            //384 ???

            //attributes
            foreach (var attr in item["attributes"].Children)
            {
                int defindex = Convert.ToInt32(attr["defindex"].Value);
                switch (defindex)
                {
                    case 500: //custom name
                        newItem.CustomName = attr["value"].Value;
                        break;
                    case 142: //painted
                        newItem.PaintCol = Convert.ToInt32(attr["float_value"].Value);
                        break;
                    case 229: //lowcraft
                        newItem.CraftNumber = Convert.ToInt32(attr["value"].Value);
                        break;
                    case 185:
                        newItem.IsGifted = true;
                        break;
                    case 380:
                    case 382:
                    case 384: //3 strange parts
                        newItem.StrangeParts.Add(Convert.ToInt32(attr["float_value"].Value));
                        break;
                    case 134: //unusual
                        newItem.Effect = schema.UnusualNames[Convert.ToInt32(attr["float_value"].Value)];
                        break;
                    case 153: //untradable
                    case 501: //description
                        break;
                    default:
                        break;
                }
            }
            return newItem;
        }

        //returns the default if the attr isn't found
        static string TryGetValue(KeyValue collection, string attributeToFind, string defaultValue = "", bool throwErrorOnInvalid = false)
        {
            if (collection[attributeToFind] == KeyValue.Invalid)
            {
                if(throwErrorOnInvalid)
                {
                    Console.WriteLine("Could not find value for {0}, terminating.", attributeToFind);
                    Console.ReadLine();
                    throw new Exception();
                }
                return defaultValue;
            }
            else
                return collection[attributeToFind].Value;
        }

        static Schema GetSchema(bool forceUpdate = false)
        {
            Schema schema = new Schema();
            Dictionary<int, ItemTemplate> itemSchema =  new Dictionary<int,ItemTemplate>();
            Dictionary<int, ItemAttributeTemplate> attrSchema = new Dictionary<int, ItemAttributeTemplate>();

            //if we're forcing an update, or the schema doesn't exist
            if (forceUpdate || !File.Exists("schema.txt"))
            {
                using (dynamic steamInterface = WebAPI.GetInterface("IEconItems_440", "7B35211AD579A53C622866CB86B847E8"))
                {
                    //get the item values from the schema
                    KeyValue items = steamInterface.GetSchema(language: "en")["items"];
                    //iterate the items, add to the schema
                    foreach (var item in items.Children)
                    {
                        ItemTemplate newItem = new ItemTemplate();
                        //add The if it's needed
                        newItem.Name = /*(item.Children[5].Value.Equals("1") ? "The " : "") +*/ item.Children[4].Value;
                        newItem.DefIndex = Convert.ToInt32(item.Children[1].Value);
                        if (newItem.Name.Contains("Strange Part:"))
                        {
                            newItem.StrangePartID = Convert.ToInt32(item["attributes"].Children[0]["value"].Value);
                        }
                        string type = item["item_slot"].Value;

                        //only really need 3 classes of items - hats, weapons, tools.
                        /*if (type.Contains("tf_weapon") || type.Contains("saxxy") || item["item_slot"].Value.Contains("secondary"))
                            newItem.Type = ItemType.Weapon;
                        else if(type.Contains("tf_wearable"))
                            newItem.Type = ItemType.Hat;
                        else if(type.Contains("tool") || type.Contains("craft_item") || type.Contains("upgrade") || type.Contains("powerup_bottle") ||
                            type.Contains("map_token") || type.Contains("bundle") || type.Contains("class_token") || type.Contains("slot_token") ||
                            type.Contains("supply_crate"))
                            newItem.Type = ItemType.Tool;
                        else
                            System.Diagnostics.Debugger.Break();*/
                        switch (type)
                        {
                            case "primary":
                            case "secondary":
                            case "melee":
                            case "pda":
                            case "pda2":
                            case "building":
                                newItem.Type = ItemType.Weapon;
                                //now we can add in the default level to make vintages easier
                                newItem.DefaultLevel = Convert.ToInt32(item["min_ilevel"].Value);
                                break;
                            case "head":
                            case "misc":
                                newItem.Type = ItemType.Hat;
                                break;
                            case "action":
                            case "craft_item":
                                newItem.Type = ItemType.Tool;
                                break;
                            default:
                                if (type == null)
                                {
                                    if (item["item_class"].Value == "craft_item")
                                        newItem.Type = ItemType.Tool;
                                }
                                else
                                    System.Diagnostics.Debugger.Break();
                                break;
                        }
                        itemSchema.Add(newItem.DefIndex, newItem);
                    }

                    KeyValue unus = steamInterface.GetSchema(language: "en")["attribute_controlled_attached_particles"];
                    foreach (var effect in unus.Children)
                    {
                        schema.UnusualNames.Add(Convert.ToInt32(effect["id"].Value), effect["name"].Value);
                    }

                    //now we get the attributes
                    KeyValue attrs = steamInterface.GetSchema(language: "en")["attributes"];

                    foreach (var attr in attrs.Children)
                    {
                        ItemAttributeTemplate newAttr = new ItemAttributeTemplate();
                        newAttr.Name = attr["name"].Value;
                        newAttr.DefIndex = Convert.ToInt32(attr["defindex"].Value);
                        attrSchema.Add(newAttr.DefIndex, newAttr);
                    }
                }
                //copy across, save, write
                schema.ItemSchema = itemSchema;
                schema.AttributeSchema = attrSchema;
                string json = JsonConvert.SerializeObject(schema, Formatting.Indented);
                using (StreamWriter writer = new StreamWriter("schema.txt"))
                {
                    writer.Write(json);
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader("schema.txt"))
                {
                    schema = JsonConvert.DeserializeObject<Schema>(reader.ReadToEnd());
                }
            }
            //now set up the strange part db
            foreach(ItemTemplate t in schema.ItemSchema.Values.Where(x => x.StrangePartID != 0))
                schema.StrangePartNames.Add(t.StrangePartID, t.Name.Substring(t.Name.IndexOf(':')+2));
            Console.WriteLine("Schema read.");
            Program.itemSchema = schema;
            return schema;
        }
    }
        #endregion

    #region Reading In Data (bp.tf)


    #endregion
}
