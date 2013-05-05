using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlackMamba.Billing.Website.Helper
{
    public class EnumHelper
    {
        public static SelectList GetSelectListFromEnumType(Type enumType)
        {
            List<SelectItem> items = GetSelectItemsFromEnumType(enumType);

            return new SelectList(items, "Value", "Text");
        }

        public static SelectList GetSelectListFromEnumType(Type enumType, object selectedValue)
        {
            List<SelectItem> items = GetSelectItemsFromEnumType(enumType);

            return new SelectList(items, "Value", "Text", selectedValue);
        }

        private static List<SelectItem> GetSelectItemsFromEnumType(Type enumType)
        {
            Array values = Enum.GetValues(enumType);
            List<SelectItem> items = new List<SelectItem>(values.Length);

            foreach (var i in values)
            {
                items.Add(new SelectItem
                {
                    Text = Enum.GetName(enumType, i),
                    Value = ((int)i).ToString()
                });
            }
            return items;
        }
    }

    public class SelectItem
    {
        public string Text { get; set; }

        public string Value { set; get; }
    }
}
