using System.Text.Json;
using System.Text.Json.Serialization;
using Pokedex.Api.Clients.Interfaces;
using Pokedex.Api.Domain;

namespace Pokedex.Api.Clients
{
	public sealed class FunTranslationsClient : IFunTranslationsClient
	{
		private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
		private readonly HttpClient _httpClient;

		public FunTranslationsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}
		public async Task<string?> TranslateDescriptionAsync(string pokemonDescription, TranslationKind translationKind, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(pokemonDescription))
			{
				return null;
			}

			var translator = translationKind switch
			{
				TranslationKind.Yoda => "yoda",
				TranslationKind.Shakespeare => "shakespeare",
				_ => throw new ArgumentOutOfRangeException(nameof(translationKind), translationKind, "Unsupported translation kind.")
			};

			try
			{
				return await TryTranslateAsync($"translate/{translator}.json", pokemonDescription, cancellationToken);
			}
			catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				return null;
			}
			catch (HttpRequestException)
			{
				return null;
			}
			catch (JsonException)
			{
				return null;
			}
		}

		#region private
		private async Task<string?> TryTranslateAsync(string path, string pokemonDescription, CancellationToken cancellationToken)
		{

			using var response = await _httpClient.PostAsJsonAsync(path, new { text = pokemonDescription });

			if (!response.IsSuccessStatusCode)
			{
				return null;
			}

			var payload = await response.Content.ReadFromJsonAsync<FunTranslationsResponse>(JsonOptions, cancellationToken);
			var translated = payload?.Contents?.Translated;

			return string.IsNullOrWhiteSpace(translated)
					? null
					: Helper.CleanText(translated);
		}

		private sealed record FunTranslationsResponse
		{
			[JsonPropertyName("contents")]
			public FunTranslationsContents? Contents { get; init; }
		}

		private sealed record FunTranslationsContents
		{
			[JsonPropertyName("translated")]
			public string? Translated { get; init; }
		}
		#endregion
	}
}
