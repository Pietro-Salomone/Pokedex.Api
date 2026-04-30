# Pokedex API (.NET 8)

A small REST API that returns basic Pokemon information and, optionally, a fun translated description.

The implementation intentionally keeps the domain logic small and explicit:

- `GET /pokemon/{name}` returns name, standard description, habitat, and legendary status.
- `GET /pokemon/translated/{name}` returns the same shape, but translates the description with:
  - Yoda when the Pokemon habitat is `cave` or the Pokemon is legendary.
  - Shakespeare for every other Pokemon.
  - The original description when the translation dependency fails or rate-limits the request.

## Requirements

Choose one of the two ways below.

### Option A - Run with Docker

Install Docker Desktop or Docker Engine.

Navigate to the folder where the Dockerfile is located.

```bash
docker build -t pokedex-api .
docker run --rm -p 5000:8080 pokedex-api
```

Then call the API:

```bash
curl http://localhost:5000/pokemon/mewtwo
curl http://localhost:5000/pokemon/translated/mewtwo
```

### Option B - Run locally with .NET 8 SDK

Install the .NET 8 SDK from Microsoft.

Navigate to the folder where the Pokedex.Api.sln is located

Restore, test, and run:

```bash
dotnet restore Pokedex.Api.sln
dotnet test Pokedex.Api.sln
dotnet run --project ./Pokedex.Api/Pokedex.Api.csproj --urls http://localhost:5000
```

Then call the API:

```bash
curl http://localhost:5000/pokemon/mewtwo
curl http://localhost:5000/pokemon/translated/mewtwo
```

Example response:

```json
{
  "name": "mewtwo",
  "description": "Created by a scientist after years of horrific gene splicing and DNA engineering experiments, it was.",
  "habitat": "rare",
  "isLegendary": true
}
```

## Configuration

The default settings live in `src/Pokedex.Api/appsettings.json`:

```json
{
  "ExternalApis": {
    "PokeApiBaseUrl": "https://pokeapi.co/api/v2/",
    "FunTranslationsBaseUrl": "https://api.funtranslations.mercxry.me/v1/",
    "TimeoutSeconds": 10
  }
}
```

## Project layout

```text
/Pokedex.Api
  Controllers/  Expose API endpoints
  Clients/      Typed HTTP clients for PokeAPI and FunTranslations
  DTO/          Public API response shape
  Domain/       Domain records and small pure helpers
  Exceptions/   Domain/dependency exceptions
  Middleware/   Centralized exception-to-ProblemDetails mapping
  Options/      External dependency configuration
  Services/     Application orchestration and translation choice

/Pokedex.Api.Tests
  Unit tests for dependency clients and translation decision logic
```

## Design notes

- The API uses the PokeAPI `pokemon-species` resource because it contains the fields needed by the exercise: flavor text, habitat, and `is_legendary`.
- The service layer owns the business rule for selecting Yoda vs Shakespeare. This keeps controllers/endpoints thin and makes the important behavior easy to unit test.
- FunTranslations failures return `null` from the client and are handled by the service by returning the original description. This matches the requirement that translation failure should not fail the request.
- Flavor text is normalized to remove PokeAPI line breaks, form-feed characters, tabs, repeated whitespace, and soft-hyphen line breaks.
- Missing PokeAPI Pokemon names return `404`; other PokeAPI dependency problems return `502`.
- If PokeAPI returns a null habitat, the response uses `unknown`. This avoids returning an empty string and keeps the response contract stable.


