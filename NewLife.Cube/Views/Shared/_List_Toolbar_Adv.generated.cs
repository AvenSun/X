﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using NewLife;
    
    #line 1 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
    using NewLife.Common;
    
    #line default
    #line hidden
    using NewLife.Cube;
    using NewLife.Reflection;
    using NewLife.Web;
    using XCode;
    using XCode.Membership;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/_List_Toolbar_Adv.cshtml")]
    public partial class _Views_Shared__List_Toolbar_Adv_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_Shared__List_Toolbar_Adv_cshtml()
        {
        }
        public override void Execute()
        {
            
            #line 2 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
  
    var user = ViewBag.User as IUser ?? User.Identity as IUser;

            
            #line default
            #line hidden
WriteLiteral("\r\n<div");

WriteLiteral(" class=\"btn-group\"");

WriteLiteral(">\r\n    <button");

WriteLiteral(" data-toggle=\"dropdown\"");

WriteLiteral(" class=\"btn btn-info btn-sm btn-sm-force dropdown-toggle\"");

WriteLiteral(" aria-expanded=\"false\"");

WriteLiteral(">\r\n        高级\r\n        <span");

WriteLiteral(" class=\"ace-icon fa fa-caret-down icon-on-right\"");

WriteLiteral("></span>\r\n    </button>\r\n\r\n    <ul");

WriteLiteral(" class=\"dropdown-menu dropdown-info dropdown-menu-right\"");

WriteLiteral(">\r\n        <li>\r\n            <a");

WriteAttribute("href", Tuple.Create(" href=\"", 431), Tuple.Create("\"", 464)
            
            #line 13 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
, Tuple.Create(Tuple.Create("", 438), Tuple.Create<System.Object, System.Int32>(Url.Action("ExportExcel")
            
            #line default
            #line hidden
, 438), false)
);

WriteLiteral(">导出Excel</a>\r\n        </li>\r\n        <li>\r\n            <a");

WriteAttribute("href", Tuple.Create(" href=\"", 522), Tuple.Create("\"", 553)
            
            #line 16 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
, Tuple.Create(Tuple.Create("", 529), Tuple.Create<System.Object, System.Int32>(Url.Action("ExportXml")
            
            #line default
            #line hidden
, 529), false)
);

WriteLiteral(">导出Xml</a>\r\n        </li>\r\n        ");

WriteLiteral("\r\n        <li>\r\n            <a");

WriteAttribute("href", Tuple.Create(" href=\"", 708), Tuple.Create("\"", 740)
            
            #line 22 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
, Tuple.Create(Tuple.Create("", 715), Tuple.Create<System.Object, System.Int32>(Url.Action("ExportJson")
            
            #line default
            #line hidden
, 715), false)
);

WriteLiteral(">导出Json</a>\r\n        </li>\r\n        ");

WriteLiteral("\r\n");

            
            #line 27 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
        
            
            #line default
            #line hidden
            
            #line 27 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
         if (user.Has(PermissionFlags.Delete))
        {

            
            #line default
            #line hidden
WriteLiteral("            <li");

WriteLiteral(" class=\"divider\"");

WriteLiteral("></li>\r\n");

WriteLiteral("            <li>\r\n                <a");

WriteLiteral(" href=\"javascript:void(0);\"");

WriteLiteral(" data-url=\"");

            
            #line 31 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
                                                   Write(Url.Action("DeleteSelect"));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-action=\"delete\"");

WriteLiteral(" data-fields=\"keys\"");

WriteLiteral(" data-confirm=\"该操作将删除选中数据并不可恢复！确认删除？\"");

WriteLiteral(" title=\"删除选中数据\"");

WriteLiteral(">删除选中</a>\r\n            </li>\r\n");

WriteLiteral("            <li>\r\n                <a");

WriteLiteral(" href=\"javascript:void(0);\"");

WriteLiteral(" data-url=\"");

            
            #line 34 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
                                                   Write(Url.Action("DeleteAll"));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-action=\"delete\"");

WriteLiteral(" data-confirm=\"该操作将删除当前查询的所有数据并不可恢复！确认删除？\"");

WriteLiteral(" title=\"删除当前查询的所有数据\"");

WriteLiteral(">删除全部</a>\r\n            </li>\r\n");

            
            #line 36 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("        ");

            
            #line 37 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
         if (SysConfig.Current.Develop)
        {

            
            #line default
            #line hidden
WriteLiteral("            <li");

WriteLiteral(" class=\"divider\"");

WriteLiteral("></li>\r\n");

            
            #line 40 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
            if (User.IsInRole("管理员"))
            {

            
            #line default
            #line hidden
WriteLiteral("                <li>\r\n                    <a");

WriteLiteral(" href=\"javascript:void(0);\"");

WriteLiteral(" data-url=\"");

            
            #line 43 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
                                                       Write(Url.Action("Clear"));

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(" data-action=\"delete\"");

WriteLiteral(" data-confirm=\"该操作将删除数据表所有数据并不可恢复！确认清空？\"");

WriteLiteral(" title=\"删除数据表所有数据\"");

WriteLiteral(">清空数据表</a>\r\n                </li>\r\n");

            
            #line 45 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("            <li>\r\n                <a");

WriteAttribute("href", Tuple.Create(" href=\"", 1828), Tuple.Create("\"", 1858)
            
            #line 47 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
, Tuple.Create(Tuple.Create("", 1835), Tuple.Create<System.Object, System.Int32>(Url.Action("MakeList")
            
            #line default
            #line hidden
, 1835), false)
);

WriteLiteral(">生成列表</a>\r\n            </li>\r\n");

WriteLiteral("            <li>\r\n                <a");

WriteAttribute("href", Tuple.Create(" href=\"", 1925), Tuple.Create("\"", 1955)
            
            #line 50 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
, Tuple.Create(Tuple.Create("", 1932), Tuple.Create<System.Object, System.Int32>(Url.Action("MakeForm")
            
            #line default
            #line hidden
, 1932), false)
);

WriteLiteral(">生成表单</a>\r\n            </li>\r\n");

            
            #line 52 "..\..\Views\Shared\_List_Toolbar_Adv.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </ul>\r\n</div><!-- /.btn-group -->");

        }
    }
}
#pragma warning restore 1591
