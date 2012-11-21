using System.Web;
using System.Web.Mvc;
using WamsVideoLibrary.Filters;

namespace WamsVideoLibrary
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new InitializeMediaContextAttribute());
        }
    }
}