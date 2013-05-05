using System;
using System.Collections.Generic;
using BlackMamba.Framework.Core;

namespace BlackMamba.Billing.Domain
{
    public class MobileParam
    {
        #region consts
        public const string Key_IMSI = "imsi";
        public const string Key_IMEI = "imei";
        public const string Key_SMSCode = "smsc";
        public const string Key_Batch = "batch";
        public const string Key_DesignHouse = "dh";
        public const string Key_Manufacturer = "pf";
        public const string Key_BrandModel = "mpm";
        public const string Key_HardwareMode = "mod";
        public const string Key_LobbyVersion = "lbyver";
        public const string Key_DateOfProduction = "tm";
        public const string Key_Resolution = "lcd";
        public const string Key_Resolution_Old = "r";
        public const string Key_MCode = "mcode";
        public const string Key_SIMNo = "sim";
        public const string Key_HasTCard = "tcard";
        public const string Key_IsTouch = "touch";
        public const string Key_HasKeyboard = "kb";
        public const string Key_HasGravity = "gs";
        public const string Key_Capacitive = "cap";
        public const string Key_JavaInfo = "java";
        public const string Key_CInfo = "c";
        public const string Key_LuaInfo = "lua";
        public const string Key_LBS = "lbs";
        public const string Key_AppNo = "no";
        public const string Key_AppVer = "ver";
        public const string Key_ClientReleaseInfo = "ud";
        public const string Key_BillingConfigVersion = "bver";
        public const string Key_NetworkType = "nt";
        public const string Key_OS = "os";
        public const string Key_PhoneType = "pht";
        public const string Key_SoftwareVersion = "pver";
        public const string Key_IsTest = "istest";
        public const string Key_Encoding = "encoding";
        public const string Key_RAM = "ram";
        public const string Key_ROM = "rom";
        #endregion

        public MobileParam()
        {
        }

        public MobileParam(IRequestRepository requestRepo)
        {
            this._requestRepo = requestRepo;
        }

        protected IRequestRepository _requestRepo;

        protected Dictionary<string, string> _realValue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private bool _hasAddedToDic = false;

        /// <summary>
        /// 软件版本
        /// </summary>
        public virtual string SoftwareVersion { get { return this.GetValue(Key_SoftwareVersion); } }

        /// <summary>
        /// IMEI号
        /// </summary>
        public virtual string IMEI { get { return this.GetValue(Key_IMEI); } }

        /// <summary>
        /// IMSI号
        /// </summary>
        public virtual string IMSI { get { return this.GetValue(Key_IMSI); } }

        /// <summary>
        /// 短信中心号码
        /// </summary>
        public virtual string SMSCode { get { return this.GetValue(Key_SMSCode); } }

        /// <summary>
        /// 芯片类型
        /// </summary>
        public virtual string Batch { get { return this.GetValue(Key_Batch); } }

        /// <summary>
        /// 设计公司
        /// </summary>
        public virtual string DesignHouse { get { return this.GetValue(Key_DesignHouse); } }

        /// <summary>
        /// 生产厂商/渠道
        /// </summary>
        public virtual string Manufacturer { get { return this.GetValue(Key_Manufacturer); } }

        /// <summary>
        /// 品牌型号
        /// </summary>
        public virtual string BrandModel { get { return this.GetValue(Key_BrandModel); } }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public virtual string FirmwareMode { get { return this.GetValue(Key_HardwareMode); } }

        /// <summary>
        /// 大厅版本号
        /// </summary>
        public virtual string LobbyVersion { get { return this.GetValue(Key_LobbyVersion); } }

        /// <summary>
        /// 出厂日期
        /// e.g. 2012/2/9 8:03:00
        /// </summary>
        public virtual string DateOfProduction { get { return this.GetValue(Key_DateOfProduction); } }

        /// <summary>
        /// 分辨率
        /// </summary>
        public virtual string Resolution
        {
            get
            {
                var resolution = this.GetValue(Key_Resolution);

                // for old project which use r as the resolution key
                if (resolution.IsNullOrEmpty()) resolution = this.GetValue(Key_Resolution_Old);

                if (!resolution.IsNullOrEmpty())
                {
                    resolution = resolution.Replace(ASCII.COMMA, ASCII.MULTIPLY);
                }

                return resolution;
            }
        }

        /// <summary>
        /// 码机号
        /// </summary>
        public virtual string MCode { get { return this.GetValue(Key_MCode); } }

