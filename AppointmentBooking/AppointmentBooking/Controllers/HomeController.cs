using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.DirectoryServices.Protocols;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.DirectoryServices;
using System.Web.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;
using AppointmentBooking.Models;

namespace AppointmentBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                using (var client = new HttpClient())
                {
                    string apiURL = ConfigurationManager.AppSettings["APIRefenenceURL"];
                    Task<HttpResponseMessage> response = client.GetAsync(apiURL + "GetFloors");
                    response.Wait();
                    if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var floorJsonString = response.Result.Content.ReadAsStringAsync().Result;
                        var floors = JsonConvert.DeserializeObject<System.Collections.Generic.IList<Floor>>(floorJsonString);
                        List<SelectListItem> floorsList = floors.Select(bidResult => new SelectListItem { Text = bidResult.Name, Value = bidResult.Id.ToString() }).ToList();

                        ViewData["Floors"] = floorsList;
                    }
                }
                return View();
            }
            else
            {
                return Logout();
            }
        }

        public JsonResult FetchAvailability(RecurrenceInfo info)
        {
            using (var client = new HttpClient())
            {
                string apiURL = ConfigurationManager.AppSettings["APIRefenenceURL"];
                CalendarInput input = new CalendarInput();
                input.Capacity = info.Capacity;
                input.FloorID = info.FloorID;
                input.UserId = (string)Session["UserName"];
                int durationInMinutes = 0;
                if (!string.IsNullOrEmpty(info.Duration))
                {
                    durationInMinutes = int.Parse(info.Duration.Split(Convert.ToChar(":"))[0]) * 60 + int.Parse(info.Duration.Split(Convert.ToChar(":"))[1]);
                }
                List<Slot> slots = new List<Slot>();
                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MinValue;
                if (DateTime.TryParse(info.StartDate + " " + info.StartTime, out start) && DateTime.TryParse(info.EndtDate, out end))
                {
                    switch (info.RecurrenceType)
                    {
                        case 1: // For Daily
                            {
                                if (info.IsEveryDay || info.IsEveryDayWorking)
                                {

                                    while (start.Date <= end.Date)
                                    {
                                        if (info.IsEveryDayWorking && (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday))
                                        {
                                            start = start.AddDays(1);
                                            continue;
                                        }
                                        slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        start = start.AddDays(1);
                                    }
                                }
                                else if (info.EverySpecifiedWorkingDate > 0)
                                {
                                    slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                    start = start.AddDays(info.EverySpecifiedWorkingDate);
                                    while (start.Date <= end.Date)
                                    {
                                        slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        start = start.AddDays(info.EverySpecifiedWorkingDate);
                                    }
                                }

                            }
                            break;
                        case 2: // For Weekly
                            {
                                do
                                {
                                    if (
                                        (info.IsSaturday && start.DayOfWeek == DayOfWeek.Saturday) ||
                                        (info.IsSunday && start.DayOfWeek == DayOfWeek.Sunday) ||
                                        (info.IsMonday && start.DayOfWeek == DayOfWeek.Monday) ||
                                        (info.IsTuesday && start.DayOfWeek == DayOfWeek.Tuesday) ||
                                        (info.IsWednesday && start.DayOfWeek == DayOfWeek.Wednesday) ||
                                        (info.IsThursday && start.DayOfWeek == DayOfWeek.Thursday) ||
                                        (info.IsFriday && start.DayOfWeek == DayOfWeek.Friday)
                                        )
                                    {
                                        slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                    }
                                    start = start.AddDays(1);
                                }
                                while (start.Date <= end.Date);
                            }
                            break;
                        case 3: // For Monthly
                            {
                                DateTime startDateAsPerCriteria = new DateTime(start.Year, start.Month, 1);
                                start = startDateAsPerCriteria;
                                if (info.DayVise)
                                {
                                    start = start.AddDays(info.Nthday-1);
                                    while (start.Date <= end.Date)
                                    {
                                        slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        start = start.AddMonths(info.DayMonth);
                                    }
                                }
                                else if (info.DayTypeVise)
                                {
                                    List<string> weekAllDays = new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                                    
                                    

                                    if (info.DayTypeMonth == "Day")
                                    {
                                        do
                                        {
                                            if (info.NthMonthDay == "First")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(0);
                                            }
                                            else if (info.NthMonthDay == "Second")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                            }
                                            else if (info.NthMonthDay == "Third")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(2);
                                            }
                                            else if (info.NthMonthDay == "Fourth")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(3);
                                            }
                                            else if (info.NthMonthDay == "Last")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(1).AddDays(-1);
                                            }
                                            if (startDateAsPerCriteria <= end.Date)
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                    }
                                    else if (info.DayTypeMonth == "WeekDay")
                                    {
                                        do
                                        {
                                            if (info.NthMonthDay == "First")
                                            {
                                                startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);
                                            }
                                            else if (info.NthMonthDay == "Second")
                                            {
                                                startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);

                                                for (int i = 0; i < 1; i++)
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                    startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);
                                                }
                                            }
                                            else if (info.NthMonthDay == "Third")
                                            {
                                                startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);

                                                for (int i = 0; i < 2; i++)
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                    startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);
                                                }
                                            }
                                            else if (info.NthMonthDay == "Fourth")
                                            {
                                                startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);

                                                for (int i = 0; i < 3; i++)
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                    startDateAsPerCriteria = GetNextWeekDays(startDateAsPerCriteria);
                                                }
                                            }
                                            else if (info.NthMonthDay == "Last")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(1).AddDays(-1);
                                                while ((startDateAsPerCriteria.DayOfWeek == DayOfWeek.Saturday) || (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Sunday))
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(-1);
                                                }
                                            }
                                            if ((startDateAsPerCriteria <= end.Date))
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                    }
                                    if (info.DayTypeMonth == "Weekend")
                                    {
                                        do
                                        {
                                            if (info.NthMonthDay == "First")
                                            {
                                                startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);
                                            }
                                            else if (info.NthMonthDay == "Second")
                                            {
                                                startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);

                                                for (int i = 0; i < 1; i++)
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                    startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);
                                                }
                                            }
                                            else if (info.NthMonthDay == "Third")
                                            {
                                                startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);
                                                for (int i = 0; i < 2; i++)
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                    startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);
                                                }
                                            }
                                            else if (info.NthMonthDay == "Fourth")
                                            {
                                                startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);
                                                for (int i = 0; i < 3; i++)
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                    startDateAsPerCriteria = GetNextWeekendDate(startDateAsPerCriteria);
                                                }
                                            }
                                            else if (info.NthMonthDay == "Last")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(1).AddDays(-1);
                                                while (!((startDateAsPerCriteria.DayOfWeek == DayOfWeek.Saturday) || (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Sunday)))
                                                {
                                                    startDateAsPerCriteria = startDateAsPerCriteria.AddDays(-1);
                                                }
                                            }
                                            if ((startDateAsPerCriteria <= end.Date))
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                    }
                                    else if (weekAllDays.Contains(info.DayTypeMonth))
                                    {
                                        do
                                        {
                                            if (info.NthMonthDay == "First")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(0);
                                            }
                                            else if (info.NthMonthDay == "Second")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(7);
                                            }
                                            else if (info.NthMonthDay == "Third")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(14);
                                            }
                                            else if (info.NthMonthDay == "Fourth")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(21);
                                            }
                                            else if (info.NthMonthDay == "Last")
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(1).AddDays(-8);
                                            }

                                            for (int i = 0; i < 7; i++)
                                            {
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                                if (
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Sunday && info.DayTypeMonth == "Sunday") ||
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Monday && info.DayTypeMonth == "Monday") ||
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Tuesday && info.DayTypeMonth == "Tuesday") ||
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Wednesday && info.DayTypeMonth == "Wednesday") ||
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Thursday && info.DayTypeMonth == "Thursday") ||
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Friday && info.DayTypeMonth == "Friday") ||
                                                    (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Saturday && info.DayTypeMonth == "Saturday")
                                                   )
                                                {
                                                    break;
                                                }
                                                
                                            }
                                            if ((startDateAsPerCriteria <= end.Date))
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                    }

                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                input.BookingSlots = slots;

                Task<HttpResponseMessage> response = client.PostAsJsonAsync(apiURL + "FetchBookings", input);
                response.Wait();
                if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var floorJsonString = response.Result.Content.ReadAsStringAsync().Result;
                    var floors = JsonConvert.DeserializeObject<System.Collections.Generic.IList<CalendarOutput>>(floorJsonString);

                    return Json(floors, JsonRequestBehavior.AllowGet);
                }
            }

            //    //Creating List    
            //    List<Employee> ObjEmp = new List<Employee>()
            //{  
            //    //Adding records to list    
            //    new Employee
            //    {
            //        Id = 1, Name = "Vithal Wadje", City = "Latur", Address = "Kabansangvi"
            //    },
            //    new Employee
            //    {
            //        Id = 2, Name = "Sudhir Wadje", City = "Mumbai", Address = "Kurla"
            //    }
            //};
            //return list as Json    
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        private static DateTime GetNextWeekDays(DateTime startDateAsPerCriteria)
        {
            while ((startDateAsPerCriteria.DayOfWeek == DayOfWeek.Saturday) || (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Sunday))
            {
                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
            }

            return startDateAsPerCriteria;
        }

        private static DateTime GetNextWeekendDate(DateTime startDateAsPerCriteria)
        {
            while (!((startDateAsPerCriteria.DayOfWeek == DayOfWeek.Saturday) || (startDateAsPerCriteria.DayOfWeek == DayOfWeek.Sunday)))
            {
                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
            }

            return startDateAsPerCriteria;
        }

        // POST: Login/Create
        [HttpPost]
        public ActionResult MultipleCommand(FormCollection collection, string Command)
        {
            if (string.Compare(Command, "Check Availability", true) == 0)
            {
                string subject = collection["AppointmentTitle"];
                string floorID = collection["FloorSelection"];
                string attendeesCount = collection["NumberOfAttendees"];
                string startDate = collection["StartDate"];
                string endDate = collection["EndDate"];
                string startTime = collection["StartTime"];
                string endTime = collection["EndTime"];
                string dailyInput_EveryWorkingDay = collection["EveryWorkingDay"];

            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                // First we clean the authentication ticket like always  
                //required NameSpace: using System.Web.Security;  
                FormsAuthentication.SignOut();

                // Second we clear the principal to ensure the user does not retain any authentication  
                //required NameSpace: using System.Security.Principal;  
                HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

                Session.Clear();
                System.Web.HttpContext.Current.Session.RemoveAll();

                // Last we redirect to a controller/action that requires authentication to ensure a redirect takes place  
                // this clears the Request.IsAuthenticated flag since this triggers a new request  
                return RedirectToLocal();
            }
            catch
            {
                throw;
            }
        }


        private ActionResult RedirectToLocal(string returnURL = "")
        {
            try
            {
                // If the return url starts with a slash "/" we assume it belongs to our site  
                // so we will redirect to this "action"  
                if (!string.IsNullOrWhiteSpace(returnURL) && Url.IsLocalUrl(returnURL))
                    return Redirect(returnURL);

                // If we cannot verify if the url is local to our host we redirect to a default location  
                return RedirectToAction("Login", "Login");
            }
            catch
            {
                throw;
            }
        }

        public ActionResult Popup()
        {
            return View();
        }
    }
}

