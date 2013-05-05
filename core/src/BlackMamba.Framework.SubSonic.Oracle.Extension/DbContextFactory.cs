using SubSonic.Oracle.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.SubSonic.Oracle
{
    public class DbContextFactory
    {
        public static IRepository CreateSimpleRepository(string connectionStringName, SimpleRepositoryOptions option = SimpleRepositoryOptions.RunMigrations)
        {
            return new SimpleRepository(connectionStringName, option);
        }
    }
}