        /// <summary>
        /// 第几张SIM卡 
        /// </summary>
        public virtual string SIMNo { get { return this.GetValue(Key_SIMNo); } }

        /// <summary>
        /// 是否有T卡
        /// </summary>
        public virtual bool? HasTCard { get { return this.GetValue<bool>(Key_HasTCard); } }

        /// <summary>
        /// 是否带触屏
        /// </summary>
        public virtual bool? IsTouch { get { return this.GetValue<bool>(Key_IsTouch); } }

        /// <summary>
        /// 是否有键盘
        /// </summary>
        public virtual bool? HasKeyboard { get { return this.GetValue<bool>(Key_HasKeyboard); } }

        /// <summary>
        /// 是否带重力感应
        /// </summary>
        public virtual bool? HasGravity { get { return this.GetValue<bool>(Key_HasGravity); } }

        /// <summary>
        /// 是否带电容屏
        /// </summary>
        public virtual bool? HasCapacitive { get { return this.GetValue<bool>(Key_Capacitive); } }

        /// <summary>
        /// 是否源自测试
        /// </summary>
        public virtual bool? IsTest { get { return this.GetValue<bool>(Key_IsTest); } }

        /// <summary>
        /// 客户端系统内核
        /// </summary>
        public virtual string OS { get { return this.GetValue(Key_OS); } }

        /// <summary>
        /// 客户端类型
        /// 
        /// 此字段为batch的一个补充，当batch不能决定一款手机的时候，用这个字段来区分
        /// </summary>
        public virtual string PhoneType { get { return this.GetValue(Key_PhoneType); } }

        /// <summary>
        /// 当前网络连接类型
        /// 比如：wifi 3g 2g
        /// </summary>
        public virtual string NetworkType { get { return this.GetValue(Key_NetworkType); } }

        /// <summary>
        /// Java信息
        /// 是否有Java?Java的版本号，及Java的内存大小
        /// 1-2.0-24M
        /// 主要用其中的那个version
        /// </summary>
        public virtual string JavaInfo
        {
            get
            {
                var javainfo = this.GetValue(Key_JavaInfo);

                return GetPlatformVersion(javainfo);
            }
        }

        /// <summary>
        /// 是否有C?C的版本号，及C的内存大小
        /// 1-1000-24M
        /// 主要用其中的那个version
        /// </summary>
        public virtual string CInfo
        {
            get
            {
                var cinfo = this.GetValue(Key_CInfo);

                return GetPlatformVersion(cinfo);
            }
        }

        /// <summary>
        /// 是否有Lua?Lua的版本号，及Lua的内存大小
        /// 1-2.0-24M
        /// 主要用其中的那个version
        /// </summary>
        public virtual string LuaInfo
        {
            get
            {
                var luaInfo = this.GetValue(Key_LuaInfo);

                return GetPlatformVersion(luaInfo);
            }
        }

        /// <summary>
        /// 基站信息
        /// Location Based Service
        /// e.g. 460:00:14145:26494
        /// </summary>
        public virtual string LBS { get { return this.GetValue(Key_LBS); } }

        /// <summary>
        /// 应用编号
        /// 可以是来源一个区分
        /// 在log中这个字段被替换成FromAppNo 以示区分
        /// </summary>
        public virtual string AppNo { get { return this.GetValue(Key_AppNo); } }

        /// <summary>
        /// 计费配置版本
        /// </summary>
        public virtual string BillingConfigVersion { get { return this.GetValue(Key_BillingConfigVersion); } }

