using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Axene.Mailer;
using Xunit;

namespace Axene.Mailer.Tests
{
    /// <summary>Captures the outgoing request and returns a scripted sequence of responses.</summary>
    internal sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;
        public readonly List<HttpRequestMessage> Requests = new();
        public readonly List<string> Bodies = new();

        public RecordingHandler(params HttpResponseMessage[] responses) => _responses = new Queue<HttpResponseMessage>(responses);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add(request);
            Bodies.Add(request.Content == null ? "" : await request.Content.ReadAsStringAsync());
            return _responses.Count > 1 ? _responses.Dequeue() : _responses.Peek();
        }

        public static HttpResponseMessage Json(HttpStatusCode code, string body) =>
            new HttpResponseMessage(code) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
    }

    public class ClientTests
    {
        private static AxeneMailerClient Client(RecordingHandler h) =>
            new AxeneMailerClient("axm_k_test", httpClient: new HttpClient(h));

        [Fact]
        public async Task Send_maps_From_to_from_underscore_and_sets_bearer()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.Accepted, "{\"id\":\"em_1\",\"status\":\"queued\"}"));
            var res = await Client(h).SendAsync(new SendEmail
            {
                From = new Address("hello@shop.co", "Shop"),
                To = { "a@example.com" },
                Subject = "Hi",
                Html = "<p>x</p>",
            });

            Assert.Equal("em_1", res.Id);
            Assert.Equal("queued", res.Status);

            var req = h.Requests[0];
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.EndsWith("/v1/emails/", req.RequestUri!.ToString());
            Assert.Equal("Bearer", req.Headers.Authorization!.Scheme);
            Assert.Equal("axm_k_test", req.Headers.Authorization!.Parameter);

            using var doc = JsonDocument.Parse(h.Bodies[0]);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("from_", out var from)); // mapped
            Assert.False(root.TryGetProperty("from", out _));
            Assert.Equal("hello@shop.co", from.GetProperty("email").GetString());
            Assert.Equal("Shop", from.GetProperty("name").GetString());
            Assert.Equal(JsonValueKind.Array, root.GetProperty("to").ValueKind);
            Assert.False(root.TryGetProperty("text", out _)); // nulls pruned
        }

        [Fact]
        public async Task Non2xx_throws_AxeneException_with_code()
        {
            var h = new RecordingHandler(RecordingHandler.Json(
                (HttpStatusCode)422, "{\"detail\":{\"code\":\"invalid\",\"message\":\"bad from\"}}"));
            var ex = await Assert.ThrowsAsync<AxeneException>(() =>
                Client(h).SendAsync(new SendEmail { From = "f@x.co", To = { "a@x.co" }, Subject = "s" }));
            Assert.Equal(422, ex.Status);
            Assert.Equal("invalid", ex.Code);
            Assert.Equal("bad from", ex.Message);
        }

        [Fact]
        public async Task Retries_5xx_then_succeeds()
        {
            var h = new RecordingHandler(
                RecordingHandler.Json(HttpStatusCode.ServiceUnavailable, "{}"),
                RecordingHandler.Json(HttpStatusCode.ServiceUnavailable, "{}"),
                RecordingHandler.Json(HttpStatusCode.Accepted, "{\"id\":\"ok\",\"status\":\"queued\"}"));
            var res = await Client(h).SendAsync(new SendEmail { From = "f@x.co", To = { "a@x.co" }, Subject = "s" });
            Assert.Equal("ok", res.Id);
            Assert.Equal(3, h.Requests.Count);
        }

        [Fact]
        public async Task Address_converts_implicitly_from_string()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.Accepted, "{\"id\":\"x\",\"status\":\"queued\"}"));
            await Client(h).SendAsync(new SendEmail { From = "f@x.co", To = { "a@x.co", "b@x.co" }, Subject = "s" });
            using var doc = JsonDocument.Parse(h.Bodies[0]);
            var to = doc.RootElement.GetProperty("to");
            Assert.Equal(2, to.GetArrayLength());
            Assert.Equal("a@x.co", to[0].GetProperty("email").GetString());
        }
    }
}
