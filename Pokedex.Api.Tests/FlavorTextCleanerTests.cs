using Pokedex.Api.Domain;
using Xunit;

namespace Pokedex.Api.Tests;

public sealed class FlavorTextCleanerTests
{
	[Fact]
	public void Clean_RemovesPokeApiLineBreaksAndRepeatedWhitespace()
	{
		var result = Helper.CleanText("When the bulb on\nits back grows\flarge, it appears\tto stand.");

		Assert.Equal("When the bulb on its back grows large, it appears to stand.", result);
	}

	[Fact]
	public void Clean_JoinsSoftHyphenLineBreaksInsideWords()
	{
		var result = Helper.CleanText("Pounds with fore\u00AD\nlegs or tail.");

		Assert.Equal("Pounds with forelegs or tail.", result);
	}
}
