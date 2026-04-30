using System.Net;
using System.Text;

namespace Pokedex.Api.Tests.TestDoubles
{
	internal sealed class StubHttpMessageHandler : HttpMessageHandler
	{
		private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses;

		public StubHttpMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
		{
			_responses = new Queue<Func<HttpRequestMessage, HttpResponseMessage>>(responses);
		}

		public int Calls { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Calls++;
			if (_responses.Count == 0)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
			}

			return Task.FromResult(_responses.Dequeue()(request));
		}

		public static HttpResponseMessage Json(HttpStatusCode statusCode, string json) =>
				new(statusCode)
				{
					Content = new StringContent(json, Encoding.UTF8, "application/json")
				};
	}
}
