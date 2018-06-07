function parseDateTime(time24) {
    var ts = time24;
    var H = +ts.substr(0, 2);
    var h = (H % 12) || 12;
    h = (h < 10) ? ("0" + h) : h;  // leading 0 at the left for 1 digit hours
    var ampm = H < 12 ? " AM" : " PM";
    ts = h + ts.substr(2, 3) + ampm;
    return ts;
};

$(function () {
    $('#StartTime').keypress(function (event) {
        event.preventDefault();
        return false;
    });
    $('#Duration').keypress(function (event) {
        event.preventDefault();
        return false;
    });
});

function CancelFetchAppointment() {
    $('#availableRooms').css('display', 'none');
    $('#unAvailableRoomsDiv').css("display", "none");
    $(".overlay").hide();
    $('#Fetch').css('display', 'block');    
}

function bookAppointment() {
    $(".overlay").show();
    $("#errorList").empty();
    $('#errorList').text('');

    var recurrenceType = 'DailyEveryDay';
    var everySpecifiedWorkingDate = 1;

    //For Weekly
    var selectedWeekDays = [];

    //For Monthly
    var dayVise = false;
    var dayTypeVise = false;

    var Nthday = -1;
    var DayMonth = -1;

    var NthMonthDay = "";
    var DayTypeMonth = "";
    var MonthNumber = -1;



    switch ($("#ActiveSelectionTab").val()) {

        //Daily
        case '1':
            switch ($('input[name=DailySelectionType]:checked').val()) {
                case '1':
                    {
                        recurrenceType = 'DailyEveryDay';
                    }
                    break;
                case '2':
                    {
                        recurrenceType = 'DailyEveryWorkingDay';
                    }
                    break;
                case '3':
                    {
                        recurrenceType = 'DailyEveryNDay';
                        everySpecifiedWorkingDate = $('#DailyRepeatFrequency').val();
                    }
                    break;
            }
            break;

            //Weekly
        case '2':
            $("input:checkbox[name=WeeklySelectionType]:checked").each(function () {
                switch ($(this).val()) {
                    case '1':
                        {
                            selectedWeekDays.push(0);
                        }
                        break;
                    case '2':
                        {
                            selectedWeekDays.push(1);
                        }
                        break;
                    case '3':
                        {
                            selectedWeekDays.push(2);
                        }
                        break;
                    case '4':
                        {
                            selectedWeekDays.push(3);
                        }
                        break;
                    case '5':
                        {
                            selectedWeekDays.push(4);
                        }
                        break;
                    case '6':
                        {
                            selectedWeekDays.push(5);
                        }
                        break;
                    case '7':
                        {
                            selectedWeekDays.push(6);

                        }
                        break;
                }
                recurrenceType = 'Weekly';
            });
            break;

            //Monthly
        case '3':
            {
                recurrenceType = "Monthly";

                switch ($('input[name=MonthlySelectionType]:checked').val()) {
                    case '1':
                        {
                            Nthday = $('#DayWiseSelection-Days').val();
                            DayMonth = $('#DayWiseSelection-Month').val();
                        }
                        break;

                    case '2':
                        {
                            dayTypeVise = true;
                            NthMonthDay = $('#MonthWiseSelection-daytype').val();
                            DayTypeMonth = $('#MonthWiseSelection-day').val();
                            MonthNumber = $('#MonthWiseSelection-Month').val();
                        }
                        break;

                }

            }
            break;

            //Custom
        case '4':
            {
                recurrenceType = 'Custom';
            }
            break;

    }


    var data = {
        FloorID: $('#FloorSelection').val(),
        Capacity: $('#NumberOfAttendees').val(),
        Subject: $('#AppointmentTitle').val(),
        RecurrenceType: recurrenceType,
        DailyNDayInterval: everySpecifiedWorkingDate,
        DayofWeeksForWeekly: selectedWeekDays,
        DayOfMonth_Month: Nthday,
        DayOfMonthInterval_Month: DayMonth,
        DayOfTheWeekIndex_Month: NthMonthDay,
        DayOfTheWeek_Month: DayTypeMonth,
        CustomMonthInterval_Month: MonthNumber,

    };
    var jqxhr = $.post("BookAppointments", data, function () { }, 'json')
.done(function (response) {
    if (response instanceof Object)
        var json = response;
    else
        var json = $.parseJSON(response);

    if (json.Errors.length > 0) {
        $('#messages').css('display', 'block');
        $('#unAvailableRoomsDiv').css("display", "none");

        for (var i = 0; i < json.Errors.length; i++) {
            $('#errorList').append('<li>' + json.Errors[i] + '</li>');
        }
        $('#errorList').css('color', 'red');
    }
    else if (json.Output.Message != null && json.Output.Message != '') {
        $('#messages').css('display', 'block');
        $('#unAvailableRoomsDiv').css("display", "none");

        $('#errorList').append('<li>' + json.Output.Message + '</li>');
        $('#errorList').css('color', 'red');
    }
    else if (json.Errors.length == 0 && (json.Output.Message == null || json.Output.Message == '')) {
        $('#messages').css('display', 'block');
        $('#unAvailableRoomsDiv').css("display", "none");

        $('#errorList').append("<li>Room booked successfully.</li>");
        $('#errorList').css('color', 'green');

        CancelFetchAppointment();
    }
    $(".overlay").hide();

});

};


