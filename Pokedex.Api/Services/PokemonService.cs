using Pokedex.Api.Clients.Interfaces;
using Pokedex.Api.Domain;
using Pokedex.Api.Services.Interfaces;

namespace Pokedex.Api.Services
{
	public sealed class PokemonService : IPokemonService
	{
		private readonly IPokeApiClient _pokeApiClient;
		private readonly IFunTranslationsClient _funTranslationsClient;

		public PokemonService(IPokeApiClient pokeApiClient, IFunTranslationsClient funTranslationsClient)
		{
			_pokeApiClient = pokeApiClient;
			_funTranslationsClient = funTranslationsClient;
		}
		public async Task<PokemonInfo> GetPokemonInfoAsync(string name, CancellationToken cancellationToken)
		{
			return await _pokeApiClient.GetPokemonInfoAsync(name, cancellationToken);
		}

		public async Task<PokemonInfo> GetTranslatedPokemonDescriptionAsync(string name, CancellationToken cancellationToken)
		{
			var pokemon = await _pokeApiClient.GetPokemonInfoAsync(name, cancellationToken);
			var translationKind = ShouldUseYoda(pokemon) ? TranslationKind.Yoda : TranslationKind.Shakespeare;

			string? translatedDescription;

			try
			{
				if (string.IsNullOrWhiteSpace(pokemon.Description))
				{
					return pokemon;
				}

				translatedDescription = await _funTranslationsClient.TranslateDescriptionAsync(
						pokemon.Description,
						translationKind,
						cancellationToken);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch
			{
				// The exercise explicitly requires a fallback to the standard description whenever translation fails.
				translatedDescription = null;
			}

			return string.IsNullOrWhiteSpace(translatedDescription)
					? pokemon
					: pokemon with { Description = Helper.CleanText(translatedDescription) };
		}

		private static bool ShouldUseYoda(PokemonInfo pokemon) =>
				pokemon.IsLegendary || string.Equals(pokemon.Habitat, "cave", StringComparison.OrdinalIgnoreCase);
	}
}
