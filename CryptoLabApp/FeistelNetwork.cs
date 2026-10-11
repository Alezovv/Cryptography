using System;
using CryptoLab.Interfaces;

namespace CryptoLab.Algorithms
{
    public class FeistelNetwork : ISymmetricAlgorithm
    {
        private readonly IKeyExpansion _keyExpansion;
        private readonly IRoundFunction _roundFunction;
        private byte[][] _roundKeys;
        public int BlockSize => 8; // Default for DES, but can be variable
        public int KeySize { get; protected set; }

        public FeistelNetwork(IKeyExpansion keyExpansion, IRoundFunction roundFunction)
        {
            _keyExpansion = keyExpansion;
            _roundFunction = roundFunction;
        }

        public void SetKey(byte[] key)
        {
            _roundKeys = _keyExpansion.ExpandKey(key);
            KeySize = key.Length;
        }

        public virtual byte[] EncryptBlock(byte[] block)
        {
            return Process(block, _roundKeys);
        }

        public virtual byte[] DecryptBlock(byte[] block)
        {
            var reversedKeys = (byte[][])_roundKeys.Clone();
            Array.Reverse(reversedKeys);
            return Process(block, reversedKeys);
        }

        private byte[] Process(byte[] block, byte[][] keys)
        {
            int halfSize = block.Length / 2;
            byte[] left = new byte[halfSize];
            byte[] right = new byte[halfSize];
            Array.Copy(block, 0, left, 0, halfSize);
            Array.Copy(block, halfSize, right, 0, halfSize);

            for (int i = 0; i < keys.Length; i++)
            {
                byte[] prevRight = (byte[])right.Clone();
                byte[] roundOut = _roundFunction.Apply(right, keys[i]);
                
                for (int j = 0; j < halfSize; j++)
                {
                    right[j] = (byte)(left[j] ^ roundOut[j]);
                }
                left = prevRight;
            }

            // Final swap is usually undone or kept depending on specific algorithm
            // DES has a final swap that effectively swaps them back.
            byte[] result = new byte[block.Length];
            Array.Copy(right, 0, result, 0, halfSize);
            Array.Copy(left, 0, result, halfSize, halfSize);
            return result;
        }
    }
}
