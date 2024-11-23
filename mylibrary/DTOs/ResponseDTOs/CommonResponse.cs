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
	public ErrorCode Code { get; set; }
	public string Message { get; set; }
}


public enum ErrorCode
{
	Success= 200,
	NotFound = 101,
	UnAuthorized = 401
}