        /// <summary>
        /// 客户端发布版本号
        /// </summary>
        public virtual string ClientReleaseVersion
        {
            get
            {
                var clientReleaseInfo = this.GetValue(Key_ClientReleaseInfo);
                if (!clientReleaseInfo.IsNullOrEmpty() && clientReleaseInfo.Contains(ASCII.UNDERSCORE))
                {
                    return clientReleaseInfo.Split(ASCII.UNDERSCORE_CHAR)[0];
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 应用版本号
        /// </summary>
        public virtual string AppVersion { get { return this.GetValue(Key_AppVer); } }

        public virtual string RequestBody { get; set; }

        /// <summary>
        /// The encoding you want to use for content
        /// </summary>
        public virtual string Encoding { get { return this.GetValue(Key_Encoding); } }

        public virtual string RAM { get { return this.GetValue(Key_RAM); } }

        public virtual string ROM { get { return this.GetValue(Key_ROM); } }


        /// <summary>
        /// For Redis
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetRealValue(string key)
        {
            if (!_hasAddedToDic)
            {
                _realValue[Key_AppNo] = this.AppNo;
                _realValue[Key_IMSI] = this.IMSI;
                _realValue[Key_IMEI] = this.IMEI;
                _realValue[Key_SMSCode] = this.SMSCode;
                _realValue[Key_Batch] = this.Batch;
                _realValue[Key_DesignHouse] = this.DesignHouse;
                _realValue[Key_Manufacturer] = this.Manufacturer;
                _realValue[Key_BrandModel] = this.BrandModel;
                _realValue[Key_HardwareMode] = this.FirmwareMode;
                _realValue[Key_LobbyVersion] = this.LobbyVersion;
                _realValue[Key_DateOfProduction] = this.DateOfProduction;
                _realValue[Key_Resolution] = this.Resolution;
                _realValue[Key_MCode] = this.MCode;
                _realValue[Key_SIMNo] = this.SIMNo;
                _realValue[Key_HasTCard] = GetStringFromNullableBool(this.HasTCard);
                _realValue[Key_IsTouch] = GetStringFromNullableBool(this.IsTouch);
                _realValue[Key_HasKeyboard] = GetStringFromNullableBool(this.HasKeyboard);
                _realValue[Key_HasGravity] = GetStringFromNullableBool(this.HasGravity);
                _realValue[Key_Capacitive] = GetStringFromNullableBool(this.HasCapacitive);
                _realValue[Key_JavaInfo] = this.JavaInfo;
                _realValue[Key_CInfo] = this.CInfo;
                _realValue[Key_LuaInfo] = this.LuaInfo;
                _realValue[Key_LBS] = this.LBS;
                _realValue[Key_AppVer] = this.AppVersion;
                _realValue[Key_ClientReleaseInfo] = this.ClientReleaseVersion;
                _realValue[Key_BillingConfigVersion] = this.BillingConfigVersion;
                _realValue[Key_OS] = this.OS;
                _realValue[Key_PhoneType] = this.PhoneType;
                _realValue[Key_NetworkType] = this.NetworkType;
                _realValue[Key_SoftwareVersion] = this.SoftwareVersion;
                _realValue[Key_IsTest] = GetStringFromNullableBool(this.IsTest);
                _realValue[Key_Encoding] = this.Encoding;
                _realValue[Key_RAM] = this.RAM;
                _realValue[Key_ROM] = this.ROM;

                _hasAddedToDic = true;
            }

            if (_realValue.ContainsKey(key))
            {
                return _realValue[key];
            }

            return string.Empty;
        }

        private string GetStringFromNullableBool(bool? bVal)
        {
            return bVal.HasValue ? bVal.GetValueOrDefault().ToInt32().ToString() : string.Empty;
        }

        public virtual string GetValue(string key)
        {
            if (_requestRepo == null) return null;
            if (key.IsNullOrEmpty()) return null;

            var headerValue = default(string);
            var queryStringValue = default(string);

            if (_requestRepo.Header != null)
            {
                headerValue = _requestRepo.Header[key];
            }

            if (headerValue.IsNullOrEmpty() && _requestRepo.QueryString != null)
            {
                queryStringValue = _requestRepo.QueryString[key];
                if (!queryStringValue.IsNullOrEmpty())
                    headerValue = queryStringValue;
            }

            return headerValue;
        }

        private Nullable<T> GetValue<T>(string key)
            where T : struct, IConvertible
        {
            if (_requestRepo == null || key.IsNullOrEmpty()) return null;

            var value = GetValue(key);
            if (value.IsNullOrEmpty()) return null;

            var ret = default(T);
            var type = typeof(T);

            if (type == typeof(Boolean)) // 0 - false, 1 - true
            {
                ret = (T)Convert.ChangeType(value.ToInt32().ToBoolean(), typeof(T));
            }

            return ret;
        }

        private List<string> _excludedPropertyNames = new List<string>();
        /// <summary>
        /// for Cache
        /// the excluded names would be ignored for cache key
        /// </summary>
        /// <param name="excludedPropertyNames"></param>
        public void SetExcludedPropertyNames(List<string> excludedPropertyNames)
        {
            _excludedPropertyNames = excludedPropertyNames;
        }

        public string GetPlatformVersion(string info)
        {
            if (!info.IsNullOrEmpty() && info.StartsWith("1"))
            {
                if (info.Split(ASCII.MINUS_CHAR).Length > 2)
                    return info.Split(ASCII.MINUS_CHAR)[1].TrimStart('y', 'l');
            }

            return string.Empty;
        }
    }
}
