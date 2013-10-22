using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jCryptionNET.lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace jCryptionNET.lib.Tests
{
    [TestClass()]
    public class openSSLFacadeTests
    {
        protected static string clientKey = "mypassword2013";
        protected static readonly string data = "The quick brown fox jumped over the lazy dog";

        [TestMethod()]
        public void openSSLAESEncryptTest()
        {
            string encrypted = openSSLFacade.openSSLAESEncrypt(data, clientKey);
            string decrypted = openSSLFacade.openSSLAESDecryptGetString(clientKey, encrypted);

            Assert.AreEqual(data, decrypted);
        }

        [TestMethod()]
        public void openSSLRSAEncryptTest()
        {
            string encrypted = openSSLFacade.openSSLRSAEncrypt(data);
            string decrypted = openSSLFacade.openSSLRSADecrypt(encrypted);

            Assert.AreEqual(data, decrypted);
        }
    }
}
