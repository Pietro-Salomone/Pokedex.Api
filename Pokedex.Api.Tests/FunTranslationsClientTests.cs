using System.Net;
using Pokedex.Api.Clients;
using Pokedex.Api.Domain;
using Pokedex.Api.Tests.TestDoubles;
using Xunit;

namespace Pokedex.Api.Tests;

public sealed class FunTranslationsClientTests
{
	[Fact]
	public async Task TranslateAsync_CallsExpectedTranslatorEndpointAndReadsTranslatedText()
	{
		const string translationJson = """
        {
          "success": { "total": 1 },
          "contents": {
            "translated": "Created by a scientist, it was.",
            "text": "It was created by a scientist.",
            "translation": "yoda"
          }
        }
        """;

		var handler = new StubHttpMessageHandler(request =>
		{
			Assert.Equal("https://api.funtranslations.mercxry.me/v1/translate/yoda.json", request.RequestUri!.ToString());
			return StubHttpMessageHandler.Json(HttpStatusCode.OK, translationJson);
		});
		var client = new FunTranslationsClient(new HttpClient(handler)
		{
			BaseAddress = new Uri("https://api.funtranslations.mercxry.me/v1/")
		});

		var result = await client.TranslateDescriptionAsync("It was created by a scientist.", TranslationKind.Yoda, CancellationToken.None);

		Assert.Equal("Created by a scientist, it was.", result);
	}

	[Fact]
	public async Task TranslateAsync_ReturnsNullWhenTranslationDependencyFails()
	{
		var client = new FunTranslationsClient(new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)))
		{
			BaseAddress = new Uri("https://api.funtranslations.mercxry.me/v1/")
		});

		var result = await client.TranslateDescriptionAsync("Hello", TranslationKind.Shakespeare, CancellationToken.None);

		Assert.Null(result);
	}

	[Fact]
	public async Task TranslateAsync_ReturnsNullForMalformedJson()
	{
		var client = new FunTranslationsClient(new HttpClient(new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, "not-json")))
		{
			BaseAddress = new Uri("https://api.funtranslations.mercxry.me/v1/")
		});

		var result = await client.TranslateDescriptionAsync("Hello", TranslationKind.Yoda, CancellationToken.None);

		Assert.Null(result);
	}
}
