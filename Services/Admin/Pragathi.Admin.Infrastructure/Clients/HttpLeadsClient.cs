using System.Text;
using Pragathi.Admin.Application.Interfaces;

namespace Pragathi.Admin.Infrastructure.Clients;

public class HttpLeadsClient : ILeadsClient
{
    private readonly HttpClient _http;
    public HttpLeadsClient(HttpClient http) => _http = http;

    public async Task<LeadsResponse> SendAsync(string method, string path, string? body = null)
    {
        try
        {
            var request = new HttpRequestMessage(new HttpMethod(method), path);
            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            return new LeadsResponse((int)response.StatusCode, text,
                response.Content.Headers.ContentType?.ToString());
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Leads is asleep or down: give a clear answer instead of crashing
            return new LeadsResponse(503, "{\"message\":\"Leads service is unavailable. Please try again.\"}",
                "application/json");
        }
    }
}