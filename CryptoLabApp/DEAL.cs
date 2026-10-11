using System;
using CryptoLab.Interfaces;

namespace CryptoLab.Algorithms
{
    public class DESAdapter : IRoundFunction
    {
        private readonly ISymmetricAlgorithm _des;

        public DESAdapter(ISymmetricAlgorithm des)
        {
            _des = des;
        }

        public byte[] Apply(byte[] inputBlock, byte[] roundKey)
        {
            _des.SetKey(roundKey);
            // DEAL's round function F takes Ri-1 and Ki and produces a 64-bit value to XOR with Li-1
            // Here inputBlock is 64-bit Ri-1, and we return its DES encryption with roundKey Ki
            return _des.EncryptBlock(inputBlock);
        }
    }

    public class DEALKeyExpansion : IKeyExpansion
    {
        private readonly int _rounds;
        public DEALKeyExpansion(int rounds) { _rounds = rounds; }

        public byte[][] ExpandKey(byte[] masterKey)
        {
            // Simplified DEAL key expansion (actual DEAL-128/192/256 varies)
            // For DEAL-128 (128-bit key), it generates 6 or 8 round keys of 64-bit each.
            // We'll implement a basic version where we split the key or derive from it.
            byte[][] roundKeys = new byte[_rounds][];
            for (int i = 0; i < _rounds; i++)
            {
                roundKeys[i] = new byte[8];
                for (int j = 0; j < 8; j++)
                    roundKeys[i][j] = masterKey[(i * 8 + j) % masterKey.Length];
            }
            return roundKeys;
        }
    }

    public class DEALAlgorithm : FeistelNetwork
    {
        // DEAL is a Feistel network where the block size is 128 bits (two 64-bit halves)
        // and the round function is DES.
        public new int BlockSize => 16; 

        public DEALAlgorithm(int rounds = 6) 
            : base(new DEALKeyExpansion(rounds), new DESAdapter(new DESAlgorithm())) 
        {
        }

        // We need to override because the base FeistelNetwork assumes block.Length / 2
        // which works, but we should ensure consistency with 128-bit blocks.
    }
}
