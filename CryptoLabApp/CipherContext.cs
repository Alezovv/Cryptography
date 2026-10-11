using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CryptoLab.Interfaces;

namespace CryptoLab.Common
{
    public class CipherContext
    {
        private readonly ISymmetricAlgorithm _algorithm;
        private readonly CipherMode _mode;
        private readonly PaddingMode _padding;
        private readonly byte[] _iv;
        private readonly object[] _extraArgs;

        public CipherContext(
            ISymmetricAlgorithm algorithm,
            byte[] key,
            CipherMode mode,
            PaddingMode padding,
            byte[] iv = null,
            params object[] extraArgs)
        {
            _algorithm = algorithm;
            _algorithm.SetKey(key);
            _mode = mode;
            _padding = padding;
            _iv = iv;
            _extraArgs = extraArgs;
        }

        public async Task<byte[]> EncryptAsync(byte[] data)
        {
            return await Task.Run(() =>
            {
                byte[] paddedData = PaddingProvider.AddPadding(data, _algorithm.BlockSize, _padding);
                int blockSize = _algorithm.BlockSize;
                int blockCount = paddedData.Length / blockSize;
                byte[] result = new byte[paddedData.Length];

                if (_mode == CipherMode.ECB)
                {
                    Parallel.For(0, blockCount, i =>
                    {
                        byte[] block = new byte[blockSize];
                        Array.Copy(paddedData, i * blockSize, block, 0, blockSize);
                        byte[] encrypted = _algorithm.EncryptBlock(block);
                        Array.Copy(encrypted, 0, result, i * blockSize, blockSize);
                    });
                }
                else if (_mode == CipherMode.CBC)
                {
                    byte[] prevBlock = (byte[])_iv.Clone();
                    for (int i = 0; i < blockCount; i++)
                    {
                        byte[] block = new byte[blockSize];
                        Array.Copy(paddedData, i * blockSize, block, 0, blockSize);
                        
                        // XOR with prev
                        for (int j = 0; j < blockSize; j++) block[j] ^= prevBlock[j];
                        
                        byte[] encrypted = _algorithm.EncryptBlock(block);
                        Array.Copy(encrypted, 0, result, i * blockSize, blockSize);
                        prevBlock = encrypted;
                    }
                }
                else if (_mode == CipherMode.OFB)
                {
                    byte[] streamBlock = (byte[])_iv.Clone();
                    for (int i = 0; i < blockCount; i++)
                    {
                        streamBlock = _algorithm.EncryptBlock(streamBlock);
                        for (int j = 0; j < blockSize; j++)
                            result[i * blockSize + j] = (byte)(paddedData[i * blockSize + j] ^ streamBlock[j]);
                    }
                }
                else if (_mode == CipherMode.CTR)
                {
                    Parallel.For(0, blockCount, i =>
                    {
                        byte[] counterBlock = (byte[])_iv.Clone();
                        // Add i to counter (simplified)
                        counterBlock[blockSize - 1] = (byte)(counterBlock[blockSize - 1] + i);
                        byte[] keystream = _algorithm.EncryptBlock(counterBlock);
                        for (int j = 0; j < blockSize; j++)
                            result[i * blockSize + j] = (byte)(paddedData[i * blockSize + j] ^ keystream[j]);
                    });
                }
                // Add other modes as needed...
                else 
                {
                     throw new NotImplementedException($"Mode {_mode} not fully implemented in this demo.");
                }

                return result;
            });
        }

        public async Task<byte[]> DecryptAsync(byte[] data)
        {
            return await Task.Run(() =>
            {
                int blockSize = _algorithm.BlockSize;
                int blockCount = data.Length / blockSize;
                byte[] decryptedPadded = new byte[data.Length];

                if (_mode == CipherMode.ECB)
                {
                    Parallel.For(0, blockCount, i =>
                    {
                        byte[] block = new byte[blockSize];
                        Array.Copy(data, i * blockSize, block, 0, blockSize);
                        byte[] decrypted = _algorithm.DecryptBlock(block);
                        Array.Copy(decrypted, 0, decryptedPadded, i * blockSize, blockSize);
                    });
                }
                else if (_mode == CipherMode.CBC)
                {
                    byte[] prevBlock = (byte[])_iv.Clone();
                    for (int i = 0; i < blockCount; i++)
                    {
                        byte[] block = new byte[blockSize];
                        Array.Copy(data, i * blockSize, block, 0, blockSize);
                        
                        byte[] currentCipherBlock = (byte[])block.Clone();
                        byte[] decrypted = _algorithm.DecryptBlock(block);
                        
                        // XOR with prev
                        for (int j = 0; j < blockSize; j++) decrypted[j] ^= prevBlock[j];
                        
                        Array.Copy(decrypted, 0, decryptedPadded, i * blockSize, blockSize);
                        prevBlock = currentCipherBlock;
                    }
                }
                else if (_mode == CipherMode.OFB)
                {
                    // OFB encryption and decryption are identical
                    byte[] streamBlock = (byte[])_iv.Clone();
                    for (int i = 0; i < blockCount; i++)
                    {
                        streamBlock = _algorithm.EncryptBlock(streamBlock);
                        for (int j = 0; j < blockSize; j++)
                            decryptedPadded[i * blockSize + j] = (byte)(data[i * blockSize + j] ^ streamBlock[j]);
                    }
                }
                else if (_mode == CipherMode.CTR)
                {
                    // CTR encryption and decryption are identical
                    Parallel.For(0, blockCount, i =>
                    {
                        byte[] counterBlock = (byte[])_iv.Clone();
                        counterBlock[blockSize - 1] = (byte)(counterBlock[blockSize - 1] + i);
                        byte[] keystream = _algorithm.EncryptBlock(counterBlock);
                        for (int j = 0; j < blockSize; j++)
                            decryptedPadded[i * blockSize + j] = (byte)(data[i * blockSize + j] ^ keystream[j]);
                    });
                }
                else 
                {
                     throw new NotImplementedException($"Mode {_mode} not fully implemented.");
                }

                return PaddingProvider.RemovePadding(decryptedPadded, blockSize, _padding);
            });
        }

        public async Task ProcessFileAsync(string inputPath, string outputPath, bool encrypt)
        {
            byte[] inputData = await File.ReadAllBytesAsync(inputPath);
            byte[] resultData = encrypt ? await EncryptAsync(inputData) : await DecryptAsync(inputData);
            await File.WriteAllBytesAsync(outputPath, resultData);
        }
    }
}
