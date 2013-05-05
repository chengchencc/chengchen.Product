using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Order = BlackMamba.Billing.Models.Payments.Order;
using SubSonic.Oracle.Schema;
using BlackMamba.Billing.Models.Payments;
using BlackMamba.Billing.Domain.Services;
using BlackMamba.Billing.Domain.Services.SMS;
using BlackMamba.Framework.SubSonic.Oracle;
using BlackMamba.Billing.Models;
using BlackMamba.Billing.Models.SMS;
using BlackMamba.Billing.Website.Helper;
using BlackMamba.Billing.Website.Controllers.Base;


namespace BlackMamba.Billing.Website.Controllers
{
    public class SMSUIController : UIBaseController
    {
        #region Ctor

        ISMSUIService SMSUIService;
        ISMSService SMSService;
        protected const int PAGESIZE = 20;

        public SMSUIController(ISMSUIService smsUIService, ISMSService smsSvc)
        {
            SMSUIService = smsUIService;
            SMSService = smsSvc;
        }
        #endregion

        #region ServiceProvider

        public ActionResult ServiceProviderManage(int page = 1)
        {
            var allSP = SMSUIService.GetPaged<ServiceProvider>(page - 1, PAGESIZE, s => s.CreatedDate);
            allSP.PageIndex = page;
            ViewBag.Title = "充值通道";
            return View(allSP);
        }

        [HttpGet]
        public ActionResult SPAdd()
        {
            ViewData["DynamicSP"] = EnumHelper.GetSelectListFromEnumType(typeof(DynamicSP));
            return View();
        }

        [HttpPost]
        public ActionResult SPAdd(ServiceProvider model)
        {
            SMSUIService.Add<ServiceProvider>(model);
            return RedirectToAction("ServiceProviderManage");
        }

        [HttpGet]
        public ActionResult SPEdit(int id)
        {
            var originModel = SMSUIService.Single<ServiceProvider>(id);
            ViewData["DynamicSP"] = EnumHelper.GetSelectListFromEnumType(typeof(DynamicSP), (int)originModel.DynamicSP);
            return View(originModel);
        }
        [HttpPost]
        public ActionResult SPEdit(ServiceProvider model)
        {
            SMSUIService.Update<ServiceProvider>(model);
            return RedirectToAction("ServiceProviderManage");
        }

        public ActionResult SPDelete(int id)
        {
            var model = this.SMSUIService.Single<ServiceProvider>(id);
            if (model != null)
            {
                var relatedSMSs = this.SMSUIService.Find<ShortMessageService>(s => s.SpId == id);
                foreach (var SMSItem in relatedSMSs)
                {
                    //TODO: SMS 下的级联删除                    
                }
                SMSUIService.Delete<ServiceProvider>(id);
            }

            return RedirectToAction("ServiceProviderManage");
        }

        #endregion

        #region SMSService

        public ActionResult SMSServices(int id, int page = 1)
        {
            var smsServices = SMSUIService.GetPaged<ShortMessageService>(page - 1, PAGESIZE, s => s.SpId == id, x => x.CreatedDate, true);
            smsServices.PageIndex = page;
            ViewData["SPID"] = id;
            return View(smsServices);
        }

        [HttpGet]
        public ActionResult ServiceAdd(int spId)
        {
            var model = new ShortMessageService();
            model.SpId = spId;
            ViewData["ServiceType"] = EnumHelper.GetSelectListFromEnumType(typeof(ServiceType));

            return View(model);
        }

        [HttpPost]
        public ActionResult ServiceAdd(ShortMessageService model)
        {
            SMSUIService.Add<ShortMessageService>(model);
            return RedirectToAction("SMSServices", new { id = model.SpId });
        }

        public ActionResult ServiceDelete(int id, int spId)
        {
            SMSUIService.Delete<ShortMessageService>(id);
            return RedirectToAction("SMSServices", new { id = spId });
        }

