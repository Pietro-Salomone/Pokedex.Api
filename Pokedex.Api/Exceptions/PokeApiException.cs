namespace Pokedex.Api.Exceptions
{
	public sealed class PokeApiException : Exception
	{
		public PokeApiException(string message)
			: base(message)
		{
		}

		public PokeApiException(string message, Exception innerException)
		: base(message, innerException)
		{
		}
	}
}
