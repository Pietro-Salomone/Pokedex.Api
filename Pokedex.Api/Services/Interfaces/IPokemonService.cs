using Pokedex.Api.Domain;

namespace Pokedex.Api.Services.Interfaces
{
	public interface IPokemonService
	{
		Task<PokemonInfo> GetPokemonInfoAsync(string name, CancellationToken cancellationToken);

		Task<PokemonInfo> GetTranslatedPokemonDescriptionAsync(string name, CancellationToken cancellationToken);
	}
}