        public ActionResult ServiceUpdate(int id)
        { 
            var model = this.SMSUIService.Single<ShortMessageService>(id);
            ViewData["ServiceType"] = EnumHelper.GetSelectListFromEnumType(typeof(ServiceType), (int)model.Type);
            return View(model);
        }

        [HttpPost]
        public ActionResult ServiceUpdate(ShortMessageService model)
        {
            this.SMSUIService.Update<ShortMessageService>(model);
            return RedirectToAction("SMSServices", new { id = model.SpId });
        }

        #endregion

        #region Instruction

        public ActionResult Instructions(int spId, int serviceId, int page = 1)
        {
            var instructions = SMSUIService.GetPaged<Instruction>(page - 1, PAGESIZE, s => s.ServiceId == serviceId, x => x.CreatedDate, true);
            instructions.PageIndex = page;
            ViewData["ServiceId"] = serviceId;
            ViewData["SPID"] = spId;
            return View(instructions);
        }

        [HttpGet]
        public ActionResult InstructionAdd(int serviceId)
        {
            ViewData["ServiceId"] = serviceId;
            Instruction model = new Instruction();
            model.ServiceId = serviceId;
            return View(model);
        }
        [HttpPost]
        public ActionResult InstructionAdd(Instruction model)
        {
            this.SMSUIService.Add<Instruction>(model);
            var spId = this.SMSUIService.Single<ShortMessageService>(model.ServiceId).SpId;
            return RedirectToAction("Instructions", new { spId = spId, serviceId = model.ServiceId });
        }

        public ActionResult InstructionDelete(int id, int serviceId)
        {
            this.SMSUIService.Delete<Instruction>(id);
            var spId = this.SMSUIService.Single<ShortMessageService>(serviceId).SpId;
            return RedirectToAction("Instructions", new { spId = spId, serviceId = serviceId });
        }

        #endregion

        #region SMS logs
        public ActionResult SMSChannelLogs(int page = 1)
        {
            var allSMSChannelLogs = SMSUIService.GetPaged<SMSChannelLog>(page - 1, PAGESIZE, s => s.CreatedDate, true);
            allSMSChannelLogs.PageIndex = page;
            ViewBag.Title = "短信代充日志";
            return View(allSMSChannelLogs);
        }

        public ActionResult SearchSMSChannelLogs(int page = 1, string mobile = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            PagedList<SMSChannelLog> allSMSChannelLogs = null;
            if (endDate == null)
            {
                endDate = DateTime.Now;
            }

            if (!string.IsNullOrEmpty(mobile))
            {
                if (startDate != null && startDate <= endDate)
                {
                    allSMSChannelLogs = SMSUIService.GetPaged<SMSChannelLog>(page - 1, PAGESIZE, x => x.Mobile == mobile && x.CreatedDate >= startDate && x.CreatedDate <= endDate, s => s.CreatedDate, true);
                }
                else
                {
                    allSMSChannelLogs = SMSUIService.GetPaged<SMSChannelLog>(page - 1, PAGESIZE, x => x.Mobile == mobile, s => s.CreatedDate, true);
                }

            }
            else
            {
                if (startDate != null && startDate <= endDate)
                {
                    allSMSChannelLogs = SMSUIService.GetPaged<SMSChannelLog>(page - 1, PAGESIZE, x => x.CreatedDate >= startDate && x.CreatedDate <= endDate, s => s.CreatedDate, true);
                }
                else
                {
                    allSMSChannelLogs = SMSUIService.GetPaged<SMSChannelLog>(page - 1, PAGESIZE, s => s.CreatedDate, true);
                }
            }

            allSMSChannelLogs.PageIndex = page;

            return View("SMSChannelLogs", allSMSChannelLogs);
        }

        public ActionResult SMSLogs(int id)
        {
            var smsLogs = this.SMSUIService.All<SMSLog>(x => x.ChannelLogId == id);

            return View(smsLogs);
        }
        #endregion

