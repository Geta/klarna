(function ($) {
    var state = {
        initializedForToken: '',
        isAuthorizing: false,
        authorizationToken: '',
        authorizationTokenExpiration: new Date()
    };

    var settings = {
        klarna_container: '',
        client_token: ''
    };

    function shouldUpdateSettings(settings, newSettings) {
        if (!newSettings) {
            return false;
        }
        if (settings.klarna_container === newSettings.klarna_container && settings.client_token === newSettings.client_token) {
            return false;
        }

        return true;
    }

    function initKlarna(state, clientToken) {
        if (state.initializedForToken !== clientToken) {
            Klarna.Credit.init({
                client_token: clientToken
            });
            state.initializedForToken = clientToken;
        }
    }

    $(document)
        .on('klarna:load', function (ev, newSettings) {
            if (shouldUpdateSettings(settings, newSettings)) {

                settings.client_token = newSettings.client_token;
                settings.klarna_container = newSettings.klarna_container;

                // Init new settings
                initKlarna(state, settings.client_token);
            }

            Klarna.Credit.load({
                container: settings.klarna_container
            }, function (result) {
                console.debug(result);
                if (result && result.show_form) {
                    // TODO Check for errors, diplay errors
                } else {
                    // TODO Do not display form
                }
            });
        })
        .on('klarna:authorize', function () {
            //TODO prevent multiple authorize calls
            if (state.isAuthorizing) {
                return;
            }

            // TODO check authorization_token_expiration
            //if (state.authorizationTokenExpiration)

            if (state.authorizationToken === '') {
                state.isAuthorizing = true;

                /*var billingAddress = {
                    "given_name": "Brian",
                    "family_name": "Weeteling",
                    "email": "brian@geta.no",
                    "title": null,
                    "street_address": "379 W Broadway",
                    "street_address2": null,
                    "postal_code": "NY 10012",
                    "city": "New York",
                    "region": "New York",
                    "phone": "+1 855-593-9675",
                    "country": "US"
                };*/

                // Get personal info from server
                $.when($.getJSON("/klarnaapi/personal"), $.getJSON("/klarnaapi/address/" + $("#BillingAddress_AddressId").val()))
                .done(function (personalInformationResult, billingAddressResult) {
                    var personalInformation = personalInformationResult[0];
                    var billingAddress = billingAddressResult[0];

                    // We should have billing and shipping address here, if not we need to populate it from the local form fields
                    personalInformation.billing_address = billingAddress;

                    Klarna.Credit.authorize(personalInformation,
                        function (result) {
                            console.debug(result);
                            if (result.approved && result.authorization_token) {
                                state.authorizationToken = result.authorization_token;
                                state.authorizationTokenExpiration = new Date();

                                $("#AuthorizationToken").val(state.authorizationToken);
                                $('.jsCheckoutForm').submit();
                            }
                        });
                })
                .fail(function (result) {
                    console.log("Something went wrong", result);
                })
                .always(function () {
                    console.log("Always");
                    state.isAuthorizing = false;
                });
            }
        });
}(jQuery));