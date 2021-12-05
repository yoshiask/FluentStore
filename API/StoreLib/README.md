# StoreLib

[![GitHub stars](https://img.shields.io/github/stars/StoreDev/StoreLib?style=social)](https://github.com/StoreDev/StoreLib)
[![GitHub Workflow - Build](https://img.shields.io/github/workflow/status/StoreDev/StoreLib/build?label=build)](https://github.com/StoreDev/StoreLib/actions?query=workflow%3Abuild)
[![Nuget downloads](https://img.shields.io/nuget/dt/StoreLib)](https://www.nuget.org/packages/StoreLib)
[![Nuget version](https://img.shields.io/nuget/v/StoreLib)](https://www.nuget.org/packages/StoreLib)

Storelib is a DotNet library that provides APIs to interact with the various Microsoft Store endpoints.

## Usage

First, you must initialize the DisplayCatalogHandler with the settings of your choice. During which, the handler can be set to use any market, locale, or endpoint.

```csharp
DisplayCatalogHandler dcathandler = new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(Market.US, Lang.en, true));
```

The above snippet will create a handler that queries the production endpoint, specifiying the US/English market.

From there, the handler can query a product listing.

```csharp
await dcathandler.QueryDCATAsync("9wzdncrfj3tj");
```

Once you have a product queried, and ensure it was found using `dcathandler.IsFound`, then you can fetch all .appx, .eappx, .xvc and .msixvc packages respectively for the listing using `GetPackagesForProductAsync();`

### Example

Fetches and prints the FE3 download links for Netflix's app packages.

```csharp
DisplayCatalogHandler dcathandler = DisplayCatalogHandler.ProductionConfig();
await dcathandler.QueryDCATAsync("9wzdncrfj3tj");
foreach(Uri download in await dcathandler.GetPackagesForProductAsync())
{
  Console.WriteLine(download.ToString());
}
```

### Tips

The DisplayCatalogHandler also supports querying with an auth token. (The Store supports both the MSA format and the XBL3.0 token format) This allows you to query products in other Xbox Live Sandboxes and query flighted listings.

```csharp
DisplayCatalogHandler dcathandler = DisplayCatalogHandler.ProductionConfig();
await dcathandler.QueryDCATAsync("9wzdncrfj3tj", "AuthToken");
```
