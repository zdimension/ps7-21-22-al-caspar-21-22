using System.Globalization;
using PS7Api.Models;

namespace PS7Api.Services;

public interface IOfficialValidationService
{
    ValidationResult ValidateDocument(Document document);

    public static IOfficialValidationService GetValidationService(RegionInfo region) =>
        new MockValidationService();
}

public abstract record ValidationResult;
public record ValidationSuccess : ValidationResult;
public record ValidationFailure(string[] Errors) : ValidationResult;

public class MockValidationService : IOfficialValidationService
{
    public ValidationResult ValidateDocument(Document document)
    {
        return new ValidationSuccess();
    }
}