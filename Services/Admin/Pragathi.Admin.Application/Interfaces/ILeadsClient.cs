namespace Pragathi.Admin.Application.Interfaces;

// What the Leads service answered
public record LeadsResponse(int StatusCode, string? Body, string? ContentType);

// "A way to call the Leads service" (the how is decided elsewhere)
public interface ILeadsClient
{
    Task<LeadsResponse> SendAsync(string method, string path, string? body = null);
}