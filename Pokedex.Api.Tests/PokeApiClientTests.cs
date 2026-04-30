using System.Net;
using Pokedex.Api.Clients;
using Pokedex.Api.Exceptions;
using Pokedex.Api.Tests.TestDoubles;
using Xunit;

namespace Pokedex.Api.Tests;

public sealed class PokeApiClientTests
{
	[Fact]
	public async Task GetPokemonInfoAsync_ReadsSpeciesPayloadAndCleansEnglishDescription()
	{
		const string speciesJson = """
        {
          "name": "mewtwo",
          "is_legendary": true,
          "habitat": { "name": "rare" },
          "flavor_text_entries": [
            {
              "flavor_text": "Japanese text should not be used.",
              "language": { "name": "ja" }
            },
            {
              "flavor_text": "It was created by a scientist after years of horrific\n gene splicing and DNA engineering experiments.",
              "language": { "name": "en" }
            }
          ]
        }
        """;

		var handler = new StubHttpMessageHandler(request =>
		{
			Assert.Equal("https://pokeapi.co/api/v2/pokemon-species/mewtwo/", request.RequestUri!.ToString());
			return StubHttpMessageHandler.Json(HttpStatusCode.OK, speciesJson);
		});
		var client = new PokeApiClient(new HttpClient(handler)
		{
			BaseAddress = new Uri("https://pokeapi.co/api/v2/")
		});

		var result = await client.GetPokemonInfoAsync(" MewTwo ", CancellationToken.None);

		Assert.Equal("mewtwo", result.Name);
		Assert.Equal("It was created by a scientist after years of horrific gene splicing and DNA engineering experiments.", result.Description);
		Assert.Equal("rare", result.Habitat);
		Assert.True(result.IsLegendary);
	}

	[Fact]
	public async Task GetPokemonInfoAsync_UsesUnknownHabitatWhenPokeApiHabitatIsNull()
	{
		const string speciesJson = """
        {
          "name": "wormadam",
          "is_legendary": false,
          "habitat": null,
          "flavor_text_entries": [
            {
              "flavor_text": "Its appearance changes depending on where it evolved.",
              "language": { "name": "en" }
            }
          ]
        }
        """;

		var client = new PokeApiClient(new HttpClient(new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, speciesJson)))
		{
			BaseAddress = new Uri("https://pokeapi.co/api/v2/")
		});

		var result = await client.GetPokemonInfoAsync("wormadam", CancellationToken.None);

		Assert.Equal("unknown", result.Habitat);
	}

	[Fact]
	public async Task GetPokemonInfoAsync_ThrowsPokemonNotFoundFor404()
	{
		var client = new PokeApiClient(new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)))
		{
			BaseAddress = new Uri("https://pokeapi.co/api/v2/")
		});

		var exception = await Assert.ThrowsAsync<PokemonNotFoundException>(() => client.GetPokemonInfoAsync("missingno", CancellationToken.None));

		Assert.Equal("missingno", exception.PokemonName);
	}

	[Fact]
	public async Task GetPokemonInfoAsync_ThrowsDependencyExceptionWhenEnglishDescriptionIsMissing()
	{
		const string speciesJson = """
        {
          "name": "pikachu",
          "is_legendary": false,
          "habitat": { "name": "forest" },
          "flavor_text_entries": [
            {
              "flavor_text": "Only non-English text.",
              "language": { "name": "ja" }
            }
          ]
        }
        """;

		var client = new PokeApiClient(new HttpClient(new StubHttpMessageHandler(_ => StubHttpMessageHandler.Json(HttpStatusCode.OK, speciesJson)))
		{
			BaseAddress = new Uri("https://pokeapi.co/api/v2/")
		});

		await Assert.ThrowsAsync<PokeApiException>(() => client.GetPokemonInfoAsync("pikachu", CancellationToken.None));
	}
}
