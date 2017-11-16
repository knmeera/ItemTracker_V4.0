function EndDateValidation()
{

    var ResolvedDateVal = $("#ResolvedDate").val();
    var AssignedDateVal = $("#AssignedDate").val();
    var workCompletedVal = $("#workCompleted").val();


    var ResolvedDateValid = ifNullOrEmpty("ResolvedDate", ResolvedDateVal, "resoDate") && DateValidation("ResolvedDate", AssignedDateVal, ResolvedDateVal, "resoDate");
    var workCompletedValid = IsNumber("workCompleted", workCompletedVal, "WC");

    if (workCompletedValid && ResolvedDateValid) {
        return true;
    }
    else
        return false;
};
function ifNullOrEmpty(fieldId, fieldIdValue, fieldErrMess) {

    if (fieldIdValue == "" || fieldIdValue == null) {
        $("#" + fieldErrMess).text("field can't be null/empty");
        $("#" + fieldId).css("border", "1px solid red");
        $("#" + fieldErrMess).css("color", "red");
        return false;
    }
    else
        $("#" + fieldId).css("border", "1px solid black");
    $("#" + fieldErrMess).text("");
    return true;

};

function IsNumber(fieldId, fieldIdVal, fieldErrMess) {

    if (!/^[0-9]+$/.test(fieldIdVal)) {
        $("#" + fieldErrMess).text("ID Should be a number");
        $("#" + fieldId).css("border", "1px solid red");
        $("#" + fieldErrMess).css("color", "red");
        return false;
    }
    else
        $("#" + fieldErrMess).text("");
    $("#" + fieldId).css("color", "black");
    $("#" + fieldId).css("border", "1px thin black");
    return true;
};

function charVAL(fieldId, fieldIdVal, fieldErrMess) {

    if (!/^[ A-Za-z0-9_.-]*$/.test(fieldIdVal)) {
        $("#" + fieldErrMess).text("invalid spl charecters");
        $("#" + fieldErrMess).css("color", "red");
        $("#" + fieldId).css("border", "1px solid red");
        return false
    }
    else
        $("#" + fieldErrMess).text("");
    $("#" + fieldId).css("color", "black");
    $("#" + fieldId).css("border", "1px solid black");
    return true;
};
function DateValidation(DateAfterId, DateBefore, DateAfter, errmsg) {
    if (new Date(DateBefore) <= new Date(DateAfter))
        return true;
    else {
        $("#" + errmsg).text("this date cannot be greater than assigned date");
        $("#" + DateAfterId).css("border", "1px solid red");
        $("#" + errmsg).css("color", "red");
        return false;
    }
};