var session = "";
var authorizationCode = "";
var sessionID = "";
var authScheme = [];
var preid = "USER";
var voiceFormValue = "";
var userName = "";
var errorDivId = "PASSWORDerror";
var PushNotificationCode = "";
var journeyToken = "";
var codeVerfierInterval = "";
var TimerInterval = "";
var Fido2Options = "";
var QrCode = "";
var client_id = "";
var clientType = "";
var redirectUrl = "";
var state = "";
var ErrorConstant = "";
var LoginControllerUrl = "";
var modal = null;
var selectAuthScheme = null;
var currentAuth = "";
var base64Data = "";
var captured = 0;
var timeoutId;
var detectionid;

var activateSuspendedAccount = null;
var VerifierCode = "";

var mediaStream;
var mediaRecorder;
var recordedChunks = [];
var audioContext;
var analyser;
var audiocanvas;
var audiocanvasnum;
var audiocanvasCtx;
var audiocanvasCtxnum;
var audioPlayback;
var audioPlaybacknum;
var base64audio;

var authenticationScheme = "";
var authenticationMethods = null;


async function init() {

    LoginControllerUrl = window.location.pathname;

    const checkPwdBtn = document.getElementById("checkPassword");
    checkPwdBtn.addEventListener("click", authenticate);

    deleteCookies();
    $('#networkOverlay').hide();
}

$(".toggle-password").click(function () {
    $(this).toggleClass("fa-eye fa-eye-slash");
    var input = $($(this).attr("toggle"));
    if (input.hasClass("pwd")) {
        input.removeClass("pwd");
    } else {
        input.addClass("pwd");
    }
});

$("input").keypress(function (event) {
    if (event.which == 13) {
        event.preventDefault();
        if (this.form.id == "UserForm")
            VerifyUser();
        else
            authenticate(this.form.id);
    }
});

function show() {

    var id = authScheme.pop();
    currentAuth = id;
    if (id == undefined) {

        $('#overlay').hide();
        $(':button').prop('disabled', true);
        var url = LoginControllerUrl + window.location.search
        postForm();

    } else {

        if (preid) {
            var div1 = $("#" + preid)
            div1.css("display", "none");
        }
        id = id.toUpperCase();
        errorDivId = id + "error";
        var userNameid = id + "userName";
        if (id != "WEB_FACE" && id != "VOICE") {
            document.getElementById(userNameid).value = userName;
        }
        preid = id;
        var div = $("#" + id)
        if (div.length) {
            div.css("display", "block");
        }
        else
            swal("Error", "The " + id + " Authentication Scheme is not available")
    }
}

function ValidateNumber(userName) {
    if (userName.startsWith("+256")) {
        if (userName.length != 13) {
            return null;
        }
    } else if (userName.startsWith("256")) {
        if (userName.length != 12) {
            return null;
        }
        userName = "+" + userName;

    } else if (userName.startsWith("+91")) {
        if (userName.length != 13) {
            return null;
        }
    }
    else if (userName.startsWith("91")) {
        if (userName.length != 12) {
            return null;
        }
        userName = "+" + userName;
    }
    else if (userName.startsWith("0")) {
        if (userName.length == 10) {
            userName = userName.replace("0", "+256");
        }
        else if (userName.length == 11) {
            userName = userName.replace("0", "+91");
        } else {
            return null;
        }
    }
    else if (userName.length == 9) {
        userName = "+256" + userName;
    }

    else if (userName.length == 10) {
        userName = "+91" + userName;
    }
    else {
        return null
    }

    return userName
}

