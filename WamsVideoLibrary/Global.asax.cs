using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.Configuration;

namespace WamsVideoLibrary
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // keep an instance of the CloudMediaContext in the Application State
            // because its creation is expensive
            this.InitiMediaContext();
        }

        private void InitiMediaContext()
        {
            string mediaAccount = ConfigurationManager.AppSettings["MediaAccount"];
            string mediaKey = ConfigurationManager.AppSettings["MediaKey"];
            Application["mctx"] = new CloudMediaContext(mediaAccount, mediaKey);
        }

    }
}