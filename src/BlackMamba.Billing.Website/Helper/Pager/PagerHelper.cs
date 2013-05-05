using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BlackMamba.Framework.Core;
using SubSonic.Oracle.Schema;
using System.Linq.Expressions;


namespace BlackMamba.Billing.Website.Web
{
    public static class PagerHelper
    {
        #region DisplayNameFor

        public static MvcHtmlString DisplayNameFor<TModel, TValue>(this HtmlHelper<PagedList<TModel>> html, Expression<Func<TModel, TValue>> expression)
        {
            return DisplayNameForInternal(html, expression, metadataProvider: null);
        }
        internal static MvcHtmlString DisplayNameForInternal<TModel, TValue>(this HtmlHelper<PagedList<TModel>> html, Expression<Func<TModel, TValue>> expression, ModelMetadataProvider metadataProvider)
        {
            return DisplayNameHelper(ModelMetadata.FromLambdaExpression(expression,new ViewDataDictionary<TModel>()),
                                     ExpressionHelper.GetExpressionText(expression));
        }
        internal static MvcHtmlString DisplayNameHelper(ModelMetadata metadata, string htmlFieldName)
        {
            // We don't call ModelMetadata.GetDisplayName here because we want to fall back to the field name rather than the ModelType.
            // This is similar to how the LabelHelpers get the text of a label.
            string resolvedDisplayName = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

            return new MvcHtmlString(HttpUtility.HtmlEncode(resolvedDisplayName));
        }

        #endregion


        #region Html Pager
        public static MvcHtmlString Pager(this HtmlHelper helper, int TotalCount, int pageSize, int pageIndex, string actionName, string controllerName,
            PagerOptions pagerOptions, string routeName, object routeValues, object htmlAttributes)
        {
            var totalPageCount = (int)Math.Ceiling(TotalCount / (double)pageSize);
            var builder = new PagerBuilder
                (
                    helper,
                    actionName,
                    controllerName,
                    totalPageCount,
                    pageIndex,
                    pagerOptions,
                    routeName,
                    new RouteValueDictionary(routeValues),
                    new RouteValueDictionary(htmlAttributes)
                );
            return builder.RenderPager();
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int TotalCount, int pageSize, int pageIndex, string actionName, string controllerName,
            PagerOptions pagerOptions, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            var totalPageCount = (int)Math.Ceiling(TotalCount / (double)pageSize);
            var builder = new PagerBuilder
                (
                    helper,
                    actionName,
                    controllerName,
                    totalPageCount,
                    pageIndex,
                    pagerOptions,
                    routeName,
                    routeValues,
                    htmlAttributes
                );
            return builder.RenderPager();
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, int TotalCount, int pageSize, int pageIndex, string actionName, string controllerName,
            PagerOptions pagerOptions, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, IDictionary<string, string> searchParamer)
        {
            var totalPageCount = (int)Math.Ceiling(TotalCount / (double)pageSize);
            var builder = new PagerBuilder
                (
                    helper,
                    actionName,
                    controllerName,
                    totalPageCount,
                    pageIndex,
                    pagerOptions,
                    routeName,
                    routeValues,
                    htmlAttributes,
                    searchParamer
                );
            return builder.RenderPager();
        }

        private static MvcHtmlString Pager(HtmlHelper helper, PagerOptions pagerOptions, IDictionary<string, object> htmlAttributes)
        {
            return new PagerBuilder(helper, null, pagerOptions, htmlAttributes).RenderPager();
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList)
        {
            if (pagedList == null)
                return Pager(helper, (PagerOptions)null, null);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, null, null, null, null);
        }

        //todo add parameter Dictionary<string, string>
        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, null);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, null, null, null);
        }
        //todo new -xing
        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions,IDictionary<string, string> searchParameter)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, null);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, null, null, null, searchParameter);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions, object htmlAttributes)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, new RouteValueDictionary(htmlAttributes));
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, null, null, htmlAttributes);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions, IDictionary<string, object> htmlAttributes)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, htmlAttributes);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, null, null, htmlAttributes);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions, string routeName, object routeValues)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, null);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, routeName, routeValues, null);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions, string routeName, RouteValueDictionary routeValues)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, null);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, routeName, routeValues, null);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions, string routeName, object routeValues, object htmlAttributes)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, new RouteValueDictionary(htmlAttributes));
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, routeName, routeValues, htmlAttributes);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, PagerOptions pagerOptions, string routeName,
            RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            if (pagedList == null)
                return Pager(helper, pagerOptions, htmlAttributes);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, pagerOptions, routeName, routeValues, htmlAttributes);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, string routeName, object routeValues, object htmlAttributes)
        {
            if (pagedList == null)
                return Pager(helper, null, new RouteValueDictionary(htmlAttributes));
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, null, routeName, routeValues, htmlAttributes);
        }

        public static MvcHtmlString Pager(this HtmlHelper helper, IPagedList pagedList, string routeName, RouteValueDictionary routeValues,
            IDictionary<string, object> htmlAttributes)
        {
            if (pagedList == null)
                return Pager(helper, null, htmlAttributes);
            return Pager(helper, pagedList.TotalCount, pagedList.PageSize, pagedList.PageIndex, null, null, null, routeName, routeValues, htmlAttributes);
        }

        #endregion
    }
}