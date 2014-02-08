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
        private static String SteamID = "76561198034183306";

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
                Schema.LoadSchema();
            //build pricelist if needed
            if (!File.Exists(TF2PricerMain.PriceLocation))
                PriceSchema.BuildPriceList();
            else
                PriceSchema.LoadPriceList();

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

            //test cases
            //fractional, whole, 1 key, 1 key + whole, 1 key + fractional, multiple keys, multiple keys + whole, multiple keys + fractional, 1 buds, 1 buds + fractional
            Price p1 = new Price();
            p1.LowRefinedPrice = 5;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 0.33;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 7;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 12;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 15.77;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 35;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 39;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 25.77;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 140;
            Console.WriteLine(p1.LowPrice);
            p1.LowRefinedPrice = 178;
            Console.WriteLine(p1.LowPrice);
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
    }
}
