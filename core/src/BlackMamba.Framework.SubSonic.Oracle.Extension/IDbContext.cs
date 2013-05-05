using SubSonic.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackMamba.Framework.SubSonic.Oracle
{
    public interface IDbContext
    {
        string ConnectionStringName { get; }

        IRepository DbContext { get; }
    }
}
