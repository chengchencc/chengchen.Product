using System;
using SubSonic.Oracle.SqlGeneration.Schema;
using BlackMamba.Framework.SubSonic.Oracle;

namespace BlackMamba.Billing.Models.Payments
{
    [SubSonicTableNameOverride("FIN_ACCOUNT")]
    public class FinancialAccount : EntityBase
    {
        [SubSonicIndexMapping]
        public int CustomerId { get; set; }

        public float Balance { get; set; }

    }
}
