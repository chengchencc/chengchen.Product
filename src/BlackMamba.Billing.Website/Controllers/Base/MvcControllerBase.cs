using BlackMamba.Billing.Domain;
using BlackMamba.Billing.Domain.Common;
using BlackMamba.Billing.Domain.Helpers;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain.ViewModels;
using BlackMamba.Billing.Domain.ViewModels.Billing;
using BlackMamba.Billing.Models;
using BlackMamba.Framework.Core;
using BlackMamba.Framework.Core.Security;
using BlackMamba.Framework.RedisMapper;
using NLog;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BlackMamba.Billing.Website.Controllers
{
    public abstract class MvcControllerBase : Controller
    {
        const string SEQ_FOR_LOG = "SEQ:BillingLOG";

        #region Properties
        public virtual IRequestRepository RequestRepository
        {
            get
            {
                if (_requestRepository == null) _requestRepository = ObjectFactory.GetInstance<IRequestRepository>();

                return _requestRepository;
            }
            set { _requestRepository = value; }
        } private IRequestRepository _requestRepository;

        protected internal virtual IRedisService InternalRedisService
        {
            get
            {
                if (_redisService == null) _redisService = ObjectFactory.GetInstance<IRedisService>();
                return _redisService;
            }
            set { _redisService = value; }
        } private IRedisService _redisService;

        /// <summary>
        /// If set to true, the response will be stream
        /// Else it is a normal action result
        /// </summary>
        protected internal virtual bool IsMobileInterface { get { return true; } }

        protected internal virtual bool ShouldCheckSignature { get { return false; } }

        protected internal virtual bool IsWriteActionLog { get { return true; } }

        #endregion

        #region Override
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            #region Old mobile interface

            if (IsMobileInterface)
            {
                //var mobileParam = this.GetMobileParam();

                //AddCustomResponseHeader(filterContext, mobileParam);

                //Response.ContentEncoding = Encoding.UTF8;

                //var controllerName = string.Empty;
                //var actionName = string.Empty;
                //if (!WillSkipCommunicationLogging(mobileParam, filterContext.ActionDescriptor, out controllerName, out actionName))
                //{
                //    GetCurrentLogId();
                //}
            }
            #endregion

            if (IsWriteActionLog)
            {
                var values = filterContext.ActionParameters;
                var sb = new StringBuilder();

                // append controller and action
                sb.AppendFormat("{0}/{1}: ",
                    filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    filterContext.ActionDescriptor.ActionName);

                foreach (var v in values)
                {
                    sb.AppendFormat("{0}={1}&", v.Key, v.Value);
                }
                Logger.Info(sb.ToString());
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            HttpContext.Items.Remove(HeaderKeys.POST);
            base.OnActionExecuted(filterContext);
        }

        private void AddCustomResponseHeader(ActionExecutingContext filterContext, MobileParam mobileParam)
        {
            // added server time
            Response.AddHeader("serverTime", DateTime.Now.ToString(DateTimeFormat.yyyyMMddHHmmss));
        }
        #endregion

        protected virtual bool ValidateModel(List<string> ignoreFields = null)
        {
            if (!this.ModelState.IsValid)
            {
                string errMsg = string.Empty;
                foreach (var k in this.ModelState.Keys)
                {
                    if (ignoreFields == null || !ignoreFields.Contains(k))
                    {
                        foreach (var err in this.ModelState[k].Errors)
                        {
                            errMsg += string.Format("<label>{0}</label><br/>", err.ErrorMessage);
                        }
                    }
                }
                TempData["errorMsg"] = errMsg;

                if (string.IsNullOrWhiteSpace(errMsg))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual MobileParam GetMobileParam()
        {
            var param = new MobileParam(this.RequestRepository);
            param.RequestBody = RequestRepository.PostedDataString;

            return param;
        }

        #region Logging
        protected Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger("PaymentsTest");
                }

                return _logger;
            }
        } private Logger _logger;

        protected long GetCurrentLogId()
        {
            if (!this.RequestRepository.HttpContextItems.Contains(HeaderKeys.CURRENT_LOG_ID))
            {
                this.RequestRepository.HttpContextItems[HeaderKeys.CURRENT_LOG_ID] = InternalRedisService.GetNextSequenceNum(SEQ_FOR_LOG);
            }

            var ret = default(long);
            if (this.RequestRepository.HttpContextItems[HeaderKeys.CURRENT_LOG_ID] != null)
            {
                ret = this.RequestRepository.HttpContextItems[HeaderKeys.CURRENT_LOG_ID].ToString().ToInt64();
            }
            return ret;
        }

        private static int? GetContentResultCode(ActionExecutedContext filterContext)
        {
            var resultCode = -1;
            var contentResult = filterContext.Result as ContentResult;
            if (contentResult != null && contentResult.Content != null)
            {
                const string COMMON_RESULT = "result=";
                const string JSON_RESULT = "{\"result\":";
                if (contentResult.Content.StartsWith(COMMON_RESULT))
                {
                    var resultEnd = contentResult.Content.IndexOf(ASCII.AND_CHAR);
                    if (resultEnd > 0)
                    {
                        resultCode = contentResult.Content.Substring(COMMON_RESULT.Length, resultEnd - COMMON_RESULT.Length).ToInt32();
                    }
                }
                else if (contentResult.Content.StartsWith(JSON_RESULT))
                {//JSON format
                    var resultEnd = contentResult.Content.IndexOf(ASCII.COMMA_CHAR);
                    if (resultEnd > 0)
                    {
                        resultCode = contentResult.Content.Substring(JSON_RESULT.Length, resultEnd - JSON_RESULT.Length).ToInt32();
                    }
                }

            }
            if (resultCode != -1) return resultCode;

            return null;
        }

        protected internal bool WillSkipCommunicationLogging(MobileParam mobileParam, ActionDescriptor actionDescriptor, out string controllerName, out string actionName)
        {
            var controllerDescriptor = default(ControllerDescriptor);
            controllerName = string.Empty;
            actionName = string.Empty;

            var skipLog = mobileParam.IsTest.GetValueOrDefault();

            if (!skipLog)
            {
                if (actionDescriptor != null)
                {
                    actionName = actionDescriptor.ActionName;

                    if (actionDescriptor.ControllerDescriptor != null)
                    {
                        controllerName = actionDescriptor.ControllerDescriptor.ControllerName;
                        controllerDescriptor = actionDescriptor.ControllerDescriptor;

                        skipLog = HasNoCommunicationLogAttribute(actionDescriptor, controllerDescriptor);
                    }
                }
            }

            return skipLog;
        }

        private static bool HasNoCommunicationLogAttribute(ActionDescriptor actionDescriptor, ControllerDescriptor controllerDescriptor)
        {
            var ret = controllerDescriptor.GetCustomAttributes(typeof(NoCommunicationLog), true).Any()
                || actionDescriptor.GetCustomAttributes(typeof(NoCommunicationLog), true).Any();
            return ret;
        }

        private void WriteDebugLog(IRequestRepository request, MobileParam mobileParam, string controllerName, string actionName)
        {
            var mode = RegistryModeFactory.GetCurrentMode();
            if (mode == RegistryMode.Debug || mode == RegistryMode.Release)
            {
                LogHelper.WriteInfo(string.Format("/{0}/{1}:URL: {2}", controllerName, actionName, request.RawUrl));

                if (request.Header != null && request.Header.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var item in request.Header.AllKeys)
                    {
                        sb.AppendFormat("{0}={1}", item, request.Header[item]);
                    }
                    LogHelper.WriteInfo(string.Format("/{0}/{1}:HeaderInfo: {2}", controllerName, actionName, sb.ToString()));
                }

                //LogHelper.WriteInfo(string.Format("/{0}/{1}:MobileParam: {2}", controllerName, actionName, mobileParam.ToString(true)));
            }
        }

        #endregion

        protected virtual Func<bool> CheckRequiredParams(string imsi)
        {
            return () =>
            {
                var ret = true;
                if (string.IsNullOrEmpty(imsi)) ret = false;

                return ret;
            };
        }

        protected HtmlHelper GetHtmlHelper(string viewPageName)
        {
            Stream filter = Stream.Null;
            StreamWriter writer = new StreamWriter(filter);
            var viewContext = new ViewContext(this.ControllerContext,
                new WebFormView(this.ControllerContext, viewPageName),
                new ViewDataDictionary(this.ViewData),
                new TempDataDictionary(), writer);
            return new HtmlHelper(viewContext, new ViewPage());
        }

        #region Build action result if interface method

        public virtual CommonActionResult BuildResult<T>(Func<bool> checkParameterAction, Func<T> getViewModelActions, bool? checkSignature = null)
            where T : IViewModel
        {
            var result = BuildResult<T>(checkParameterAction, () =>
            {
                var ret = new List<T>();

                var value = getViewModelActions();

                // if the return value is null, we think the record does not exist
                if (value != null && value is T)
                    ret.Add(value);
                return ret;
            }, checkSignature);

            result.IsSingleJsonResult = true;

            return result;
        }

        public virtual CommonActionResult BuildResult<T>(Func<bool> checkParameterAction, Func<IList<T>> getViewModelsActions, bool? checkSignature = null)
            where T : IViewModel
        {
            var defaultViewModels = new List<IViewModel>();
            var actionResult = new CommonActionResult(this.RequestRepository, defaultViewModels);
            actionResult.CommonResult = new CommonResult();

            try
            {
                var shouldCheckSignature = checkSignature ?? this.ShouldCheckSignature;

                if (shouldCheckSignature)
                {
                    var urlSig = new UrlSignature(this.RequestRepository, Encoding.UTF8);
                    if (urlSig.IsValid())
                    {
                        CheckParamAndGetResult<T>(checkParameterAction, getViewModelsActions, actionResult);
                    }
                    else
                    {
                        actionResult.CommonResult.Result = ResultCode.ENCRYPTION_SIGN_INVALID;
                    }
                }
                else
                {
                    CheckParamAndGetResult<T>(checkParameterAction, getViewModelsActions, actionResult);
                }

            }
            catch (Exception ex)
            {
                actionResult.CommonResult.Result = ResultCode.System_Error;
                Logger.Error(string.Format("BuildResult Error:{1}{0}URL:{2}{0}Stacktrace{3}", Environment.NewLine, ex.Message, this.RequestRepository.RawUrl, ex.StackTrace));
            }

            return actionResult;
        }

        private static void CheckParamAndGetResult<T>(Func<bool> checkParameterAction, Func<IList<T>> getViewModelsActions, CommonActionResult actionResult) where T : IViewModel
        {
            if (checkParameterAction())
            {
                var viewModels = getViewModelsActions();
                if (viewModels.Any()) { actionResult.CommonResult.Result = ResultCode.Successful; }
                else actionResult.CommonResult.Result = ResultCode.No_Record_Found;

                if (viewModels != null && viewModels.Any()) viewModels.ToList().ForEach(s => actionResult.ViewModels.Add(s));
            }
            else
            {
                actionResult.CommonResult.Result = ResultCode.Invalid_Parameter;
            }
        }

        public virtual IViewModel BuildResult<T>(Func<T> getViewModelAction, Func<T> defaultModelAction) where T : IViewModel
        {
            var model = default(IViewModel);

            try
            {
                model = getViewModelAction();
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("BuildResult Error:{1}{0}URL:{2}{0}Stacktrace{3}", Environment.NewLine, ex.Message, this.RequestRepository.RawUrl, ex.StackTrace));

                if (defaultModelAction != null) model = defaultModelAction();
            }

            return model;
        }


        protected virtual ActionResult BuildRedirectResult(Func<bool> checkParameterAction, Func<string> getRedirectUrlAction, bool isCheckSign = false)
        {
            string redirectUrl = string.Empty;

            try
            {
                if (isCheckSign)
                {
                    var urlSig = new UrlSignature(this.RequestRepository, Encoding.UTF8);
                    if (!urlSig.IsValid())
                    {
                        var defaultViewModels = new List<IViewModel>();
                        var actionResult = new CommonActionResult(this.RequestRepository, defaultViewModels);
                        actionResult.CommonResult = new CommonResult();
                        actionResult.CommonResult.Result = ResultCode.ENCRYPTION_SIGN_INVALID;
                        return Content(actionResult.ToString());
                    }
                }

                if (checkParameterAction())
                {
                    redirectUrl = getRedirectUrlAction();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BuildRedirectResult" + ex.Message);
                Logger.Info("BuildRedirectResult" + ex.StackTrace);
            }

            if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                return Redirect(redirectUrl);
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}