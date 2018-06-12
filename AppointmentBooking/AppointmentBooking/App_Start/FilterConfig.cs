using AppointmentBooking.Filters;
using System.Web;
using System.Web.Mvc;

namespace AppointmentBooking
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthenticationFilter());
        }
    }
}
