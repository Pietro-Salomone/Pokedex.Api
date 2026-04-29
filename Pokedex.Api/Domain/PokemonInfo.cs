namespace Pokedex.Api.Domain
{
	public sealed record PokemonInfo(
			string? Name,
			string? Description,
			string? Habitat,
			bool IsLegendary);
}
