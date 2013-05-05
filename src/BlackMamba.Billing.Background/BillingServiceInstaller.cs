using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace BlackMamba.Billing.Background
{
    [RunInstaller(true)]
    public partial class BillingServiceInstaller : System.Configuration.Install.Installer
    {
        public BillingServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
