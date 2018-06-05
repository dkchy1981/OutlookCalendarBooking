using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIForCalandarOperations.Models
{
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

        public int RoomID { get; set; }

        public Slot BookingSlot { get; set; }

        public List<string> Messages { get; set; }

        public CalendarOutput()
        {
            this.Messages = new List<string>();
        }
    }



    public class CalendarInputForBooking
    {
        public int FloorID { get; set; }

        public int Capacity { get; set; }

        public string UserId { get; set; }

        public List<SlotForBooking> BookingSlots { get; set; }

        public string Subject { get; set; }

        public List<string> RecipientsTo { get; set; }

        public List<string> RecipientsCC { get; set; }

        public int ReminderMinutesBeforeStart { get; set; }

        public string RecurrenceType { get; set; }

        public int DailyNDayInterval { get; set; }

        public DayOfTheWeek[] DayofWeeksForWeekly { get; set; }
    }

    public class SlotForBooking
    {
        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public int RoomID { get; set; }

        public string Message { get; set; }
    }


    public class CalendarOutputForBooking
    {

        public List<SlotForBooking> ErrorSlots { get; set; }

        public string Message { get; set; }

        public CalendarOutputForBooking()
        {
            this.ErrorSlots = new List<SlotForBooking>();
        }
    }
}