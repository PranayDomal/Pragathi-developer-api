using Microsoft.AspNetCore.Mvc;
using PragathiApi.Application.Interfaces;
using PragathiApi.Application.Common;
using PragathiApi.Application.Dtos;

namespace PragathiApi.Controllers;

// DEV MODE ONLY: there is no login, so every action returns 404 outside Development.
[ApiController]
[Route("api/admin")]
public class AdminController(IAdminRepository admin, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet("leads")]
    public async Task<IActionResult> Leads(CancellationToken ct) =>
        env.IsDevelopment() ? Ok(await admin.GetLeadsAsync(ct)) : NotFound();

    [HttpGet("activities")]
    public async Task<IActionResult> Activities(CancellationToken ct) =>
        env.IsDevelopment() ? Ok(await admin.GetActivitiesAsync(ct)) : NotFound();

    [HttpGet("visits")]
    public async Task<IActionResult> Visits(CancellationToken ct) =>
        env.IsDevelopment() ? Ok(await admin.GetVisitsAsync(ct)) : NotFound();

    [HttpGet("configurations")]
    public async Task<IActionResult> Configurations(CancellationToken ct) =>
        env.IsDevelopment() ? Ok(await admin.GetConfigurationsAsync(ct)) : NotFound();


    // ---------- Update ----------
    [HttpPut("leads/{id:int}")]
    public Task<IActionResult> UpdateLead(int id, UpdateLeadRequest body, CancellationToken ct) =>
        Run(() => admin.UpdateLeadAsync(id, body, ct));

    [HttpPut("activities/{id:int}")]
    public Task<IActionResult> UpdateActivity(int id, UpdateActivityRequest body, CancellationToken ct) =>
        Run(() => admin.UpdateActivityAsync(id, body, ct));

    [HttpPut("visits/{id:int}")]
    public Task<IActionResult> UpdateVisit(int id, UpdateVisitRequest body, CancellationToken ct) =>
        Run(() => admin.UpdateVisitAsync(id, body, ct));

    // ---------- Delete ----------
    [HttpDelete("leads/{id:int}")]
    public Task<IActionResult> DeleteLead(int id, CancellationToken ct) =>
        Run(() => admin.DeleteLeadAsync(id, ct));

    [HttpDelete("activities/{id:int}")]
    public Task<IActionResult> DeleteActivity(int id, CancellationToken ct) =>
        Run(() => admin.DeleteActivityAsync(id, ct));

    [HttpDelete("visits/{id:int}")]
    public Task<IActionResult> DeleteVisit(int id, CancellationToken ct) =>
        Run(() => admin.DeleteVisitAsync(id, ct));

    // Shared: Development only, 204 on success, 404 if the row doesn't exist, 400 on bad values
    private async Task<IActionResult> Run(Func<Task<bool>> action)
    {
        if (!env.IsDevelopment()) return NotFound();
        try
        {
            return await action() ? NoContent() : NotFound();
        }
        catch (ValidationFailedException ex)
        {
            ModelState.AddModelError(ex.Field, ex.Message);
            return ValidationProblem(ModelState);
        }
    }
}