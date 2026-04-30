using Pokedex.Api.Domain;

namespace Pokedex.Api.Clients.Interfaces
{
	public interface IFunTranslationsClient
	{
		Task<string?> TranslateDescriptionAsync(string text, TranslationKind translationKind, CancellationToken cancellationToken);
	}
}