function checkAvailability() {

    $(".overlay").show();

    $("#Cal").empty();
    $("#CalForNotMatched").empty();
    $('#errorList').text('');
    $('#messages').css('display', 'none');


    var tr;
    tr = $('<tr class=\'trRoomHeader\'/>');
    tr.append("<th style=\'width:40%\' class=\'tdRoom\'>Room Name</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>Start Date</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>Start Time</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>End Date</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>End Time</th>");
    $('#Cal').append(tr);

    var trForNotMatched;
    trForNotMatched = $('<tr class=\'trRoomHeader\'/>');
    trForNotMatched.append("<th style=\'width:30%\' class=\'tdRoom\'>Start Date</th>");
    trForNotMatched.append("<th style=\'width:30%\' class=\'tdRoom\'>Start Time</th>");
    trForNotMatched.append("<th style=\'width:20%\' class=\'tdRoom\'>End Time</th>");
    trForNotMatched.append("<th style=\'width:20%\' class=\'tdRoom\'>Action</th>");
    $('#CalForNotMatched').append(trForNotMatched);


    //For Daily
    var isEveryDay = false;
    var isEveryDayWorking = false;
    var everySpecifiedWorkingDate = -1;

    //For Weekly
    var isSunday = false;
    var isMonday = false;
    var isTuesday = false;
    var isWednesday = false;
    var isThursday = false;
    var isFriday = false;
    var isSaturday = false;

    //For Monthly
    var dayVise = false;
    var dayTypeVise = false;

    var Nthday = -1;
    var DayMonth = -1;

    var NthMonthDay = "";
    var DayTypeMonth = "";
    var MonthNumber = -1;

    //For Custom
    var AppointmentDates = [];

    switch ($("#ActiveSelectionTab").val()) {

        //Daily
        case '1':
            switch ($('input[name=DailySelectionType]:checked').val()) {
                case '1':
                    {
                        isEveryDay = true;
                    }
                    break;
                case '2':
                    {
                        isEveryDayWorking = true;
                    }
                    break;
                case '3':
                    {
                        everySpecifiedWorkingDate = $('#DailyRepeatFrequency').val();
                    }
                    break;

            }
            break;

            //Weekly
        case '2':
            $("input:checkbox[name=WeeklySelectionType]:checked").each(function () {
                switch ($(this).val()) {
                    case '1':
                        {
                            isSunday = true;
                        }
                        break;
                    case '2':
                        {
                            isMonday = true;
                        }
                        break;
                    case '3':
                        {
                            isTuesday = true;
                        }
                        break;
                    case '4':
                        {
                            isWednesday = true;
                        }
                        break;
                    case '5':
                        {
                            isThursday = true;
                        }
                        break;
                    case '6':
                        {
                            isFriday = true;
                        }
                        break;
                    case '7':
                        {
                            isSaturday = true;

                        }
                        break;
                }
            });
            break;

            //Monthly
        case '3': switch ($('input[name=MonthlySelectionType]:checked').val()) {
            case '1':
                {
                    var dayVise = true;
                    Nthday = $('#DayWiseSelection-Days').val();
                    DayMonth = $('#DayWiseSelection-Month').val();
                }
                break;

            case '2':
                {
                    dayTypeVise = true;
                    NthMonthDay = $("#MonthWiseSelection-daytype option:selected").text();
                    DayTypeMonth = $("#MonthWiseSelection-day option:selected").text();
                    MonthNumber = $('#MonthWiseSelection-Month').val();
                }
                break;

        }
            break;

            //Custom
        case '4':
            {
                AppointmentDates.push('2018-06-08');
                AppointmentDates.push('2018-06-09');
                AppointmentDates.push('2018-06-11');
            }
            break;

    }

    var data = {
        Capacity: $('#NumberOfAttendees').val(),
        FloorID: $('#FloorSelection').val(),
        StartDate: $('#StartDate').val(),
        EndtDate: $('#EndDate').val(),
        StartTime: $('#StartTime').val(),
        Duration: $('#Duration').val(),

        RecurrenceType: $('#ActiveSelectionTab').val(),
        //ForDaily
        IsEveryDay: isEveryDay,
        IsEveryDayWorking: isEveryDayWorking,
        EverySpecifiedWorkingDate: everySpecifiedWorkingDate,
        //ForWeekly
        IsSunday: isSunday,
        IsMonday: isMonday,
        IsTuesday: isTuesday,
        IsWednesday: isWednesday,
        IsThursday: isThursday,
        IsFriday: isFriday,
        IsSaturday: isSaturday,
        //For Weekly
        DayVise: dayVise,
        Nthday: Nthday,
        DayMonth: DayMonth,
        DayTypeVise: dayTypeVise,
        NthMonthDay: NthMonthDay,
        DayTypeMonth: DayTypeMonth,
        MonthNumber: MonthNumber,
        AppointmentDates: AppointmentDates
    };
    var jqxhr = $.post("FetchAvailability", data, function () { }, 'json')
.done(function (response) {
    if (response instanceof Object)
        var json = response;
    else
        var json = $.parseJSON(response);

    if (json.Errors.length > 0) {
        $('#messages').css('display', 'block');
        $('#unAvailableRoomsDiv').css("display", "none");

        for (var i = 0; i < json.Errors.length; i++) {
            $('#errorList').append('<li>' + json.Errors[i] + '</li>');
        }
        $('#errorList').css('color', 'red');
    }
    else if (json.NeedToLogout) {
        window.location.href = '../login'
    }
    else {
        BindGrid(json.AvailableRooms);
        $('#Fetch').css('display', 'none');
    }
    $(".overlay").hide();
});
};


