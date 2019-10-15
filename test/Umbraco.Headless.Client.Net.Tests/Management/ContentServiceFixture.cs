using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Umbraco.Headless.Client.Net.Configuration;
using Umbraco.Headless.Client.Net.Management;
using Umbraco.Headless.Client.Net.Management.Models;
using Xunit;

namespace Umbraco.Headless.Client.Net.Tests.Management
{
    public class ContentServiceFixture
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly IHeadlessConfiguration _configuration = new FakeHeadlessConfiguration();

        public ContentServiceFixture()
        {
            _mockHttp = new MockHttpMessageHandler();
        }

        [Fact]
        public async Task Create_ReturnsCreatedContent()
        {
            var content = new Content();

            var service = new ContentService(_configuration,
                GetMockedHttpClient("/content", ContentServiceJson.Create));

            var result = await service.Create(content);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Delete_ReturnsDeletedContent()
        {
            var service = new ContentService(_configuration,
                GetMockedHttpClient("/content/05a38d71-0ae8-48d6-8215-e0cb857a31a8", ContentServiceJson.Delete));

            var result = await service.Delete(new Guid("05a38d71-0ae8-48d6-8215-e0cb857a31a8"));

            Assert.NotNull(result);
            Assert.Equal(DateTime.Parse("2019-08-22T12:11:34.4136405Z").ToUniversalTime(), result.DeleteDate.GetValueOrDefault().ToUniversalTime());
        }

        [Fact]
        public async Task GetById_ReturnsContent()
        {
            var service = new ContentService(_configuration,
                GetMockedHttpClient("/content/05a38d71-0ae8-48d6-8215-e0cb857a31a8", ContentServiceJson.ById));

            var result = await service.GetById(new Guid("05a38d71-0ae8-48d6-8215-e0cb857a31a8"));

            Assert.NotNull(result);
            Assert.Equal(DateTime.Parse("2019-06-17T13:46:24.497Z").ToUniversalTime(),
                result.CreateDate.ToUniversalTime());
            Assert.Collection(result.CurrentVersionState,
                pair =>
                {
                    var (culture, value) = pair;
                    Assert.Equal("en-US", culture);
                    Assert.Equal(ContentSavedState.Draft, value);
                },
                pair =>
                {
                    var (culture, value) = pair;
                    Assert.Equal("da", culture);
                    Assert.Equal(ContentSavedState.Published, value);
                }
            );
            Assert.Collection(result.Name,
                pair =>
                {
                    var (culture, value) = pair;
                    Assert.Equal("en-US", culture);
                    Assert.Equal("Biker Jacket", value);
                },
                pair =>
                {
                    var (culture, value) = pair;
                    Assert.Equal("da", culture);
                    Assert.Equal("Biker Jakke", value);
                }
            );
            Assert.Collection(result.UpdateDate,
                pair =>
                {
                    var (culture, value) = pair;
                    Assert.Equal("en-US", culture);
                    Assert.Equal(DateTime.Parse("2019-06-26T22:51:22.48Z").ToUniversalTime(),
                        value.GetValueOrDefault().ToUniversalTime());
                },
                pair =>
                {
                    var (culture, value) = pair;
                    Assert.Equal("da", culture);
                    Assert.Equal(DateTime.Parse("2019-06-26T22:38:16.617Z").ToUniversalTime(),
                        value.GetValueOrDefault().ToUniversalTime());
                }
            );
            Assert.Null(result.DeleteDate);
            Assert.False(result.HasChildren);
            Assert.Equal(2, result.Level);
            Assert.Equal("product", result.ContentTypeAlias);
            Assert.Equal(new Guid("ec4aafcc-0c25-4f25-a8fe-705bfae1d324"), result.ParentId);
            Assert.Equal(7, result.SortOrder);
            Assert.Collection(result.Properties,
                pair =>
                {
                    var (alias, cultures) = pair;
                    Assert.Equal("productName", alias);
                    Assert.Collection(cultures,
                        cultureValue =>
                        {
                            var (culture, value) = cultureValue;
                            Assert.Equal("en-US", culture);
                            Assert.Equal("Biker Jacket", value);
                        },
                        cultureValue =>
                        {
                            var (culture, value) = cultureValue;
                            Assert.Equal("da", culture);
                            Assert.Equal("Biker Jakke", value);
                        }
                    );
                },
                _ => { },
                _ => { },
                pair =>
                {
                    var (alias, cultures) = pair;
                    Assert.Equal("sku", alias);
                    Assert.Equal("UMB-BIKER-JACKET", cultures["$invariant"]);
                },
                _ => { },
                pair =>
                {
                    var (alias, cultures) = pair;
                    Assert.Equal("features", alias);
                    Assert.Collection(cultures,
                        cultureValue =>
                        {
                            var (culture, value) = cultureValue;
                            Assert.Equal("en-US", culture);
                            Assert.IsAssignableFrom<JArray>(value);
                        },
                        _ => {}
                    );
                }
            );
        }

        [Fact]
        public async Task GetRoot_ReturnsContent()
        {
            var service = new ContentService(_configuration,
                GetMockedHttpClient("/content", ContentServiceJson.AtRoot));

            var result = await service.GetRoot();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetChildren_ReturnsContent()
        {
            var service = new ContentService(_configuration,
                GetMockedHttpClient("/content/ec4aafcc-0c25-4f25-a8fe-705bfae1d324/children?page=2&pageSize=5", ContentServiceJson.Children));

            var result = await service.GetChildren(new Guid("ec4aafcc-0c25-4f25-a8fe-705bfae1d324"), 2, 5);

            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(8, result.TotalItems);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.Content.Items.Count());
        }

        private HttpClient GetMockedHttpClient(string url, string jsonResponse)
        {
            _mockHttp.When(url).Respond("application/json", jsonResponse);
            var client = new HttpClient(_mockHttp) { BaseAddress = new Uri(Constants.Urls.BaseApiUrl) };
            return client;
        }
    }
}
