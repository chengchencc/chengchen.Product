using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace BlackMamba.Framework.Core.Security
{
    public class MD5Signature : ISignature
    {
        public string Sign(SignatureContext context)
        {
            var ret = string.Empty;
            var source = new StringBuilder();
            source.Append(context.Secret);

            if (context.SortedQuery != null)
            {
                foreach (var key in context.SortedQuery.AllKeys)
                {
                    source.Append(key);
                    source.Append(context.SortedQuery[key]);
                }
            }

            using (MD5 md5Hash = MD5.Create())
            {
                ret = GetMd5Hash(context, md5Hash, source.ToString());
            }

            return ret;
        }

        static string GetMd5Hash(SignatureContext context, MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            var data = md5Hash.ComputeHash(context.Encoding.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sb = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sb.ToString();
        }
    }
}
