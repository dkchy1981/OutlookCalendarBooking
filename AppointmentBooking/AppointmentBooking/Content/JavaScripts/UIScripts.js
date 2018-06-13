function openTab(evt, TabName)
{
    var i, tabcontent, tablinks;

    tabcontent = document.getElementsByClassName("tabcontent");

    for (i = 0; i < tabcontent.length; i++)
    {
        tabcontent[i].style.display = "none";
    }


    tablinks = document.getElementsByClassName("tablinks");

    for (i = 0; i < tablinks.length; i++)
    {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    document.getElementById(TabName).style.display = "block";

    evt.className += " active";
}

function SelectSelection(element)
{
    switch(element.value)
    {
        case 'Daily':
            document.getElementById("ActiveSelectionTab").value = 1;
            document.getElementById("StartDate").disabled = false;
            document.getElementById("EndDate").disabled = false;           
            openTab(element, 'Daily')
            break;
        case 'Weekly':
            document.getElementById("ActiveSelectionTab").value = 2;
            document.getElementById("StartDate").disabled = false;
            document.getElementById("EndDate").disabled = false;
            openTab(element, 'Weekly')
            break;
        case 'Monthly':
            document.getElementById("ActiveSelectionTab").value = 3;
            document.getElementById("StartDate").disabled = false;
            document.getElementById("EndDate").disabled = false;
            openTab(element, 'Monthly')
            break;
        case 'Custom':
            document.getElementById("ActiveSelectionTab").value = 4;
            document.getElementById("StartDate").disabled = true;
            document.getElementById("EndDate").disabled = true;
            openTab(element, 'Custom')
            break;

    }
}

function DailyRadioCheck()
{
    if (document.getElementById("EveryDay").checked == true)
    {
        document.getElementById("DailyRepeatFrequency").disabled = true;
    }
    else if (document.getElementById("EveryWorkingDay").checked == true)
    {
        document.getElementById("DailyRepeatFrequency").disabled = true;
    }
    else
    {
        document.getElementById("DailyRepeatFrequency").disabled = false;
    }
}

function MonthlyRadioCheck()
{

    if(document.getElementById("DayWise").checked == true)
    {
        document.getElementById("MonthWiseSelection-daytype").disabled = true;
        document.getElementById("MonthWiseSelection-day").disabled = true;
        document.getElementById("MonthWiseSelection-Month").disabled = true;

        document.getElementById("DayWiseSelection-Days").disabled = false;
        document.getElementById("DayWiseSelection-Month").disabled = false;
    }
    else
    {
        document.getElementById("DayWiseSelection-Days").disabled = true;
        document.getElementById("DayWiseSelection-Month").disabled = true;
        document.getElementById("MonthWiseSelection-daytype").disabled = false;
        document.getElementById("MonthWiseSelection-day").disabled = false;
        document.getElementById("MonthWiseSelection-Month").disabled = false;
    }
}

function LimitNumber(element,LowerLimit,UpperLimit)
{
    LimitUpper(element, UpperLimit);
    LimitLower(element, LowerLimit);
}

function LimitUpper(element, UpperLimit)
{

    if (element.value > UpperLimit)
    {
        element.value = UpperLimit;
    }
}
function LimitLower(element, LowerLimit)
{

    if (element.value < LowerLimit)
    {
        element.value = LowerLimit;
    }
}

window.onload = function () {
    if (document.getElementById("defaultOpen") != null) {
        SelectSelection(document.getElementById("defaultOpen"));
    }
};

//Radio
function customRadio(radioName) {
    var radioButton = $('input[name="' + radioName + '"]');
    $(radioButton).each(function () {
        $(this).wrap("<span class='custom-radio'></span>");
        if ($(this).is(':checked')) {
            $(this).parent().addClass("selected");
        }
    });
    $(radioButton).click(function () {
        if ($(this).is(':checked')) {
            $(this).parent().addClass("selected");
        }
        $(radioButton).not(this).each(function () {
            $(this).parent().removeClass("selected");
        });
    });
}

function customCheckbox(checkboxName) {
    var checkBox = $('input[name="' + checkboxName + '"]');
    $(checkBox).each(function () {
        $(this).wrap("<span class='custom-checkbox'></span>");
        if ($(this).is(':checked')) {
            $(this).parent().addClass("selected");
        }
    });
    $(checkBox).click(function () {
        $(this).parent().toggleClass("selected");
    });
}


$(document).ready(function () {
    customRadio("DailySelectionType");
    customRadio("MonthlySelectionType");
    customCheckbox("WeeklySelectionType");
})