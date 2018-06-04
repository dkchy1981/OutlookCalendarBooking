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
using System.Transactions;

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
                if (lstPriorityRooms.Count <= 0)
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
                    calendarOutput.RoomID = room.Id;
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



        public CalendarOutputForBooking BookRooms(string connKey, CalendarInputForBooking input)
        {
            CalendarOutputForBooking calendarOutputForBooking = new CalendarOutputForBooking();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                IList<Room> lstRooms = GetRoomsByFloorID(connKey, input.FloorID);
                ExchangeService service = GetExchangeService();

                DateTime startDate = input.BookingSlots.OrderBy(t => t.StartDateTime).FirstOrDefault().StartDateTime.Date;
                DateTime endtDate = input.BookingSlots.OrderByDescending(t => t.StartDateTime).FirstOrDefault().StartDateTime.Date;

                string strSQL = @"INSERT INTO BookedMeeting (UserID,StartDateTime,EndDateTime,Description,IsConfirmed) output INSERTED.ID VALUES(@UserID,@StartDateTime,@EndDateTime,@Description,@IsConfirmed)";
                int bookedMeetingID = 0;
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlCommand theSQLCommand = new SqlCommand(strSQL, connection))
                    {
                        theSQLCommand.Parameters.AddWithValue("@UserID", input.UserId);
                        theSQLCommand.Parameters.AddWithValue("@StartDateTime", startDate);
                        theSQLCommand.Parameters.AddWithValue("@EndDateTime", endtDate);
                        theSQLCommand.Parameters.AddWithValue("@Description", input.Subject);
                        theSQLCommand.Parameters.AddWithValue("@IsConfirmed", true);
                        if (theSQLCommand.Connection.State == ConnectionState.Closed)
                        {
                            theSQLCommand.Connection.Open();
                        }
                        bookedMeetingID = (int)theSQLCommand.ExecuteScalar();
                    }
                    foreach (SlotForBooking slot in input.BookingSlots)
                    {
                        Room room = lstRooms.Where(t => t.Id == slot.RoomID).FirstOrDefault();
                        if (input.UserId.Contains("@"))
                        {
                            input.RecipientsTo.Add(input.UserId);
                        }
                        string strSQLInner = @"INSERT INTO Recurrence (RoomID,BookedMeetingID,StartDateTime,EndDateTime,IsConfirmed) VALUES(@RoomID,@BookedMeetingID,@StartDateTime,@EndDateTime,@IsConfirmed)";

                        using (SqlCommand theSQLCommandInner = new SqlCommand(strSQLInner, connection))
                        {
                            theSQLCommandInner.Parameters.AddWithValue("@RoomID", room.Id);
                            theSQLCommandInner.Parameters.AddWithValue("@BookedMeetingID", bookedMeetingID);
                            theSQLCommandInner.Parameters.AddWithValue("@StartDateTime", slot.StartDateTime);
                            theSQLCommandInner.Parameters.AddWithValue("@EndDateTime", slot.EndDateTime);
                            try
                            {
                                if (BookAppointment(service, slot, input.Subject, room, input.RecipientsTo, input.RecipientsCC, input.ReminderMinutesBeforeStart))
                                {
                                    theSQLCommandInner.Parameters.AddWithValue("@IsConfirmed", true);
                                }
                                else
                                {
                                    theSQLCommandInner.Parameters.AddWithValue("@IsConfirmed", false);
                                }
                                if (theSQLCommandInner.Connection.State == ConnectionState.Closed)
                                {
                                    theSQLCommandInner.Connection.Open();
                                }
                                theSQLCommandInner.ExecuteNonQuery();
                            }
                            catch (Microsoft.Exchange.WebServices.Data.ServiceRequestException ex)
                            {
                                calendarOutputForBooking.ErrorSlots.Add(new SlotForBooking()
                                {
                                    Message = string.Format("User do not have access on room '{0}'", room.Name),
                                    StartDateTime = slot.StartDateTime,
                                    EndDateTime = slot.EndDateTime,
                                    RoomID = slot.RoomID
                                });
                            }
                            catch (Exception ex)
                            {
                                calendarOutputForBooking.ErrorSlots.Add(new SlotForBooking()
                                {
                                    Message = string.Format("Error occured to book meeting for room '{0}'", room.Name),
                                    StartDateTime = slot.StartDateTime,
                                    EndDateTime = slot.EndDateTime,
                                    RoomID = slot.RoomID
                                });
                            }
                        }
                    }
                    if(calendarOutputForBooking.ErrorSlots!=null && calendarOutputForBooking.ErrorSlots.Count()<=0)
                    {
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                calendarOutputForBooking.Message = "Error occured during meeting booking";
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return calendarOutputForBooking;
        }


        private bool BookAppointment(ExchangeService service, SlotForBooking slot, string subject, Room objRoom, List<string> toRecepient, List<string> ccRecepient, int reminderMinutesBeforeStart)
        {
            Appointment meeting = new Appointment(service);

            // Set the properties on the meeting object to create the meeting.
            meeting.Subject = subject;
            meeting.Body = subject;
            meeting.Start = slot.StartDateTime;
            meeting.End = slot.EndDateTime;
            meeting.Location = objRoom.Name;
            meeting.RequiredAttendees.Add(objRoom.Email);
            if (toRecepient != null)
            {
                foreach (var email in toRecepient)
                {
                    meeting.RequiredAttendees.Add(email);
                }
            }
            if (ccRecepient != null)
            {
                foreach (var email in ccRecepient)
                {
                    meeting.RequiredAttendees.Add(email);
                }
            }
            meeting.ReminderMinutesBeforeStart = reminderMinutesBeforeStart;

            // Save the meeting to the Calendar folder and send the meeting request.
            meeting.Save(SendInvitationsMode.SendToAllAndSaveCopy);

            // Verify that the meeting was created.
            Item item = Item.Bind(service, meeting.Id, new PropertySet(ItemSchema.Subject));
            if (item != null && !string.IsNullOrWhiteSpace(item.Subject))
            {
                return true;
            }
            return false;
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