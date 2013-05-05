using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.Core;
using System.Security.Cryptography;

namespace BlackMamba.Framework.Core.Security
{
    public class SHA1Signature : ISignature
    {
        public string Sign(SignatureContext context)
        {
            var hmac = new HMACSHA1();
            var secret = context.Secret.UrlEncode();

            if (secret == null) secret = Guid.NewGuid().ToString();
            hmac.Key = context.Encoding.GetBytes(secret);


            var source = new StringBuilder();
            if (context.SortedQuery != null)
            {
                // append path
                foreach (var key in context.SortedQuery.AllKeys)
                {
                    source.Append(key);
                    source.Append(context.SortedQuery[key]);
                }
            }

            var buffer = context.Encoding.GetBytes(source.ToString());

            var signBytes = hmac.ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (byte b in signBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
