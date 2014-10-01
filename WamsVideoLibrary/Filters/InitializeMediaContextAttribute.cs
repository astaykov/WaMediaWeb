using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WamsVideoLibrary.Models;
using WamsVideoLibrary.Controllers;

namespace WamsVideoLibrary.Filters
{
    public sealed class InitializeMediaContextAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Type cType = filterContext.ActionDescriptor.ControllerDescriptor.ControllerType;
            if (filterContext.HttpContext.Session["mctx"] == null)
            {
                // need to create the context - so redirect to InitiMediaController
                // but if it is request to InitiMediaController, just pass it by
                if (!cType.Equals(typeof(InitMediaController)))
                {
                    filterContext.HttpContext.Response.Redirect("~/InitMedia/");
                }
            }
            
            
            base.OnActionExecuting(filterContext);
        }
    }
}
