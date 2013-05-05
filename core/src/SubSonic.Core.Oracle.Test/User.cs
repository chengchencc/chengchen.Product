using SubSonic.Oracle.SqlGeneration.Schema;
using System;

namespace SubSonic.Core.Oracle.Test
{
    [SubSonicTableNameOverride("GENERAL_USER")]
    public class GeneralUser
    {
        [SubSonicPrimaryKey]
        public int ID { get; set; }

        [SubSonicNullString]
        public string PhoneNumber { get; set; }

        [SubSonicNullString]
        public string Email { get; set; }

        public string Password { get; set; }

        private DateTime _createDate = DateTime.Now;
        public DateTime CreateDate
        {
            get 
            {
                return _createDate;
            }
            set
            {
                _createDate = value;
            }
        }
    }
}
