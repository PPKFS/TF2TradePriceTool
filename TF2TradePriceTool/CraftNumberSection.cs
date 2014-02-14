using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2TradePriceTool
{
    class CraftNumberSection : Section
    {
        public override void Sort()
        {
            OrderedList = Items.Keys.OrderBy(t => t.Name).ThenBy(t => t[Item.CraftNumber]);
        }

        public override void Print(System.IO.StreamWriter writer)
        {
            writer.WriteLine("**Craft Numbers**\n\n");
            Console.WriteLine("# Numbers\n\n");
            int cnt = 0;
            foreach (Item i in OrderedList)
            {
                double percent = Math.Round(((double)cnt) * 100 / ((double)Items.Keys.Count));
                Console.WriteLine("Progress: Item {0} of {1} (" + percent + "%)", cnt + 1, Items.Keys.Count);
                List<String> attribs = new List<string>();
                attribs.AddIfNotNull(i.PaintName);
                if (i.IsGifted)
                    attribs.Add("Gifted");
                //pretty print the item
                String item = TF2PricerMain.FormatItem(i, false, Items[i], attribs.ToArray());
                Price paint = null;
                Price[] parts = new Price[3] { null, null, null };
                if (i.PaintName != null)
                    paint = TF2PricerMain.PriceSchema.GetPaintPrice(i[Item.Paint]);
                if (i.StrangeParts != null)
                {
                    for (int partCount = 0; partCount < 3; ++partCount)
                    {
                        parts[partCount] = TF2PricerMain.PriceSchema.GetPartPrice(i[Item.StrangePart1 + partCount]);
                    }
                }
                Price p = TF2PricerMain.PriceSchema.GetPrice(i);
                //so write the item, then follow up with the bp.tf prices
                Console.WriteLine(item + "\n");
                if (paint != null || i.StrangeParts != null)
                    Console.WriteLine("Original: " + p.ToString());
                if (paint != null)
                {
                    Console.WriteLine("Paint: " + paint.ToString());
                    p += paint;
                }
                Console.WriteLine("Base Price: " + p.ToString());
                TF2PricerMain.GetInputPrice(item, writer, p.LowPrice, p.HighPrice);
                cnt++;
            }
        }

        public override bool TryAdd(Item item)
        {
            if (item[Item.CraftNumber] != null && Convert.ToInt32(item[Item.CraftNumber]) <= 100)
            {
                Items.AddCounted(item);
                return true;
            }
            else
                return false;
        }
    }
}
