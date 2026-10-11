namespace CryptoLab.Interfaces
{
    public interface IKeyExpansion
    {
        byte[][] ExpandKey(byte[] masterKey);
    }

    public interface IRoundFunction
    {
        byte[] Apply(byte[] inputBlock, byte[] roundKey);
    }

    public interface ISymmetricAlgorithm
    {
        void SetKey(byte[] key);
        byte[] EncryptBlock(byte[] block);
        byte[] DecryptBlock(byte[] block);
        int BlockSize { get; }
        int KeySize { get; }
    }
}
