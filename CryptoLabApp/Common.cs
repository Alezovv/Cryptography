namespace CryptoLab.Common
{
    public enum CipherMode
    {
        ECB,
        CBC,
        PCBC,
        CFB,
        OFB,
        CTR,
        RandomDelta
    }

    public enum PaddingMode
    {
        None,
        Zeros,
        AnsiX923,
        PKCS7,
        ISO10126
    }

    public static class BitUtils
    {
        public enum BitIndexing
        {
            MsbFirst, // Big-endian bit
            LsbFirst  // Little-endian
        }

        public static byte[] Permute(byte[] input, int[] pBox, BitIndexing indexing, int startIndex)
        {
            int outputBitCount = pBox.Length;
            int outputByteCount = (outputBitCount + 7) / 8;
            byte[] output = new byte[outputByteCount];

            for (int i = 0; i < pBox.Length; i++)
            {
                int bitPos = pBox[i] - startIndex;
                if (GetBit(input, bitPos, indexing))
                {
                    SetBit(output, i, true, indexing);
                }
            }

            return output;
        }

        public static bool GetBit(byte[] data, int bitIndex, BitIndexing indexing)
        {
            int byteIdx = bitIndex / 8;
            int bitInByte = bitIndex % 8;

            if (byteIdx >= data.Length) return false;

            if (indexing == BitIndexing.MsbFirst)
            {
                return (data[byteIdx] & (0x80 >> bitInByte)) != 0;
            }
            else
            {
                return (data[byteIdx] & (1 << bitInByte)) != 0;
            }
        }

        public static void SetBit(byte[] data, int bitIndex, bool value, BitIndexing indexing)
        {
            int byteIdx = bitIndex / 8;
            int bitInByte = bitIndex % 8;

            if (byteIdx >= data.Length) return;

            if (indexing == BitIndexing.MsbFirst)
            {
                if (value)
                    data[byteIdx] |= (byte)(0x80 >> bitInByte);
                else
                    data[byteIdx] &= (byte)~(0x80 >> bitInByte);
            }
            else
            {
                if (value)
                    data[byteIdx] |= (byte)(1 << bitInByte);
                else
                    data[byteIdx] &= (byte)~(1 << bitInByte);
            }
        }

        public static byte[] Xor(byte[] a, byte[] b)
        {
            byte[] res = new byte[a.Length];
            for (int i = 0; i < a.Length; i++) res[i] = (byte)(a[i] ^ b[i]);
            return res;
        }
    }
}
