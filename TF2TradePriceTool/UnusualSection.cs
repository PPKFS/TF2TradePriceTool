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
            OrderedList = Items.Keys.OrderBy(t => t.Name).ThenBy(t => t.EffectName);
        }

        public override void Print(StreamWriter writer)
        {
            Section.WriteTitle(writer, "Unusuals", "Effect");
            int cnt = 0;
            foreach (Item i in OrderedList)
            {
                double percent = Math.Round(((double)cnt)*100 / ((double)Items.Keys.Count));
                Console.WriteLine("Progress: Item {0} of {1} ("+percent+"%)", cnt+1, Items.Keys.Count);
                List<String> attribs = new List<string>();
                attribs.Add(i.EffectName);
                //attribs.AddIfNotNull(i.PaintName);
                if (i.IsGifted)
                    attribs.Add("Gifted");
                //pretty print the item
                String item = TF2PricerMain.FormatItem(i, true, Items[i], attribs.ToArray());
                Price p = TF2PricerMain.PriceSchema.GetPrice(i);
                Price paint = TF2PricerMain.PriceSchema.GetPaintPrice(i[Item.Paint]);
                //so write the item, then follow up with the bp.tf prices
                Console.WriteLine(item+"\n");
                Console.WriteLine("Price: " + p.ToString());
                TF2PricerMain.GetInputPrice(item, writer, p.LowPrice, p.HighPrice);
                cnt++;
            }
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
