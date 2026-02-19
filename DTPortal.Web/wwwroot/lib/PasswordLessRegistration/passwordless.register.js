document.getElementById('register-form').addEventListener('submit', handleRegisterSubmit);

async function handleRegisterSubmit(event) {
    $('#overlay').show();
    if (!document.getElementById("USERerror").classList.contains("hide")) {
        document.getElementById("USERerror").classList.remove("show");
        document.getElementById("USERerror").classList.add("hide");
    }
       

    event.preventDefault();
    var count = 0;
    let suid = this.userName.value;
    let userDetails = this.userDetails.value;
    let FullName = this.FullName.value;
    let MailID_MobileNO = this.MailID_MobileNO.value;
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
        credentialOptions = await fetchMakeCredentialOptions({ suid: suid, userDetails: userDetails, FullName: FullName, MailID_MobileNO: MailID_MobileNO});

    } catch (e) {
        $('#overlay').hide();
        console.error(e);
        window.location.href = errorUrl+"?error=Inernal_error&error_description="+e
        return;
    }

    if (credentialOptions.status !== "ok") {
        alert(credentialOptions.errorMessage);
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
            title: "Registration Failed",
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
                document.getElementById("registerBtn").innerText = "Try Again";
                document.getElementById("cancel").style.display = "block"
            } else {
                swal.close();
                document.getElementById("registerBtn").innerText = "Try Again";
                document.getElementById("cancel").style.display = "block"
            }
        });
        return;
    }

    if (!newCredential) {
        $('#overlay').hide();
        document.getElementById("registerBtn").innerText = "Try Again";
        document.getElementById("USERerror").classList.remove("hide");
        document.getElementById("USERerror").classList.add("show");
        document.getElementById("USERerror").innerText = "Something went wrong..! please try again"
    }
    $('#overlay').show();
    let response;
    try {
        response =  await registerNewCredential(newCredential, userDetails);
      
    } catch (e) {
        $('#overlay').hide();
        window.location.href = errorUrl+"?error=Inernal_error&error_description=" + e
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
                    if (isConfirm) {
                        window.location.href = errorUrl + "?error=Invalid Request&error_description=" + response.errorMessage
                    } else {
                        swal.close();
                        window.location.href = errorUrl + "?error=Invalid Request&error_description=" + response.errorMessage
                    }
                });

               
            }
            title = response.errorMessage

        }

        swal({
            title: title,
            text: "Click on Ok button for registration again",
            type: "error",
            showCancelButton: true,
            //  confirmButtonColor: "#DD6B55",
            confirmButtonText: "OK",
            cancelButtonText: "Cancel",
            closeOnConfirm: true,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) {
                window.location.reload();
            }
            else {
                window.location.href = errorUrl+"?error=Inernal_error&error_description=" + response.errorMessage
                return;
            }
        });
    } else {
        window.location.replace(successUrl);
    }
   // alert("You've registered successfully. You will now be redirected to sign in page");
}

$("#cancle").click(function () {
    window.location.href = errorUrl+"?error=Registration Cancled&error_description=User has cancled this token registration." 
})

async function fetchMakeCredentialOptions(formData) {
    let response = await fetch(getCredentialOptionsUrl, {
        method: 'POST',
        body: JSON.stringify(formData),
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });

    let data = await response.json();
    return JSON.parse(data.option);
}

// This should be used to verify the auth data with the server
async function registerNewCredential(newCredential, userDetails) {
    let attestationObject = new Uint8Array(newCredential.response.attestationObject);
    let clientDataJSON = new Uint8Array(newCredential.response.clientDataJSON);
    //let clientDataJSON = ChangeOrigin(newCredential.response.clientDataJSON, Origin);
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
       // alert(e);
        throw e;
    }
}

async function registerCredentialWithServer(formData) {
    let response = await fetch(SaveCredentialUrl, {
        method: 'POST',
        body: JSON.stringify(formData),
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    });

    let data = await response.json();

    return data;
}
