using System;
using System.IO;
using System.Security.Cryptography;


namespace AMStock.Core.Models.Common
{
    public class Crypto
    {
        #region Ecrypt/Decrypt Strings

        // Encrypt a byte array into a byte array using a key and an IV 
        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                Rijndael alg = Rijndael.Create();
                alg.Key = Key;
                alg.IV = IV;

                CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(clearData, 0, clearData.Length);
                cs.Close();

                byte[] encryptedData = ms.ToArray();

                return encryptedData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string EncryptString(string clearText)
        {
            return EncryptString(clearText, "");//ConfigManager.Get("CryptoPassword")
        }

        public static string EncryptString(string clearText, string password)
        {
            try
            {
                byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);

                PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));

                return Convert.ToBase64String(encryptedData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                Rijndael alg = Rijndael.Create();
                alg.Key = Key;
                alg.IV = IV;

                CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();

                byte[] decryptedData = ms.ToArray();

                return decryptedData;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string DecryptString(string clearText)
        {
            return DecryptString(clearText, "");//ConfigManager.Get("CryptoPassword")
        }

        public static string DecryptString(string encryptedText, string password)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(encryptedText);

                PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsNullOrEmpty(string encryptedText, string password)
        {
            if (String.IsNullOrEmpty(encryptedText))
                return true;

            string result = DecryptString(encryptedText, password);
            if (String.IsNullOrEmpty(result))
                return true;

            return false;
        }

        #endregion
    }
}
