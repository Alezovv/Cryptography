using System;
using System.Linq;

namespace CryptoLab.Common
{
    public static class PaddingProvider
    {
        public static byte[] AddPadding(byte[] data, int blockSize, PaddingMode mode)
        {
            if (mode == PaddingMode.None) return data;

            int paddingLength = blockSize - (data.Length % blockSize);
            
            byte[] padded = new byte[data.Length + paddingLength];
            Array.Copy(data, padded, data.Length);

            switch (mode)
            {
                case PaddingMode.Zeros:
                    break;
                case PaddingMode.PKCS7:
                    for (int i = data.Length; i < padded.Length; i++)
                        padded[i] = (byte)paddingLength;
                    break;
                case PaddingMode.AnsiX923:
                    padded[padded.Length - 1] = (byte)paddingLength;
                    break;
                case PaddingMode.ISO10126:
                    Random rand = new Random();
                    for (int i = data.Length; i < padded.Length - 1; i++)
                        padded[i] = (byte)rand.Next(256);
                    padded[padded.Length - 1] = (byte)paddingLength;
                    break;
            }
            return padded;
        }

        public static byte[] RemovePadding(byte[] data, int blockSize, PaddingMode mode)
        {
            if (mode == PaddingMode.None || data.Length == 0) return data;

            int paddingLength = 0;
            switch (mode)
            {
                case PaddingMode.Zeros:
                    int i = data.Length - 1;
                    while (i >= 0 && data[i] == 0) i--;
                    return data.Take(i + 1).ToArray();
                case PaddingMode.PKCS7:
                    paddingLength = data[data.Length - 1];
                    break;
                case PaddingMode.AnsiX923:
                case PaddingMode.ISO10126:
                    paddingLength = data[data.Length - 1];
                    break;
            }

            if (paddingLength <= 0 || paddingLength > blockSize) return data;
            return data.Take(data.Length - paddingLength).ToArray();
        }
    }
}
