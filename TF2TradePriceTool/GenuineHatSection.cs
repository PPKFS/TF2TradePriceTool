using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TF2TradePriceTool
{
    class GenuineHatSection : Section
    {
        public override void Sort()
        {
            OrderedList = Items.Keys.OrderBy(t => t.Name).ThenBy(t => t.PaintName);
        }

        public override void Print(System.IO.StreamWriter writer)
        {
            Section.WriteTitle(writer, "Genuine Hats");
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
                String item = TF2PricerMain.FormatItem(i, true, Items[i], attribs.ToArray());
                Price paint = null;
                if (i.PaintName != null)
                    paint = TF2PricerMain.PriceSchema.GetPaintPrice(i[Item.Paint]);
                Price p = TF2PricerMain.PriceSchema.GetPrice(i);
                //so write the item, then follow up with the bp.tf prices
                Console.WriteLine(item + "\n");
                if (paint != null)
                {
                    Console.WriteLine("Original: " + p.ToString());
                    Console.WriteLine("Paint: " + paint.ToString());
                    p += paint;
                }
                Console.WriteLine("Price: " + p.ToString());
                TF2PricerMain.GetInputPrice(item, writer, p.LowPrice, p.HighPrice);
                cnt++;
            }
        }

        public override bool TryAdd(Item item)
        {
            if (item.Quality == Quality.Genuine && item.Type == ItemType.Hat)
            {
                Items.AddCounted(item);
                return true;
            }
            else
                return false;
        }
    }
}
