namespace Pokedex.Api.Exceptions
{
	public class PokemonNotFoundException : Exception
	{
		public PokemonNotFoundException(string pokemonName)
	: base($"Pokemon '{pokemonName}' not found.")
		{
			PokemonName = pokemonName;
		}
		public string PokemonName { get; }
	}
}
