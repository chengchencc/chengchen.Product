using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.Oracle.Repository;

namespace SubSonic.Core.Oracle.Test
{
    public class SimpleRepositoryTest
    {
        public void SimpleRepository_Add_test()
        {
            SimpleRepository repository = new SimpleRepository("ConnectionStringOracleGeneral", SimpleRepositoryOptions.RunMigrations);
            int result = repository.Add<GeneralUser>(new GeneralUser {
                Email= "3w@fd.com",
                Password = "12341234",
                PhoneNumber = "136487979797"
            });

            var id = repository.NewAdd<GeneralUser>(new GeneralUser
            {
                Email = "3w@fd.com",
                Password = "12341234",
                PhoneNumber = "136487979797"
            });

            Console.WriteLine(id);
        }
    }
}
