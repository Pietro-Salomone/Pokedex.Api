using Pokedex.Api.Domain;

namespace Pokedex.Api.Clients.Interfaces
{
	public interface IPokeApiClient
	{
		Task<PokemonInfo> GetPokemonInfoAsync(string name, CancellationToken cancellationToken);
	}
}
