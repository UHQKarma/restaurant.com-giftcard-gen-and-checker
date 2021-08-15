using System;
using System.Runtime.InteropServices;

namespace Restaurant.Core
{
    public static class Clr
    {
        public const string Black = "\u001b[30m";
        public const string Red = "\u001b[31m";
        public const string Green = "\u001b[32m";
        public const string Yellow = "\u001b[33m";
        public const string Blue = "\u001b[34m";
        public const string Magenta = "\u001b[35m";
        public const string Cyan = "\u001b[36m";
        public const string White = "\u001b[37m";
        public const string Reset = "\u001b[0m";

        public const string LtGray = "\u001b[37m";
        public const string DrkGray = "\u001b[90m";
        public const string LtRed = "\u001b[91m";
        public const string LtGreen = "\u001b[92m";
        public const string LtYellow = "\u001b[93m";
        public const string LtBlue = "\u001b[94m";
        public const string LtMagenta = "\u001b[95m";
        public const string LtCyan = "\u001b[96m";

        private const int StdOutputHandle = -11;
        private const uint EnableVirtualTerminalProcessing = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void InitConsole()
        {
            var handle = GetStdHandle(StdOutputHandle);
            GetConsoleMode(handle, out var mode);
            mode |= EnableVirtualTerminalProcessing;
            SetConsoleMode(handle, mode);
        }

        public static void WriteLine(string text) =>
            Console.WriteLine(text);

        public static void Write(string text) =>
            Console.Write(text);
    }
}