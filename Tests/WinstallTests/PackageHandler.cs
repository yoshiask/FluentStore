using Microsoft.Marketplace.Storefront.Contracts;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetTests;

public class PackageHandler
{
    private readonly StorefrontApi _api = new();

    [Fact]
    public async Task GetFeatured()
    {
        var collection = await _api.GetRecommendationCollection("TopFree", options: TestOptions);
        Assert.NotNull(collection);
        Assert.NotNull(collection.Payload);
        Assert.NotEmpty(collection.Payload.Cards);
    }

    [Theory]
    [InlineData("XP9KHM4BK9FZ7Q", CatalogIdType.ProductId)]
    public async Task GetPage(string id, CatalogIdType idType)
    {
        var page = await _api.GetPage(id, idType, options: TestOptions);
        Assert.NotEmpty(page);
    }

    [Theory]
    [InlineData("XP9KHM4BK9FZ7Q", CatalogIdType.ProductId)]
    public async Task GetProduct(string id, CatalogIdType idType)
    {
        var response = await _api.GetProduct(id, idType, options: TestOptions);
        Assert.NotNull(response);

        var product = response.Payload;
        Assert.NotNull(product);
        Assert.NotNull(product.Title);
    }

    [Theory]
    [InlineData("XP9KHM4BK9FZ7Q")]
    public async Task GetProductReviews(string id)
    {
        var response = await _api.GetProductReviews(id, options: TestOptions);

        Assert.NotNull(response);
        Assert.NotNull(response.Payload);
        Assert.NotNull(response.Payload.Reviews);
        Assert.NotEmpty(response.Payload.Reviews);
    }

    [Theory]
    [InlineData("XP9KHM4BK9FZ7Q")]
    public async Task GetAllProductReviews(string id)
    {
        var reviews = await _api.GetAllProductReviews(id, options: TestOptions).ToListAsync();

        Assert.NotNull(reviews);
        Assert.NotEmpty(reviews);
    }

    [Theory]
    [InlineData("visual")]
    public async Task Search(string query)
    {
        var response = await _api.Search(query, options: TestOptions);

        Assert.NotNull(response);
        Assert.NotNull(response.Payload);
        Assert.NotEmpty(response.Payload.SearchResults);
    }

    [Theory]
    [InlineData("visual")]
    public async Task SearchSuggestions(string query)
    {
        var response = await _api.GetSearchSuggestions(query, options: TestOptions);

        Assert.NotNull(response);
        Assert.NotNull(response.Payload);
        Assert.NotEmpty(response.Payload.SearchSuggestions);
    }

    [Fact]
    public async Task GetHomeSpotlight()
    {
        var page = await _api.GetHomeSpotlight(options: TestOptions);
        Assert.NotNull(page);
        Assert.NotEmpty(page.Cards);
    }

    private static RequestOptions TestOptions => new()
    {
        DeviceArchitecture = "x64",
        DeviceFamily = "Windows.Desktop",
        Version = 9
    };
}
