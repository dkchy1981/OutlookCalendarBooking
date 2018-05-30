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

        [Route("GetFloors"), HttpGet]
        public HttpResponseMessage GetFloors()
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Floor> floorList = getFloorAndRooms.GetFloorList(System.Configuration.ConfigurationManager.AppSettings["Connection"]);
            return Request.CreateResponse(HttpStatusCode.NotFound, floorList);
        }

        [Route("GetAllRooms"), HttpGet]
        public async Task<HttpResponseMessage> GetAllRooms()
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Room> roomList = getFloorAndRooms.GetAllRooms(System.Configuration.ConfigurationManager.AppSettings["Connection"]);
            return Request.CreateResponse(HttpStatusCode.NotFound, roomList);
        }

        [Route("GetRoomsById/{roomID}"), HttpGet]
        public async Task<HttpResponseMessage> GetRoomsById([FromUri] int roomID)
        {

            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Room> roomList = getFloorAndRooms.GetRoomsByID(System.Configuration.ConfigurationManager.AppSettings["Connection"], roomID);
            return Request.CreateResponse(HttpStatusCode.NotFound, roomList);
        }

        [Route("GetRoomsByFloorId/{floorID}"), HttpGet]
        public async Task<HttpResponseMessage> GetRoomsByFloorId([FromUri] int floorID)
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<Room> roomList = getFloorAndRooms.GetRoomsByFloorID(System.Configuration.ConfigurationManager.AppSettings["Connection"], floorID);
            return Request.CreateResponse(HttpStatusCode.NotFound, roomList);
        }



        [HttpPost]
        [Route("FetchBookings")]
        public async Task<HttpResponseMessage> FetchBookings(CalendarInput input)
        {
            GetFloorAndRooms getFloorAndRooms = new GetFloorAndRooms();
            IList<CalendarOutput> calendarOutputList = getFloorAndRooms.GetRoomsAvailabilityByCalendateInput(System.Configuration.ConfigurationManager.AppSettings["Connection"], input);

            return Request.CreateResponse(HttpStatusCode.NotFound, calendarOutputList);
        }


        [HttpPost]
        [Route("SaveCalendar")]
        public IEnumerable<string> SaveCalendar(List<Models.Student> students)
        {
            return new string[] { students[0].Id.ToString(), students[0].Name };
        }
    }

    public class UserLoginInfo
    {
        public string userName { get; set; }

        public string password { get; set; }
    }

    public class CalendarInput
    {
        public int FloorID { get; set; }

        public int Capacity { get; set; }

        public string UserId { get; set; }

        public List<Slot> BookingSlots { get; set; }
    }

    public class Slot
    {
        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }
    }

    public class CalendarOutput
    {
        
        public bool IsAvailable { get; set; }

        public string RoomName { get; set; }

        public Slot BookingSlot { get; set; }

        public List<string> Messages { get; set; }

        public CalendarOutput()
        {
            this.Messages = new List<string>();
        }
    }
}