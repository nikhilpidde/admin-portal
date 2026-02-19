jQuery.validator.addMethod("ratecardvalidity",
    function (value, element, param) {

        if (value > param.validTo) {
            return false;
        }
        else {
            return true;
        }
    });

jQuery.validator.unobtrusive.adapters.addBool("ratecardvalidity");