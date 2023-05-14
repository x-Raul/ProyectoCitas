using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Citas_.Filters
{
    public class AdminSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["admin"] == null)
            {
                filterContext.Result = new RedirectResult("~/Access/Login");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}