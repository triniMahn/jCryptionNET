using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics;
using System.IO;

using OpenSSL.Crypto;

namespace jCryptionNET.lib
{
    public class openSSLFacade
    {

        public static string openSSLAESEncrypt(string data, string password)
        {
            //With help from: http://stackoverflow.com/questions/2201631/how-do-i-use-the-openssl-net-c-sharp-wrapper-to-encrypt-a-string-with-aes
            
            var salt = new Byte[8]; 
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            string result = null;

            using (var cc = new CipherContext(Cipher.AES_256_CBC))
            {
                //Constructing key and init vector from string password
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] iv;
                byte[] key = cc.BytesToKey(MessageDigest.MD5, salt, passwordBytes, 1, out iv);

                var memoryStream = new MemoryStream();

                //Performing encryption thru unmanaged wrapper
                byte[] aesData = cc.Crypt(dataBytes, key, iv, true);

                //Append salt so final data will look Salted___SALT|RESTOFTHEDATA
                memoryStream.Write(Encoding.Default.GetBytes("Salted__"), 0, 8);
                memoryStream.Write(salt, 0, 8);
                memoryStream.Write(aesData, 0, aesData.Length);

                result = Convert.ToBase64String(memoryStream.ToArray());
            }
            return result;
        }
        
        public static string openSSLAESDecryptGetString(string password, string encryptedData)
        {
            string result = Encoding.Default.GetString(openSSLAESDecrypt(password, encryptedData));
            return result;
        }

        //From: http://stackoverflow.com/questions/2201631/how-do-i-use-the-openssl-net-c-sharp-wrapper-to-encrypt-a-string-with-aes
        public static byte[] openSSLAESDecrypt(string password, string encryptedData)
        {
            byte[] salt = null;
            byte[] encryptedDataBytes = Convert.FromBase64String(encryptedData);
            byte[] result = null;

            //extracting salt if presented
            if (encryptedDataBytes.Length > 16)
            {
                if (Encoding.UTF8.GetString(encryptedDataBytes).StartsWith("Salted__"))
                {
                    salt = new Byte[8];
                    System.Buffer.BlockCopy(encryptedDataBytes, 8, salt, 0, 8);
                }
            }

            //Removing salt from the original array
            int aesDataLength = encryptedDataBytes.Length - 16;
            byte[] aesData = new byte[aesDataLength];
            System.Buffer.BlockCopy(encryptedDataBytes, 16, aesData, 0, aesDataLength);


            using (var cc = new CipherContext(Cipher.AES_256_CBC))
            {
                //Constructing key and init vector from string password and salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] iv;
                byte[] key = cc.BytesToKey(MessageDigest.MD5, salt, passwordBytes, 1, out iv);

                //Decrypting
                result = cc.Decrypt(aesData, key, iv, 0);

            }

            return result;
        }


        public static string openSSLRSAEncrypt(string data)
        {
            //With help from: http://stackoverflow.com/questions/7234155/decrypting-rsa-using-openssl-net-with-existing-key
            OpenSSL.Crypto.RSA rsa = null;
            string encryptedString = null;
            RSAPublicKey pubKey = lib.Keys.getPublicKey();

            CryptoKey ck = OpenSSL.Crypto.CryptoKey.FromPublicKey(pubKey.publickey, null);

            using (rsa = ck.GetRSA())
            {
                byte[] input = lib.Utils.stringToByteArray(data);
                byte[] result = rsa.PublicEncrypt(input, OpenSSL.Crypto.RSA.Padding.PKCS1);
                encryptedString = lib.Utils.byteArrayToString(result);
            }

            return encryptedString;
        }

        public static string openSSLRSADecrypt(string encryptedStr)
        {
            //With help from: http://stackoverflow.com/questions/7234155/decrypting-rsa-using-openssl-net-with-existing-key

            string decryptedString = null;
            OpenSSL.Crypto.RSA rsa = null;

            RSAPrivateKey privKey = lib.Keys.getPrivateKey();
            CryptoKey ck = OpenSSL.Crypto.CryptoKey.FromPrivateKey(privKey.privatekey, null);

            using (rsa = ck.GetRSA())
            {
                //This type of conversion seems to work better with openSSL decrypt -- get an error otherwise.
                byte[] encrypted = Convert.FromBase64String(encryptedStr);

                byte[] result = rsa.PrivateDecrypt(encrypted, OpenSSL.Crypto.RSA.Padding.PKCS1);

                decryptedString = lib.Utils.byteArrayToString(result);

                Debug.WriteLine("****Decrypted String: " + decryptedString);
            }
            return decryptedString;
        }

    }
}