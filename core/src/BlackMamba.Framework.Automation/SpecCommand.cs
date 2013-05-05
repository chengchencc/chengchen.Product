using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Sdk;
using Watcher.Core.Entity;
using StructureMap;
using Watcher.Core;
using BlackMamba.Framework.Core;
using Watcher.Core.Notify;

namespace BlackMamba.Framework.Automation
{
    public class SpecCommand : TestCommand
    {
        public bool LiveOnly { get; set; }

        public SpecCommand(IMethodInfo method, bool liveOnly)
            : base(method, MethodUtility.GetDisplayName(method), MethodUtility.GetTimeoutParameter(method))
        {
            this.LiveOnly = liveOnly;
        }

        public SpecCommand(IMethodInfo method)
            : this(method, false)
        {
        }

        public override MethodResult Execute(object testClass)
        {
            try
            {
                var currentMode = RegistryModeFactory.GetCurrentMode();

                var shouldProcess = true;

                if (LiveOnly && currentMode != RegistryMode.Live) { shouldProcess = false; }

                if (shouldProcess)
                {
                    var observer = ObjectFactory.GetInstance<IObserver>();
                    if (observer != null)
                    {
                        observer.PopAll();

                        var ret = testMethod.MethodInfo.Invoke(testClass, null);

                        var specItem = ret as SpecItem;
                        if (specItem == null) throw new InvalidOperationException("The function has no return type or the type is not SpecItem!");

                        specItem.ToWatchItem().RunTest(true);

                        if (observer.HasItem)
                        {
                            // send email
                            #region SendEmail
                            var notify = new MailNotify()
                            {
                                SMTP = "smtp.exmail.qq.com",
                                UserName = "alarm@nfungame.com",
                                Password = "4rfv%TGB",
                                Subject = "【自动化测试】接口测试异常报警",
                                MailTo = new string[] { "staff@nfungame.com" },
                                MailFrom = "alarm@nfungame.com"
                            };

                            var mailContent = new StringBuilder();
                            while (observer.HasItem)
                            {
                                var item = observer.Pop().Display();

                                mailContent.AppendLine(item);
                            }

                            notify.SendMail(mailContent.ToString());

                            #endregion

                            return new FailedResult(testMethod, new Exception(mailContent.ToString()), DisplayName);
                        }
                    }
                }
            }
            catch (ParameterCountMismatchException)
            {
                throw new InvalidOperationException("Fact method " + TypeName + "." + MethodName + " cannot have parameters");
            }

            return new PassedResult(testMethod, DisplayName);
        }
    }
}
