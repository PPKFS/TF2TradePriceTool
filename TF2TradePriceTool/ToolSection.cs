using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2TradePriceTool
{
    class ToolSection : Section
    {
        public override void Sort()
        {
            OrderedList = Items.Keys.OrderBy(t => t.Name).ThenBy(t => t.IsCraftable).ThenBy(t => t[Item.Quantity]);
        }

        public override void Print(System.IO.StreamWriter writer)
        {
            writer.WriteLine("**Tools 'n' Stuff**\n\n");
            Console.WriteLine("Tools\n\n");
            int cnt = 0;
            foreach (Item i in OrderedList)
            {
                double percent = Math.Round(((double)cnt) * 100 / ((double)Items.Keys.Count));
                Console.WriteLine("Progress: Item {0} of {1} (" + percent + "%)", cnt + 1, Items.Keys.Count);
                List<String> attribs = new List<string>();
                if (i.IsGifted)
                    attribs.Add("Gifted");
                if (!i.IsCraftable)
                    attribs.Add("Uncraftable");
                if (!i.IsTradable)
                    attribs.Add("Untradable");
                if (i[Item.Quantity] != null)
                    attribs.Add(i[Item.Quantity] + " Uses");
                //pretty print the item
                String item = TF2PricerMain.FormatItem(i, false, Items[i], attribs.ToArray());
                Price p = TF2PricerMain.PriceSchema.GetPrice(i);
                //so write the item, then follow up with the bp.tf prices
                Console.WriteLine(item + "\n");
                Console.WriteLine("Price: " + p.ToString());
                TF2PricerMain.GetInputPrice(item, writer, p.LowPrice, p.HighPrice);
                cnt++;
            }
        }

        public override bool TryAdd(Item item)
        {
            if (item.Type == ItemType.Tool)
            {
                Items.AddCounted(item);
                return true;
            }
            else
                return false;
        }
    }
}
