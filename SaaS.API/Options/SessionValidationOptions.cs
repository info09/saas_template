namespace SaaS.API.Options;

public class SessionValidationOptions
{
    public const string SectionName = "Auth:SessionValidation";

    public bool FailOpenOnStateUnavailability { get; set; }
}
