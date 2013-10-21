using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Services;
using System.Web.Services;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;
using OpenSSL.Crypto;

namespace jCryptionNET
{
    public class RSAPublicKey
    {
        public string publickey { get; set; }
    }
    public class RSAPublicPrivateKey
    {
        public string publicprivatekey { get; set; }
    }

    public class EncryptedChallenge
    {
        public string challenge { get; set; }
    }

    public class ClientKey
    {
        public string key { get; set; }
    }

    public class DecryptTestResponse
    {
        public string encrypted { get; set; }
        public string unencrypted { get; set; }
    }

    public class DecryptResponse
    {
        public string data { get; set; }
    }

    public class ClientFile
    {
        public string name { get; set; }
        public byte[] data { get; set; }
        public string type { get; set; }
    }

    public partial class jCryption : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            saveFile();
            if (string.IsNullOrEmpty(Request.QueryString["getFile"]) == false)
                outputFile();
        }

        protected void outputFile()
        {
            ClientFile file = (ClientFile)Session["clientFile"];
            

            if (file.type.Contains("image/"))
            {
                Response.ClearContent();
                Response.ContentType = file.type;
                Response.BinaryWrite(file.data);
                Response.End();
            }
            else
            {
                Response.AddHeader("Content-Disposition", "attachment; filename=" + file.name);
                Response.Charset = "UTF-8";
                Response.ContentType = file.type;

                Response.BinaryWrite(file.data);
                Response.End();
            }
        }
        
        protected void saveFile()
        {
            string contentType = Request.Form["conType"];
            if (string.IsNullOrEmpty(contentType))
                return;

            string encFileData = Request.Form["encryptedFileData"];
            
            string fileName = Request.Form["name"];
            string clientKey = (string)Session["key"];

            byte[] fileBytes = openSSLAESDecrypt(clientKey, encFileData);
            //byte[] fileBytes = file.InputStream.Read(//Encoding.Default.GetBytes(encFileData);

            Session["clientFile"] = new ClientFile{ name = fileName, data = fileBytes, type = contentType };

        }

        protected void testEncDec()
        {
            StringBuilder output = new StringBuilder();

            string testEncDec = "This is a sentence 2013 This is a sentence 2013 This is a sentence 2013 This is a sentence 2013 This is a sentence 2013";
            output.AppendLine("Orig: " + testEncDec);

            string enc = openSSLEncrypt(testEncDec);
            output.AppendLine("Encrypted: " + enc);

            string dec = openSSLDecrypt(enc);
            output.AppendLine("Decrypted: " + dec);

            Response.Write(output.ToString());
        }

        protected static string readFromFile(string fileName)
        {
            string text = System.IO.File.ReadAllText(fileName);
            return text;
        }

        public static RSAPublicKey getPublicKey2()
        {
            string pubKey = readFromFile("rsa_1024_pub.pem");
            RSAPublicKey p = new RSAPublicKey { publickey = pubKey };
            //string json = new JavaScriptSerializer().Serialize(p);
            return p;
        }

        protected static EncryptedChallenge performHandshake(string key)
        {
            HttpRequest request = HttpContext.Current.Request;
            var session = HttpContext.Current.Session;

            string clientKey = key;//request.Form["key"];

            //Decrypt the client's request
            string decryptedClientKey = openSSLDecrypt(clientKey);

            session["key"] = decryptedClientKey;

            EncryptedChallenge ec = new EncryptedChallenge { challenge = createChallenge(decryptedClientKey) };

            return ec;//new JavaScriptSerializer().Serialize(ec);
        }

        protected static string createChallenge(string decryptedClientKey)
        {
            string challenge = openSSLAESEncrypt(decryptedClientKey, decryptedClientKey);

            return challenge;
        }

        protected static string openSSLAESEncrypt2(string data, string password)
        {
            CipherContext cc = new OpenSSL.Crypto.CipherContext(Cipher.AES_256_CBC);
            string encrypted = null;
            //Just random 8 bytes for salt
            var salt = new Byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (cc)
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] iv;
                byte[] key = cc.BytesToKey(MessageDigest.MD5, salt, passwordBytes, 1, out iv);
                byte[] encryptedData = cc.Encrypt(dataBytes, key, iv);
                encrypted = byteArrayToString(encryptedData);
            }
            return encrypted;
        }


        public static string openSSLAESDecryptGetString(string password, string encryptedData)
        {
            string result = Encoding.Default.GetString(openSSLAESDecrypt(password,encryptedData));
            return result;
        }
        
        //From: http://stackoverflow.com/questions/2201631/how-do-i-use-the-openssl-net-c-sharp-wrapper-to-encrypt-a-string-with-aes
        public static byte[] openSSLAESDecrypt(string password, string encryptedData)
        {
            byte[] salt = null;
            byte[] encryptedDataBytes = Convert.FromBase64String(encryptedData);
            byte [] result = null;

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

        protected static string openSSLAESEncrypt(string data, string password)
        {
            //Just random 8 bytes for salt
            var salt = new Byte[8]; //{ 1, 2, 3, 4, 5, 6, 7, 8 };
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

        protected static string openSSLEncrypt(string data)
        {
            //Process proc = new Process();
            //ProcessStartInfo startInfo = new ProcessStartInfo{
            //    FileName = "
            //}

            //http://stackoverflow.com/questions/7234155/decrypting-rsa-using-openssl-net-with-existing-key
            string encryptedString = null;
            RSAPublicKey pubKey = getPublicKey2();
            CryptoKey ck = OpenSSL.Crypto.CryptoKey.FromPublicKey(pubKey.publickey, null);
            OpenSSL.Crypto.RSA rsa = ck.GetRSA();
            using (rsa)
            {
                byte[] input = stringToByteArray(data);
                byte[] result = rsa.PublicEncrypt(input, OpenSSL.Crypto.RSA.Padding.None);
                encryptedString = byteArrayToString(result);
            }
            return encryptedString;
        }

        protected static string openSSLNETDecrypt(string encryptedStr)
        {
            X509Certificate2 cert = new X509Certificate2("jCryption_certificate.pfx", "JimBob*67", X509KeyStorageFlags.Exportable);
            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert.PrivateKey;

            byte[] res = rsa.Decrypt(stringToByteArray(encryptedStr), false);
            string decryptedString = byteArrayToString(res);

            return decryptedString;
        }

        protected static string openSSLCMDLineDecrypt(string encryptedStr)
        {
            string decryptedString = null;
            Process proc = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Users\R\Dropbox\Visual Studio 2012\Projects\jCryptionTest\jCryptionTest\bin\openssl.exe",
                Arguments = "rsautl -decrypt -inkey rsa_1024_priv.pem",
                RedirectStandardInput = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            proc.StartInfo = startInfo;
            proc.Start();
            //proc.StandardInput.Write("rsautl -decrypt -inkey rsa_1024_priv.pem");
            //proc.StandardInput.Write(encryptedStr);

            //while (!proc.StandardOutput.EndOfStream)
            //{
            //    decryptedString = proc.StandardOutput.ReadLine();
            //}
            proc.WaitForExit();
            string error = proc.StandardError.ReadToEnd();
            Debug.WriteLine(error);
            //decryptedString = proc.StandardOutput.ReadToEnd();

            //proc.WaitForExit();
            proc.Close();
            return decryptedString;
        }

        protected static string openSSLDecrypt(string encryptedStr)
        {

            //http://stackoverflow.com/questions/7234155/decrypting-rsa-using-openssl-net-with-existing-key
            string decryptedString = null;
            string privKey = readFromFile("rsa_1024_priv.pem");
            CryptoKey ck = OpenSSL.Crypto.CryptoKey.FromPrivateKey(privKey, null);
            OpenSSL.Crypto.RSA rsa = ck.GetRSA();
            using (rsa)
            {

                //This type of conversion seems to work better with openSSL decrypt -- get an error otherwise.
                byte[] encrypted = Convert.FromBase64String(encryptedStr);

                byte[] result = rsa.PrivateDecrypt(encrypted, OpenSSL.Crypto.RSA.Padding.PKCS1);
                decryptedString = byteArrayToString(result);

                Debug.WriteLine("****Decrypted String: " + decryptedString);
            }
            return decryptedString;
        }

        protected static byte[] stringToByteArray(string input)
        {
            //http://stackoverflow.com/questions/472906/net-string-to-byte-array-c-sharp
            //Second answer
            return System.Text.Encoding.Default.GetBytes(input);
            //return Convert.FromBase64String(input);
        }

        protected static string byteArrayToString(byte[] input)
        {
            return System.Text.Encoding.Default.GetString(input);
        }

        //public static RSAPublicKey getPublicKey()
        //{
        //    string publicKeyXML = null, pubPrivKeyXML;
        //    using (var rsa = new RSACryptoServiceProvider(1024))
        //    {
        //        try
        //        {
        //            // Do something with the key...
        //            // Encrypt, export, etc.
        //            //http://stackoverflow.com/questions/14047532/get-the-public-key-from-rsacryptoserviceprovider

        //            //http://stackoverflow.com/questions/3260319/interoperability-between-rsacryptoserviceprovider-and-openssl
        //            //http://stackoverflow.com/questions/4294689/how-to-generate-a-key-with-passphrase-from-the-command-line

        //            publicKeyXML = rsa.ToXmlString(false);
        //            pubPrivKeyXML = rsa.ToXmlString(true);

        //            //RSAParameters RSAParams = rsa.ExportParameters;


        //        }
        //        finally
        //        {
        //            rsa.PersistKeyInCsp = false;
        //        }

        //        RSAPublicKey pub = new RSAPublicKey { publickey = publicKeyXML };
        //        RSAPublicPrivateKey priv = new RSAPublicPrivateKey { publicprivatekey = pubPrivKeyXML };

        //        return pub;
        //    }
        //}

        [WebMethod()]
        [ScriptMethod(UseHttpGet = true)]
        public static RSAPublicKey getPublicKey()
        {
            return getPublicKey2();
        }

        [WebMethod(EnableSession=true)]
        public static EncryptedChallenge handshake(string key)
        {
            return performHandshake(key);
        }

        
        
        [WebMethod(EnableSession=true)]
        public static DecryptTestResponse decryptTest()
        {
            var session = HttpContext.Current.Session;
            string encrypted = null;
            string toEncrypt = DateTime.Now.ToUniversalTime().ToShortDateString();
            string clientKey = (string)session["key"];

            encrypted = openSSLAESEncrypt(toEncrypt, clientKey);

            return new DecryptTestResponse { encrypted = encrypted, unencrypted = toEncrypt };
        }

        [WebMethod(EnableSession = true)]
        public static DecryptResponse decrypt(string encryptedStr)
        {
            string data = null;
            var session = HttpContext.Current.Session;
            string clientKey = (string)session["key"];

            data = openSSLAESDecryptGetString(clientKey, encryptedStr);

            return new DecryptResponse { data = data };
        }
    }



}