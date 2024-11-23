using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using System.Text;

namespace mylibrary.Utility;

public class AESCryptography
{
    public static string Encrypt(string plainText)
    {
        byte[] encryptedBytes = null;
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] key = Encoding.UTF8.GetBytes("b14ca5898a4e4133bbce2ea2315a1916");
        byte[] iv = new byte[16];

        // Set up the encryption objects
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Encrypt the input plaintext using the AES algorithm
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string cipherText)
    {
        byte[] decryptedBytes = null;
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        byte[] key = Encoding.UTF8.GetBytes("b14ca5898a4e4133bbce2ea2315a1916");
        byte[] iv = new byte[16];

        // Set up the encryption objects
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream plainTextStream = new MemoryStream())
                    {
                        csDecrypt.CopyTo(plainTextStream);
                        decryptedBytes = plainTextStream.ToArray();
                    }
                }
            }
        }

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}

