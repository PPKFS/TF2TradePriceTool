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

        public static Schema Schema = new Schema();

        static void Main(String[] args)
        {

            //build schema if needed
            if (!File.Exists("schema.txt"))
                Schema.BuildSchema();
            else
                Schema.LoadSchema();
            Backpack backpack = new Backpack();
            if (!File.Exists("items.txt"))
                backpack.GetContents(SteamID);
            else
            {
                using (StreamReader reader = new StreamReader("items.txt"))
                {
                    //out of date
                    if (reader.ReadLine() != DateTime.Now.Date.ToString())
                        backpack.GetContents(SteamID);
                    else
                        backpack = JsonConvert.DeserializeObject<Backpack>(reader.ReadToEnd());
                }
            }

            Schema.BuildPriceList();

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
                    using (StreamReader reader = new StreamReader("data/apikeys.txt"))
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
                using (StreamReader reader = new StreamReader("data/apikeys.txt"))
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
