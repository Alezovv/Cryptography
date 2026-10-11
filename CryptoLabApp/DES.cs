using System;
using System.Linq;
using CryptoLab.Interfaces;
using CryptoLab.Common;

namespace CryptoLab.Algorithms
{
    public class DESRoundFunction : IRoundFunction
    {
        private static readonly int[] E = {
            32, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13, 12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21, 20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1
        };

        private static readonly int[] P = {
            16, 7, 20, 21, 29, 12, 28, 17, 1, 15, 23, 26, 5, 18, 31, 10,
            2, 8, 24, 14, 32, 27, 3, 9, 19, 13, 30, 6, 22, 11, 4, 25
        };

        private static readonly byte[][,] S = new byte[8][,] {
            new byte[4, 16] { { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 }, { 0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8 }, { 4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0 }, { 15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13 } },
            new byte[4, 16] { { 15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10 }, { 3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5 }, { 0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15 }, { 13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9 } },
            new byte[4, 16] { { 10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8 }, { 13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1 }, { 13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7 }, { 1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12 } },
            new byte[4, 16] { { 7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15 }, { 13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9 }, { 10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4 }, { 3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14 } },
            new byte[4, 16] { { 2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9 }, { 14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6 }, { 4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14 }, { 11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3 } },
            new byte[4, 16] { { 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 }, { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 }, { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 }, { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 } },
            new byte[4, 16] { { 4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1 }, { 13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6 }, { 1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2 }, { 6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12 } },
            new byte[4, 16] { { 13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7 }, { 1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2 }, { 7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8 }, { 2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11 } }
        };

        public byte[] Apply(byte[] right, byte[] roundKey)
        {
            // 1. Expansion E (32 -> 48 bits)
            byte[] expanded = BitUtils.Permute(right, E, BitUtils.BitIndexing.MsbFirst, 1);

            // 2. XOR with round key
            for (int i = 0; i < expanded.Length; i++) expanded[i] ^= roundKey[i];

            // 3. Substitution S-boxes (48 -> 32 bits)
            byte[] substituted = new byte[4];
            for (int i = 0; i < 8; i++)
            {
                // Each S-box takes 6 bits
                int bitStart = i * 6;
                int row = (BitUtils.GetBit(expanded, bitStart, BitUtils.BitIndexing.MsbFirst) ? 2 : 0) +
                          (BitUtils.GetBit(expanded, bitStart + 5, BitUtils.BitIndexing.MsbFirst) ? 1 : 0);
                int col = (BitUtils.GetBit(expanded, bitStart + 1, BitUtils.BitIndexing.MsbFirst) ? 8 : 0) +
                          (BitUtils.GetBit(expanded, bitStart + 2, BitUtils.BitIndexing.MsbFirst) ? 4 : 0) +
                          (BitUtils.GetBit(expanded, bitStart + 3, BitUtils.BitIndexing.MsbFirst) ? 2 : 0) +
                          (BitUtils.GetBit(expanded, bitStart + 4, BitUtils.BitIndexing.MsbFirst) ? 1 : 0);
                
                byte val = S[i][row, col];
                // Put 4 bits into substituted array
                for (int b = 0; b < 4; b++)
                {
                    bool bitVal = (val & (8 >> b)) != 0;
                    BitUtils.SetBit(substituted, i * 4 + b, bitVal, BitUtils.BitIndexing.MsbFirst);
                }
            }

            // 4. Permutation P
            return BitUtils.Permute(substituted, P, BitUtils.BitIndexing.MsbFirst, 1);
        }
    }

    public class DESKeyExpansion : IKeyExpansion
    {
        private static readonly int[] PC1 = {
            57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15, 7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29, 21, 13, 5, 28, 20, 12, 4
        };

        private static readonly int[] PC2 = {
            14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4,
            26, 8, 16, 7, 27, 20, 13, 2, 41, 52, 31, 37, 47, 55, 30, 40,
            51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
        };

        private static readonly int[] SHIFTS = { 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1 };

        public byte[][] ExpandKey(byte[] masterKey)
        {
            byte[] pc1Key = BitUtils.Permute(masterKey, PC1, BitUtils.BitIndexing.MsbFirst, 1);
            
            // Split into C0, D0 (28 bits each)
            // Representing 28 bits in bytes is tricky, we'll use bit-by-bit
            bool[] c = new bool[28];
            bool[] d = new bool[28];
            for (int i = 0; i < 28; i++)
            {
                c[i] = BitUtils.GetBit(pc1Key, i, BitUtils.BitIndexing.MsbFirst);
                d[i] = BitUtils.GetBit(pc1Key, i + 28, BitUtils.BitIndexing.MsbFirst);
            }

            byte[][] roundKeys = new byte[16][];
            for (int i = 0; i < 16; i++)
            {
                // Shift
                for (int s = 0; s < SHIFTS[i]; s++)
                {
                    bool c0 = c[0]; Array.Copy(c, 1, c, 0, 27); c[27] = c0;
                    bool d0 = d[0]; Array.Copy(d, 1, d, 0, 27); d[27] = d0;
                }

                // Combine and PC2
                byte[] combined = new byte[7]; // 56 bits
                for (int j = 0; j < 28; j++)
                {
                    BitUtils.SetBit(combined, j, c[j], BitUtils.BitIndexing.MsbFirst);
                    BitUtils.SetBit(combined, j + 28, d[j], BitUtils.BitIndexing.MsbFirst);
                }
                roundKeys[i] = BitUtils.Permute(combined, PC2, BitUtils.BitIndexing.MsbFirst, 1);
            }
            return roundKeys;
        }
    }

    public class DESAlgorithm : FeistelNetwork
    {
        private static readonly int[] IP = {
            58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1, 59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7
        };

        private static readonly int[] FP = {
            40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25
        };

        public DESAlgorithm() : base(new DESKeyExpansion(), new DESRoundFunction()) { }

        public override byte[] EncryptBlock(byte[] block)
        {
            byte[] permuted = BitUtils.Permute(block, IP, BitUtils.BitIndexing.MsbFirst, 1);
            byte[] processed = base.EncryptBlock(permuted);
            return BitUtils.Permute(processed, FP, BitUtils.BitIndexing.MsbFirst, 1);
        }

        public override byte[] DecryptBlock(byte[] block)
        {
            byte[] permuted = BitUtils.Permute(block, IP, BitUtils.BitIndexing.MsbFirst, 1);
            byte[] processed = base.DecryptBlock(permuted);
            return BitUtils.Permute(processed, FP, BitUtils.BitIndexing.MsbFirst, 1);
        }
    }
}
