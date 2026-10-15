using PragathiApi.Application.Dtos;

namespace PragathiApi.Application.Interfaces;

public interface ISubmissionService
{
    Task<SubmitResponse> SubmitAsync(SubmitRequest request, CancellationToken ct);
}