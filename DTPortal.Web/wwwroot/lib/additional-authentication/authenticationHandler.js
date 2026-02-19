//var operationName = "";    //critical operation name
var sessionID = "";        //authentication session id
var authScheme = "";       //array of authn scheme
var preid = "";            //store id of open popup model to close
var voiceFormValue = "";   //store voice form field values
var PushNotificationCode = "";
var codeVerfierInterval = "";
var Fido2Options = "";
var ActionUrl = "";

//attache html element to jscript variable
const modal = document.querySelector('.backdrop');               //popup model
const closeBtn = document.querySelector('#close');               //colse popup
const checkPwdBtn = document.querySelector('#checkPassword');    //password popup
const checkTOTPBtn = document.querySelector('#checkTOTP');       //totp popup   
const checkEmailBtn = document.querySelector('#checkEmail');
const submitBtn = document.querySelector("#submitFormBtn");
// Events
closeBtn.addEventListener('click', closeModal);                 //close popup
checkPwdBtn.addEventListener('click', authenticate);
checkTOTPBtn.addEventListener('click', authenticate);
checkEmailBtn.addEventListener('click', authenticate);
window.addEventListener('click', outsideClick);
if (submitBtn) {
    submitBtn.addEventListener('click', isAuthenticationRequired);
}

//$(".toggle-authentication-password").click(function () {
//    $(this).toggleClass("fa-eye fa-eye-slash");
//    var input = $($(this).attr("toggle"));
//    if (input.attr("type") == "password") {
//        input.attr("type", "text");
//    } else {
//        input.attr("type", "password");
//    }
//});

$(".toggle-authentication-password").click(function () {
    $(this).toggleClass("fa-eye fa-eye-slash");
    var input = $($(this).attr("toggle"));
    if (input.hasClass("pwd")) {
        input.removeClass("pwd");
    } else {
        input.addClass("pwd");
    }
});

function getAllValue() {
    $('#uform :input').each(function () {
        $(this).data('originalvalue', getCurrentValue($(this)));
    });
}
//add listener to check fields values change or not
$('#uform').on('input change', function () {
    var btnselector = ""
    if (submitBtn)
        btnselector = '#submitFormBtn'
    else
        btnselector = "#submitForm"

    if (checkvalues()) {
        $(btnselector).attr("disabled", true);
        $(btnselector).css('opacity', '0.5');
    } else {
        $(btnselector).css('opacity', '1');
        $(btnselector).attr("disabled", false);
    }
});

//function for return fields original value
function getCurrentValue(checkMe) {
    if (checkMe.is(':radio') || checkMe.is(':checkbox'))
        return checkMe.prop('checked')
    else if (checkMe.is(':hidden'))
        return checkMe.val()
    else
        return checkMe.val()

}

//funtion for compare original value or change value are same or not
function checkvalues() {
    var original = true;
    $('#uform :input').each(function () {
        var d = getCurrentValue($(this));
        var c = $(this).data('originalvalue');
        if (getCurrentValue($(this)) !== $(this).data('originalvalue')) {
            original = false;
        }
    });
    return original;
}





