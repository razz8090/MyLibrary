using System;
namespace mylibrary.DTOs.ResponseDTOs;

public class CommonResponse<T>
{
    public T Data { get; set; }
    public ErrorCode Code { get; set; }
    public ErrorResponse ErrorResponse { get; set; }
}


public class ErrorResponse
{
    public ErrorResponse(ErrorCode code, string message)
    {
        this.Code = code;
        this.Message = message;
    }
    public ErrorCode Code { get; set; }
    public string Message { get; set; }
}


public enum ErrorCode
{
    Success = 200,
    NotFound = 104,
    UnAuthorized = 401,
    AlreadyExist = 101,
    UserAcountLocked = 102,
    LoginAttemptExeeded = 103

}

