using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SteamKit2;
using Newtonsoft.Json;

namespace TF2TradePriceTool
{
    class TF2PricerMain
    {
        private static String SteamAPIKey = null;
        private static String BackpackTFAPIKey = null;
        public static String SteamID = /*"76561198085492556";//*/"76561198034183306";

        public static String SchemaLocation = "data/schema.txt";
        public static String PriceLocation = "data/prices.txt";
        public static String BackpackLocation = "data/items.txt";
        public static String OutputLocation = "data/output.txt";
        public static String APIKeyLocation = "data/apikeys.txt";

        public static Schema Schema = new Schema();
        public static PriceSchema PriceSchema = new PriceSchema();

        static void Main(String[] args)
        {

            //build schema if needed
            if (!File.Exists(TF2PricerMain.SchemaLocation))
                Schema.BuildSchema();
            else
                Schema./*BuildSchema();//*/LoadSchema();
            //build pricelist if needed
            if (!File.Exists(TF2PricerMain.PriceLocation))
                PriceSchema.BuildPriceList();
            else
                PriceSchema.BuildPriceList();

            //now the backpack
            Backpack backpack = new Backpack();
            if (!File.Exists(TF2PricerMain.BackpackLocation))
                backpack.GetContents(SteamID);
            else
            {
                using (StreamReader reader = new StreamReader(TF2PricerMain.BackpackLocation))
                {
                    //out of date
                    if (reader.ReadLine() != DateTime.Now.Date.ToString())
                        backpack.GetContents(SteamID);
                    else
                        backpack = JsonConvert.DeserializeObject<Backpack>(reader.ReadToEnd());
                }
            }

            backpack.Sort();
            //so now everything is loaded. Hooray!
            //get the backpack to print out everything
            backpack.PrintOutItems();
        }

        public static String GetSteamAPIKey()
        {
            if (SteamAPIKey == null)
            {
                //read from the API key file
                try
                {
                    using (StreamReader reader = new StreamReader(TF2PricerMain.APIKeyLocation))
                        SteamAPIKey = reader.ReadLine();
                }
                catch (Exception e)
                {
                    if (e is FileNotFoundException || e is DirectoryNotFoundException)
                    {
                        Console.WriteLine("No api key file found.");
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                    else
                        throw;
                }
            }
            return SteamAPIKey;
        }

        public static String GetBackpackTFKey()
        {
            if (BackpackTFAPIKey == null)
            {
                //read from the API key file
                using (StreamReader reader = new StreamReader(TF2PricerMain.APIKeyLocation))
                {
                    reader.ReadLine();
                    //second line is bp.tf's key
                    BackpackTFAPIKey = reader.ReadLine();
                }
            }
            return BackpackTFAPIKey;
        }

        public static String GetValue(KeyValue json, String valueToGet)
        {
            if (json[valueToGet] == null)
                return null;
            else
                return json[valueToGet].Value;
        }

        public static String FormatItem(Item item, bool printQuality, int count, params String[] attributes)
        {
            Quality quality = item.Quality;
            String qual = (!printQuality && quality == Quality.Unique) ? "" : quality.ToString();
            String name = qual +" "+ ((item[Item.CraftNumber] != null && Convert.ToInt32(item[Item.CraftNumber]) <= 100) ? item.Name + " #" + item[Item.CraftNumber] : item.Name);
            if (attributes.Count() == 0)
            {
                //return String.Join(" ", "*", qual, name, (count != 1) ? "x" + count : "");
                return name + ((count != 1) ? " x " + count : "")+"|";
            }
            else
            {
                return name + ((count != 1) ? " x " + count : "") + "|" + String.Join(", ", attributes);
                //return String.Join(" ", "*", qual, name, "(" + String.Join(", ", attributes) + ")", (count != 1) ? "x" + count : "");
            }
        }

        public static String GetInputPrice(String formattedItem, StreamWriter writer, String lowPrice, String highPrice)
        {
            Console.WriteLine("1) Low. 2) High. 3) blank. Other) Custom.");
            String input = Console.ReadLine();
            String price = "";
            switch (input)
            {
                case "":
                    break;
                case "1":
                    price = lowPrice;
                    break;
                case "2":
                    price = highPrice;
                    break;
                case "3":
                    price = ".";
                    break;
                default:
                    price = input;
                    break;
                //price = TF2PricerMain.PriceSchema
            }
            if(price == ".")
                writer.WriteLine(formattedItem+"|"); //i.e. don't list a price at all, for bulk stuff
            else if (price != "")
                writer.WriteLine(formattedItem + "|" + price);
            writer.Flush();
            Console.Clear();
            return price;
        }
    }
}
