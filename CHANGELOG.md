# Changelog

All notable changes to this project will be documented in this file.

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
