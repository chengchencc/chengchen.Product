using BlackMamba.Framework.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Core.Security
{
    public interface IUrlSignature
    {
        bool IsValid();

        string Sign(SignatureContext context);
    }
}
