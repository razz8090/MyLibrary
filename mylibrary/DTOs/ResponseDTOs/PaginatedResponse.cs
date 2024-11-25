using System;
namespace mylibrary.DTOs.ResponseDTOs;

public class PaginatedResponse<T>
{
	public List<T> Data { get; set; }
	public int PageNumber { get; set; }
	public int PageIndex { get; set; }
	public int TotalCount { get; set; }
}

