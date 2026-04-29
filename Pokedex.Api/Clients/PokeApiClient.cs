using System.Text.Json;
using System.Text.Json.Serialization;
using Pokedex.Api.Clients.Interfaces;
using Pokedex.Api.Domain;

namespace Pokedex.Api.Clients
{
	public sealed class PokeApiClient : IPokeApiClient
	{
		private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
		private readonly HttpClient _httpClient;

		public PokeApiClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<PokemonInfo> GetPokemonInfoAsync(string name, CancellationToken cancellationToken)
		{
			var normalizedName = Helper.Normalize(name);
			var path = $"pokemon-species/{Uri.EscapeDataString(normalizedName)}/";

			using var response = await _httpClient.GetAsync(path, cancellationToken);

			PokemonSpeciesResponse? species;

			species = await response.Content.ReadFromJsonAsync<PokemonSpeciesResponse>(JsonOptions, cancellationToken);

			var description = species.FlavorTextEntries
					.Where(entry => string.Equals(entry.Language?.Name, "en", StringComparison.OrdinalIgnoreCase))
					.Select(entry => entry.FlavorText)
					.FirstOrDefault(flavorText => !string.IsNullOrWhiteSpace(flavorText));

			return new PokemonInfo
			(
				species.Name ?? normalizedName,
				Helper.CleanText(description),
				species.Habitat?.Name ?? "unknown",
				species.IsLegendary
			);
		}

		#region private
		private sealed record PokemonSpeciesResponse
		{
			[JsonPropertyName("name")]
			public string? Name { get; init; }

			[JsonPropertyName("is_legendary")]
			public bool IsLegendary { get; init; }

			[JsonPropertyName("habitat")]
			public NamedResource? Habitat { get; init; }

			[JsonPropertyName("flavor_text_entries")]
			public IReadOnlyList<FlavorTextEntry> FlavorTextEntries { get; init; } = Array.Empty<FlavorTextEntry>();
		}

		private sealed record FlavorTextEntry
		{
			[JsonPropertyName("flavor_text")]
			public string? FlavorText { get; init; }

			[JsonPropertyName("language")]
			public NamedResource? Language { get; init; }
		}

		private sealed record NamedResource
		{
			[JsonPropertyName("name")]
			public string? Name { get; init; }
		}

		#endregion
	}
}

