using System;
using System.IO;
using System.Threading.Tasks;
using CryptoLab.Common;
using CryptoLab.Algorithms;

namespace CryptoLabApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--- Cryptography Lab 1 ---");

            byte[] key = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            
            Console.WriteLine("\n[DES Demonstration]");
            var des = new DESAlgorithm();
            
            var modes = new[] { CipherMode.CBC, CipherMode.ECB, CipherMode.CTR };
            var paddings = new[] { PaddingMode.PKCS7, PaddingMode.ISO10126, PaddingMode.Zeros };

            foreach (var mode in modes)
            {
                foreach (var padding in paddings)
                {
                    Console.WriteLine($"\nTesting DES with Mode: {mode}, Padding: {padding}");
                    var context = new CipherContext(des, key, mode, padding, iv);
                    byte[] data = System.Text.Encoding.UTF8.GetBytes("Testing multiple modes and paddings with DES algorithm.");
                    
                    byte[] enc = await context.EncryptAsync(data);
                    byte[] dec = await context.DecryptAsync(enc);
                    
                    string decStr = System.Text.Encoding.UTF8.GetString(dec).TrimEnd('\0');
                    Console.WriteLine($"Decrypted matches original: {decStr.StartsWith("Testing multiple")}");
                }
            }

            Console.WriteLine("\n[DEAL Demonstration]");
            byte[] dealKey = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };
            byte[] dealIv = new byte[16]; 
            var deal = new DEALAlgorithm(6);
            
            Console.WriteLine("Testing DEAL with Mode: CBC, Padding: PKCS7");
            var dealContext = new CipherContext(deal, dealKey, CipherMode.CBC, PaddingMode.PKCS7, dealIv);

            byte[] original = System.Text.Encoding.UTF8.GetBytes("DEAL uses DES as a round function in a larger Feistel network.");
            byte[] dealEncrypted = await dealContext.EncryptAsync(original);
            byte[] dealDecrypted = await dealContext.DecryptAsync(dealEncrypted);
            Console.WriteLine($"DEAL Decrypted: {System.Text.Encoding.UTF8.GetString(dealDecrypted)}");

            Console.WriteLine("\n[File Processing Demonstration]");
            string testFile = "test.txt";
            string encryptedFile = "test.enc";
            string decryptedFile = "test.dec";
            await File.WriteAllTextAsync(testFile, "File content to be encrypted using DES and DEAL algorithms.");

            var finalDesContext = new CipherContext(des, key, CipherMode.CBC, PaddingMode.PKCS7, iv);
            await finalDesContext.ProcessFileAsync(testFile, encryptedFile, true);
            await finalDesContext.ProcessFileAsync(encryptedFile, decryptedFile, false);
            Console.WriteLine($"DES File Processing completed. Decrypted content: {await File.ReadAllTextAsync(decryptedFile)}");
        }
    }
}
