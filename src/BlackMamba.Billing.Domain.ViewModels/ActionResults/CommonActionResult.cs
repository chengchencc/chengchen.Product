using BlackMamba.Framework.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlackMamba.Billing.Domain.ViewModels
{
    public class CommonActionResult
    {
        internal IRequestRepository RequestRepo { get; set; }

        public CommonActionResult(IViewModel viewModel)
            : this(null, viewModel)
        {
        }

        public CommonActionResult(IRequestRepository requestRepo, IViewModel viewModel)
        {
            this.ViewModels = new List<IViewModel> { viewModel };
            this.RequestRepo = requestRepo;
        }

        public CommonActionResult(IRequestRepository requestRepo, IEnumerable<IViewModel> viewModels)
        {
            this.ViewModels = new List<IViewModel>(viewModels);
            this.RequestRepo = requestRepo;
        }

        #region Common Properties
        public CommonResult CommonResult { get; set; }

        public IList<IViewModel> ViewModels { get; set; }

        /// <summary>
        /// Only for json format
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// if has value, will display this field
        /// </summary>
        public int? Total { get; set; }

        public List<CustomHeaderItem> CustomResultHeaders
        {
            get
            {
                if (_customResultHeaderContent == null) _customResultHeaderContent = new List<CustomHeaderItem>();

                return _customResultHeaderContent;
            }
            set
            {
                _customResultHeaderContent = value;
            }
        } private List<CustomHeaderItem> _customResultHeaderContent;

        public bool IsSingleJsonResult { get; set; }
        #endregion

        public override string ToString()
        {
            var jsonResult = default(JsonResultBase);
            if (this.IsSingleJsonResult && this.ViewModels.Count <= 1)
                jsonResult = new JsonSingleDataResult(this);
            else
                jsonResult = new JsonCommonResult(this);

            var ret = jsonResult.ToString();

            return ret;
        }
    }

    public class CustomHeaderItem
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public bool IsValueType { get; set; }

    }
}