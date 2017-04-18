(function ($) {
    var state = {
        initializedForToken: ''
    };

    var settings = {
        klarna_container: '',
        client_token: '',
        authorization_token: "",
        authorization_token_expiration: new Date()
    };

    function updateSettings(settings, newSettings) {
        if (!newSettings) {
            return false;
        }
        if (settings.klarna_container === newSettings.klarna_container && settings.client_token === newSettings.client_token) {
            return false;
        }
        
        settings.klarna_container = newSettings.klarna_container;
        settings.client_token = newSettings.client_token;
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
        .on('klarna:settings', function (ev, newSettings) {
            updateSettings(settings, newSettings);
        })
        .on('klarna:init', function (ev) {
            initKlarna(state, settings.client_token);
        })
        .on('klarna:load', function (ev, newSettings) {
            if (updateSettings(settings, newSettings)) {
                // Init new settings
                initKlarna(state, settings.client_token);
            }

            if (!document.querySelectorAll(settings.klarna_container).length) {
                console.log('Cant find container');
                return;
            }

            Klarna.Credit.load({
                container: settings.klarna_container
            }, { }, function (result) {
                console.debug(result);
                if (result && result.show_form) {
                    // TODO Check for errors, diplay errors
                } else {
                    // TODO Do not display form
                }
            });
        })
        .on('klarna:authorize', function () {
            // TODO check authorization_token_expiration
            if (!settings.authorization_token) {
                //TODO prevent multiple authorize calls
                Klarna.Credit.authorize({
                    // No data here as we update the session server side
                }, function (result) {
                        console.debug(result);
                        if (result.approved && result.authorization_token) {

                            settings.authorization_token_expiration = new Date();
                            settings.authorization_token = result.authorization_token;

                            $("#AuthorizationToken").val(settings.authorization_token);
                            $('.jsCheckoutForm').submit();
                        }
                    });
            }
        });    
}(jQuery));