using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2TradePriceTool
{
    abstract class Section
    {
        public List<Item> Items { get; set; }

        public Section()
        {
            Items = new List<Item>();
        }

        public abstract void Sort();

        public abstract void Print();

        public abstract bool TryAdd(Item newItem);
    }
}
