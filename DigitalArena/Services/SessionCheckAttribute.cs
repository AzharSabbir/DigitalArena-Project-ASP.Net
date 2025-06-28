using System;
using System.Web;
using System.Web.Mvc;

public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // Skip check for AllowAnonymous
        if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ||
            filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
        {
            return;
        }

        // Check if Session["UserId"] is null
        if (HttpContext.Current.Session["UserId"] == null)
        {
            // Optionally sign out user (if FormsAuthentication used)
            System.Web.Security.FormsAuthentication.SignOut();

            // Redirect to login
            filterContext.Result = new RedirectResult("~/Auth/Login");
        }

        base.OnActionExecuting(filterContext);
    }
}
