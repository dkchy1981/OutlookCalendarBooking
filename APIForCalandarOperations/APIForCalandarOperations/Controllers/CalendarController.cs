using APIForCalandarOperations.DataAccess;
using APIForCalandarOperations.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;

namespace APIForCalandarOperations.Controllers
{
    [RoutePrefix("api/Calendar")]
    public class CalendarController : ApiController
    {
        [Route("ValidateUser"), HttpPost]
        public async Task<HttpResponseMessage> ValidateUser(UserLoginInfo user)
        {
            string domain = "baroda";
            DirectoryEntry de = new DirectoryEntry(null, domain + "\\" + user.userName, user.password);
            try
            {
                object o = de.NativeObject;
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.Filter = "samaccountname=" + user.userName;
                ds.PropertiesToLoad.Add("cn");
                SearchResult sr = ds.FindOne();
                if (sr == null) throw new Exception();
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, false);
            }
        }

        [Route("ValidateUserForOutlookConnection"), HttpPost]
        public async Task<HttpResponseMessage> ValidateUserForOutlookConnection(UserLoginInfo user)
        {
            try
            {
                GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
                bool returnVal = getFloorAndRooms.CheckExchangeServiceForProvidedUserDetails(user.userName, user.password);
                if (returnVal)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, returnVal);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, false);
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, false);
            }
        }

        [Route("GetFloors"), HttpGet]
        public HttpResponseMessage GetFloors()
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Floor> floorList = getFloorAndRooms.GetFloorList(System.Configuration.ConfigurationManager.AppSettings["Connection"]);
            return Request.CreateResponse(HttpStatusCode.OK, floorList);
        }

        [Route("GetAllRooms"), HttpGet]
        public async Task<HttpResponseMessage> GetAllRooms()
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Room> roomList = getFloorAndRooms.GetAllRooms(System.Configuration.ConfigurationManager.AppSettings["Connection"]);
            return Request.CreateResponse(HttpStatusCode.OK, roomList);
        }

        [Route("GetRoomsById/{roomID}"), HttpGet]
        public async Task<HttpResponseMessage> GetRoomsById([FromUri] int roomID)
        {

            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Room> roomList = getFloorAndRooms.GetRoomsByID(System.Configuration.ConfigurationManager.AppSettings["Connection"], roomID);
            return Request.CreateResponse(HttpStatusCode.OK, roomList);
        }

        [Route("GetRoomsByFloorId/{floorID}"), HttpGet]
        public async Task<HttpResponseMessage> GetRoomsByFloorId([FromUri] int floorID)
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Room> roomList = getFloorAndRooms.GetRoomsByFloorID(System.Configuration.ConfigurationManager.AppSettings["Connection"], floorID);
            return Request.CreateResponse(HttpStatusCode.OK, roomList);
        }

        [HttpPost]
        [Route("FetchBookings")]
        public async Task<HttpResponseMessage> FetchBookings(CalendarInput input)
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<CalendarOutput> calendarOutputList = getFloorAndRooms.GetRoomsAvailabilityByCalendateInput(System.Configuration.ConfigurationManager.AppSettings["Connection"], input);

            return Request.CreateResponse(HttpStatusCode.OK, calendarOutputList);
        }


        [HttpPost]
        [Route("BookCalandar")]
        public async Task<HttpResponseMessage> BookCalandar(CalendarInputForBooking inputForRoomBooking)
        {
            CalendarInput input = new CalendarInput();
            CalendarOutputForBooking output = new CalendarOutputForBooking();

            input.BookingSlots = inputForRoomBooking.BookingSlots.Select(
                q => new Slot()
                {
                    StartDateTime = q.StartDateTime,
                    EndDateTime = q.EndDateTime

                }).ToList();
            input.Capacity = inputForRoomBooking.Capacity;
            input.FloorID = inputForRoomBooking.FloorID;
            input.UserId = inputForRoomBooking.UserId;
            input.Password = inputForRoomBooking.Password;

            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<CalendarOutput> calendarOutputList = getFloorAndRooms.GetRoomsAvailabilityByCalendateInput(System.Configuration.ConfigurationManager.AppSettings["Connection"], input);

            if (calendarOutputList.Count > 0 && !(calendarOutputList.Any(t => t.IsAvailable == false)))
            {
                output = getFloorAndRooms.BookRooms(System.Configuration.ConfigurationManager.AppSettings["Connection"], inputForRoomBooking);                
            }
            else
            {
                output.Message = "Conflict occured for provided slots against system bookings.";
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }
    }
}