using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Core.Security
{
    public class SignatureFactory
    {
        public static ISignature GetSignature(SignatureMethod method)
        {
            var sig = default(ISignature);
            switch (method)
            {
                case SignatureMethod.HMAC_SHA1:
                    sig= new SHA1Signature();
                    break;
                case SignatureMethod.MD5:
                default:
                    sig= new MD5Signature();
                    break;
            }

            return sig;
        }
    }
}
