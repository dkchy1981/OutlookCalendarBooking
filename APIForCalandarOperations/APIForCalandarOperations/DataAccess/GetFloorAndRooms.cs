using APIForCalandarOperations.Controllers;
using APIForCalandarOperations.Models;
using APIForCalandarOperations.MSSQL.DataAccess;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;

namespace APIForCalandarOperations.DataAccess
{
    public class GetFloorAndRooms
    {
        public GetFloorAndRooms()
        {

        }

        public IList<Floor> GetFloorList(string connKey)
        {
            IList<Floor> floorList = new List<Floor>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM FLOORLIST"))
                {
                    Floor floor;
                    while (reader.Read())
                    {
                        floor = new Floor()
                        {
                            Id = SqlHelper.To<int>(reader["Id"], 0),
                            Name = SqlHelper.To<string>(reader["FloorName"], string.Empty)
                        };
                        floorList.Add(floor);
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return floorList;
        }

        public IList<Room> GetAllRooms(string connKey)
        {
            IList<Room> roomList = new List<Room>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM ROOM"))
                {
                    Room room;
                    while (reader.Read())
                    {
                        room = new Room()
                        {
                            Id = SqlHelper.To<int>(reader["ID"], 0),
                            Name = SqlHelper.To<string>(reader["RoomName"], string.Empty),
                            Email = SqlHelper.To<string>(reader["RoomEmail"], string.Empty),
                            Capacity = SqlHelper.To<int>(reader["Capacity"], 0),
                            FloorId = SqlHelper.To<int>(reader["FloorID"], 0)
                        };
                        roomList.Add(room);
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return roomList;
        }

        public IList<Room> GetRoomsByID(string connKey, int roomID)
        {
            IList<Room> roomList = new List<Room>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM ROOM WHERE ID = " + roomID))
                {
                    Room room;
                    while (reader.Read())
                    {
                        room = new Room()
                        {
                            Id = SqlHelper.To<int>(reader["ID"], 0),
                            Name = SqlHelper.To<string>(reader["RoomName"], string.Empty),
                            Email = SqlHelper.To<string>(reader["RoomEmail"], string.Empty),
                            Capacity = SqlHelper.To<int>(reader["Capacity"], 0),
                            FloorId = SqlHelper.To<int>(reader["FloorID"], 0)
                        };
                        roomList.Add(room);
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return roomList;
        }


        public IList<Room> GetRoomsByFloorID(string connKey, int floorID)
        {
            IList<Room> roomList = new List<Room>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM ROOM WHERE (FloorID = " + floorID + " OR " + floorID + "=1)"))
                {
                    Room room;
                    while (reader.Read())
                    {
                        room = new Room()
                        {
                            Id = SqlHelper.To<int>(reader["ID"], 0),
                            Name = SqlHelper.To<string>(reader["RoomName"], string.Empty),
                            Email = SqlHelper.To<string>(reader["RoomEmail"], string.Empty),
                            Capacity = SqlHelper.To<int>(reader["Capacity"], 0),
                            FloorId = SqlHelper.To<int>(reader["FloorID"], 0)
                        };
                        roomList.Add(room);
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return roomList;
        }

        public IList<CalendarOutput> GetRoomsAvailabilityByCalendateInput(string connKey, CalendarInput input)
        {
            IList<CalendarOutput> calendarOutputList = new List<CalendarOutput>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                IList<Room> lstRooms = GetRoomsByFloorID(connKey, input.FloorID);
                IList<Room> lstPriorityRooms = lstRooms.Where(t => (t.Capacity >= input.Capacity)).OrderBy(t => t.Capacity).ToList();
                string commonMessage = string.Empty;
                ExchangeService service = GetExchangeService();
                if(lstPriorityRooms.Count<=0)
                {
                    commonMessage = "Room is not available for matching capacity";
                }

                foreach (Slot slot in input.BookingSlots)
                {
                    CalendarOutput calendarOutput = new CalendarOutput();
                    calendarOutput.BookingSlot = slot;
                    if (commonMessage != string.Empty)
                    {
                        calendarOutput.Messages.Add(commonMessage);
                    }
                    calendarOutputList.Add(calendarOutput);                    
                }
                DateTime startDate = input.BookingSlots.OrderBy(t => t.StartDateTime).FirstOrDefault().StartDateTime.Date;
                DateTime endtDate = input.BookingSlots.OrderByDescending(t => t.StartDateTime).FirstOrDefault().StartDateTime.Date.AddDays(1);
                GetRoomAvailabilityRecursivly(calendarOutputList, lstPriorityRooms, service, 0, startDate, endtDate);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return calendarOutputList;
        }

        private void GetRoomAvailabilityRecursivly(IList<CalendarOutput> calendarOutputList, IList<Room> lstPriorityRooms, ExchangeService service, int roomSkipCount, DateTime startDate, DateTime endtDate)
        {
            if (calendarOutputList.Any(i => i.IsAvailable == false) && lstPriorityRooms.ToList().Count > roomSkipCount)
            {
                Room room = lstPriorityRooms.ToList().Skip(roomSkipCount).FirstOrDefault();
                FindItemsResults<Appointment> fapts = null;

                foreach (CalendarOutput calendarOutput in calendarOutputList.Where(i => i.IsAvailable == false).ToList())
                {
                    calendarOutput.RoomName = room.Name;
                    try
                    {
                        if (CheckRoomAvailability(room.Name, room.Email, calendarOutput.BookingSlot, service, startDate, endtDate, ref fapts))
                        {
                            calendarOutput.Messages.Add("Room Available");
                            calendarOutput.IsAvailable = true;
                        }
                        else
                        {
                            calendarOutput.Messages.Add("Room Not Available");
                        }
                    }
                    catch (Microsoft.Exchange.WebServices.Data.ServiceRequestException ex)
                    {
                        calendarOutput.Messages.Add(string.Format("User do not have access on room '{0}'", room.Name));
                    }
                    catch (Exception ex)
                    {
                        calendarOutput.Messages.Add(string.Format("Error occured to fetch rooms availability for room '{0}'", room.Name));
                    }
                }
                GetRoomAvailabilityRecursivly(calendarOutputList, lstPriorityRooms, service, roomSkipCount + 1, startDate, endtDate);
            }
            else
            {
                return;
            }
        }

        private bool CheckRoomAvailability(string roomName, string email, Slot slot, ExchangeService service, DateTime startDate, DateTime endtDate, ref FindItemsResults<Appointment> fapts)
        {
            bool blnReturn = true;

            if (fapts == null)
            {
                FolderId CalendarFolderId = new FolderId(WellKnownFolderName.Calendar, email);
                CalendarView cv = new CalendarView(startDate, endtDate);
                fapts = service.FindAppointments(CalendarFolderId, cv);
            }
            if (fapts.Count() > 0)
            {
                var starts = fapts.Select(t => t.Start).ToList();
                var ends = fapts.Select(t => t.End).ToList();


                if (fapts.Where(t => ((t.Start < slot.StartDateTime && t.End > slot.StartDateTime) || (t.Start < slot.EndDateTime && t.End > slot.EndDateTime) ||
                     (slot.StartDateTime < t.Start && slot.EndDateTime > t.Start) || (slot.StartDateTime < t.End && slot.EndDateTime > t.End)
                     || (slot.StartDateTime == t.Start && slot.EndDateTime == t.End)
                     )).Count() > 0)
                {
                    return false;
                }
            }
            return blnReturn;
        }

        private ExchangeService GetExchangeService()
        {
            string userEmail = System.Configuration.ConfigurationManager.AppSettings["UserAccountEmail"];
            string userPassword = System.Configuration.ConfigurationManager.AppSettings["Password"];

            ExchangeService service = new ExchangeService();

            #region Authentication

            // Set specific credentials.
            service.Credentials = new NetworkCredential(userEmail, userPassword);
            #endregion

            #region Endpoint management

            // Look up the user's EWS endpoint by using Autodiscover.
            //service.AutodiscoverUrl(userEmailAddress, RedirectionCallback);
            service.Url = new Uri("https://mail.civica.com/ews/exchange.asmx");

            #endregion Endpoint management
            return service;
        }


    }
}