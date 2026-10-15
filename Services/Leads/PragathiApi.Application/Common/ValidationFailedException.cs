namespace PragathiApi.Application.Common;

/// <summary>Thrown when the data is understood but not acceptable (the API turns it into a 400 error).</summary>
public class ValidationFailedException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}