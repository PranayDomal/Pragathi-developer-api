using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PragathiApi.Application.Interfaces;
using PragathiApi.Application.Common;
using PragathiApi.Application.Dtos;

namespace PragathiApi.Controllers;

// Private door for other services (e.g. Admin). Needs the secret X-Service-Key header.
[ApiController]
[Route("internal")]
[ServiceKey]
public class InternalController(IAdminRepository admin) : ControllerBase
{
    // The login page calls this to check the ID and password: 204 = correct, 401 = wrong.
    [HttpGet("ping")]
    public IActionResult Ping() => NoContent();

    [HttpGet("leads")]
    public async Task<IActionResult> Leads(CancellationToken ct) =>
        Ok(await admin.GetLeadsAsync(ct));

    [HttpGet("activities")]
    public async Task<IActionResult> Activities(CancellationToken ct) =>
        Ok(await admin.GetActivitiesAsync(ct));

    [HttpGet("visits")]
    public async Task<IActionResult> Visits(CancellationToken ct) =>
        Ok(await admin.GetVisitsAsync(ct));

    [HttpGet("configurations")]
    public async Task<IActionResult> Configurations(CancellationToken ct) =>
        Ok(await admin.GetConfigurationsAsync(ct));


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

    // Shared: 204 on success, 404 if the row doesn't exist, 400 on bad values
    private async Task<IActionResult> Run(Func<Task<bool>> action)
    {
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