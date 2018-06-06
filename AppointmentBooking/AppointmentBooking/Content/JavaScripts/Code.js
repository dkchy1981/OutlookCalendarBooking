function CancelFetchAppointment() {
    $('#availableRooms').css('display', 'none');
    $('#unAvailableRoomsDiv').css("display", "none");
}

function bookAppointment() {

    $("#errorList").empty();

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
    }
    else if (json.Output.Message != null && json.Output.Message != '') {
        $('#messages').css('display', 'block');
        $('#unAvailableRoomsDiv').css("display", "none");
        $('#errorList').append('<li>' + json.Output.Message + '</li>');
    }
    else if (json.Errors.length == 0 && (json.Output.Message == null || json.Output.Message == '')) {
        $('#messages').css('display', 'block');
        $('#unAvailableRoomsDiv').css("display", "none");
        $('#errorList').append("<li>Room booked successfully.</li>");
        CancelFetchAppointment();
    }
});
};


function checkAvailability() {

    $("#Cal").empty();
    $("#CalForNotMatched").empty();
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
        MonthNumber: MonthNumber


    };
    var jqxhr = $.post("FetchAvailability", data, function () { }, 'json')
.done(function (response) {
    if (response instanceof Object)
        var json = response;
    else
        var json = $.parseJSON(response);

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
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + json[i].BookingSlot.StartTime + "</td>");
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + json[i].BookingSlot.EndDate + "</td>");
        tr.append("<td style=\'width:15%\' class=\'tdRoom\'>" + json[i].BookingSlot.EndTime + "</td>");
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
            trForNotMatched = $('<tr/>');
            trForNotMatched.append("<td style=\'width:25%\' class=\'tdRoom\'>" + json[i].BookingSlot.StartDate + "</td>");
            trForNotMatched.append("<td style=\'width:25%\' class=\'tdRoom\'>" + json[i].BookingSlot.StartTime + "</td>");
            trForNotMatched.append("<td style=\'width:25%\' class=\'tdRoom\'>" + json[i].BookingSlot.EndTime + "</td>");
            trForNotMatched.append("<td style=\'width:25%\' class=\'tdRoom\'></td>");
            countForunAvailableRooms++;
        }
        $('#CalForNotMatched').append(trForNotMatched);
    }
    if (countForunAvailableRooms > 0) {
        $('#unAvailableRoomsDiv').css("display", "block");
    }
});
};