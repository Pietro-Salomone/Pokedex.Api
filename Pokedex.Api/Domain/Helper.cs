using System.Text.RegularExpressions;

namespace Pokedex.Api.Domain
{
	public static partial class Helper
	{
		public static string Normalize(string name)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(name);
			return name.Trim().ToLowerInvariant();
		}
		public static string CleanText(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			var withoutSoftHyphenBreaks = SoftHyphenAndFollowingWhitespaceRegex().Replace(value, string.Empty);
			var normalizedLineBreaks = withoutSoftHyphenBreaks
					.Replace("\r\n", " ", StringComparison.Ordinal)
					.Replace('\n', ' ')
					.Replace('\r', ' ')
					.Replace('\f', ' ')
					.Replace('\t', ' ');

			return WhitespaceRegex().Replace(normalizedLineBreaks, " ").Trim();
		}

		[GeneratedRegex("\\u00AD\\s*")]
		private static partial Regex SoftHyphenAndFollowingWhitespaceRegex();

		[GeneratedRegex("\\s+")]
		private static partial Regex WhitespaceRegex();

	}
}
