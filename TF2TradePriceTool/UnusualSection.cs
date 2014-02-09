using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TF2TradePriceTool
{
    
    class UnusualSection : Section
    {
        public override void Sort()
        {
        }

        public override void Print(StreamWriter writer)
        {
            writer.WriteLine("**Unusual Hats**\n\n");
            Console.WriteLine("Unusuals\n\n");
            int cnt = 1;
            foreach (Item i in Items.Keys)
            {
                Console.WriteLine("Progress: Item {0} of {1} ({2}%)", cnt, Items.Keys.Count, Math.Round((double)cnt / Items.Keys.Count));
                List<String> attribs = new List<string>();
                attribs.Add(i.EffectName);
                attribs.AddIfNotNull(i.PaintName);
                if (i.IsGifted)
                    attribs.Add("Gifted");
                //pretty print the item
                String item = TF2PricerMain.FormatItem(i, true, Items[i], attribs.ToArray());
                Price p = TF2PricerMain.PriceSchema.GetPrice(i);
                //so write the item, then follow up with the bp.tf prices
                Console.WriteLine(item+"\n");
                Console.WriteLine("Price: " + p.ToString());
                TF2PricerMain.GetInputPrice(item, writer, p.LowPrice, p.HighPrice);

            }
            int ftfg;
        }

        public override bool TryAdd(Item item)
        {
            if (item.Quality == Quality.Unusual && item.Type == ItemType.Hat)
            {
                Items.AddCounted(item);
                return true;
            }
            else
                return false;
        }
    }
}
