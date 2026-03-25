namespace SaaS.API.Options;

public class TokenVersionValidationOptions
{
    public const string SectionName = "Auth:TokenVersionValidation";

    public bool FailOpenOnStateUnavailability { get; set; }
}
