using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jCryptionNET.lib
{
    public class Keys
    {
        private static readonly string publicKey = null;
        private static readonly string privateKey = null;

        static Keys()
        {
            publicKey = lib.Utils.readFromFile("rsa_1024_pub.pem");
            privateKey = lib.Utils.readFromFile("rsa_1024_priv.pem");
        }

        public static RSAPublicKey getPublicKey()
        {
            RSAPublicKey key = new RSAPublicKey { publickey = publicKey };
            return key;
        }

        public static RSAPrivateKey getPrivateKey()
        {
            RSAPrivateKey key = new RSAPrivateKey { privatekey = privateKey };
            return key;
        }
    }
}