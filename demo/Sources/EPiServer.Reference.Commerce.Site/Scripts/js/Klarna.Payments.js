var KlarnaPayments = {};

(function ($) {
    // quick and dirty country mapping
    var countryMapping = {
        "AFG": "AF","ALA": "AX","ALB": "AL","DZA": "DZ","ASM": "AS","AND": "AD","AGO": "AO","AIA": "AI","ATA": "AQ","ATG": "AG","ARG": "AR","ARM": "AM","ABW": "AW","AUS": "AU","AUT": "AT","AZE": "AZ","BHS": "BS","BHR": "BH","BGD": "BD","BRB": "BB","BLR": "BY","BEL": "BE","BLZ": "BZ","BEN": "BJ","BMU": "BM","BTN": "BT","BOL": "BO","BES": "BQ","BIH": "BA","BWA": "BW","BVT": "BV","BRA": "BR","IOT": "IO","BRN": "BN","BGR": "BG","BFA": "BF","BDI": "BI","CPV": "CV","KHM": "KH","CMR": "CM","CAN": "CA","CYM": "KY","CAF": "CF","TCD": "TD","CHL": "CL","CHN": "CN","CXR": "CX","CCK": "CC","COL": "CO","COM": "KM","COG": "CG","COD": "CD","COK": "CK","CRI": "CR","CIV": "CI","HRV": "HR","CUB": "CU","CUW": "CW","CYP": "CY","CZE": "CZ","DNK": "DK","DJI": "DJ","DMA": "DM","DOM": "DO","ECU": "EC","EGY": "EG","SLV": "SV","GNQ": "GQ","ERI": "ER","EST": "EE","ETH": "ET","FLK": "FK","FRO": "FO","FJI": "FJ","FIN": "FI","FRA": "FR","GUF": "GF","PYF": "PF","ATF": "TF","GAB": "GA","GMB": "GM","GEO": "GE","DEU": "DE","GHA": "GH","GIB": "GI","GRC": "GR","GRL": "GL","GRD": "GD","GLP": "GP","GUM": "GU","GTM": "GT","GGY": "GG","GIN": "GN","GNB": "GW","GUY": "GY","HTI": "HT","HMD": "HM","VAT": "VA","HND": "HN","HKG": "HK","HUN": "HU","ISL": "IS","IND": "IN","IDN": "ID","IRN": "IR","IRQ": "IQ","IRL": "IE","IMN": "IM","ISR": "IL","ITA": "IT","JAM": "JM","JPN": "JP","JEY": "JE","JOR": "JO","KAZ": "KZ","KEN": "KE","KIR": "KI","PRK": "KP","KOR": "KR","KWT": "KW","KGZ": "KG","LAO": "LA","LVA": "LV","LBN": "LB","LSO": "LS","LBR": "LR","LBY": "LY","LIE": "LI","LTU": "LT","LUX": "LU","MAC": "MO","MKD": "MK","MDG": "MG","MWI": "MW","MYS": "MY","MDV": "MV","MLI": "ML","MLT": "MT","MHL": "MH","MTQ": "MQ","MRT": "MR","MUS": "MU","MYT": "YT","MEX": "MX","FSM": "FM","MDA": "MD","MCO": "MC","MNG": "MN","MNE": "ME","MSR": "MS","MAR": "MA","MOZ": "MZ","MMR": "MM","NAM": "NA","NRU": "NR","NPL": "NP","NLD": "NL","NCL": "NC","NZL": "NZ","NIC": "NI","NER": "NE","NGA": "NG","NIU": "NU","NFK": "NF","MNP": "MP","NOR": "NO","OMN": "OM","PAK": "PK","PLW": "PW","PSE": "PS","PAN": "PA","PNG": "PG","PRY": "PY","PER": "PE","PHL": "PH","PCN": "PN","POL": "PL","PRT": "PT","PRI": "PR","QAT": "QA","REU": "RE","ROU": "RO","RUS": "RU","RWA": "RW","BLM": "BL","SHN": "SH","KNA": "KN","LCA": "LC","MAF": "MF","SPM": "PM","VCT": "VC","WSM": "WS","SMR": "SM","STP": "ST","SAU": "SA","SEN": "SN","SRB": "RS","SYC": "SC","SLE": "SL","SGP": "SG","SXM": "SX","SVK": "SK","SVN": "SI","SLB": "SB","SOM": "SO","ZAF": "ZA","SGS": "GS","SSD": "SS","ESP": "ES","LKA": "LK","SDN": "SD","SUR": "SR","SJM": "SJ","SWZ": "SZ","SWE": "SE","CHE": "CH","SYR": "SY","TWN": "TW","TJK": "TJ","TZA": "TZ","THA": "TH","TLS": "TL","TGO": "TG","TKL": "TK","TON": "TO","TTO": "TT","TUN": "TN","TUR": "TR","TKM": "TM","TCA": "TC","TUV": "TV","UGA": "UG","UKR": "UA","ARE": "AE","GBR": "GB","USA": "US","UMI": "UM","URY": "UY","UZB": "UZ","VUT": "VU","VEN": "VE","VNM": "VN","VGB": "VG","VIR": "VI","WLF": "WF","ESH": "EH","YEM": "YE","ZMB": "ZM","ZWE": "ZW"
    };

    var state = {
        initializedForToken: '',
        isAuthorizing: false,
        authorizationToken: '',
        paymentMethodCategory: ''
    };

    var settings = {
        klarna_container: '',
        client_token: '',
        payment_method_categories: []
    };

    var urls = {
        personalInformation: '/klarnaapi/personal/',
        allowSharingOfPersonalInformation: '/klarnaapi/personal/allow'
    };

    function getCustomerInfo() {
        //TODO: implement form for the user to fill in this information
        return {
            dateOfBirth: "1980-01-01",
            gender: "Male",
            lastFourSsn: "1234"
        };
    }

    function getAddress(identifier) {
        return {
            "title": null,
            "given_name": $("#" + identifier + "_FirstName").val(),
            "family_name": $("#" + identifier + "_LastName").val(),
            "email": $("#" + identifier + "_Email").val(),
            "street_address": $("#" + identifier + "_Line1").val(),
            "street_address2": null,
            "postal_code": $("#" + identifier + "_PostalCode").val(),
            "city": $("#" + identifier + "_City").val(),
            "region": $("#" + identifier + "_CountryRegion_Region").val(),
            "phone": $("#" + identifier + "_DaytimePhoneNumber").val(),
            "country": countryMapping[$("#" + identifier + "_CountryCode").val()]
        }
    }

    function getBillingAddress() {
        return getAddress("BillingAddress");
    }

    function getShippingAddress() {
        if ($("#AlternativeAddressButton").is(":visible")) {
            return getBillingAddress();
        }
        return getAddress("Shipments_0__Address");
    }

    function shouldUpdateSettings(settings, newSettings) {
        if (!newSettings) {
            return false;
        }
        if (settings.klarna_container === newSettings.klarna_container
            && settings.client_token === newSettings.client_token
            && settings.payment_method_categories === newSettings.payment_method_categories) {
            return false;
        }

        return true;
    }

    function initKlarna(state, clientToken) {
        if (state.initializedForToken !== clientToken) {
            Klarna.Payments.init({
                client_token: clientToken
            });
            state.initializedForToken = clientToken;
        }
    }

    function hideForm() {
        console.error("Hide Klarna, display an error to the user");
        $("#klarna_container_error").show();
    }

    function load(newSettings) {
        if (newSettings && newSettings.client_token && shouldUpdateSettings(settings, newSettings)) {
            settings.client_token = newSettings.client_token;
            settings.klarna_container = newSettings.klarna_container;
            settings.payment_method_categories = newSettings.payment_method_categories;

            // Init new settings
            initKlarna(state, settings.client_token);
        }

        if (!settings.client_token || !settings.klarna_container) {
            return;
        }

        $('[data-klarna-payments-select]').on('change',
            function () {
                setPaymentMethodCategory(state);
                setWidgetVisibility();
            });

        setPaymentMethodCategory(state);
        setWidgetVisibility();
        loadWidgets();
    }

    function setPaymentMethodCategory(state) {
        var paymentMethodCategory = $('[data-klarna-payments-select]:checked').data('klarna-payments-select');
        if (!paymentMethodCategory) return;
        state.paymentMethodCategory = paymentMethodCategory;
    }

    function setWidgetVisibility() {
        settings.payment_method_categories.forEach(function(category) {

            var container = settings.klarna_container + "_" + category;
            var errorContainer = settings.klarna_container + "_error_" + category;

            if (state.paymentMethodCategory === category) {
                $(container).show();
            } else {
                $(container).hide();
                $(errorContainer).hide();
            }
        });
    }

    function loadWidgets() {

        settings.payment_method_categories.forEach(function(category) {

            var container = settings.klarna_container + "_" + category;
            var errorContainer = settings.klarna_container + "_error_" + category;

            Klarna.Payments.load({
                container: container,
                payment_method_category: category
            }, function (result) {
                if (!result.show_form) {
                    // Unrecoverable error
                    hideForm();
                } else {
                    $(errorContainer).hide();
                }
            });
        });
    }

    function authorize() {
        // Prevent multiple authorize calls
        if (state.isAuthorizing) {
            return;
        }

        if (state.authorizationToken === '') {
            state.isAuthorizing = true;

            var getPersonalInfoPromise;
            if ($("#BillingAddress_AddressId").length === 0) {
                // Anonymous checkout, server doesn't have address info
                getPersonalInfoPromise = new $.Deferred().resolve({
                    customer: getCustomerInfo(),
                    billing_address: getBillingAddress(),
                    shipping_address: getShippingAddress()
                });
            } else {
                // Normal checkout, server has address info
                getPersonalInfoPromise =
                    $.ajax({
                        type: 'POST',
                        url: urls.personalInformation,
                        data: "=" + $("#BillingAddress_AddressId").val(),
                        contentType: 'application/x-www-form-urlencoded'
                    })
                    .then(function (data) { return data; });
            }

            // Get personal info from server
            getPersonalInfoPromise
            .done(function (personalInformation) {
                // We should have all necessary personal information here, pass it to authorize call
                Klarna.Payments.authorize({
                    payment_method_category: state.paymentMethodCategory
                },
                personalInformation,
                function (result) {
                    // We're allowed to share personal information after this call, only need this info if authorize failed
                    if (!result.show_form || !result.approved || !result.authorization_token) {
                        $.post(urls.allowSharingOfPersonalInformation);
                    }

                    if (!result.show_form) {
                        // 'Unrecoverable' error
                        hideForm();
                    } else {
                        if (result.approved && result.authorization_token) {
                            state.authorizationToken = result.authorization_token;

                            // Set auth token in form field and submit the form
                            $("#AuthorizationToken").val(state.authorizationToken);
                            $('.jsCheckoutForm').submit();
                        }
                    }
                });
            })
            .fail(function (result) {
                console.error("Something went wrong", result);
                alert("Something went wrong");
            })
            .always(function () {
                state.isAuthorizing = false;
            });
        } else {
            console.error("Tried to authorize again, should never happen");
        }
    };

    KlarnaPayments = {
        load: load,
        authorize: authorize
    };
}(jQuery));