function checkTimeSlotforConflict(startDate, id) {
    //For Daily
    var isEveryDay = true;
    var isEveryDayWorking = false;
    var everySpecifiedWorkingDate = -1;

    $("#errorList").empty();

    var data = {
        Capacity: $('#NumberOfAttendees').val(),
        FloorID: $('#FloorSelection').val(),
        StartDate: startDate,
        EndtDate: startDate,
        StartTime: $('#StartTime' + id).val(),
        Duration: $('#Duration').val(),

        RecurrenceType: 1,
        //ForDaily
        IsEveryDay: isEveryDay,
        IsEveryDayWorking: isEveryDayWorking,
        EverySpecifiedWorkingDate: everySpecifiedWorkingDate,

    };
    $(".overlay").show()
    var jqexhr = $.post("FetchNewAvailableSlot", data, function () { }, 'json')
    .done(function (response) {
        if (response instanceof Object)
            var editJson = response;
        else
            var editJson = $.parseJSON(response);

        if (editJson.Errors != null && editJson.Errors.length > 0) {
            $('#messages').css('display', 'block');

            for (var i = 0; i < editJson.Errors.length; i++) {
                $('#errorList').append('<li>' + editJson.Errors[i] + '</li>');
            }
            $('#errorList').css('color', 'red');
        }
        else {
            $('#messages').css('display', 'none');
        }

        for (var k = 0; k < editJson.length; k++) {
            if (editJson[k].IsAvailable == true) {
                $("#tdAvailable" + id).css("display", "none");
                $("#tdConfirm" + id).css("display", "block");

            }
        }
        $(".overlay").hide()
    });

};


