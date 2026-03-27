namespace SaaS.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static Result<T> Success(T data, string? message = null) =>
        new() { Succeeded = true, Data = data, Message = message };

    public static Result<T> Failure(IEnumerable<string> errors, string? message = null) =>
        new() { Succeeded = false, Errors = errors, Message = message };

    public static Result<T> Failure(string error, string? message = null) =>
        new() { Succeeded = false, Errors = new[] { error }, Message = message };
}

public class Result
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static Result Success(string? message = null) =>
        new() { Succeeded = true, Message = message };

    public static Result Failure(IEnumerable<string> errors, string? message = null) =>
        new() { Succeeded = false, Errors = errors, Message = message };

    public static Result Failure(string error, string? message = null) =>
        new() { Succeeded = false, Errors = new[] { error }, Message = message };
}

