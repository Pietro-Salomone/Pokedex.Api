namespace Pokedex.Api.Options
{
	public sealed class ExternalApiOptions
	{
		public const string SectionName = "ExternalApis";
		public string PokeApiBaseUrl { get; init; } = "https://pokeapi.co/api/v2/";
		public string FunTranslationsBaseUrl { get; init; } = "https://api.funtranslations.mercxry.me/v1/";
		public int TimeoutSeconds { get; init; } = 10;
	}
}
