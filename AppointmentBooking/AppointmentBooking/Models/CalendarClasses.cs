using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppointmentBooking.Models
{
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

        public string StartDate { get { return StartDateTime.ToString("dd-MM-yyyy"); } }
        public string StartTime { get { return StartDateTime.ToString("HH:mm"); } }

        public string EndDate { get { return EndDateTime.ToString("dd-MM-yyyy"); } }
        public string EndTime { get { return EndDateTime.ToString("HH:mm"); } }
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

    public class RecurrenceInfo
    {
        public int Capacity { get; set; }

        public int FloorID { get; set; }

        public string StartDate { get; set; }

        public string EndtDate { get; set; }

        public string StartTime { get; set; }
                
        public string Duration { get; set; }

        public int RecurrenceType { get; set; }

        public bool IsEveryDay { get; set; }

        public bool IsEveryDayWorking { get; set; }

        public int EverySpecifiedWorkingDate { get; set; }

        public bool IsSunday { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }

    }
}