function isAuthenticationRequired(requiredFormValidation = true) {

    //var isValidationErrorPresent = $(".field-validation-error");
    //if (isValidationErrorPresent.length != 0) {
    //    swal("Invalid Input", "Please enter valid input","info");
    //    return false;
    //}
    var validate = true;
    if (requiredFormValidation) {
        $("Form").validate();
        validate = $("Form").valid();
    }

    if (validate) {
        $.ajax({
            type: 'POST',
            url: isAuthenticationRequiredURL,
            data: {
                id: operationName
            },
            beforeSend: function () {
                $('#overlay').show();
            },
            complete: function () {
                $('#overlay').hide();
            },
            success: function (result, status, xhr) {
                if (result.success) {
                    submitForm();
                }
                else {
                    if (result.result != null) {
                        authScheme = result.result.authenticationScheme;
                        if (authScheme == "PUSH_NOTIFICATION") {
                            PushNotificationCode = result.result.randomCode;//get pushnotification code
                        }
                        if (authScheme == "FIDO2") {
                            handleSignInSubmit(result.result.fido2Options, result.result.tempSession)
                            return;

                        } else {
                            show();
                        }
                    } else {
                        if (result.message == "User is not provisioned for this authentication scheme") {
                            swal({ type: 'info', title: "Authentication data not found", text: result.message }, function (isConfirm) {
                                if (isConfirm) {
                                    location.reload();
                                }
                            });
                        } else {
                            swal({ type: 'info', title: "Some thing went wrong!", text: result.message }, function (isConfirm) {
                                if (isConfirm) {
                                    location.reload();
                                }
                            });
                        }
                    }
                }
            },
            error: AdditionalAuthnajaxErrorHandler

        })
    }
    else {
        $('#overlay').hide();
    }

   
}

//funtion for manage div flow
function show() {
    var id = authScheme;
    if (id == undefined) {
        modal.style.display = 'none';
        submitForm();
        
        
    } else {
        // if (modal.style.display == '')
        modal.style.display = 'block';

        if (id == "VOICE_TI" || id == "VOICE_DIGIT_RECOGNISATION" ||
            id == "VOICE_TPD_RECOGNISATION") {
            setElementValues(id);
            id = "VOICE";
        }

        if (id == "PUSH_NOTIFICATION") {
            document.getElementById("PushNotificationCode").innerText = PushNotificationCode;
            document.cookie = "verifiedCodeCount=" + 0
            isUserVerifiedCode();
        }

        preid = id;
        var div = $("#" + id)
        if (div.length)
            div.css("display", "block");
        else
            swal("Error", "The " + id + " Authentication Scheme is not available")
    }
}


function authenticate() {
    var formName = $(this).parents("form").attr("id");
    var form = new FormData(document.getElementById(formName));
    $.ajax({
        type: 'POST',
        url: VerifyAuthenticationDataURL,
        contentType: 'application/json',
        dataType: 'json',
        data: JSON.stringify({
            // "temprorySession": sessionID,
            "authSchemeName": form.get("authScheme"),
            "credential": form.get("password")
        }),
        success: function (result, status, xhr) {
            if (result.success) {
                modal.style.display = 'none';
                $('#overlay1').show();
                submitForm();
            }
            else {
                swal("Error", result.message, "error")
            }
        },
        error: AdditionalAuthnajaxErrorHandler
    })
}



