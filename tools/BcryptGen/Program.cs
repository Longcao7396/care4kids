using System;
using BCrypt.Net;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: BcryptGen <password>");
            Environment.Exit(1);
        }
        string hash = BCrypt.Net.BCrypt.HashPassword(args[0], 11);
        Console.WriteLine(hash);
    }
}
