using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Watcher.Core;

namespace BlackMamba.Framework.Automation
{
    public class SpecificationBase : IUseFixture<SpecificationBase>, IXmlGeneratable
    {
        public void SetFixture(SpecificationBase data)
        {
            Bootstrapper.Start();
        }

        public virtual string ModuleName { get { return string.Empty; } }
    }
}
