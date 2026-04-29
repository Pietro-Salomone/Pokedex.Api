using Microsoft.Extensions.Options;
using Pokedex.Api.Clients;
using Pokedex.Api.Clients.Interfaces;
using Pokedex.Api.Options;
using Pokedex.Api.Services;
using Pokedex.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
		.AddOptions<ExternalApiOptions>()
		.Bind(builder.Configuration.GetSection(ExternalApiOptions.SectionName))
		.Validate(options => Uri.TryCreate(options.PokeApiBaseUrl, UriKind.Absolute, out _),
				"ExternalApis:PokeApiBaseUrl must be an absolute URL.")
		.Validate(options => Uri.TryCreate(options.FunTranslationsBaseUrl, UriKind.Absolute, out _),
				"ExternalApis:FunTranslationsBaseUrl must be an absolute URL.")
		.Validate(options => options.TimeoutSeconds is > 0 and <= 30,
				"ExternalApis:TimeoutSeconds must be between 1 and 30 seconds.")
		.ValidateOnStart();

builder.Services.AddScoped<IPokemonService, PokemonService>();

builder.Services.AddHttpClient<IPokeApiClient, PokeApiClient>((serviceProvider, httpClient) =>
{
	var options = serviceProvider.GetRequiredService<IOptions<ExternalApiOptions>>().Value;
	httpClient.BaseAddress = EnsureTrailingSlash(options.PokeApiBaseUrl);
	httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

static Uri EnsureTrailingSlash(string value)
{
	var url = value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
	return new Uri(url, UriKind.Absolute);
}