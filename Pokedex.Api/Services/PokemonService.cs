using Pokedex.Api.Clients.Interfaces;
using Pokedex.Api.Domain;
using Pokedex.Api.Services.Interfaces;

namespace Pokedex.Api.Services
{
	public sealed class PokemonService : IPokemonService
	{
		private readonly IPokeApiClient _pokeApiClient;

		public PokemonService(IPokeApiClient pokeApiClient)
		{
			_pokeApiClient = pokeApiClient;
		}
		public async Task<PokemonInfo> GetPokemonInfoAsync(string name, CancellationToken cancellationToken)
		{
			return await _pokeApiClient.GetPokemonInfoAsync(name, cancellationToken);
		}
	}
}