        [HttpGet]
        public ActionResult ChannelSettings(int? spId, int? serviceId, int? instructionId, int? channelId)
        {
            string serviceNumber = string.Empty;
            if (serviceId.HasValue && !instructionId.HasValue && !channelId.HasValue)
            {
                var smsSvc = this.SMSUIService.Single<ShortMessageService>(x => x.Id == serviceId);
                var smsChannel = this.SMSUIService.Single<SMSChannel>(x => x.ServiceId == serviceId);
                serviceNumber = smsSvc.ServiceNumber;
                if (smsChannel == null)
                {
                    SMSChannel channel = new SMSChannel
                    {
                        ServiceId = serviceId.GetValueOrDefault(),
                        ServiceNumber = smsSvc.ServiceNumber,
                    };
                    this.SMSUIService.Add<SMSChannel>(channel);
                    channelId = channel.Id;
                }
                else
                {
                    channelId = smsChannel.Id;
                }
            }

            if (!serviceId.HasValue && !instructionId.HasValue && channelId.HasValue)
            {
                var smsChannel = this.SMSUIService.Single<SMSChannel>(channelId.GetValueOrDefault());
                serviceId = smsChannel.ServiceId;
                var smsSvc = this.SMSUIService.Single<ShortMessageService>(x => x.Id == serviceId);
                serviceNumber = smsSvc.ServiceNumber;
                spId = smsSvc.SpId;
            }

            ViewData["ServiceNumber"] = serviceNumber;
            ViewData["SPId"] = spId;
            ViewData["ServiceId"] = serviceId.GetValueOrDefault();
            ViewData["ChannelId"] = channelId.GetValueOrDefault();
            var models = this.SMSUIService.Find<SMSChannelSetting>(x => x.ChannelId == channelId.GetValueOrDefault()).ToList();

            if (instructionId.HasValue)
            {
                models = this.SMSUIService.Find<SMSChannelSetting>(x => x.InstructionId == instructionId.GetValueOrDefault()).ToList();
                var instruction = this.SMSUIService.Single<Instruction>(instructionId.GetValueOrDefault());
                var service = this.SMSUIService.Single<ShortMessageService>(instruction.ServiceId);

                ViewData["ServiceNumber"] = instruction.Code;
                ViewData["SPId"] = service.SpId;
                ViewData["ServiceId"] = service.Id;
                ViewData["InstructionId"] = instructionId.GetValueOrDefault();
            }



            return View(models);
        }

