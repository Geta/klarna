# Changelog

All notable changes to this project will be documented in this file.

## [3.5.6]

### Changed

- [Klarna Order Management] Fixed #172 - Missing payment type PAY_BY_CARD

## [4.0.1]

### Changed

- [Klarna Order Management] Fixed #172 - Missing payment type PAY_BY_CARD

## [4.0.0]

### Breaking changes

- [All packages] Upgraded to .NET 5 and Optimizely Commerce 14
- [All packages] Configuration for payment methods has been moved from Commerce Manager to appsettings.json. See the documentation for Klarna Payments and Klarna Checkout for details
- [Klarna Payments] Updated configuration class
- [All packages] Removed Refit dependency
- [All packages] Changed from JSON.NET to System.Text.Json Serializer

## [3.5.5]

### Changed

- [All packages] Fixed #114 - Image URL for line item returns bad request when using string.Empty - Contributed by: [hyllengren](https://github.com/hyllengren)

## [3.5.4]

### Changed

- [Klarna Order Management] Fixed #99 - Missing payment type SWISH - Contributed by: [Sebbe](https://github.com/sebbe)

## [3.5.3]

### Changed

- [Klarna Payments] Added AutoCapture (default: false, configurable in Commerce Manager), AcquiringChannel and CustomPaymentMethodIds to the Session

## [3.5.2]

### Changed

- [Klarna Checkout] Fixed #92 - Missing orderId on updates
- [All packages] Fixed #93 - Language comparison - Contributed by: [Vincent](https://github.com/javafun)

## [3.5.0]

### Breaking changes

- [All packages] Fixed #84 - Removed Klarna.Rest.Core dependency
- [All packages] Updated - Local logic for detecting country and language

## [3.0.1]

### Breaking changes

- [All packages] Fixed #68 - Upgrade to Episerver Commerce 13
- [All packages] Fixed #58 - Update deprecated package Klarna.Rest to Klarna.Rest.Core
- [Klarna Checkout] KlarnaCheckoutService is now async
- [Klarna Order Management] KlarnaOrderService is now async

### Added

AsyncHelper has been added to help call async methods synchronize - this can be used to make the upgrade easier in your code. We do recommend making your controller async.

```
Klarna.Common.AsyncHelper.RunSync(() => MyAsyncMethod());
```

## [2.0.13]

### Changed

- Fix for shipment amount validation issue when updating shipping option [#63](https://github.com/Geta/Klarna/pull/63)

## [2.0.12]

### Changed

- Use PricesIncludeTax property on market to determine if tax should be included on orderline

## [2.0.11]

### Changed

- [Klarna Order management] Exception handling when order can not be retrieved

## [2.0.10]

### Changed

- Using primary host as a site URL with fallback to site URL.

## [2.0.8]

### Changed

- Fixed discount calculation being wrong.

## [2.0.7]

### Changed

- Fixed a bug when shipping tax was not calculated properly for markets which has "PriceIncludeTax" setting.

## [2.0.6]

### Changed

- Fixed a bug when tax was not calculated properly for markets which has "PriceIncludeTax" setting.

## [2.0.5]

### Changed

- Added mapping from language NO to NB to make the widget render in norwegian.

## [2.0.4]

### Changed

- Fixed shipment option loading by language.

## [2.0.3]

### Changed

- Update System.Security.Cryptography.Xml to version="4.4.2" (security vulnerabilities)

## [2.0.2]

### Changed

- [Klarna Checkout] Made KlarnaCheckoutService overrideable, changed functions to virtual.

## [2.0.1]

### Changed

- [Klarna Checkout] Use market default languange when loading settings from Commerce Manager

## [2.0.0]

### Added

- Initial release to Episerver nuget
- Added changelog file

### Changed

- Added ".v3" to all package names to prevent issues with existing nuget packages on the official nuget feed
