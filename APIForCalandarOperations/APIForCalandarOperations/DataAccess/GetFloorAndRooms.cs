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
        string timeFormat = "HH:mm";
        string dateFormat = "yyyy/MM/dd ";

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
                ExchangeService service = GetExchangeService(input.UserId, input.Password);
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
                ExchangeService service = GetExchangeService(input.UserId, input.Password);

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

                    List<SlotForBooking> recurrSlots = ExtractGroupsBasedOnRoomAndStartEndTime(input.BookingSlots, input.RecurrenceType);

                    foreach (SlotForBooking slot in recurrSlots)
                    {
                        Room room = lstRooms.Where(t => t.Id == slot.RoomID).FirstOrDefault();
                        string strSQLInner = @"INSERT INTO Recurrence (RoomID,BookedMeetingID,StartDateTime,EndDateTime,IsConfirmed) VALUES(@RoomID,@BookedMeetingID,@StartDateTime,@EndDateTime,@IsConfirmed)";

                        using (SqlCommand theSQLCommandInner = new SqlCommand(strSQLInner, connection))
                        {
                            theSQLCommandInner.Parameters.AddWithValue("@RoomID", room.Id);
                            theSQLCommandInner.Parameters.AddWithValue("@BookedMeetingID", bookedMeetingID);
                            theSQLCommandInner.Parameters.AddWithValue("@StartDateTime", slot.StartDateTime);
                            theSQLCommandInner.Parameters.AddWithValue("@EndDateTime", slot.EndDateTime);
                            try
                            {
                                if (BookAppointment(service, slot, input, room))
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
                                calendarOutputForBooking.Message = string.Format("User do not have access on room '{0}'", room.Name);
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
                                calendarOutputForBooking.Message = string.Format("Error occured to book meeting for room '{0}'", room.Name);
                            }
                        }
                    }
                    if (calendarOutputForBooking.ErrorSlots != null && calendarOutputForBooking.ErrorSlots.Count() <= 0)
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

        private List<SlotForBooking> ExtractGroupsBasedOnRoomAndStartEndTime(List<SlotForBooking> bookingSlots, string recurrenceType)
        {
            if (recurrenceType == "DailyEveryDay" || recurrenceType == "DailyEveryWorkingDay" || recurrenceType == "DailyEveryNDay" || recurrenceType == "Weekly" || recurrenceType == "Monthly")
            {
                List<SlotForBooking> groupsBasedOnRoomAndStartEndTime = new List<SlotForBooking>();

                int roomId = 0;
                DateTime startDate = DateTime.MinValue;
                DateTime endtDate = DateTime.MinValue;
                string startTime = "";
                string endTime = "";
                foreach (SlotForBooking item in bookingSlots.OrderBy(t => t.RoomID).OrderBy(t => t.StartDateTime).ToList())
                {
                    if (roomId == 0)
                    {
                        roomId = item.RoomID;
                    }
                    if (startDate == DateTime.MinValue)
                    {
                        startDate = item.StartDateTime.Date;
                    }
                    if (string.IsNullOrWhiteSpace(startTime))
                    {
                        startTime = item.StartDateTime.ToString(timeFormat);
                    }
                    if (string.IsNullOrWhiteSpace(endTime))
                    {
                        endTime = item.EndDateTime.ToString(timeFormat);
                    }
                    if (roomId != item.RoomID || startTime != item.StartDateTime.ToString(timeFormat))
                    {
                        groupsBasedOnRoomAndStartEndTime.Add(new SlotForBooking() { RoomID = roomId, StartDateTime = DateTime.Parse(startDate.ToString(dateFormat) + startTime), EndDateTime = DateTime.Parse(endtDate.ToString(dateFormat) + endTime) });
                        roomId = item.RoomID;
                        startDate = item.StartDateTime.Date;
                        startTime = item.StartDateTime.ToString(timeFormat);
                        endTime = item.EndDateTime.ToString(timeFormat);
                    }
                    endtDate = item.EndDateTime.Date;
                }
                groupsBasedOnRoomAndStartEndTime.Add(new SlotForBooking() { RoomID = roomId, StartDateTime = DateTime.Parse(startDate.ToString(dateFormat) + startTime), EndDateTime = DateTime.Parse(endtDate.ToString(dateFormat) + endTime) });

                return groupsBasedOnRoomAndStartEndTime;
            }
            else
            {
                return bookingSlots;
            }
        }

        private bool BookAppointment(ExchangeService service, SlotForBooking slot, CalendarInputForBooking input, Room objRoom)
        {
            Appointment meeting = new Appointment(service);

            // Set the properties on the meeting object to create the meeting.
            meeting.Subject = input.Subject;
            meeting.Body = input.Subject;
            meeting.Location = objRoom.Name;
            meeting.Start = slot.StartDateTime;
            DateTime startDateWithEndTime = DateTime.Parse(slot.StartDateTime.ToString(dateFormat) + slot.EndDateTime.ToString(timeFormat));
            meeting.End = startDateWithEndTime;
            meeting.RequiredAttendees.Add(objRoom.Email);
            if (input.RecipientsTo != null)
            {
                foreach (var email in input.RecipientsTo)
                {
                    meeting.RequiredAttendees.Add(email);
                }
            }
            if (input.RecipientsCC != null)
            {
                foreach (var email in input.RecipientsCC)
                {
                    meeting.RequiredAttendees.Add(email);
                }
            }
            meeting.ReminderMinutesBeforeStart = input.ReminderMinutesBeforeStart;


            if (input.RecurrenceType == "DailyEveryDay" || input.RecurrenceType == "DailyEveryWorkingDay" || input.RecurrenceType == "DailyEveryNDay" || input.RecurrenceType == "Weekly" || input.RecurrenceType == "Monthly")
            {
                if (input.RecurrenceType == "DailyEveryDay" || input.RecurrenceType == "DailyEveryNDay")
                {
                    meeting.Recurrence = new Recurrence.DailyPattern(meeting.Start.Date, input.DailyNDayInterval);
                    meeting.Recurrence.StartDate = slot.StartDateTime.Date;
                    meeting.Recurrence.EndDate = slot.EndDateTime.Date;
                }
                else if (input.RecurrenceType == "DailyEveryWorkingDay")
                {
                    meeting.Recurrence = new Recurrence.WeeklyPattern(meeting.Start.Date, 1, new DayOfTheWeek[] { DayOfTheWeek.Monday, DayOfTheWeek.Tuesday, DayOfTheWeek.Wednesday, DayOfTheWeek.Thursday, DayOfTheWeek.Friday });
                    meeting.Recurrence.StartDate = slot.StartDateTime.Date;
                    meeting.Recurrence.EndDate = slot.EndDateTime.Date;
                }
                else if (input.RecurrenceType == "Weekly")
                {
                    meeting.Recurrence = new Recurrence.WeeklyPattern(meeting.Start.Date, 1, input.DayofWeeksForWeekly);
                    meeting.Recurrence.StartDate = slot.StartDateTime.Date;
                    meeting.Recurrence.EndDate = slot.EndDateTime.Date;
                }
                else if (input.RecurrenceType == "Monthly" && input.DayOfMonth_Month > 0 && input.DayOfMonthInterval_Month > 0)
                {
                    meeting.Recurrence = new Recurrence.MonthlyPattern(meeting.Start.Date, input.DayOfMonthInterval_Month, input.DayOfMonth_Month);
                    meeting.Recurrence.StartDate = slot.StartDateTime.Date;
                    meeting.Recurrence.EndDate = slot.EndDateTime.Date;
                }
                else if (input.RecurrenceType == "Monthly" && input.CustomMonthInterval_Month > 0)
                {
                    meeting.Recurrence = new Recurrence.RelativeMonthlyPattern(meeting.Start.Date, input.CustomMonthInterval_Month, (DayOfTheWeek)input.DayOfTheWeek_Month, (DayOfTheWeekIndex)input.DayOfTheWeekIndex_Month);
                    meeting.Recurrence.StartDate = slot.StartDateTime.Date;
                    meeting.Recurrence.EndDate = slot.EndDateTime.Date;
                }
            }

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


        private ExchangeService GetExchangeService(string userEmail = "", string userPassword = "")
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                userEmail = System.Configuration.ConfigurationManager.AppSettings["UserAccountEmail"];
            }
            if (string.IsNullOrWhiteSpace(userPassword))
            {
                userPassword = System.Configuration.ConfigurationManager.AppSettings["Password"];
            }

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

        public bool CheckExchangeServiceForProvidedUserDetails(string userEmail, string userPassword)
        {
            try
            {
                ExchangeService service = new ExchangeService();

                // Set specific credentials.
                service.Credentials = new NetworkCredential(userEmail, userPassword);

                // Look up the user's EWS endpoint by using Autodiscover.
                //service.AutodiscoverUrl(userEmailAddress, RedirectionCallback);
                service.Url = new Uri("https://mail.civica.com/ews/exchange.asmx");

                FolderId CalendarFolderId = new FolderId(WellKnownFolderName.Calendar, userEmail);
                CalendarView cv = new CalendarView(DateTime.Now, DateTime.Now.AddHours(1));
                service.FindAppointments(CalendarFolderId, cv);


                return true;
            }
            catch (Microsoft.Exchange.WebServices.Data.ServiceRequestException ex)
            {
                return false;
            }
        }


    }
}