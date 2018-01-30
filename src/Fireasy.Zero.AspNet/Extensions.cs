﻿using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using Fireasy.Common.Serialization;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Validation;
using Fireasy.Web.Mvc;
using Fireasy.Zero.Helpers;
using Fireasy.Zero.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Fireasy.Zero.AspNet
{
    public static class Extensions
    {
        /// <summary>
        /// 包装 Json 数据。
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="value"></param>
        /// <param name="converters"></param>
        /// <returns></returns>
        public static JsonResult Json(this Controller controller, object value, params JsonConverter[] converters)
        {
            var option = new JsonSerializeOption();
            if (converters != null)
            {
                option.Converters.AddRange(converters);
            }

            return new JsonResultWrapper(value, option);
        }

        /// <summary>
        /// 根据权限输出工具栏按钮。
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static IHtmlString Toolbar(this HtmlHelper html)
        {
            var session = html.ViewContext.HttpContext.GetSession();
            var adminSvr = ContainerUnity.GetContainer().Resolve<IAdminService>();
            var operates = adminSvr.GetPurviewOperates(session.UserID, HttpContext.Current.Request.RawUrl);
            var sb = new StringBuilder();

            foreach (var oper in operates)
            {
                if (oper != null)
                {
                    sb.AppendFormat("<a key=\"{0}\" class=\"easyui-linkbutton\" onclick=\"toolbarClick('{0}')\" data-options=\"iconCls: '{1}', plain: true\">{2}</a>", oper.Code, oper.Icon, oper.Name);
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// 绑定枚举类型。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="enumType">枚举类型。</param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static IHtmlString BindEnum(this UrlHelper url, Type enumType, ItemFlag? flag = null)
        {
            var list = enumType.GetEnumList().OrderBy(s => s.Key).Select(s => new { Value = s.Key, Name = s.Value });

            var list1 = ItemFlagHelper.Insert(list, flag, s => new { Value = string.Empty, Name = s.GetDescription() });

            return MvcHtmlString.Create(new JsonSerializer().Serialize(list1));
        }

        /// <summary>
        /// 输出实体验证特性。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html"></param>
        /// <returns></returns>
        public static IHtmlString AddValidations<T>(this HtmlHelper html)
        {
            var dict = new Dictionary<string, IEnumerable<string>>();
            foreach (var property in PropertyUnity.GetPersistentProperties(typeof(T)))
            {
                var validations = ValidationUnity.GetValidations(property).Select(s => GetValidation(s)).Where(s => !string.IsNullOrEmpty(s)).ToList();
                if (validations.Count > 0)
                {
                    dict.Add(property.Name, validations);
                }
            }

            return MvcHtmlString.Create(new JsonSerializer().Serialize(dict));
        }

        private static string GetValidation(ValidationAttribute attribute)
        {
            if (attribute is RequiredAttribute)
            {
                return "required";
            }
            else if (attribute is StringLengthAttribute)
            {
                var lengAttr = attribute as StringLengthAttribute;
                return "length[" + lengAttr.MinimumLength + ", " + lengAttr.MaximumLength + "]";
            }
            else if (attribute is EmailAttribute)
            {
                return "email";
            }
            else if (attribute is MobileAttribute)
            {
                return "mobile";
            }
            else if (attribute is TelphoneAttribute)
            {
                return "phone";
            }
            else if (attribute is TelphoneOrMobileAttribute)
            {
                return "phoneOrMobile";
            }
            else if (attribute is ZipCodeAttribute)
            {
                return "zipCode";
            }

            return string.Empty;
        }
    }
}