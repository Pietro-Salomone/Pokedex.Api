using Pokedex.Api.Domain;

namespace Pokedex.Api.DTO
{
	public sealed record PokemonResponse(
			string? Name,
			string? Description,
			string? Habitat,
			bool IsLegendary)
	{

		public static PokemonResponse FromDomain(PokemonInfo pokemonInfo) =>
				new PokemonResponse
				(
					pokemonInfo.Name,
					pokemonInfo.Description,
					pokemonInfo.Habitat,
					pokemonInfo.IsLegendary
				);
	}
}
