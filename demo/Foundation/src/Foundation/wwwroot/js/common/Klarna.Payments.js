(function ($) {
    // quick and dirty country mapping
    var countryMapping = {
        "AFG": "AF", "ALA": "AX", "ALB": "AL", "DZA": "DZ", "ASM": "AS", "AND": "AD", "AGO": "AO", "AIA": "AI", "ATA": "AQ", "ATG": "AG", "ARG": "AR", "ARM": "AM", "ABW": "AW", "AUS": "AU", "AUT": "AT", "AZE": "AZ", "BHS": "BS", "BHR": "BH", "BGD": "BD", "BRB": "BB", "BLR": "BY", "BEL": "BE", "BLZ": "BZ", "BEN": "BJ", "BMU": "BM", "BTN": "BT", "BOL": "BO", "BES": "BQ", "BIH": "BA", "BWA": "BW", "BVT": "BV", "BRA": "BR", "IOT": "IO", "BRN": "BN", "BGR": "BG", "BFA": "BF", "BDI": "BI", "CPV": "CV", "KHM": "KH", "CMR": "CM", "CAN": "CA", "CYM": "KY", "CAF": "CF", "TCD": "TD", "CHL": "CL", "CHN": "CN", "CXR": "CX", "CCK": "CC", "COL": "CO", "COM": "KM", "COG": "CG", "COD": "CD", "COK": "CK", "CRI": "CR", "CIV": "CI", "HRV": "HR", "CUB": "CU", "CUW": "CW", "CYP": "CY", "CZE": "CZ", "DNK": "DK", "DJI": "DJ", "DMA": "DM", "DOM": "DO", "ECU": "EC", "EGY": "EG", "SLV": "SV", "GNQ": "GQ", "ERI": "ER", "EST": "EE", "ETH": "ET", "FLK": "FK", "FRO": "FO", "FJI": "FJ", "FIN": "FI", "FRA": "FR", "GUF": "GF", "PYF": "PF", "ATF": "TF", "GAB": "GA", "GMB": "GM", "GEO": "GE", "DEU": "DE", "GHA": "GH", "GIB": "GI", "GRC": "GR", "GRL": "GL", "GRD": "GD", "GLP": "GP", "GUM": "GU", "GTM": "GT", "GGY": "GG", "GIN": "GN", "GNB": "GW", "GUY": "GY", "HTI": "HT", "HMD": "HM", "VAT": "VA", "HND": "HN", "HKG": "HK", "HUN": "HU", "ISL": "IS", "IND": "IN", "IDN": "ID", "IRN": "IR", "IRQ": "IQ", "IRL": "IE", "IMN": "IM", "ISR": "IL", "ITA": "IT", "JAM": "JM", "JPN": "JP", "JEY": "JE", "JOR": "JO", "KAZ": "KZ", "KEN": "KE", "KIR": "KI", "PRK": "KP", "KOR": "KR", "KWT": "KW", "KGZ": "KG", "LAO": "LA", "LVA": "LV", "LBN": "LB", "LSO": "LS", "LBR": "LR", "LBY": "LY", "LIE": "LI", "LTU": "LT", "LUX": "LU", "MAC": "MO", "MKD": "MK", "MDG": "MG", "MWI": "MW", "MYS": "MY", "MDV": "MV", "MLI": "ML", "MLT": "MT", "MHL": "MH", "MTQ": "MQ", "MRT": "MR", "MUS": "MU", "MYT": "YT", "MEX": "MX", "FSM": "FM", "MDA": "MD", "MCO": "MC", "MNG": "MN", "MNE": "ME", "MSR": "MS", "MAR": "MA", "MOZ": "MZ", "MMR": "MM", "NAM": "NA", "NRU": "NR", "NPL": "NP", "NLD": "NL", "NCL": "NC", "NZL": "NZ", "NIC": "NI", "NER": "NE", "NGA": "NG", "NIU": "NU", "NFK": "NF", "MNP": "MP", "NOR": "NO", "OMN": "OM", "PAK": "PK", "PLW": "PW", "PSE": "PS", "PAN": "PA", "PNG": "PG", "PRY": "PY", "PER": "PE", "PHL": "PH", "PCN": "PN", "POL": "PL", "PRT": "PT", "PRI": "PR", "QAT": "QA", "REU": "RE", "ROU": "RO", "RUS": "RU", "RWA": "RW", "BLM": "BL", "SHN": "SH", "KNA": "KN", "LCA": "LC", "MAF": "MF", "SPM": "PM", "VCT": "VC", "WSM": "WS", "SMR": "SM", "STP": "ST", "SAU": "SA", "SEN": "SN", "SRB": "RS", "SYC": "SC", "SLE": "SL", "SGP": "SG", "SXM": "SX", "SVK": "SK", "SVN": "SI", "SLB": "SB", "SOM": "SO", "ZAF": "ZA", "SGS": "GS", "SSD": "SS", "ESP": "ES", "LKA": "LK", "SDN": "SD", "SUR": "SR", "SJM": "SJ", "SWZ": "SZ", "SWE": "SE", "CHE": "CH", "SYR": "SY", "TWN": "TW", "TJK": "TJ", "TZA": "TZ", "THA": "TH", "TLS": "TL", "TGO": "TG", "TKL": "TK", "TON": "TO", "TTO": "TT", "TUN": "TN", "TUR": "TR", "TKM": "TM", "TCA": "TC", "TUV": "TV", "UGA": "UG", "UKR": "UA", "ARE": "AE", "GBR": "GB", "USA": "US", "UMI": "UM", "URY": "UY", "UZB": "UZ", "VUT": "VU", "VEN": "VE", "VNM": "VN", "VGB": "VG", "VIR": "VI", "WLF": "WF", "ESH": "EH", "YEM": "YE", "ZMB": "ZM", "ZWE": "ZW"
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

    function getAddress(identifier) {
        return {
            "title": null,
            "given_name": $('[name="' + identifier + '.FirstName"]').val(),
            "family_name": $('[name="' + identifier + '.LastName"]').val(),
            "email": $('[name="' + identifier + '.Email"]').val(),
            "street_address": $('[name="' + identifier + '.Line1"]').val(),
            "street_address2": $('[name="' + identifier + '.Line2"]').val(),
            "postal_code": $('[name="' + identifier + '.PostalCode"]').val(),
            "city": $('[name="' + identifier + '.City"]').val(),
            "region": $('[name="' + identifier + '.CountryRegion.Region"]').val(),
            "phone": $('[name="' + identifier + '.DaytimePhoneNumber"]').val(),
            "country": countryMapping[$('[name="' + identifier + '.CountryCode"]').val()]
        }
    }

    function getBillingAddress() {
        // Check if new billing address form is visible and used
        if ($('#newBillingAddressForm').is(':visible')) {
            return getAddress("BillingAddress");
        }

        // Otherwise use shipping address for billing
        return getShippingAddress();
    }

    function getShippingAddress() {
        return getAddress("Shipments[0].Address");
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
        Klarna.Payments.init({
            client_token: clientToken
        });
        state.initializedForToken = clientToken;
    }

    function load(newSettings, callback) {
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
        loadWidgets(callback);
    }

    function setPaymentMethodCategory(state) {
        var paymentMethodCategory = $('[data-klarna-payments-select]:checked').data('klarna-payments-select');
        if (!paymentMethodCategory) return;
        state.paymentMethodCategory = paymentMethodCategory;
    }

    function setWidgetVisibility() {
        settings.payment_method_categories.forEach(function (category) {

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

    function loadWidgets(callback) {

        settings.payment_method_categories.forEach(function (category) {

            var container = settings.klarna_container + "_" + category;
            var errorContainer = settings.klarna_container + "_error_" + category;
            var item = settings.klarna_container + "_item_" + category;

            Klarna.Payments.load({
                container: container,
                payment_method_category: category
            }, function (result) {
                if (!result.show_form) {
                    // Unrecoverable error
                    $(item).hide();
                } else {
                    $(errorContainer).hide();
                }
                if (callback) {
                    callback();
                }
            });
        });
    }

    function authorize(onError) {
        // Prevent multiple authorize calls
        if (state.isAuthorizing) {
            return;
        }

        $('.loading-box').show();

        if (state.authorizationToken === '') {
            state.isAuthorizing = true;

            var getPersonalInfoPromise;
            var addressId = $('select[name="Shipments[0].Address.AddressId"]').val();

            if (!addressId || addressId.length === 0) {
                // Anonymous checkout, server doesn't have address info
                getPersonalInfoPromise = new $.Deferred().resolve({
                    customer: null,
                    shipping_address: getShippingAddress(),
                    billing_address: getBillingAddress()
                });
            } else {
                // Normal checkout, server has address info
                getPersonalInfoPromise =
                    $.ajax({
                        type: 'POST',
                        url: urls.personalInformation,
                        data: { billingAddressId: addressId }
                    }).then(function (data) { return data; });
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
                                $('.loading-box').hide();

                                if (onError) {
                                    onError();
                                }
                                $.post(urls.allowSharingOfPersonalInformation);
                            }

                            if (!result.show_form) {
                                // 'Unrecoverable' error

                                $(settings.klarna_container + "_item_" + state.paymentMethodCategory).hide();

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