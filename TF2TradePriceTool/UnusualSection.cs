using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2TradePriceTool
{
    static class Extensions
    {
        public static void AddIfNotNull<T>(this List<T> list, T item)
        {
            if (item == null)
                return;
            else
                list.Add(item);
        }
    }
    
    class UnusualSection : Section
    {
        public override void Sort()
        {
        }

        public override void Print()
        {
            Console.WriteLine("**Unusual Hats**\n\n");

            foreach (Item i in Items)
            {
                List<String> attribs = new List<string>();
                attribs.Add(i[Item.Effect]);
                attribs.AddIfNotNull(i.PaintName);
                if (i.IsGifted)
                    attribs.Add("Gifted");
            }
        }

        public override bool TryAdd(Item item)
        {
            if (item.Quality == Quality.Unusual && item.Type == ItemType.Hat)
            {
                Items.Add(item);
                return true;
            }
            else
                return false;
        }
    }
}