        [HttpGet]
        public ActionResult ChannelSettingAdd(int? channelId, int? instructionId)
        {
            SMSChannelSetting model = new SMSChannelSetting();
            if (channelId.HasValue)
            {
                model.ChannelId = channelId.GetValueOrDefault();
            }
            if (instructionId.HasValue)
            {
                model.InstructionId = instructionId.GetValueOrDefault();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult ChannelSettingAdd(SMSChannelSetting model)
        {
            this.SMSUIService.Add<SMSChannelSetting>(model);
            if (model.InstructionId.HasValue)
            {
                return RedirectToAction("ChannelSettings", new { instructionId = model.InstructionId });

            }
            return RedirectToAction("ChannelSettings", new { channelId = model.ChannelId });
        }

        public ActionResult ChannelSettingDelete(int id)
        {
            var model = this.SMSUIService.Single<SMSChannelSetting>(id);
            if (model != null)
            {
                this.SMSUIService.Delete<SMSChannelSetting>(id);

                if (model.InstructionId.HasValue)
                {
                    return RedirectToAction("ChannelSettings", new { instructionId = model.InstructionId });
                }
                return RedirectToAction("ChannelSettings", new { channelId = model.ChannelId });
            }

            return RedirectToAction("ServiceProviderManage");
        }

        public ActionResult ChannelRestrictions(int? spId, int? serviceId, int? channelId,int? instructionId)
        {
            string serviceNumber = string.Empty;
            if (serviceId.HasValue &&!instructionId.HasValue && !channelId.HasValue)
            {
                var smsSvc = this.SMSUIService.Single<ShortMessageService>(x => x.Id == serviceId);
                var smsChannel = this.SMSUIService.Single<SMSChannel>(x => x.ServiceId == serviceId);
                serviceNumber = smsSvc.ServiceNumber;
                if (smsChannel == null)
                {
                    SMSChannel channel = new SMSChannel
                    {
                        ServiceId = serviceId.GetValueOrDefault(),
                        ServiceNumber = smsSvc.ServiceNumber,
                    };
                    this.SMSUIService.Add<SMSChannel>(channel);
                    channelId = channel.Id;
                }
                else
                {
                    channelId = smsChannel.Id;
                }
            }

            if (!serviceId.HasValue && !instructionId.HasValue && channelId.HasValue)
            {
                var smsChannel = this.SMSUIService.Single<SMSChannel>(channelId.GetValueOrDefault());
                serviceId = smsChannel.ServiceId;
                var smsSvc = this.SMSUIService.Single<ShortMessageService>(x => x.Id == serviceId);
                serviceNumber = smsSvc.ServiceNumber;
                spId = smsSvc.SpId;
            }

            ViewData["SPId"] = spId;
            ViewData["ServiceId"] = serviceId.GetValueOrDefault();
            ViewData["ChannelId"] = channelId.GetValueOrDefault();
            var models = this.SMSUIService.Find<ChannelRestriction>(x => x.ChannelId == channelId.GetValueOrDefault()).ToList();

            if (instructionId.HasValue)
            {
                models = this.SMSUIService.Find<ChannelRestriction>(x => x.InstructionId == instructionId.GetValueOrDefault()).ToList();
                var instruction = this.SMSUIService.Single<Instruction>(s => s.Id == instructionId.GetValueOrDefault());
                var service = this.SMSUIService.Single<ShortMessageService>(s => s.Id == instruction.ServiceId);
                ViewData["SPId"] = service.SpId;
                ViewData["ServiceId"] = service.Id;
                ViewData["InstructionId"] = instruction.Id;
                ViewData["ServiceNumber"] = instruction.Code;
            }

            return View(models);
        }


        [HttpGet]
        public ActionResult ChannelRestrictionAdd(int? channelId, int? provinceId,int? instructionId)
        {
            ChannelRestriction model = new ChannelRestriction();
            if (channelId.HasValue)
            {
                model.ChannelId = channelId.GetValueOrDefault();
            }

            ViewData["RestrictionType"] = EnumHelper.GetSelectListFromEnumType(typeof(RestrictionType));
            ViewData["Province"] = new SelectList(ChinaRegionInfo.AllProvinces, "Id", "Name", provinceId.GetValueOrDefault());
            var cities = new List<City> { new City { Id = 0, Name = "请选择", ProvinceId = 0 } };

            if (provinceId.HasValue)
            {
                var relatedCities = this.SMSService.GetCityByProvinceId(provinceId.GetValueOrDefault());
                if (relatedCities != null)
                {
                    foreach (var c in relatedCities)
                    {
                        cities.Add(new City { Id = c.Id, Name = c.Name, ProvinceId = c.ProvinceId });
                    }
                }
            }

            ViewData["City"] = new SelectList(cities, "Id", "Name");
            ViewData["Operator"] = EnumHelper.GetSelectListFromEnumType(typeof(Operators));
            ViewData["TimeSpan"] = EnumHelper.GetSelectListFromEnumType(typeof(RestrictionByTimeSpan));
            ViewData["InstructionId"] = instructionId.GetValueOrDefault();

            if (instructionId.HasValue)
            {
                model.InstructionId = instructionId.GetValueOrDefault();
            }
            TempData["InstructionId"] = instructionId.GetValueOrDefault();
            return View(model);
        }

        [HttpPost]
        public ActionResult ChannelRestrictionAdd(ChannelRestriction model)
        {
            this.SMSUIService.Add<ChannelRestriction>(model);
            if (model.InstructionId.HasValue)
            {
                return RedirectToAction("ChannelRestrictions", new { instructionId = model.InstructionId });
            }
            return RedirectToAction("ChannelRestrictions", new { channelId = model.ChannelId });
        }

        public ActionResult ChannelRestrictionDelete(int id)
        {
            var model = this.SMSUIService.Single<ChannelRestriction>(id);
            if (model!=null)
            {
                this.SMSUIService.Delete<ChannelRestriction>(id);
                if (model.InstructionId.HasValue)
                {
                    return RedirectToAction("ChannelRestrictions", new { instructionId = model.InstructionId });
                }
                return RedirectToAction("ChannelRestrictions", new { channelId = model.ChannelId });
            }
            return RedirectToAction("ServiceProviderManage");
        }

        public ActionResult RegionRestriction(int settingId, int spId, int serviceId, int? instructionId, int page = 1)
        {
            var regionRestrictions = this.SMSUIService.GetPaged<SettingInRegion>(page - 1, PAGESIZE, s => s.SettingId == settingId, x => x.CreatedDate, true);
            regionRestrictions.PageIndex = page;
            ViewData["SettingId"] = settingId;
            ViewData["SPId"] = spId;
            ViewData["ServiceId"] = serviceId;
            ViewData["InstructionId"] = instructionId.GetValueOrDefault();

            TempData["InstructionId"] = instructionId.GetValueOrDefault();
            TempData["SPId"] = spId;
            TempData["ServiceId"] = serviceId;
            return View(regionRestrictions);
        }

        [HttpGet]
        public ActionResult AddRegionRestriction(int settingId, int? provinceId,int? instructionId)
        {
            ViewData["RestrictionType"] = EnumHelper.GetSelectListFromEnumType(typeof(RestrictionType));
            ViewData["Province"] = new SelectList(ChinaRegionInfo.AllProvinces, "Id", "Name", provinceId.GetValueOrDefault());
            var cities = new List<City> { new City { Id = 0, Name = "请选择", ProvinceId = 0 } };

            if (provinceId.HasValue)
            {
                var relatedCities = this.SMSService.GetCityByProvinceId(provinceId.GetValueOrDefault());
                if (relatedCities != null)
                {
                    foreach (var c in relatedCities)
                    {
                        cities.Add(new City { Id = c.Id, Name = c.Name, ProvinceId = c.ProvinceId });
                    }
                }
            }

            ViewData["City"] = new SelectList(cities, "Id", "Name");


            SettingInRegion model = new SettingInRegion();
            model.SettingId = settingId;

            //if (instructionId.HasValue)
            //{
            ViewData["InstructionId"] = instructionId.GetValueOrDefault();
            
            TempData["InstructionId"] = TempData["InstructionId"];
            TempData["SPId"] = TempData["SPId"];
            TempData["ServiceId"] = TempData["ServiceId"];
            return View(model);
        }
        [HttpPost]
        public ActionResult AddRegionRestriction(SettingInRegion model)
        {
            this.SMSUIService.Add<SettingInRegion>(model);
            return RedirectToAction("RegionRestriction", new { settingId = model.SettingId, spId = TempData["SPId"], serviceId = TempData["ServiceId"],InstructionId= TempData["InstructionId"] });
        }

        public ActionResult DeleteRegionRestriction(int settingId, int id, int? instructionId)
        {
            this.SMSUIService.Delete<SettingInRegion>(id);
            return RedirectToAction("RegionRestriction", new { settingId = settingId, spId = TempData["SPId"], serviceId = TempData["ServiceId"], instructionId = instructionId.GetValueOrDefault() });
        }


    }
}
