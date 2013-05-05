using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.Core.Security;

namespace BlackMamba.Framework.Core.Security
{
    public class UrlSignature : IUrlSignature
    {
        public UrlSignature(IRequestRepository requestRepository, Encoding encoding)
        {
            this.RequestRepository = requestRepository;
            this.Encoding = encoding;
            this.Context = new SignatureContext(RequestRepository, Encoding);
            this.Signature = SignatureFactory.GetSignature(Context.Method);
        }

        public IRequestRepository RequestRepository { get; set; }
        public Encoding Encoding { get; set; }
        public SignatureContext Context { get; set; }
        public ISignature Signature { get; set; }

        public bool IsValid()
        {
            var isValid = false;

            if (!Context.Secret.IsNullOrEmpty())
            {
                var signature = this.Sign(Context);

                if (signature.EqualsOrdinalIgnoreCase(Context.ClientSignature))
                {
                    isValid = true;
                }
            }

            return isValid;
        }


        public string Sign(SignatureContext context)
        {
            return this.Signature.Sign(context);
        }

        public string Sign()
        {
            return this.Signature.Sign(this.Context);
        }

    }
}
