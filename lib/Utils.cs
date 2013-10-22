using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jCryptionNET.lib
{
    public class Utils
    {
        
        protected static string cookieEncryptionKey = "Th*quickbr&wnf%x";
        
        public static string readFromFile(string fileName)
        {
            string text = System.IO.File.ReadAllText(fileName);
            return text;
        }

        public static byte[] stringToByteArray(string input)
        {
            //http://stackoverflow.com/questions/472906/net-string-to-byte-array-c-sharp

            return System.Text.Encoding.Default.GetBytes(input);
        }

        public static string byteArrayToString(byte[] input)
        {
            return System.Text.Encoding.Default.GetString(input);
        }

        public static string getCookie(string key)
        {
            string output = null;
            HttpRequest request = HttpContext.Current.Request;
            output = request.Cookies[key].Value;
            output = openSSLFacade.openSSLAESDecryptGetString(cookieEncryptionKey, output);
            return output;
        }

        public static void setCookie(string key, string value)
        {
            HttpRequest request = HttpContext.Current.Request;
            value = openSSLFacade.openSSLAESEncrypt(value, cookieEncryptionKey);
            HttpCookie cookie = new HttpCookie(key, value);
            
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}