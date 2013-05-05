using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackMamba.Framework.Core;

namespace BlackMamba.Billing.Domain
{
    public class ActionRepository : SingletonBase<ActionRepository>
    {
        protected internal Dictionary<string, int> ControllerNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        protected internal Dictionary<string, int> ActionNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public ActionRepository()
        {
            AddControllerName();

            AddActionName();
        }

        private void AddControllerName()
        {
            ControllerNames["Appstores"] = 10;
            ControllerNames["OTA"] = 11;
            ControllerNames["Theme"] = 20;
            ControllerNames["Wallpaper"] = 21;
            ControllerNames["Ringstone"] = 22;
            ControllerNames["Font"] = 23;
            ControllerNames["Cloud"] = 30;
            ControllerNames["KuSix"] = 40;
            ControllerNames["GoldenEggs"] = 50;
            ControllerNames["KosMusic"] = 60;
            ControllerNames["UserInteract"] = 70;
            ControllerNames["Network"] = 80;
            ControllerNames["KosBook"] = 90;
            ControllerNames["Users"] = 100;
            ControllerNames["GameCenter"] = 110;
            ControllerNames["AVWiki"] = 120;
            ControllerNames["SMS"] = 130;
            ControllerNames["Yeepay"] = 140;
            ControllerNames["Order"] = 150;
            ControllerNames["Trace"] = 160;
            ControllerNames["AppStoresWapUI"] = 170;
            ControllerNames["Alipay"] = 180;
            ControllerNames["AppStoresWebUI"] = 190;
        }