//push notification verifire function
function isUserVerifiedCode() {
    codeVerfierInterval = setInterval(function () {
        $.ajax({
            type: 'post',
            url: IsUserVerifiedCodeURL,
            success: function (result, status, xhr) {
                if (result.success && result.status == "success") {
                    clearInterval(codeVerfierInterval);
                    $('#overlay1').show();
                    modal.style.display = 'none';
                    submitForm();
                }
                else {

                    if (!result.success && result.status == "failed") {

                        if (result.message == "authnToken expired/does not exists") {
                            swal({ type: 'info', title: result.message, text: "Click on 'Ok' button for login again!" }, function (isConfirm) {
                                if (isConfirm) {
                                    deleteCookies();
                                    window.location.reload();
                                }
                            });
                        } else {
                            swal({
                                title: result.message,
                                text: `Do you want to reauthenticate!\nPlease click on the 'send notification' button\n to resend notification on mobile.`,
                                type: "error",
                                showCancelButton: true,
                                //  confirmButtonColor: "#DD6B55",
                                confirmButtonText: "Send notification",
                                cancelButtonText: "No",
                                closeOnConfirm: true,
                                closeOnCancel: true
                            }, function (isConfirm) {
                                if (isConfirm) {
                                    clearInterval(codeVerfierInterval);
                                    sendCodeAgain();
                                }
                                else {
                                    clearInterval(codeVerfierInterval);
                                    deleteCookies();
                                    window.location.href = redirectUrl + "?error=access_denied&error_description=user not authenticate"
                                    //window.history.back();
                                    //window.location.reload();
                                }
                            });

                        }
                    }
                    if (result.operation == "warning") {
                        document.getElementById(errorDivId).className = "danger";
                        document.getElementById(errorDivId).style.display = "block"
                        document.getElementById(errorDivId).innerHTML = result.message;
                    }
                    if (result.operation == "stop") {

                        clearInterval(codeVerfierInterval);

                        swal({
                            title: "Request time out",
                            html: true,
                            text: `We send push notification on your DTIDP authenticator app ,<br/>but we didn't get response ,<br/><strong><b>Do you want to reauthenticate!</b></strong><br/>Please click on the 'send notification' button\n to resend notification on mobile.`,
                            type: "error",
                            showCancelButton: true,
                            //  confirmButtonColor: "#DD6B55",
                            confirmButtonText: "Resend notification",
                            cancelButtonText: "No",
                            closeOnConfirm: true,
                            closeOnCancel: true
                        }, function (isConfirm) {
                            if (isConfirm) {
                                clearInterval(codeVerfierInterval);
                                sendCodeAgain();
                            }
                            else {
                                clearInterval(codeVerfierInterval);
                                deleteCookies();
                                window.location.href = redirectUrl + "?error=access_denied&error_description=user not authenticate"
                                //window.history.back();
                                //window.location.reload();
                            }
                        });
                    }
                }
            },
            error: AdditionalAuthnajaxErrorHandler
        })

    }, 10000);
}

//
function sendCodeAgain() {
    $.ajax({
        type: 'post',
        url: SendPushNotificationURL,
        success: function (result, status, xhr) {
            if (result.success) {
                swal({
                    title: "Success!",
                    text: "Code send successfully!",
                    timer: 2000,
                    type: "success",
                    showConfirmButton: false
                });

                if (document.getElementById(errorDivId).style.display == "block") {
                    document.getElementById(errorDivId).className = "info";
                    document.getElementById(errorDivId).style.display = "none"
                    document.getElementById(errorDivId).innerHTML = "";
                }
                document.getElementById("PushNotificationCode").innerText = result.randomCode;
                document.cookie = "verifiedCodeCount=" + 0
                isUserVerifiedCode();

            }
            else {

                if (result.message == "authnToken expired/does not exists") {
                    swal({ type: 'info', title: result.message, text: "Click on 'Ok' button for login again!" }, function (isConfirm) {
                        if (isConfirm) {
                            deleteCookies();
                            window.location.reload();
                        }
                    });
                } else {
                    swal({
                        title: result.message,
                        text: `Do you want to reauthenticate!\nPlease click on the 'send notification' button\n to resend notification on mobile.`,
                        type: "error",
                        showCancelButton: true,
                        // confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Resend notification",
                        cancelButtonText: "Cancel",
                        closeOnConfirm: false,
                        closeOnCancel: false
                    }, function (isConfirm) {
                        if (isConfirm) {
                            sendCodeAgain();
                        }
                        else {
                            clearInterval(codeVerfierInterval);
                            deleteCookies();
                            window.location.href = redirectUrl + "?error=access_denied&error_description=user not authenticate"
                            //window.history.back();
                            //window.location.reload();
                        }
                    });
                }
            }
        },
        error: AdditionalAuthnajaxErrorHandler
    })
}

