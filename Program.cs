using SpookVooper.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SVNetworth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter SV Username which you want networth for:");
            string SVID = await SpookVooperAPI.Users.GetSVIDFromUsername(Console.ReadLine());
            await Task.Delay(100);

            string[] tickers = { "B", "IDE", "NEWS", "POT", "TECH", "TYCO", "VC", "VNB", "VU", "X" };

            List<decimal> stock_values = new List<decimal>();

            foreach (var ticker in tickers)
            {
                var OwnerData = await SpookVooperAPI.Economy.GetOwnerData(ticker);
                await Task.Delay(100);
                foreach (var owner in OwnerData)
                {
                    if (owner.Owner_Id == SVID)
                    {
                        decimal value = await SpookVooperAPI.Economy.GetStockBuyPrice(ticker);
                        Console.WriteLine(value * owner.Amount);
                        stock_values.Add(value * owner.Amount);
                        await Task.Delay(100);
                    }
                }
            }

            Console.WriteLine($"Networth: ¢{stock_values.Sum()}");    
        }
    }
}
