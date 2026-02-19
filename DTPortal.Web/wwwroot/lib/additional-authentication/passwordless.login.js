
async function handleSignInSubmit(Fido2Options, Origin) {

    $('#overlay').show();

    if (typeof Fido2Options != "string")
        showError("Something went wrong.! try again.")

    var origin = Origin;

    let publicKeyOptions = JSON.parse(Fido2Options);

    const challenge = publicKeyOptions.challenge.replace(/-/g, "+").replace(/_/g, "/");
    publicKeyOptions.challenge = Uint8Array.from(atob(challenge), c => c.charCodeAt(0));

    publicKeyOptions.allowCredentials.forEach(function (listItem) {
        var fixedId = listItem.id.replace(/\_/g, "/").replace(/\-/g, "+");
        listItem.id = Uint8Array.from(atob(fixedId), c => c.charCodeAt(0));
    });

    /*$('#overlay').hide();*/
    // ask browser for credentials (browser will ask connected authenticators)
    let credential;
    try {
        credential = await navigator.credentials.get({ publicKey: publicKeyOptions });
        try {
            $('#overlay').show();
            var response = await verifyAssertionWithServer(credential, origin);
            if (!response.success) {
                showError(response.message);
            } else {
                modal.style.display = 'none';
                $('#overlay').hide();
                submitForm();
            }

        } catch (e) {
            $('#overlay').hide();
            showError("Something went wrong.! try again.")
        }
    } catch (err) {
        $('#overlay').hide();
        showError("The Authentication Request has expired / cancelled. ")
    }
}

$("#cancle").click(function () {
    window.location.reload();
})

function showError(msg) {
    if (msg == "Session Expired !") {
        swal({ type: 'info', title: "Operation Failed", text: msg }, function (isConfirm) {
            if (isConfirm) {
                window.location.href = SessionOutLoginUrl
            }
        });

    }
    swal({ type: 'info', title: "Operation Failed", text: msg }, function (isConfirm) {
        if (isConfirm) {
            window.location.reload();
        }
    });
}

async function verifyAssertionWithServer(assertedCredential, Origin) {
    let authData = new Uint8Array(assertedCredential.response.authenticatorData);
    // let clientDataJSON = new Uint8Array(assertedCredential.response.clientDataJSON);
    let clientDataJSON = ChangeOrigin(assertedCredential.response.clientDataJSON, Origin);
    let rawId = new Uint8Array(assertedCredential.rawId);
    let sig = new Uint8Array(assertedCredential.response.signature);
    const FidoDATA = {
        id: assertedCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: assertedCredential.type,
        extensions: assertedCredential.getClientExtensionResults(),
        response: {
            authenticatorData: coerceToBase64Url(authData),
            clientDataJson: coerceToBase64Url(clientDataJSON),
            signature: coerceToBase64Url(sig)
        }
    };


    const data = {
        authSchemeName: "FIDO2",
        credential: JSON.stringify(FidoDATA)
    }
    let response;
    try {
        let res = await fetch(VerifyAuthenticationDataUrl, {
            method: 'POST',
            body: JSON.stringify(data),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (res.status === 200) {
            response = await res.json();
        }
        else if (res.status === 401) {
            response = {
                success: false,
                message: "Session Expired !"
            }
        } else {
            response = {
                success: false,
                message: res.statusText
            }
        }

    } catch (e) {

        throw e;
    }

    return response;
}
