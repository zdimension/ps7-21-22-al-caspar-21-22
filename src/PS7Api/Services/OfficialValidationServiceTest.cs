using PS7Api.Models;

namespace PS7Api.Services;

public class OfficialValidationServiceTest
{
	ValidationResult ValidateDocument(Document document)
	{
		if (document.Verified)
			return new ValidationSuccess();
		
		if (document.Id % 2 == 0)
		{
			return new ValidationSuccess();
		}

		return new ValidationFailure(new string[] { "this is an error" });
	}
}