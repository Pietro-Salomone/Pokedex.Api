using Microsoft.AspNetCore.Mvc;
using Pokedex.Api.DTO;
using Pokedex.Api.Services.Interfaces;

namespace Pokedex.Api.Controllers
{
	[ApiController]
	[Route("pokemon")]
	public class PokemonController : ControllerBase
	{
		private readonly ILogger<PokemonController> _logger;
		private readonly IPokemonService _pokemonService;

		public PokemonController(ILogger<PokemonController> logger, IPokemonService pokemonService)
		{
			_logger = logger;
			_pokemonService = pokemonService;
		}

		[HttpGet("{name}", Name = "GetPokemon")]
		[ProducesResponseType(typeof(PokemonResponse), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetPokemonAsync(string name, CancellationToken cancellationToken)
		{
			var pokemonInfo = await _pokemonService.GetPokemonInfoAsync(name, cancellationToken);

			return Ok(PokemonResponse.FromDomain(pokemonInfo));
		}
	}
}
