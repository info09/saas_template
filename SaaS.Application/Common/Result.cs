namespace SaaS.Application.Common;

public class Result
{
    public bool Succeeded { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static Result Success() => new Result { Succeeded = true };
    public static Result Failure(params string[] errors) => new Result { Succeeded = false, Errors = errors };
}
