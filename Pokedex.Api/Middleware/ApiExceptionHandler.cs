using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pokedex.Api.Exceptions;

namespace Pokedex.Api.Middleware
{
	public sealed class ApiExceptionHandler : IExceptionHandler
	{
		private readonly ILogger<ApiExceptionHandler> _logger;

		public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
		{
			_logger = logger;
		}

		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			var problem = exception switch
			{
				PokemonNotFoundException notFound => new ProblemDetails
				{
					Status = StatusCodes.Status404NotFound,
					Title = "Pokemon not found",
					Detail = $"No Pokemon species named '{notFound.PokemonName}' was found.",
					Instance = httpContext.Request.Path
				},
				PokeApiException => new ProblemDetails
				{
					Status = StatusCodes.Status502BadGateway,
					Title = "PokeAPI dependency error",
					Detail = "Pokemon information could not be retrieved from the upstream API.",
					Instance = httpContext.Request.Path
				},
				ArgumentException => new ProblemDetails
				{
					Status = StatusCodes.Status400BadRequest,
					Title = "Invalid request",
					Detail = "The Pokemon name must be provided.",
					Instance = httpContext.Request.Path
				},
				_ => new ProblemDetails
				{
					Status = StatusCodes.Status500InternalServerError,
					Title = "Unexpected server error",
					Detail = "The API could not complete the request.",
					Instance = httpContext.Request.Path
				}
			};

			if (problem.Status >= StatusCodes.Status500InternalServerError)
			{
				_logger.LogError(exception, "Unhandled API exception.");
			}
			else
			{
				_logger.LogInformation(exception, "Handled API exception.");
			}

			httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
			await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
			return true;
		}

	}
}
