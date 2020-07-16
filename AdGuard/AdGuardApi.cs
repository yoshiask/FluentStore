using AdGuard.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static AdGuard.Constants;

namespace AdGuard
{
    public static class AdGuardApi
    {
        /// <summary>
        /// Gets a list of all public packages associated with the product
        /// </summary>
        public static async Task<List<Package>> GetFilesFromProductId(string productId, string lang = "en-us")
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(API_HOST)
            };
            var response = await client.PostAsync("/api/GetFiles", new FormUrlEncodedContent(
                new Dictionary<string, string>()
                {
                    { "type", "ProductId" },
                    { "url", productId },
                    { "ring", "RP" },
                    { "lang", lang }
                }
            ));

            if (!response.IsSuccessStatusCode || response.Content == null)
                return null;

            var html = new HtmlDocument();
            html.LoadHtml(await response.Content.ReadAsStringAsync());
            var linkNodes = html.DocumentNode.SelectNodes("//a");

            var packages = new List<Package>();
            if (linkNodes == null)
                return packages;
            foreach (var a in linkNodes)
            {
                packages.Add(new Package()
                {
                    Name = a.InnerText,
                    Url = a.GetAttributeValue("href", ""),
                    ExpiryDate = DateTimeOffset.Parse(a.ParentNode.NextSibling.InnerText)
                });
            }
            return packages;
        }
    }
}
