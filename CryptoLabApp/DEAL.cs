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
            return _des.EncryptBlock(inputBlock);
        }
    }

    public class DEALKeyExpansion : IKeyExpansion
    {
        private readonly int _rounds;
        public DEALKeyExpansion(int rounds) { _rounds = rounds; }

        public byte[][] ExpandKey(byte[] masterKey)
        {
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
        public new int BlockSize => 16; 

        public DEALAlgorithm(int rounds = 6) 
            : base(new DEALKeyExpansion(rounds), new DESAdapter(new DESAlgorithm())) 
        {
        }
    }
}
