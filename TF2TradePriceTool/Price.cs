using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TF2TradePriceTool
{
    class Price
    {
        //can't be bothered to get this sorted neatly
        public static double KeyPrice = 7.00;
        public static double BudsPrice = 20.00;

        public double LowRefinedPrice { get; set; } //the price in refined

        public double HighRefinedPrice { get; set; }

        public String LowPrice
        {
            get
            {
                //if it's just refined, return it in refined
                //if it's a key or more, return it in keys+refined
                //if it's buds or more, return it in buds + keys
                if (LowRefinedPrice < KeyPrice)
                    return LowRefinedPrice + " ref";
                else if (LowRefinedPrice == KeyPrice)
                    return "1 key";
                else if (LowRefinedPrice < 2 * KeyPrice)
                {
                    return "1 key + " + (LowRefinedPrice % KeyPrice) + " ref";
                }
                else if (LowRefinedPrice < BudsPrice * KeyPrice)
                {
                    return Math.Truncate(LowRefinedPrice / KeyPrice) + " keys + " + (LowRefinedPrice % KeyPrice) + " ref";
                }
                else
                    return Math.Round(LowRefinedPrice / (BudsPrice * KeyPrice), 2) + " buds";

            }
        }
    }
}
