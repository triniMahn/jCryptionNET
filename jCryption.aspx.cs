using System;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace jCryptionNET
{
    #region Typed Response Wrapper Classes
    //I like using these instead of anonymous objects, for it makes code more verbose and easier for others to understand.

    public class RSAPublicKey
    {
        public string publickey { get; set; }
    }

    public class RSAPrivateKey
    {
        public string privatekey { get; set; }
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

    #endregion

    public partial class jCryption : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected static EncryptedChallenge performHandshake(string key)
        {
            HttpRequest request = HttpContext.Current.Request;
            
            //If you have access to shared, in-memory storage (i.e. session/memcached), or a DB,
            //you should store the client key there
            //var session = HttpContext.Current.Session;

            //Decrypt the client's request
            string decryptedClientKey = lib.openSSLFacade.openSSLRSADecrypt(key);

            //Would store it in the session when running this locally, but 
            //I don't have access to this on Azure.
            //session["key"] = decryptedClientKey;

            //This is definitely not the most secure, or efficient way to store the client key,
            //but again, I have no access to in-memory storage on this server.
            lib.Utils.setCookie("clientAESKey", decryptedClientKey);

            string challengeStr = createChallenge(decryptedClientKey);
            EncryptedChallenge ec = new EncryptedChallenge { challenge =  challengeStr};

            return ec;
        }

        protected static string createChallenge(string decryptedClientKey)
        {
            //To verify that the server has received the correct key from the client,
            //we'll encrypt the key with itself and send it back to the browser
            string challenge = lib.openSSLFacade.openSSLAESEncrypt(decryptedClientKey, decryptedClientKey);

            return challenge;
        }

        #region Web Methods
        /*
         *  From jCryption.org:
         *  1. Client requests RSA public key from server
         *  2. Client encrypts a randomly generated key with the RSA public key
         *  3. Server decrypts key with the RSA private key and stores it in the session
         *  4. Server encrypts the decrypted key with AES and sends it back to the client
         *  5. Client decrypts it with AES, if the key matches the client is in sync with the server and is ready to go
         *  6. Everything else is encrypted using AES
         */

        /// <summary>
        /// From step 1, client side requests the server's RSA public key 
        /// </summary>
        /// <returns></returns>
        [WebMethod()]
        [ScriptMethod(UseHttpGet = true)]
        public static RSAPublicKey getPublicKey()
        {
            return lib.Keys.getPublicKey();
        }

        /// <summary>
        /// Accepts the randomly generated key from the browser (JS) via an async req,
        /// decrypts with the server's RSA private key, re-encrypts the key with itself as the 
        /// key and the AES algorithm, and sends it back to the client.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //[WebMethod(EnableSession=true)] //Un-comment if you want to use the session to store the client's key.
        [WebMethod()]
        public static EncryptedChallenge handshake(string key)
        {
            return performHandshake(key);
        }
        
        //[WebMethod(EnableSession=true)]
        [WebMethod()]
        public static DecryptTestResponse decryptTest()
        {
            //var session = HttpContext.Current.Session;
            string encrypted = null;
            string toEncrypt = DateTime.Now.ToUniversalTime().ToShortDateString();
            
            //string clientKey = (string)session["key"];
            string clientKey = lib.Utils.getCookie("clientAESKey");

            encrypted = lib.openSSLFacade.openSSLAESEncrypt(toEncrypt, clientKey);

            return new DecryptTestResponse { encrypted = encrypted, unencrypted = toEncrypt };
        }

        /// <summary>
        /// Decrypt data sent from the browser and echo it back to demonstrate that we can decrypt using the shared key
        /// </summary>
        /// <param name="encryptedStr"></param>
        /// <returns></returns>
        //[WebMethod(EnableSession = true)]
        [WebMethod()]
        public static DecryptResponse decrypt(string encryptedStr)
        {
            string data = null;
            //var session = HttpContext.Current.Session;
            
            //string clientKey = (string)session["key"];
            string clientKey = lib.Utils.getCookie("clientAESKey");

            data = lib.openSSLFacade.openSSLAESDecryptGetString(clientKey, encryptedStr);

            return new DecryptResponse { data = data };
        }

        #endregion

    }



}