function AdditionalAuthnajaxErrorHandler(xhr, status, error) {
    switch (xhr.status) {
        case 400:
            swal("Error", "Cannot process your request", "error");
            break;
        case 401:
            swal({ type: 'info', title: "Session Expired", text: "Click on 'Ok' button to login again!" }, function (isConfirm) {
                if (isConfirm) {
                    window.location.href = SessionOutLoginUrl
                }
            });
            break;
        case 403:
            swal("Forbidden", "You do not have access to this resource", "error");
            break;
        case 404:
            swal("Not Found", "The resource you requested could not be found", "error");
            break;
        case 500:
            swal({ title: "Server Error", text: "Internal Server error Occurred", type: "error" }, function (isConfirm) {
                if (isConfirm) {
                    window.location.reload();
                }
            });
            break;
        case 502:
            swal("Bad Gateway", "Invalid response", "error");
            break;
        case 503:
            swal("Service Unavailable", "The Service is currently unavailable, please try after sometime", "error");
            break;
        default:
            errors = ["Bad Request", "Bad Gateway", "Not Found", "Internal Server Error", "Forbidden", "Unauthorized", "Service Unavailable"]
            if (xhr.readyState == 0 && xhr.status == 0 && xhr.responseJSON == undefined) {
                if (!errors.includes(error)) {
                    if (window.navigator.onLine) {
                        console.log(xhr);
                        if (error == "") {
                            error = "Something went wrong, please try later"
                        }
                        swal({ type: 'error', title: "Server Down", text: error }, function (isConfirm) {
                            if (isConfirm) {
                                window.location.reload();
                            }
                        });
                    }
                    else {
                        swal({ type: 'info', title: "No Internet", text: "Check your network connection!" }, function (isConfirm) {
                            if (isConfirm) {
                                window.location.reload();
                            }
                        });
                    }
                } else {
                    swal({
                        type: 'error', title: "Error", text: "Something went wrong, please try later" }, function (isConfirm) {
                        if (isConfirm) {
                            window.location.reload();
                        }
                    });
                }
            }
            break;
    }
}

function geterror(err) {
    if (err == 2)
        return "AUTHENTICATION_FAILED"
    else
        return "Something is wrong..!"
}
/*     //funtion for manage div flow
    function show() {
        var id = authScheme.pop();
        if (id == undefined) {
            modal.style.display = 'none';
            updateform();
        } else {
            // if (modal.style.display == '')
            modal.style.display = 'block';

            if (id == "VOICE_TI" || id == "VOICE_DIGIT_RECOGNISATION" ||
                id == "VOICE_TPD_RECOGNISATION") {
                setElementValues(id);
                id = "VOICE";
            }

            preid = id;
            var div = $("#" + id)
            if (div.length)
                div.css("display", "block");
            else
                swal("Error", "The " + id + " Authentication Scheme is not available")
        }
    }
 */
var timerId = "";
function startTimer(duration, display) {
    var timer = duration, minutes, seconds;
    timerId = setInterval(function () {
        // minutes = parseInt(timer / 60, 10);
        // seconds = parseInt(timer % 60, 10);
        seconds = timer;
        // minutes = minutes < 10 ? "0" + minutes : minutes;
        // seconds = seconds < 10 ? "0" + seconds : seconds;

        // display.textContent = minutes + ":" + seconds;
        display.textContent = seconds + "s";
        ++timer;
        // if (++timer < 0) {
        // timer = duration;
        // }
    }, 1000);
}
//show message for voice authentication popup



// Close popup model
function closeModal() {
    swal({
        title: "Do you want to proceed?",
        text: `You need to do Additional Authentication!`,
        type: "info",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Yes",
        cancelButtonText: "No",
        closeOnConfirm: true,
        closeOnCancel: true,
    }, function (isConfirm) {
        if (isConfirm) {

        } else {
            location.reload();
            modal.style.display = 'none';
        }
    });

}

// prevent Outside Click when popup open
function outsideClick(e) {
    if (e.target == modal) {
        modal.style.display = 'block';
    }
}

//prevent form submitting by pressing enter key
$(".passwordInput").keydown(function (event) {
    if (event.keyCode == 13) {
        event.preventDefault();
        var inputId = $(this).attr("id");
        if (inputId == "password") {
            checkPwdBtn.click();
        } else if (inputId == "totp") {
            checkTOTPBtn.click();
        }
        else if (inputId == "emailotp") {
            checkEmailBtn.click();
        }
        else {

        }
        return false;
    }
});