        private void AddActionName()
        {
            ActionNames.Add("ApkDetail", 1);
            ActionNames.Add("ApkDownload", 2);
            ActionNames.Add("ApkUpdateList", 3);
            ActionNames.Add("AppDetail", 4);
            ActionNames.Add("AppList", 5);
            ActionNames.Add("BuildRedirectResult", 6);
            ActionNames.Add("CategoryList", 7);
            ActionNames.Add("ChangePassword", 8);
            ActionNames.Add("CheckAppList", 9);
            ActionNames.Add("CheckUpdate", 10);
            ActionNames.Add("Details", 11);
            ActionNames.Add("Download", 12);
            ActionNames.Add("DownloadAppImagePackage", 13);
            ActionNames.Add("DownloadSingleAppImage", 14);
            ActionNames.Add("Draw", 15);
            ActionNames.Add("DrawComplete", 16);
            ActionNames.Add("GetChannel", 17);
            ActionNames.Add("GetDownLoadUrl", 18);
            ActionNames.Add("GetHotMusicList", 19);
            ActionNames.Add("GetInfo", 20);
            ActionNames.Add("GetKeyList", 21);
            ActionNames.Add("GetKeyWord", 22);
            ActionNames.Add("GetNodeList", 23);
            ActionNames.Add("GetRankList", 24);
            ActionNames.Add("GetReco", 25);
            ActionNames.Add("GetSeed", 26);
            ActionNames.Add("GetUser", 27);
            ActionNames.Add("Index", 28);
            ActionNames.Add("LatestList", 29);
            ActionNames.Add("List", 30);
            ActionNames.Add("NewUser", 31);
            ActionNames.Add("PopularList", 32);
            ActionNames.Add("Search", 33);
            ActionNames.Add("ColumnList", 34);
            ActionNames.Add("UpdateList", 35);
            ActionNames.Add("Upload", 36);
            ActionNames.Add("AndroidUserInfoUpload", 37);
            ActionNames.Add("Push", 38);
            ActionNames.Add("EncTest", 39);
            ActionNames.Add("GetHotBookList", 40);
            ActionNames.Add("GetCollection", 41);
            ActionNames.Add("DownloadComplete", 42);
            ActionNames.Add("AwardCoin", 43);
            ActionNames.Add("GetAndroidChannel", 44);
            ActionNames.Add("GetAndroidTop", 45);
            ActionNames.Add("VerifyByIMSI", 46);
            ActionNames.Add("VerifyByMobile", 47);
            ActionNames.Add("RegisterUser", 48);
            ActionNames.Add("QuickRegisterUser", 49);
            ActionNames.Add("DemoUserRegister", 50);
            ActionNames.Add("BindCustomer", 51);
            ActionNames.Add("ResetPassword", 52);
            ActionNames.Add("LogOn", 53);
            ActionNames.Add("GetCustomerIdByIMSI", 54);
            ActionNames.Add("Register", 55);
            ActionNames.Add("GetCompleteList", 56);
            ActionNames.Add("GetKeywordList", 57);
            ActionNames.Add("Receive", 58);
            ActionNames.Add("ActivateUser", 59);
            ActionNames.Add("AllAnnouncements", 60);
            ActionNames.Add("AppListByTags", 61);
            ActionNames.Add("AnnouncementDetail", 62);
            ActionNames.Add("AppAnnouncements", 63);
            ActionNames.Add("GlobalAnnouncements", 64);
            ActionNames.Add("AnnouncementColumnList", 65);
            ActionNames.Add("TagGroups", 66);
            ActionNames.Add("Tags", 67);
            ActionNames.Add("ApkAnnouncements", 68);
            ActionNames.Add("AnnouncementApkColumnList", 69);
            ActionNames.Add("YeepayPayments", 70);
            ActionNames.Add("YeepayCallBack", 71);
            ActionNames.Add("GenerateOrder", 72);
            ActionNames.Add("Create", 73);
            ActionNames.Add("UpdateStatus", 74);
            ActionNames.Add("ReissueOrder", 75);
            ActionNames.Add("RefundMoney", 76);
            ActionNames.Add("AppListByTagIds", 77);
            ActionNames.Add("SearchAppListByName", 78);
            ActionNames.Add("Communication", 79);
            ActionNames.Add("Keywords", 80);
            ActionNames.Add("Startup", 81);
            ActionNames.Add("CloseApp", 82);
            ActionNames.Add("DownloadSuccess", 83);
            ActionNames.Add("SubjectList", 84);
            ActionNames.Add("YeepayCardPayments", 85);
            ActionNames.Add("YeepayCardCallBack", 86);
            ActionNames.Add("CardPay", 87);
            ActionNames.Add("Notify", 88);
            //ActionNames.Add("YeepaySDKCallBack", 89);
            ActionNames.Add("YeepayPayment", 90);
            ActionNames.Add("BankCallBack", 91);
            ActionNames.Add("WapPay", 92);
            ActionNames.Add("WapNotify", 93);
            ActionNames.Add("WapCallback", 94);
            ActionNames.Add("WapCancel", 95);
            ActionNames.Add("TestWapPay", 96);
            ActionNames.Add("GetAppListByTagId", 97);
            ActionNames.Add("SearchForMarket31Stat", 98);
            ActionNames.Add("UpdateKeywordForApp", 99);
            ActionNames.Add("YouleAppStat", 100);
            ActionNames.Add("SearchGame", 101);
            ActionNames.Add("CheckPaymentStatus", 102);
        }

        /// <summary>
        /// Get Action name id
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public int? this[string actionName]
        {
            get
            {
                if (ActionNames.ContainsKey(actionName))
                {
                    return ActionNames[actionName];
                }
                return null;
            }
        }

        public int? GetActionId(string controllerName, string actionName)
        {
            var ret = default(int?);
            var controllerId = 0;
            if (ControllerNames.ContainsKey(controllerName))
            {
                controllerId = ControllerNames[controllerName];
                var actionId = this[actionName];

                if (actionId.HasValue) ret = controllerId * 1000 + actionId;

            }

            return ret;
        }


        /// <summary>
        /// Return controller/action
        /// </summary>
        /// <param name="id"></param>
        /// <returns>controller/action</returns>
        public string GetDescription(int id)
        {
            var name = string.Empty;
            var actionId = id % 1000;
            var controllerId = id / 1000;

            if (ControllerNames.ContainsValue(controllerId) && ActionNames.ContainsValue(actionId))
            {
                name = string.Format("{0}/{1}",
                    ControllerNames.First(c => c.Value == controllerId).Key,
                    ActionNames.First(s => s.Value == actionId).Key);
            }

            return name;
        }

    }
}
