using System.Text.Json.Serialization;

namespace UrbanCommunity.Application.Common.Models;

public class Result
{
    [JsonConstructor]
    public Result(bool isSuccess, string[]? errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? [];
    }

    public bool IsSuccess { get; }
    public string[] Errors { get; }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors.ToArray());
    }
}

public class Result<T> : Result
{
    [JsonConstructor]
    public Result(bool isSuccess, T? data, string[]? errors)
        : base(isSuccess, errors)
    {
        Data = data;
    }

    public T? Data { get; }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, Array.Empty<string>());
    }

    public static new Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default, errors.ToArray());
    }
}
