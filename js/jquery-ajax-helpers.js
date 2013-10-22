/*
* jQuery Ajax Call Wrappers
* Mostly, for use with ASP.NET WebMethods
* 
* Copyright (c) 2013 Ren Ramkhelawan
* MIT license.
* http://www.opensource.org/licenses/mit-license.php
*
* If you need any further information about this script please
* visit my homepage or contact me under ren@arkitekt.ca
*/

//methodName: should be in form PageName.aspx/MethodName
//parms: should be in form "{'parm1':'value1', 'parm2':'value2'}"
function jQueryCallWrapper(methodName, parms, successFunc, errorFunc, successFuncParm) {
    var parmString = formatParmsForPost(parms);
    $.ajax({
        type: "POST",
        url: methodName,
        data: parmString,
        contentType: "application/json; charset=utf-8",
        success: function(msg) { successFunc(msg, successFuncParm); },
        error: function(msg) { errorFunc(msg); }
        /*,
        dataFilter: function(data) {
        //jQuery 1.4 now has a jQuery.parseJSON method that does the same, so this can be switched
        //at any point.
            if (typeof (JSON) !== 'undefined' && typeof (JSON.parse) === 'function') {
                return JSON.parse(data);
            }
            else {
                return eval('(' + data + ')');
            }
        }*/
    });
}
//methodName: should be in form PageName.aspx/MethodName
//parms: should be in form parmName=%22input%22 (%22 = quotation marks)
//25/07/2011: simplified this function according to: http://encosia.com/save-yourself-some-typing-when-you-call-asp-net-services/
function jQueryCallWrapperGET(methodName, parms, successFunc, errorFunc, successFuncParm) {
    var parmString = formatParmsForQueryString(parms);
    $.ajax({
        type: "GET",
        url: methodName + "?" + parmString,
        contentType: "application/json; charset=utf-8",
        success: function(response) { successFunc(response, successFuncParm); },
        error: function(msg) { errorFunc(msg); }
        /*,
        dataFilter: function(data, type) {
            //jQuery 1.4 now has a jQuery.parseJSON method that does the same, so this can be switched
            //at any point.
            var prsd;
            if (typeof (JSON) !== 'undefined' && typeof (JSON.parse) === 'function') {
                prsd = JSON.parse(data);
                return prsd;
            }
            else {
                prsd = jQuery.parseJSON(data); //eval('(' + data + ')');
                return prsd;
            }
        }*/
    });
}

function formatParmsForPost(parms) {
    var output;
    if (parms instanceof Array) {
        output = formatSimpleParmsForPost(parms);
    }
    else if (parms instanceof Object) {
        if (typeof (JSON) !== 'undefined' && typeof (JSON.stringify) === 'function') {
            output = JSON.stringify(parms);
        }
        else {
            alert("JSON parser not present");
        }

    }
    return output;
}
//expects array of strings in format parmName=input
function formatSimpleParmsForPost(parms){
    var out = "{", tempArr = null, i = 0;

    for (i = 0; i < parms.length; i++) {
        tempArr = parms[i].split('=');
        out += "'" + tempArr[0] + "':'" + tempArr[1].replace(/'/g, "\\'").replace(/"/g, "\\\"") + "'" + (i == (parms.length - 1) ? '' : ',');
    }
    return out + "}";
}
//expects array of strings in format parmName=input
function formatParmsForQueryString(parms) {
    var out = "", tempArr = null, i = 0;
    for (i = 0; i < parms.length; i++) {
        tempArr = parms[i].split('=');
        out += tempArr[0] + '=%22' + tempArr[1] + '%22' + (i == (parms.length - 1) ? '' : '&');
    }
    return out;
}
//Use when transferring complex object types.
//DTO (Data Transfer Object): Used to encapsulate Web Method parameters. Ex. DTO['parmName1'] = parmObj1; DTO['parmName2'] = parmObj2; etc.
//At this point, because the method below uses JSON.stringify, the page must reference "/jquery/json2.js".
function jQueryCallWrapperDTO(methodName, DTO, successFunc, errorFunc, successFuncParm) {
    var s=JSON.stringify(DTO);
    $.ajax({
        type: "POST",
        url: methodName,
        data: s,
        contentType: "application/json; charset=utf-8",
        success: function(msg) { successFunc(msg, successFuncParm); },
        error: function(msg) { errorFunc(msg); }
        /*,
        dataFilter: function(data) {
        //jQuery 1.4 now has a jQuery.parseJSON method that does the same, so this can be switched
        //at any point.
            if (typeof (JSON) !== 'undefined' && typeof (JSON.parse) === 'function') {
                return JSON.parse(data);
            }
            else {
                return eval('(' + data + ')');
            }
        }*/
    });
}

function genAjaxErrorFunc(response) {
    var error = response.d;
    console.log(error);
    //alert(error);
}