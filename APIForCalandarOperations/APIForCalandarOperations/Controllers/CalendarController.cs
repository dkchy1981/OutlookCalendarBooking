using APIForCalandarOperations.DataAccess;
using APIForCalandarOperations.Models;
using System;
using System.Collections.Generic;
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
        public IEnumerable<string> SaveCalendar(List<Models.Student> students)
        {
            return new string[] { students[0].Id.ToString(), students[0].Name };
        }
    }

    internal class ResponseMSG
    {
        public List<string> Output { get; set; }

    }
}