﻿using APIForCalandarOperations.DataAccess;
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
            XDocument xmlDoc = XDocument.Load(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/FloorsAndRooms.xml"));
            List<Room> roomList = new List<Room>();

            foreach (XElement floor in xmlDoc.Elements("Floors").Elements("Floor"))
            {
                foreach (XElement room in floor.Elements("Room"))
                {
                    roomList.Add(new Room() { Id = Convert.ToInt32(room.Attribute("Id").Value), Name = room.Attribute("Name").Value , Email = room.Attribute("Email").Value });
                }
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, roomList);            
        }

        [Route("GetRoomsById/{roomID}"), HttpGet]
        public async Task<HttpResponseMessage> GetRoomsById([FromUri] int roomID)
        {
            XDocument xmlDoc = XDocument.Load(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/FloorsAndRooms.xml"));
            List<Room> roomList = new List<Room>();

            foreach (XElement floor in xmlDoc.Elements("Floors").Elements("Floor"))
            {
                if (Convert.ToInt32(floor.Attribute("Id").Value) == roomID)
                {
                    foreach (XElement room in floor.Elements("Room"))
                    {
                        roomList.Add(new Room() { Id = Convert.ToInt32(room.Attribute("Id").Value), Name = room.Attribute("Name").Value, Email = room.Attribute("Email").Value });
                    }
                    break;
                }
            }
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