using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Leaf.xNet;

namespace Restaurant
{
    public static class Globals
    {
        public static readonly ConcurrentBag<string> Queue = new ConcurrentBag<string>();
        public static readonly List<string> Proxies = new List<string>();

        public static int Threads = 500;
        public static int OutputBad = 2;
        public const int Timeout = 500;
        public static ProxyType EProxyType = ProxyType.Socks4;

        public static readonly string ResultsPath = $@"Results\{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";

        private static readonly Random Random = new Random();

        public static string GetRandomCard()
        {
            return new string(Enumerable.Repeat("123456789", 10)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}