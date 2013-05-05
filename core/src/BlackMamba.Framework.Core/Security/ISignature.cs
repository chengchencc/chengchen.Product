using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Core.Security
{
    public interface ISignature
    {
        string Sign(SignatureContext context);
    }
}
