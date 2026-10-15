using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PragathiApi.Application.Common;
using PragathiApi.Application.Dtos;
using PragathiApi.Application.Interfaces;

namespace PragathiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("submit")]
public class LeadsController(ISubmissionService submissions) : ControllerBase
{
    /// <summary>
    /// Every form on the website sends its data here (enquiry, site visit, floor plan download, ...).
    /// </summary>
    /// <remarks>
    /// The same person doing the same thing again within 5 minutes is not saved twice:
    /// the response then has <c>alreadyReceived = true</c>.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(SubmitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SubmitResponse>> Submit(SubmitRequest request, CancellationToken ct)
    {
        try
        {
            return Ok(await submissions.SubmitAsync(request, ct));
        }
        catch (ValidationFailedException ex)
        {
            ModelState.AddModelError(ex.Field, ex.Message);
            return ValidationProblem(ModelState);
        }
    }
}