function confirmNewTimeSlotforConflict(startDate, id) {
    $(".overlay").show()
    var jqexhr = $.post("ConfirmNewAvailableSlot", function () { }, 'json')
    .done(function (response) {
        if (response instanceof Object)
            var editJson = response;
        else
            var editJson = $.parseJSON(response);

        BindGrid(editJson);

        var allNotAvailable = true;

        for (var k = 0; k < editJson.length; k++) {
            if (editJson[k].IsAvailable != true) {
                allNotAvailable = false;
                break;
            }
        }

        if (allNotAvailable) {
            $("#CalForNotMatched").empty();
            $('#unAvailableRoomsDiv').css("display", "none");
        }
        $(".overlay").hide()
    });


};

function BindGrid(json) {

    $("#Cal").empty();
    $("#CalForNotMatched").empty();


    var tr;
    tr = $('<tr class=\'trRoomHeader\'/>');
    tr.append("<th style=\'width:40%\' class=\'tdRoom\'>Room Name</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>Start Date</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>Start Time</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>End Date</th>");
    tr.append("<th style=\'width:15%\' class=\'tdRoom\'>End Time</th>");
    $('#Cal').append(tr);
    //Append each row to html table
    for (var i = 0; i < json.length; i++) {
        if (json[i].IsAvailable == true) {
            tr = $('<tr/>');
        }
        else {
            tr = $('<tr class=\'trRoom\'/>');
        }
        tr.append("<td style=\'width:40%\' class=\'tdRoom\'>" + json[i].RoomName + "</td>");
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + json[i].BookingSlot.StartDate + "</td>");
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + parseDateTime(json[i].BookingSlot.StartTime) + "</td>");
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + json[i].BookingSlot.EndDate + "</td>");
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + parseDateTime(json[i].BookingSlot.EndTime) + "</td>");
        $('#Cal').append(tr);
    }
    if (json.length > 0) {
        $('#availableRooms').css('display', 'block');
    }



    var trForNotMatched;
    trForNotMatched = $('<tr class=\'trRoomHeader\'/>');
    trForNotMatched.append("<th style=\'width:30%\' class=\'tdRoom\'>Start Date</th>");
    trForNotMatched.append("<th style=\'width:30%\' class=\'tdRoom\'>Start Time</th>");
    trForNotMatched.append("<th style=\'width:20%\' class=\'tdRoom\'>End Time</th>")
    trForNotMatched.append("<th style=\'width:20%\' class=\'tdRoom\'>Action</th>");

    $('#CalForNotMatched').append(trForNotMatched);
    var countForunAvailableRooms = 0;
    //Append each row to html table
    for (var i = 0; i < json.length; i++) {
        if (json[i].IsAvailable == false) {
            var changeSlot = json[i];
            trForNotMatched = $('<tr/>');
            trForNotMatched.append("<td style=\'width:25%\' class=\'tdRoom\'>" + json[i].BookingSlot.StartDate + "</td>");
            trForNotMatched.append("<td style=\'width:25%;padding:3px\' class=\'tdRoom\'>  <input id=\'StartTime" + i + "\' name=\'StartTime\' class=\'TimeInputGrd\' type=\'text\' style=\'width:100px\' value=\'" + json[i].BookingSlot.StartTime + "\' onchange=onChangeTimeSlot(" + i + ") /> </td>");
            trForNotMatched.append("<td style=\'width:25%\' class=\'tdRoom\'> <span id=EndTime" + i + "> " + parseDateTime(json[i].BookingSlot.EndTime) + " </span> </td>");
            trForNotMatched.append("<td id=\'tdAvailable" + i + "\' style=\'width:25%\;display:block' class=\'tdRoom\'> <img id=\'actionImg" + i + "\' style=\'cursor:pointer\'  width='100px' src=\'../Content/Images/Checkavailablility.JPG\' onclick=checkTimeSlotforConflict('" + changeSlot.BookingSlot.StartDate + "','" + i + "')  /> </td>");
            trForNotMatched.append("<td id=\'tdConfirm" + i + "\' style=\'width:25%;display:none\', class=\'tdRoom\'> <img id=\'actionImg" + i + "\' style=\'cursor:pointer\'  width='100px' src=\'../Content/Images/ConfirmImg.JPG\' onclick=confirmNewTimeSlotforConflict('" + changeSlot.BookingSlot.StartDate + "','" + i + "') /> </td>");

            countForunAvailableRooms++;

        }
        $('#CalForNotMatched').append(trForNotMatched);

        var strScript = "<script src=\'http://localhost/AppointmentBooking/Content/Plugins/jquery-timepicker-1.11.13/jquery.timepicker.min.js\'><";
        strScript += "/script>";
        strScript += "<script>$('.TimeInputGrd').timepicker({  timeFormat: 'h:i A' , step: 15, minTime: '10', maxTime: '10:00pm', defaultTime: 'now', startTime: '10:00', dynamic: false, dropdown: true, scrollbar: true, scrollDefault : 'now'});";
        strScript += "</script>";
        strScript += "<script> $('.TimeInputGrd').keypress(function (event) { event.preventDefault(); return false; });</script>";

        $("#editTimeSlot").append(strScript);
    }
    if (countForunAvailableRooms > 0) {
        $('#unAvailableRoomsDiv').css("display", "block");
        $('#messages').css('display', 'none');
    }
    else {
        $('#unAvailableRoomsDiv').css("display", "none");
        $('#errorList').append("<li>You are good to go for book meetings.</li>");
        $('#messages').css('display', 'block');
        $('#errorList').css('color', 'green');
    }
}
function onChangeTimeSlot(id) {
    var newTime = convertTo24Hour($("#StartTime" + id).val().toLowerCase());
    var time = new Date('1970-01-01T' + newTime);
    DurationTime = String($('#Duration').val()).split(":");
    var DurHour = parseInt(DurationTime[0]);
    var DurMin = parseInt(DurationTime[1]);


    var hours = time.getHours() + DurHour;
    var minutes = time.getMinutes() + DurMin;
    var ampm = hours >= 12 ? 'PM' : 'AM';

    minutes = minutes < 10 ? '0' + minutes : minutes;
    if (minutes >= 60) {
        hours = hours + Math.floor(minutes / 60);
        minutes = minutes % 60;
    }
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    var strTime = hours + ':';
    if (String(minutes).length < 2) {
        strTime = strTime + '0' + minutes;
    }
    else {
        strTime = strTime + minutes;
    }
    strTime = strTime + ' ' + ampm;

    $("#EndTime" + id)[0].innerText = strTime;

    $("#tdAvailable" + id).css("display", "block");
    $("#tdConfirm" + id).css("display", "none");
}

function convertTo24Hour(time) {
    var hours = parseInt(time.substr(0, 2));
    var strHours;
    if (hours < 10)
        strHours = '0' + hours;
    if (time.indexOf('am') != -1 && hours == 12) {
        time = time.replace('12', '0');
    }
    if (time.indexOf('pm') != -1 && hours < 12) {
        time = time.replace(strHours, (hours + 12));
    }
    return time.replace(/(am|pm)/, '').trim();
}