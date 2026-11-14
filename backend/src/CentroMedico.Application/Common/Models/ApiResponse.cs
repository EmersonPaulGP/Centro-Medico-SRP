namespace CentroMedico.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiErrorItem>? Errors { get; set; }

    public ApiResponse()
    {
    }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa.") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };

    public static ApiResponse<T> Fail(string message, List<ApiErrorItem>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors
        };

    public static ApiResponse<T> Fail(string message, string field, string errorMessage) =>
        new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = new List<ApiErrorItem> { new(field, errorMessage) }
        };
}

public record ApiErrorItem(string Field, string Message);
