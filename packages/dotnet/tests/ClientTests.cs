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
    
        [Fact]
        public async Task SendBatch_posts_bare_array()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.Accepted,
                "{\"total\":1,\"sent\":1,\"failed\":0,\"results\":[{\"id\":\"a\",\"status\":\"queued\"}]}"));
            var r = await Client(h).SendBatchAsync(new[] {
                new SendEmail { From = "f@x.co", To = { "a@x.co" }, Subject = "s" } });
            Assert.Equal(1, r.Total);
            Assert.Equal("a", r.Results[0].Id);
            var body = h.Bodies[0].TrimStart();
            Assert.StartsWith("[", body); // bare array, not { "emails": ... }
            Assert.Contains("from_", body);
        }

        [Fact]
        public async Task Validate_posts_full_send_body()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.OK,
                "{\"valid\":true,\"can_send\":true,\"issues\":[],\"plan\":\"starter\"," +
                "\"usage\":{\"daily\":1,\"daily_limit\":100,\"monthly\":5,\"monthly_limit\":3000}}"));
            var r = await Client(h).Emails.ValidateAsync(new SendEmail
            {
                From = new Address("f@x.co", "F"),
                To = { "a@x.co" },
                Subject = "s",
                Html = "<p>hi</p>",
            });

            Assert.True(r.Valid);
            Assert.True(r.CanSend);
            Assert.Equal("starter", r.Plan);
            Assert.Equal(100, r.Usage!.DailyLimit);

            var req = h.Requests[0];
            Assert.EndsWith("/v1/emails/validate", req.RequestUri!.ToString());
            using var doc = JsonDocument.Parse(h.Bodies[0]);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("from_", out _)); // full send body, mapped
            Assert.Equal("s", root.GetProperty("subject").GetString());
            Assert.Equal("<p>hi</p>", root.GetProperty("html").GetString());
        }

        [Fact]
        public async Task UploadCsv_sends_multipart_file_field()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.OK,
                "{\"imported\":2,\"skipped\":0,\"errors\":[]}"));
            var bytes = System.Text.Encoding.UTF8.GetBytes("email,name\na@x.co,A\n");
            var r = await Client(h).Contacts.UploadCsvAsync("list_1", bytes, "people.csv");

            Assert.Equal(2, r.Imported);

            var req = h.Requests[0];
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.EndsWith("/v1/contacts/list_1/upload", req.RequestUri!.ToString());
            Assert.StartsWith("multipart/form-data", req.Content!.Headers.ContentType!.MediaType);
            Assert.Contains("name=file", h.Bodies[0].Replace("\"", "")); // field name is `file`
            Assert.Contains("people.csv", h.Bodies[0]);
        }

        [Fact]
        public async Task Suppressions_list_parses_envelope()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.OK,
                "{\"items\":[{\"id\":\"s1\",\"email_address\":\"a@x.co\",\"reason\":\"bounce\"}]," +
                "\"total\":1,\"page\":0,\"limit\":50}"));
            var page = await Client(h).Suppressions.ListAsync();

            Assert.Equal(1, page.Total);
            Assert.Equal(0, page.Page_);
            Assert.Equal(50, page.Limit);
            Assert.Single(page.Items);
            Assert.Equal("a@x.co", page.Items[0].EmailAddress);

            Assert.Contains("page=0", h.Requests[0].RequestUri!.ToString());
        }

        [Fact]
        public async Task Templates_create_maps_html_to_html_body()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.Created,
                "{\"id\":\"t1\",\"name\":\"Welcome\",\"html_body\":\"<p>hi</p>\"}"));
            var t = await Client(h).Templates.CreateAsync("Welcome", html: "<p>hi</p>", text: "hi");

            Assert.Equal("t1", t.Id);

            using var doc = JsonDocument.Parse(h.Bodies[0]);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("html_body", out var html));
            Assert.Equal("<p>hi</p>", html.GetString());
            Assert.True(root.TryGetProperty("text_body", out _));
            Assert.False(root.TryGetProperty("html", out _)); // not the raw `html` key
        }

        [Fact]
        public async Task Webhooks_update_maps_isActive_to_is_active()
        {
            var h = new RecordingHandler(RecordingHandler.Json(HttpStatusCode.OK,
                "{\"id\":\"w1\",\"url\":\"https://x.co/hook\",\"events\":[\"email.delivered\"]," +
                "\"is_active\":false,\"created_at\":\"2026-01-01T00:00:00Z\"}"));
            var w = await Client(h).Webhooks.UpdateAsync("w1", isActive: false);

            Assert.False(w.IsActive);

            var req = h.Requests[0];
            Assert.Equal("PATCH", req.Method.Method);
            using var doc = JsonDocument.Parse(h.Bodies[0]);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("is_active", out var active));
            Assert.False(active.GetBoolean());
            Assert.False(root.TryGetProperty("url", out _)); // pruned: not supplied
        }
}
}
