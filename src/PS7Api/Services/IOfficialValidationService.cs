using PS7Api.Models;

namespace PS7Api.Services;

public interface IOfficialValidationService
{
    ValidationResult ValidateDocument(Document document);
}

public abstract record ValidationResult;
public record ValidationSuccess : ValidationResult;
public record ValidationFailure(string[] Errors) : ValidationResult;