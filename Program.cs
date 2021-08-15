using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using Restaurant.Core;

namespace Restaurant
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Clr.InitConsole();
            var threadList = new List<Thread>();

            if (!File.Exists("proxies.txt"))
            {
                Console.WriteLine($"{Clr.LtRed}[ERROR] File proxies.txt doesn't exist! {Clr.Reset}");
                Console.ReadLine();
                Environment.Exit(1);
            }
            
            File.ReadAllLines("proxies.txt").ToList().ForEach(Globals.Proxies.Add);
            
            Console.Write($"{Clr.LtCyan}[{Clr.White}?{Clr.LtCyan}] Amount: {Clr.Reset}");
            Stats.Amount = int.Parse(Console.ReadLine()!);
            
            Console.Write($"{Clr.LtCyan}[{Clr.White}?{Clr.LtCyan}] Threads: {Clr.Reset}");
            Globals.Threads = int.Parse(Console.ReadLine()!);
            
            Console.Write($"{Clr.LtCyan}[{Clr.White}?{Clr.LtCyan}] Show Bad [1 - Yes, 2 - No]: {Clr.Reset}");
            Globals.OutputBad = int.Parse(Console.ReadLine()!);

            Globals.EProxyType = GetUserProxy();

            if (!Directory.Exists("Results")) Directory.CreateDirectory("Results");
            if (!Directory.Exists(Globals.ResultsPath)) Directory.CreateDirectory(Globals.ResultsPath);

            Task.Factory.StartNew(Checker.TitleUpdater);
            Task.Factory.StartNew(Checker.CpmCounter);

            for (var i = 0; i < Globals.Threads; i++)
            {
                var t = new Thread(() =>
                {
                    while (Stats.Amount > Stats.Checked)
                    {
                        Checker.Check();
                    }
                });
                
                t.Start();
                threadList.Add(t);
            }
            
            foreach (var task in threadList)
                task.Join();

            Stats.Finished = true;
            Console.Clear();
            
            Console.WriteLine($"{Clr.LtGreen}[HITS] {Stats.Working}{Clr.Reset}");
            Console.WriteLine($"{Clr.LtYellow}[CUSTOM] {Stats.Custom}{Clr.Reset}");
            Console.WriteLine($"{Clr.LtRed}[BAD] {Stats.Invalid}{Clr.Reset}\n");
            
            Console.WriteLine($"{Clr.LtMagenta}[5] {Stats._5}{Clr.Reset}");
            Console.WriteLine($"{Clr.LtMagenta}[10] {Stats._10}{Clr.Reset}");
            Console.WriteLine($"{Clr.LtMagenta}[15] {Stats._15}{Clr.Reset}\n");
            
            Console.WriteLine($"{Clr.LtMagenta}[25] {Stats._25}{Clr.Reset}");
            Console.WriteLine($"{Clr.LtMagenta}[50] {Stats._50}{Clr.Reset}");
            Console.WriteLine($"{Clr.LtMagenta}[100] {Stats._100}{Clr.Reset}\n");
            
            Console.ReadLine();
        }
        
        private static ProxyType GetUserProxy()
        {
            var proxyTypes = new Dictionary<int, ProxyType>
            {
                {1, ProxyType.HTTP},
                {2, ProxyType.Socks4},
                {3, ProxyType.Socks5}
            };

            while (true)
            {
                try
                {
                    Console.Write($"{Clr.LtCyan}[{Clr.White}?{Clr.LtCyan}] ProxyType [1 - Http/S, 2 - Socks4, 3 - Socks5]: {Clr.Reset}");
                    var proxyNumber = Convert.ToInt16(Console.ReadLine());
                    
                    return proxyTypes[proxyNumber];
                }
                catch
                {
                    //ignored
                }
            }
        }
    }
}