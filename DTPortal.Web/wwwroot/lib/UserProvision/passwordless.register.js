document.getElementById('RegisterFido').addEventListener('click', getConfirmation);

function getConfirmation() {
    if (FidoStatus == "1") {
        swal({
            title: "Fido2 Device already registred",
            text: "Do you want to register again!",
            type: "info",
            showCancelButton: true,
            confirmButtonText: "Yes",
            cancelButtonText: "No",
            closeOnConfirm: true,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) {
                handleRegisterSubmit();
            } else {
                return false;
            }
        });
    }
    else {
        handleRegisterSubmit();
    }

}

async function handleRegisterSubmit() {

    $('#overlay').show();

    event.preventDefault();
    var count = 0;
    let suid = document.getElementById("fidoData_suid").value;
    let userDetails = document.getElementById("fidoData_userDetails").value;
    let FullName = document.getElementById("fidoData_FullName").value;
    let MailID_MobileNO = document.getElementById("fidoData_MailID_MobileNO").value;
    // possible values: none, direct, indirect
    let attestationType = "none";
    // possible values: <empty>, platform, cross-platform
    let authenticatorAttachment = "";

    // possible values: preferred, required, discouraged
    let userVerification = "preferred";

    // possible values: true,false
    let requireResidentKey = "false";

    // send to server for registering
    let credentialOptions;
    try {
        credentialOptions = await fetchMakeCredentialOptions({ suid: suid, userDetails: userDetails, FullName: FullName, MailID_MobileNO: MailID_MobileNO });

    } catch (e) {
        $('#overlay').hide();
        swal("Error", e, "error");
        return;
    }

    if (credentialOptions.status !== "ok") {
        swal("Error", credentialOptions.errorMessage, "error");
        return;
    }

    // Turn the challenge back into the accepted format of padded base64
    credentialOptions.challenge = coerceToArrayBuffer(credentialOptions.challenge);
    credentialOptions.user.id = coerceToArrayBuffer(credentialOptions.user.id);

    credentialOptions.excludeCredentials = credentialOptions.excludeCredentials.map((c) => {
        c.id = coerceToArrayBuffer(c.id);
        return c;
    });

    if (credentialOptions.authenticatorSelection.authenticatorAttachment === null) {
        credentialOptions.authenticatorSelection.authenticatorAttachment = undefined;
    }

    let newCredential;
    /* $('#overlay').hide();*/
    try {
        newCredential = await navigator.credentials.create({
            publicKey: credentialOptions
        });
    } catch (e) {
        $('#overlay').hide();
        swal({
            title: "Device registration Failed",
            text: "The operation either timed out or was cancled. We couldn’t verify you or the security key you use. please try again",
            button: "Close", // Text on button
            icon: "error", //built in icons: success, warning, error, info
            timer: 4000, //timeOut for auto-close
            buttons: {
                confirm: {
                    text: "OK",
                    value: true,
                    visible: true,
                    className: "",
                    closeModal: true
                },
                cancel: {
                    text: "Cancel",
                    value: false,
                    visible: true,
                    className: "",
                    closeModal: true,
                }
            }
        }, function (isConfirm) {
            if (isConfirm) {

            } else {
                swal.close();

            }
        });
        return;
    }

    if (!newCredential) {
        $('#overlay').hide();
        swal("Error", "Something went wrong..! please try again", "error");
        return;
    }
    $('#overlay').show();
    let response;
    try {
        response = await registerNewCredential(newCredential, userDetails);

    } catch (e) {
        $('#overlay').hide();
        swal("Error", e, "error");
        return;
        //alert("Could not register new credentials on server");
    }

    console.log("Credential Object", response);
    $('#overlay').hide();
    // show error
    if (response.status !== "ok") {
        var title = "Something went wrong..!"
        if (response.errorMessage != "" && response.errorMessage != null && response.errorMessage != undefined) {
            if (response.errorMessage == "This device is already active" ||
                response.errorMessage == "User is already provisioned with Fido2 device") {
                swal({
                    title: "Information",
                    text: response.errorMessage,
                    button: "Close", // Text on button
                    icon: "info", //built in icons: success, warning, error, info
                    timer: 4000, //timeOut for auto-close
                    buttons: {
                        confirm: {
                            text: "OK",
                            value: true,
                            visible: true,
                            className: "",
                            closeModal: true
                        },
                        cancel: {
                            text: "Cancel",
                            value: false,
                            visible: true,
                            className: "",
                            closeModal: true,
                        }
                    }
                }, function (isConfirm) {
                    if (isConfirm) { } else { swal.close(); }
                    window.location.reload();
                });


            }
            title = response.errorMessage

        }

        swal({
            title: title,
            text: "Please try again later!",
            type: "error",
            showCancelButton: false,
            //  confirmButtonColor: "#DD6B55",
            confirmButtonText: "OK",
            closeOnConfirm: true,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) { } else { }
            window.location.reload();
        });
    } else {
        swal("Registration Success", "Fido2 device register successfully", "success");
        swal({
            title: "Registration Success",
            text: "Fido2 device register successfully",
            button: "Close", // Text on button
            icon: "success", //built in icons: success, warning, error, info
            timer: 4000, //timeOut for auto-close
            buttons: {
                confirm: {
                    text: "OK",
                    value: true,
                    visible: true,
                    className: "",
                    closeModal: true
                },
                cancel: {
                    text: "Cancel",
                    value: false,
                    visible: true,
                    className: "",
                    closeModal: true,
                }
            }
        }, function (isConfirm) {
                if (isConfirm) {
                    window.location.reload();
            } else {
                swal.close();
                    window.location.reload();
            }
        });
    }
    return;
}

$("#cancle").click(function () {
    window.location.href = errorUrl + "?error=Registration Cancled&error_description=User has cancled this token registration."
})

async function fetchMakeCredentialOptions(formData) {
    try {
        let response = await fetch(getCredentialOptionsUrl, {
            method: 'POST',
            body: JSON.stringify(formData),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) {
            response = await response.json();
            throw response;
        } else {
            let data = await response.json();
            if (data.status == "Success")
                return JSON.parse(data.option);
            else
                throw data.errorMessage;
        }
    } catch (e) {
        throw e;
    }
}

// This should be used to verify the auth data with the server
async function registerNewCredential(newCredential, userDetails) {
    let attestationObject = new Uint8Array(newCredential.response.attestationObject);
    let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
    let rawId = new Uint8Array(newCredential.rawId);

    const data = {
        id: newCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: newCredential.type,
        extensions: newCredential.getClientExtensionResults(),
        response: {
            AttestationObject: coerceToBase64Url(attestationObject),
            clientDataJson: coerceToBase64Url(clientDataJSON)
        }
    };

    var args = {
        attestationResponse: JSON.stringify(data),
        user: userDetails,
    }
    let response;
    try {
        response = await registerCredentialWithServer(args);
        return response;
    } catch (e) {
        throw e;
    }
}

async function registerCredentialWithServer(formData) {
    try {
        let response = await fetch(SaveCredentialUrl, {
            method: 'POST',
            body: JSON.stringify(formData),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            response = await response.json();
            throw response;
        } else {
            let data = await response.json();
            if (data.status == "ok")
                return data;
            else
                throw data.errorMessage;
        }
    } catch (e) {
        throw e
    }
}
