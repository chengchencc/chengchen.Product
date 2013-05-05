using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Watcher.Core;
using System.IO;
using Watcher.Core.Entity;
using System.Xml.Serialization;
//using BlackMamba.Framework.Core;

namespace BlackMamba.Framework.Automation
{
    public class XmlGenerator
    {
        public const string FILE_NAME = "integration_watch.xml";
        public void Do(string assemblyNamePath, Action<WatchConfig> setMailAction = null, Action<WatchConfig> setSMSAction = null, string fileName = "", bool removeUnwatchedItems = false)
        {
            if (assemblyNamePath.IsNullOrEmpty() || !File.Exists(assemblyNamePath)) return;

            var watchItems = GetWatchItems(GetWatchItemMethods(assemblyNamePath, removeUnwatchedItems));

            WatchConfig its = new WatchConfig();
            its.Items = new List<WatchItem>();
            its.Items.AddRange(watchItems);
            ConstructConfigFile(its);

            CustomMailSettings(setMailAction, its);
            CustomSMSSettings(setSMSAction, its);
            its.NotifyErrorOnly = true;

            if (fileName.IsNullOrEmpty()) fileName = FILE_NAME;

            using (TextWriter writer = new StreamWriter(fileName))
            {
                var xs = new XmlSerializer(typeof(WatchConfig));
                xs.Serialize(writer, its);
            }
        }

        private static void ConstructConfigFile(WatchConfig its)
        {
            its.MailSettings = new MailNotifySettings();

            if (BlackMamba.Framework.Core.ProjectConfigHelper.IsInLiveMode()) its.MailSettings.Enable = true;

            its.MailSettings.MailFrom = "dept.mis.bot@youleonline.com";
            its.MailSettings.MailTo = new string[] { "cuijy@youleonline.com", "jili@youleonline.com", "humy@youleonline.com", "huangchao@youleonline.com", "chengchen@youleonline.com", "cailj@youleonline.com", "zhangmx@youleonline.com" };

            its.MailSettings.Subject = "网络开发部 - 自动化集成测试报告";
            its.MailSettings.SMTP = "smtp.exmail.qq.com";
            its.MailSettings.UserName = "dept.mis.bot@youleonline.com";
            its.MailSettings.Password = "123456";

            its.SMSSettings = new SMSNotifySettings();
            its.SMSSettings.Enable = false;
            its.SMSSettings.Mobile = "13511111111";

            its.SMSSettings.Password = ""; //unknow
            its.SMSSettings.UserID = "statsmsget";
        }

        private static void CustomMailSettings(Action<WatchConfig> setMailAction, WatchConfig its)
        {
            if (setMailAction != null)
            {
                setMailAction(its);
            }
        }

        private static void CustomSMSSettings(Action<WatchConfig> setSMSAction, WatchConfig its)
        {
            if (setSMSAction != null)
            {
                setSMSAction(its);
            }
        }

        internal static List<MethodInfo> GetWatchItemMethods(string assemblyNamePath, bool removeUnwatchedItems = false)
        {
            var generatableTypes = new List<Type>();
            var specMethodInfos = new List<MethodInfo>();

            var assembly = Assembly.LoadFrom(assemblyNamePath);
            assembly.GetTypes().ToList().ForEach(s =>
            {
                if (s.IsXmlGeneratableType()) generatableTypes.Add(s);
            });

            foreach (var type in generatableTypes)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                if (!removeUnwatchedItems)
                {
                    var specMethods = methods.Where(s => s.GetCustomAttributes(typeof(SpecAttribute), true).Any());
                    specMethodInfos.AddRange(specMethods);
                }
                else
                {
                    foreach (var method in methods)
                    {
                        var customeAttributes = method.GetCustomAttributes(typeof(SpecAttribute), true);

                        if (customeAttributes.Length >= 1)
                        {
                            if ((customeAttributes[0] as SpecAttribute).UnderWatching)
                                specMethodInfos.Add(method);
                        }
                    }
                }
            }

            return specMethodInfos;
        }

        internal static List<WatchItem> GetWatchItems(List<MethodInfo> methods)
        {
            var watchItems = new List<WatchItem>();
            foreach (var item in methods)
            {
                var ret = item.Invoke(Activator.CreateInstance(item.ReflectedType), null);

                var spec = ret as SpecItem;
                if (spec != null)
                {
                    watchItems.Add(spec.ToWatchItem());
                }
            }

            return watchItems;
        }
    }
}
