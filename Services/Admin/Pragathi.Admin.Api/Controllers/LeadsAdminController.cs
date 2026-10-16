using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pragathi.Admin.Application.Interfaces;

namespace Pragathi.Admin.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]                       // you must be logged in (any role) for everything below
public class LeadsAdminController : ControllerBase
{
    private readonly ILeadsClient _leads;
    public LeadsAdminController(ILeadsClient leads) => _leads = leads;

    // ---------- LOOK: Viewer, Editor and Superuser ----------
    [HttpGet("ping")]
    public IActionResult Ping() => NoContent();

    [HttpGet("leads")]
    public Task<IActionResult> GetLeads() => Forward("GET", "internal/leads");

    [HttpGet("activities")]
    public Task<IActionResult> GetActivities() => Forward("GET", "internal/activities");

    [HttpGet("visits")]
    public Task<IActionResult> GetVisits() => Forward("GET", "internal/visits");

    [HttpGet("configurations")]
    public Task<IActionResult> GetConfigurations() => Forward("GET", "internal/configurations");

    // ---------- CHANGE: Editor and Superuser only ----------
    [HttpPut("leads/{id:int}")]
    [Authorize(Roles = "Editor,Superuser")]
    public async Task<IActionResult> UpdateLead(int id) =>
        await Forward("PUT", $"internal/leads/{id}", await ReadBody());

    [HttpPut("activities/{id:int}")]
    [Authorize(Roles = "Editor,Superuser")]
    public async Task<IActionResult> UpdateActivity(int id) =>
        await Forward("PUT", $"internal/activities/{id}", await ReadBody());

    [HttpPut("visits/{id:int}")]
    [Authorize(Roles = "Editor,Superuser")]
    public async Task<IActionResult> UpdateVisit(int id) =>
        await Forward("PUT", $"internal/visits/{id}", await ReadBody());

    [HttpDelete("leads/{id:int}")]
    [Authorize(Roles = "Editor,Superuser")]
    public Task<IActionResult> DeleteLead(int id) => Forward("DELETE", $"internal/leads/{id}");

    [HttpDelete("activities/{id:int}")]
    [Authorize(Roles = "Editor,Superuser")]
    public Task<IActionResult> DeleteActivity(int id) => Forward("DELETE", $"internal/activities/{id}");

    [HttpDelete("visits/{id:int}")]
    [Authorize(Roles = "Editor,Superuser")]
    public Task<IActionResult> DeleteVisit(int id) => Forward("DELETE", $"internal/visits/{id}");

    // ---------- helpers ----------
    private async Task<string> ReadBody()
    {
        using var reader = new StreamReader(Request.Body);
        return await reader.ReadToEndAsync();
    }

    // Call the Leads service and hand its answer straight back
    private async Task<IActionResult> Forward(string method, string path, string? body = null)
    {
        var r = await _leads.SendAsync(method, path, body);
        return new ContentResult
        {
            StatusCode = r.StatusCode,
            Content = r.Body,
            ContentType = string.IsNullOrEmpty(r.Body) ? null : (r.ContentType ?? "application/json")
        };
    }
}