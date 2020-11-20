using HtmlAgilityPack;
using SpookVooper.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SVNetworth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter SV name (Group/User) which you want networth for:");
            string name = Console.ReadLine();
            string gSVID = await SpookVooperAPI.Groups.GetSVIDFromName(name);
            await Task.Delay(100);
            string uSVID = await SpookVooperAPI.Users.GetSVIDFromUsername(name);
            string SVID = null;
            bool isgSVID = false;
            if (gSVID == null && uSVID == null)
            {
                while (gSVID == null && uSVID == null)
                {
                    Console.WriteLine("This SV name does not equal an actual SV Group or User's name. Try again.");
                    name = Console.ReadLine();
                    gSVID = await SpookVooperAPI.Groups.GetSVIDFromName(name);
                    await Task.Delay(100);
                    uSVID = await SpookVooperAPI.Users.GetSVIDFromUsername(name);
                    await Task.Delay(100);
                }
            }
            if (gSVID != null)
            {
                SVID = gSVID;
                isgSVID = true;
            }
            else if (uSVID != null)
            {
                SVID = uSVID;
            }
            else if (gSVID != null && uSVID != null)
            {
                Console.WriteLine("Something with SV really fucking broke. Please contact Spike immiditally as this name works for both a company and user");
                Environment.Exit(0xA0);
            }

            string ticker = null;
            if (isgSVID == true)
            {
                Console.WriteLine("Write the companies stock ticker. If none leave empty.");
                string input = Console.ReadLine();
                decimal check = await SpookVooperAPI.Economy.GetStockValue(input);
                if (check == 9995654632999 || input == "C4")
                    while (check == 9995654632999 && input != "C4")
                    {
                        Console.WriteLine("This string does not equal a stock ticker. Try again or cancel with C4.");
                        input = Console.ReadLine();
                    }
                if (input == "C4")
                {
                    ticker = "";
                }
                else
                {
                    ticker = input;
                }
            }

            bool findSVIDs = false;
            if (isgSVID == false)
            {
                Console.WriteLine("Do you wish to search for new companies? (cannot be used if allGroupsSVIDs.json does not exist!) (Y/N)");
                string input = Console.ReadLine();
                while (input.ToLower() != "n" && input.ToLower() != "no" && input.ToLower() != "y" && input.ToLower() != "yes")
                {
                    Console.WriteLine("This string does not equal Y/N or lower case y/n. Try again.");
                    input = Console.ReadLine();
                }

                if (input.ToLower() == "y" || input.ToLower() == "yes")
                {
                    findSVIDs = true;
                }
            }

            string[] tickers = { "B", "IDE", "NEWS", "POT", "TECH", "TYCO", "VC", "VNB", "VU", "X" };

            List<decimal> stock_values = new List<decimal>();

            List<decimal> stock_values_self = new List<decimal>();

            foreach (var ticks in tickers)
            {
                var OwnerData = await SpookVooperAPI.Economy.GetOwnerData(ticks);
                await Task.Delay(100);
                foreach (var owner in OwnerData)
                {
                    if (owner.Owner_Id == SVID)
                    {
                        decimal value = await SpookVooperAPI.Economy.GetStockBuyPrice(ticks);
                        stock_values.Add(value * owner.Amount);
                        await Task.Delay(100);
                        if (ticks == ticker)
                        {
                            stock_values_self.Add(value * owner.Amount);
                        }
                    }
                }
            }

            decimal company_value = 0;

            if (isgSVID == false)
            {
                List<string> GroupIDs;

                if (findSVIDs == true)
                {
                    GroupIDs = await GroupSVIDsAsync().ConfigureAwait(false);
                    foreach (var svid in GroupIDs)
                    {
                        bool EcoPerms = await SpookVooperAPI.Groups.HasGroupPermission(svid, SVID, "eco");
                        await Task.Delay(100);
                        if (EcoPerms == true)
                        {
                            company_value += await SpookVooperAPI.Groups.GetBalance(svid);
                            await Task.Delay(100);
                        }
                    }
                }

                else if (findSVIDs == false)
                {
                    var lines = System.IO.File.ReadAllLines(@"allGroupsSVIDs.txt");
                    foreach (var line in lines)
                    {
                        bool EcoPerms = await SpookVooperAPI.Groups.HasGroupPermission(line, SVID, "eco");
                        await Task.Delay(100);
                        if (EcoPerms == true)
                        {
                            company_value += await SpookVooperAPI.Groups.GetBalance(line);
                            await Task.Delay(100);
                        }
                    }
                }
            }

            decimal balance = await SpookVooperAPI.Economy.GetBalance(SVID);

            decimal stock_value = stock_values.Sum();

            decimal stock_value_self = stock_values_self.Sum();

            decimal total = balance + company_value + (stock_value - stock_value_self);

            if (isgSVID == false)
            {
                Console.WriteLine($"Networth: ¢{total}\nBalance: {balance}\nStocks Value: {stock_value}\nCompanies Value (only with eco access): {company_value}\nPress enter to end program.");
            }
            else if (total != total + stock_value_self)
            {
                Console.WriteLine($"Networth: ¢{total}\nBalance: {balance}\nStocks Value: {stock_value}\nNetworth + Total Own Stocks Value: {total + stock_value_self}\nPress enter to end program.");
            }
            else
            {
                Console.WriteLine($"Networth: ¢{total}\nBalance: {balance}\nStocks Value: {stock_value}\nPress enter to end program.");
            }
            Console.ReadKey();
        }

        static async Task<List<string>> GroupSVIDsAsync()
        {
            HtmlWeb web = new HtmlWeb();

            char[] az = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (Char)i).ToArray();
            List<int> numberList = Enumerable.Range(0, 9).ToList();

            List<string> tableeallgroups = new List<string>();

            foreach (var c in az)
            {
                HtmlDocument doc = web.Load("https://spookvooper.com/Group/Search/" + c);

                List<string> list = new List<string>();

                foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
                {
                    ///This is the table.    
                    foreach (HtmlNode row in table.SelectNodes("tr"))
                    {
                        ///This is the row.
                        foreach (HtmlNode cell in row.SelectNodes("td"))
                        {
                            ///This the cell.
                            foreach (HtmlNode pain in cell.SelectNodes("a"))
                            {
                                list.Add(pain.GetAttributeValue("href", "").Replace("/User/Info?svid=", ""));
                            }
                        }
                    }
                }

                tableeallgroups.AddRange(list);
                await Task.Delay(100);
            }

            foreach (var c in numberList)
            {

                HtmlDocument doc = web.Load("https://spookvooper.com/Group/Search/" + c);

                List<string> list = new List<string>();

                foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
                {
                    ///This is the table.    
                    foreach (HtmlNode row in table.SelectNodes("tr"))
                    {
                        ///This is the row.
                        foreach (HtmlNode cell in row.SelectNodes("td"))
                        {
                            ///This the cell.
                            foreach (HtmlNode pain in cell.SelectNodes("a"))
                            {
                                list.Add(pain.GetAttributeValue("href", "").Replace("/User/Info?svid=", ""));
                            }
                        }
                    }
                }

                tableeallgroups.AddRange(list);
                await Task.Delay(100);
            }

            List<string> nodupetableallgroups = tableeallgroups.Distinct().ToList();

            nodupetableallgroups.RemoveAt(0);

            List<string> result = new List<string>();

            TextWriter twsvidgroups = new StreamWriter("allGroupsSVIDs.txt");
            foreach (string s in nodupetableallgroups)
            {
                if (s != null)
                {
                    twsvidgroups.WriteLine(s);
                    result.Add(s);
                }
            }
            twsvidgroups.Close();
            return result;
        }
    }
}
