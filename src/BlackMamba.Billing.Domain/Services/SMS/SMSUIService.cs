using BlackMamba.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.SubSonic.Oracle;
using BlackMamba.Billing.Domain.Common;
using SubSonic.Oracle.Repository;
using System.Text.RegularExpressions;
using BlackMamba.Framework.Core;
using BlackMamba.Billing.Models.Payments;
using System.Security.Cryptography;
using NLog;
using System.Xml;

namespace BlackMamba.Billing.Domain.Services.SMS
{
    public class SMSUIService: ISMSUIService
    {
        #region IDbContext
        public string ConnectionStringName
        {
            get
            {
                return ConnectionStrings.KEY_ORACLE_GENERAL;
            }
        }

        public IRepository DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = DbContextFactory.CreateSimpleRepository(this.ConnectionStringName);
                }

                return _dbContext;
            }
            internal set
            {
                _dbContext = value;
            }
        } private IRepository _dbContext;
        #endregion

        //public IRESTfulClient RESTfulCient { get; set; }
        //public IPaymentsService PaymentsService { get; set; }

        public SMSUIService()
        {
            //this.RESTfulCient = restfulCient;
            //PaymentsService = paymentsService;
        }



    }


}
