using AppointmentBooking.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace AppointmentBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated && Session["UserName"] != null)
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

        public JsonResult BookAppointments(CalendarInputForBooking info)
        {
            BookingResponse output = new BookingResponse();

            if ((Session["floors"] as System.Collections.Generic.IList<CalendarOutput>).Any(i => i.IsAvailable == false))
            {
                output.Errors.Add("For booking all slots whould be available. Please re-check availability.");
            }
            if (string.IsNullOrWhiteSpace(info.Subject))
            {
                output.Errors.Add("Appointment title is required field.");
            }
            if (string.IsNullOrWhiteSpace(Convert.ToString(Session["UserName"])))
            {
                output.Errors.Add("Username cannot be empty.");
            }
            if (!Request.IsAuthenticated)
            {
                output.Errors.Add("Invalid Request.");
            }
            if (!(Session["floors"] != null && (Session["floors"] is System.Collections.Generic.IList<CalendarOutput>) && (Session["floors"] as System.Collections.Generic.IList<CalendarOutput>).Count > 0))
            {
                output.Errors.Add("Session Expired.");
            }

            if (output.Errors.Count > 0)
            {
                return Json(output, JsonRequestBehavior.AllowGet);
            }

            using (var client = new HttpClient())
            {
                string apiURL = ConfigurationManager.AppSettings["APIRefenenceURL"];
                info.UserId = (string)Session["UserName"];
                info.Password = (string)Session["Password"];

                foreach (CalendarOutput item in (Session["floors"] as System.Collections.Generic.IList<CalendarOutput>))
                {
                    info.BookingSlots.Add(new SlotForBooking() { StartDateTime = item.BookingSlot.StartDateTime, EndDateTime = item.BookingSlot.EndDateTime, RoomID = item.RoomId });
                }

                Task<HttpResponseMessage> response = client.PostAsJsonAsync(apiURL + "BookCalandar", info);
                response.Wait();
                if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var floorJsonString = response.Result.Content.ReadAsStringAsync().Result;
                    var calendarOutputForBooking = JsonConvert.DeserializeObject<CalendarOutputForBooking>(floorJsonString);
                    output.Output = calendarOutputForBooking;

                    return Json(output, JsonRequestBehavior.AllowGet);
                }
            }

            output.Errors.Add("Session Expired.");
            return Json(output, JsonRequestBehavior.AllowGet);

        }

        public JsonResult FetchAvailability(RecurrenceInfo info)
        {
            using (var client = new HttpClient())
            {
                string apiURL = ConfigurationManager.AppSettings["APIRefenenceURL"];
                CalendarInput input = new CalendarInput();
                FetchRoomsRtesponse fetchRoomsRtesponse = new FetchRoomsRtesponse();

                if (Convert.ToString(Session["UserName"]) == string.Empty || Convert.ToString(Session["Password"]) == string.Empty)
                {
                    fetchRoomsRtesponse.NeedToLogout = true;
                }
                input.Capacity = info.Capacity;
                input.FloorID = info.FloorID;
                input.UserId = (string)Session["UserName"];
                input.Password = (string)Session["Password"];
                int durationInMinutes = 0;
                if (!string.IsNullOrEmpty(info.Duration))
                {
                    durationInMinutes = int.Parse(info.Duration.Split(Convert.ToChar(":"))[0]) * 60 + int.Parse(info.Duration.Split(Convert.ToChar(":"))[1]);
                }
                if (durationInMinutes <= 0)
                {
                    fetchRoomsRtesponse.Errors.Add("Duration can not be zero");
                }


                List<Slot> slots = new List<Slot>();
                DateTime start = DateTime.MinValue;
                DateTime startOriginal = DateTime.MinValue;
                DateTime end = DateTime.MinValue;
                if (DateTime.TryParse(info.StartDate + " " + info.StartTime, out start) && DateTime.TryParse(info.EndtDate, out end))
                {
                    startOriginal = start;
                    switch (info.RecurrenceType)
                    {
                        #region For Daily
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
                                        if (start > DateTime.Now)
                                        {
                                            slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        }
                                        start = start.AddDays(1);
                                    }
                                }
                                else if (info.EverySpecifiedWorkingDate > 0)
                                {
                                    if (start > DateTime.Now)
                                    {
                                        slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                    }
                                    start = start.AddDays(info.EverySpecifiedWorkingDate);
                                    while (start.Date <= end.Date)
                                    {
                                        slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        start = start.AddDays(info.EverySpecifiedWorkingDate);
                                    }
                                }

                            }
                            break;
                        #endregion For Daily

                        #region For Weekly
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
                        #endregion For Weekly

                        #region For Monthly
                        case 3: // For Monthly
                            {
                                DateTime startDateAsPerCriteria = new DateTime(start.Year, start.Month, 1, start.Hour, start.Minute, start.Second);
                                start = startDateAsPerCriteria;
                                if (info.DayVise)
                                {
                                    #region For Monthly Day wise
                                    start = start.AddDays(info.Nthday - 1);
                                    while (start.Date <= end.Date)
                                    {
                                        if (start > startOriginal.Date && startDateAsPerCriteria <= end.Date)
                                        {
                                            slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        }
                                        start = start.AddMonths(info.DayMonth);
                                    }
                                    #endregion For Monthly Day wise
                                }
                                else if (info.DayTypeVise)
                                {
                                    #region For Monthly Day Type wise
                                    List<string> weekAllDays = new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                                    if (info.DayTypeMonth == "Day")
                                    {
                                        #region For Monthly Day Type wise -Day
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
                                            if (startDateAsPerCriteria > startOriginal.Date && startDateAsPerCriteria <= end.Date)
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1, startDateAsPerCriteria.Hour, startDateAsPerCriteria.Minute, startDateAsPerCriteria.Second);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);

                                        #endregion For Monthly Day Type wise -Day
                                    }
                                    else if (info.DayTypeMonth == "WeekDay")
                                    {
                                        #region For Monthly Day Type wise -Weekday
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
                                            if (startDateAsPerCriteria > startOriginal.Date && startDateAsPerCriteria <= end.Date)
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1, startDateAsPerCriteria.Hour, startDateAsPerCriteria.Minute, startDateAsPerCriteria.Second);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                        #endregion For Monthly Day Type wise -Weekday
                                    }
                                    if (info.DayTypeMonth == "Weekend")
                                    {
                                        #region For Monthly Day Type wise -Weekend
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
                                            if (startDateAsPerCriteria > startOriginal.Date && startDateAsPerCriteria <= end.Date)
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1, startDateAsPerCriteria.Hour, startDateAsPerCriteria.Minute, startDateAsPerCriteria.Second);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                        #endregion For Monthly Day Type wise -Weekend
                                    }
                                    else if (weekAllDays.Contains(info.DayTypeMonth))
                                    {
                                        #region For Monthly Day Type wise -SpecificDay
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
                                                startDateAsPerCriteria = startDateAsPerCriteria.AddDays(1);
                                            }
                                            if (startDateAsPerCriteria > startOriginal.Date && startDateAsPerCriteria <= end.Date)
                                            {
                                                slots.Add(new Slot() { StartDateTime = startDateAsPerCriteria, EndDateTime = startDateAsPerCriteria.AddMinutes(durationInMinutes) });
                                            }
                                            startDateAsPerCriteria = startDateAsPerCriteria.AddMonths(info.MonthNumber);
                                            startDateAsPerCriteria = new DateTime(startDateAsPerCriteria.Year, startDateAsPerCriteria.Month, 1, startDateAsPerCriteria.Hour, startDateAsPerCriteria.Minute, startDateAsPerCriteria.Second);
                                        }
                                        while (startDateAsPerCriteria <= end.Date);
                                        #endregion For Monthly Day Type wise -SpecificDay
                                    }
                                    #endregion For Monthly Day Type wise
                                }
                            }
                            break;
                        #endregion For Monthly

                        case 4: // For Custom
                            {
                                foreach (var item in info.AppointmentDates)
                                {
                                    if (DateTime.TryParse(item + " " + info.StartTime, out start))
                                    {
                                        if (start > DateTime.Now)
                                        {
                                            slots.Add(new Slot() { StartDateTime = start, EndDateTime = start.AddMinutes(durationInMinutes) });
                                        }
                                    }
                                    else
                                    {
                                        fetchRoomsRtesponse.Errors.Add("Date is not proper.");
                                    }
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    fetchRoomsRtesponse.Errors.Add("Start date, end date must be proper.");
                }
                if (DateTime.Parse(info.StartDate) > DateTime.Parse(info.EndtDate))
                {
                    fetchRoomsRtesponse.Errors.Add("Start date must be less then or equal to end date.");
                }
                input.BookingSlots = slots;

                if (slots.Count == 0)
                {
                    fetchRoomsRtesponse.Errors.Add("No slots to book room.");
                }
                if (fetchRoomsRtesponse.Errors.Count > 0 || fetchRoomsRtesponse.NeedToLogout)
                {
                    return Json(fetchRoomsRtesponse, JsonRequestBehavior.AllowGet);
                }

                Task<HttpResponseMessage> response = client.PostAsJsonAsync(apiURL + "FetchBookings", input);
                response.Wait();
                if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var roomsJsonString = response.Result.Content.ReadAsStringAsync().Result;
                    fetchRoomsRtesponse.AvailableRooms = JsonConvert.DeserializeObject<System.Collections.Generic.IList<CalendarOutput>>(roomsJsonString);

                    Session["floors"] = fetchRoomsRtesponse.AvailableRooms;

                    return Json(fetchRoomsRtesponse, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FetchNewAvailableSlot(RecurrenceInfo info)
        {
            using (var client = new HttpClient())
            {
                BookingResponse output = new BookingResponse();

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

                input.BookingSlots = slots;

                Task<HttpResponseMessage> response = client.PostAsJsonAsync(apiURL + "FetchBookings", input);
                response.Wait();
                if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var roomsJsonString = response.Result.Content.ReadAsStringAsync().Result;
                    var availableRooms = JsonConvert.DeserializeObject<IList<CalendarOutput>>(roomsJsonString);

                    Session["newfloor"] = availableRooms;

                    if (availableRooms.Any(i => i.IsAvailable == false) && availableRooms.Any(x => x.RoomName != null))
                    {
                        output.Errors.Add("Time slot is already booked for " + info.StartDate + ", please select another time slot.");
                        return Json(output, JsonRequestBehavior.AllowGet);
                    }
                    else if (availableRooms.Any(i => i.IsAvailable == false) && availableRooms.Any(x => x.RoomName == null))
                    {
                        output.Errors.Add("No any room is available for (" + input.Capacity + ") no of attendees.");
                        return Json(output, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(availableRooms, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ConfirmNewAvailableSlot()
        {
            if (Session["floors"] != null && Session["newfloor"] != null)
            {


                var availableRooms = (List<CalendarOutput>)Session["floors"];
                var newFetchRoom = (List<CalendarOutput>)Session["newfloor"];

                var fetchRoom = newFetchRoom.FirstOrDefault();

                availableRooms.RemoveAll(x => x.BookingSlot.StartDate == fetchRoom.BookingSlot.StartDate);
                availableRooms.Add(fetchRoom);

                Session["floors"] = availableRooms;

                return Json(availableRooms.OrderBy(x => x.BookingSlot.StartDate), JsonRequestBehavior.AllowGet);
            }
            else
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

