using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static string ComputePbkdf2(string password, byte[] salt, int iterations, int bytes)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(bytes);
            return string.Format(
                "$pbkdf2$v=1$i={0}$s={1}$h={2}",
                iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }
    }

    static void Main()
    {
        // Use a fixed salt for this test so the hash is deterministic.
        byte[] salt = new byte[16];
        salt[0] = 0x12; salt[1] = 0x34; salt[2] = 0x56; salt[3] = 0x78;
        salt[4] = 0x9A; salt[5] = 0xBC; salt[6] = 0xDE; salt[7] = 0xF0;
        salt[8] = 0x11; salt[9] = 0x22; salt[10] = 0x33; salt[11] = 0x44;
        salt[12] = 0x55; salt[13] = 0x66; salt[14] = 0x77; salt[15] = 0x88;

        string hash = ComputePbkdf2("Admin@123", salt, 120000, 32);
        Console.WriteLine(hash);
    }
}
