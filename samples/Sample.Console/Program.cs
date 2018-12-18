using System;

namespace Sample.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine(System.IO.Path.Combine(AppContext.BaseDirectory, "file.config"));
            System.Console.WriteLine(System.IO.Path.Combine(Environment.CurrentDirectory, "file.config"));
        }
    }
}
