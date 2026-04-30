using Pokedex.Api.Clients.Interfaces;
using Pokedex.Api.Domain;
using Pokedex.Api.Services;
using Xunit;

namespace Pokedex.Api.Tests;

public sealed class PokemonServiceTests
{
	private static readonly PokemonInfo NonLegendaryPokemon = new PokemonInfo
	(
		"bulbasaur",
		"A strange seed was planted on its back at birth.",
		"grassland",
		false
	);

	[Fact]
	public async Task GetBasicInfoAsync_ReturnsPokemonWithoutCallingTranslator()
	{
		var pokeApi = new StubPokeApiClient(NonLegendaryPokemon);
		var translator = new StubFunTranslationsClient("translated");
		var service = new PokemonService(pokeApi, translator);

		var result = await service.GetPokemonInfoAsync("bulbasaur", CancellationToken.None);

		Assert.Equal(NonLegendaryPokemon, result);
		Assert.Empty(translator.Calls);
	}

	[Fact]
	public async Task GetTranslatedInfoAsync_UsesYodaForLegendaryPokemon()
	{
		var mewtwo = NonLegendaryPokemon with
		{
			Name = "mewtwo",
			Habitat = "rare",
			IsLegendary = true
		};
		var translator = new StubFunTranslationsClient("Created by a scientist, it was.");
		var service = new PokemonService(new StubPokeApiClient(mewtwo), translator);

		var result = await service.GetTranslatedPokemonDescriptionAsync("mewtwo", CancellationToken.None);

		Assert.Equal("Created by a scientist, it was.", result.Description);
		var call = Assert.Single(translator.Calls);
		Assert.Equal(TranslationKind.Yoda, call.Kind);
		Assert.Equal(mewtwo.Description, call.Text);
	}

	[Fact]
	public async Task GetTranslatedInfoAsync_UsesYodaForCavePokemonEvenWhenNotLegendary()
	{
		var zubat = NonLegendaryPokemon with
		{
			Name = "zubat",
			Habitat = "cave",
			IsLegendary = false
		};
		var translator = new StubFunTranslationsClient("In caves, lives it does.");
		var service = new PokemonService(new StubPokeApiClient(zubat), translator);

		var result = await service.GetTranslatedPokemonDescriptionAsync("zubat", CancellationToken.None);

		Assert.Equal("In caves, lives it does.", result.Description);
		Assert.Equal(TranslationKind.Yoda, Assert.Single(translator.Calls).Kind);
	}

	[Fact]
	public async Task GetTranslatedInfoAsync_UsesShakespeareForOtherPokemon()
	{
		var translator = new StubFunTranslationsClient("A most strange seed was planted upon its back at birth.");
		var service = new PokemonService(new StubPokeApiClient(NonLegendaryPokemon), translator);

		var result = await service.GetTranslatedPokemonDescriptionAsync("bulbasaur", CancellationToken.None);

		Assert.Equal("A most strange seed was planted upon its back at birth.", result.Description);
		Assert.Equal(TranslationKind.Shakespeare, Assert.Single(translator.Calls).Kind);
	}

	[Fact]
	public async Task GetTranslatedInfoAsync_FallsBackToStandardDescriptionWhenTranslatorReturnsNothing()
	{
		var translator = new StubFunTranslationsClient(null);
		var service = new PokemonService(new StubPokeApiClient(NonLegendaryPokemon), translator);

		var result = await service.GetTranslatedPokemonDescriptionAsync("bulbasaur", CancellationToken.None);

		Assert.Equal(NonLegendaryPokemon.Description, result.Description);
	}

	[Fact]
	public async Task GetTranslatedInfoAsync_FallsBackToStandardDescriptionWhenTranslatorThrows()
	{
		var translator = new StubFunTranslationsClient("ignored") { ThrowOnTranslate = true };
		var service = new PokemonService(new StubPokeApiClient(NonLegendaryPokemon), translator);

		var result = await service.GetTranslatedPokemonDescriptionAsync("bulbasaur", CancellationToken.None);

		Assert.Equal(NonLegendaryPokemon.Description, result.Description);
	}

	private sealed class StubPokeApiClient : IPokeApiClient
	{
		private readonly PokemonInfo _pokemonInfo;

		public StubPokeApiClient(PokemonInfo pokemonInfo)
		{
			_pokemonInfo = pokemonInfo;
		}

		public Task<PokemonInfo> GetPokemonInfoAsync(string name, CancellationToken cancellationToken) =>
				Task.FromResult(_pokemonInfo);
	}

	private sealed class StubFunTranslationsClient : IFunTranslationsClient
	{
		private readonly string? _translation;

		public StubFunTranslationsClient(string? translation)
		{
			_translation = translation;
		}

		public List<(string Text, TranslationKind Kind)> Calls { get; } = [];

		public bool ThrowOnTranslate { get; init; }

		public Task<string?> TranslateDescriptionAsync(string text, TranslationKind translationKind, CancellationToken cancellationToken)
		{
			Calls.Add((text, translationKind));

			if (ThrowOnTranslate)
			{
				throw new InvalidOperationException("Translator failed.");
			}

			return Task.FromResult(_translation);
		}
	}
}