function authenticate(FormID) {

    if (!navigator.cookieEnabled) {
        document.getElementById("browserErrordiv").style.display = "flex"
        document.getElementById("browserError").innerHTML = "Your browser cookies option is disabled.\ Please enable cookies option \n Go browser setting -> Cookies and other site data -> Enable 'Allow all cookies'!";
        return false;
    }

    var formName = ""
    if (typeof FormID == "string")
        formName = FormID;
    else
        formName = $(this).parents("form").attr("id");

    var form = new FormData(document.getElementById(formName));
    var password = form.get("password");

    var mail = form.get("PASSWORDuserName");

    if (!password) {

        var id = form.get("authScheme").toLowerCase();
        var element = document.getElementById(id);
        element.classList.add("invalid");
        document.getElementById("password").style.boxShadow = "0px 10px 10px -1px #bfd01f";
        document.getElementById(errorDivId).className = "danger";
        document.getElementById(errorDivId).style.visibility = "visible";
        var msg = (id != "email") ? id : id + " otp";
        document.getElementById(errorDivId).innerHTML = "Please enter " + msg
        return false;

    } else {
        $.ajax({
            type: 'POST',
            url: document.getElementById('authUrl').value,
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify({
                userEmail: form.get("PASSWORDuserName"),
                authenticationData: form.get("password")
            }),
            beforeSend: showLoder,
            complete: hideLoder,
            error: ErrorHandler,
            success: function (result, status, xhr) {
                if (result.success) {
                    if (result && result.result && result.result.randomCode) {
                        PushNotificationCode = result.result.randomCode;
                    }
                    if (document.getElementById(errorDivId).style.visibility == "visible") {
                        document.getElementById(errorDivId).style.visibility = "hidden"
                        document.getElementById(errorDivId).className = "info"
                    }

                    show();
                }
                else {
                    document.getElementById("password").value = "";
                    if (result.errorCode == 106) {
                        swal({ type: 'info', title: result.message, text: "Click on 'Ok' button for login again!" }, function (isConfirm) {
                            if (isConfirm) {
                                deleteCookies();
                                window.location.reload();
                            }
                        });
                    }
                    else if (result.errorCode == 103) {
                        swal({ type: 'info', title: "Account Suspended", text: result.message }, function (isConfirm) {
                            if (isConfirm) {
                                clearInterval(codeVerfierInterval);
                                deleteCookies();
                                window.location.reload();
                            }
                        });
                    } else {
                        document.getElementById(errorDivId).className = "danger";
                        document.getElementById(errorDivId).style.visibility = "visible"
                        document.getElementById(errorDivId).innerHTML = result.message;
                    }
                    return false;
                }
            }

        })
    }
}
function showLoder() {
    $('#overlay').show();
}
function hideLoder() {
    $('#overlay').hide();
}
function ErrorHandler(xhr, status, error) {

    switch (xhr.status) {
        case 400:
            document.getElementById(errorDivId).className = "danger";
            document.getElementById(errorDivId).style.visibility = "visible"
            document.getElementById(errorDivId).innerHTML = xhr.responseJSON.message
            break;
        case 403:
            swal("Forbidden", "You do not have access to this resource", "error");
            break;
        case 404:
            swal("Abort", "The resource you requested could not be found", "error");
            break;

        case 500:
            swal({ title: "Server Error", text: "Internal Server error Occurred", type: "error" }, function (isConfirm) {
                if (isConfirm) {
                    deleteCookies();
                    window.location.reload();
                }
            });
            break;

        case 502:
            swal("Bad Gateway", "Invalid response", "error");
            break;
        case 503:
            swal("Service unavailable", "The Service is currently unavailable, please try after sometime.", "error");
            break;
        default:
            errors = ["Bad Request", "Bad Gateway", "Not Found", "Internal Server Error", "Forbidden", "Unauthorized", "Service Unavailable"]
            if (xhr.readyState == 0 && xhr.status == 0 && xhr.responseJSON == undefined) {
                if (!errors.includes(error)) {

                    if (window.navigator.onLine) {
                        if (error == "") {
                            error = "Something went wrong, Please Try later";
                        }

                        swal({ type: 'error', title: "Server Down", text: error }, function (isConfirm) {
                            if (isConfirm) {
                                deleteCookies();
                                window.location.reload();
                            }
                        });
                    } else {
                        swal({ type: 'info', title: "No Internet", text: "Check your network connection!" }, function (isConfirm) {
                            if (isConfirm) {
                                deleteCookies();
                                window.location.reload();
                            }
                        });
                    }
                } else {
                    swal({
                        type: 'error', title: "Error", text: "Something went wrong, please try later"
                    }, function (isConfirm) {
                        if (isConfirm) {
                            window.location.reload();
                        }
                    });
                }
            }
            break;
    }
}

function postForm() {
    clearInterval(codeVerfierInterval);

    var form = document.createElement('form');
    form.method = 'post';
    form.action = LoginControllerUrl.toLowerCase().includes("/login")
        ? LoginControllerUrl + "/Authenticate"
        : LoginControllerUrl + "/Login/Authenticate";

    document.body.appendChild(form);
    form.submit();
    $('#overlay1').show();
}

function ForgotPassword() {
    debugger;

    var username = document.getElementById("PASSWORDuserName").value;

    if (username == null || username == "") {
        return swal({ type: 'error', title: "Invalid", text: "Please Enter Email" });
    }
    var userType = getInputType(username);
   
    window.location.href = LoginControllerUrl + "/Login" + "/ForgotPassword/" + username + "?UserType=" + userType
}
function getInputType(userName) {
    var length = userName.length;

    if (/^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/.test(userName))
        return 2;
    else if (/^(((\+91)?|(91)?|(0)?)\d{10}|((\+256)?|(256)?|(0)?)\d{9})$/.test(userName))
        return 1;
    else if (length == 14 && /^[A-Z0-9]{14}$/.test(userName))
        return 3;
    else if (/^[A-Z]{1,4}[0-9]{5,16}$/.test(userName))
        return 4;
    else
        return 5;


}

function getDeviceType() {
    const ua = navigator.userAgent;
    if (/(tablet|ipad|playbook|silk)|(android(?!.*mobi))/i.test(ua)) {
        return "tablet";
    }
    if (
        /Mobile|iP(hone|od)|Android|BlackBerry|IEMobile|Kindle|Silk-Accelerated|(hpw|web)OS|Opera M(obi|ini)/.test(
            ua
        )
    ) {
        return "mobile";
    }
    return "desktop";
};

function setCookies() {
    var expires = (new Date(Date.now() + 60 * 1000)).toGMTString();
    document.cookie = "NotificationVerifierValidTime=yes; expires=" + expires + ";path=/;"
}

function deleteCookies() {

    document.cookie = "NotificationVerifierValidTime=yes; expires=" + new Date(0).toUTCString();

}

init();