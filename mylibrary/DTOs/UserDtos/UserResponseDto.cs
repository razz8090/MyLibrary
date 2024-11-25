using System;
using mylibrary.Models.CommonModel;
using mylibrary.Models.User;

namespace mylibrary.DTOs.UserDtos;

public class UserResponseDto
{
	public string ID { get; set; }
	public RegisterStep RegisterStep { get; set; }
}

public class UsersDto
{
	public string ID { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string EmailId { get; set; }
	public string MobileNumber { get; set; }
	public TnC TnC { get; set; }
    public Nullable<DateTime> LastLogin { get; set; }

    public RegisterStep RegisterStep { get; set; }
	public Status Status { get; set; }
}


public class ChangeStatus
{
	public string ID { get; set; }
	public Status Status { get; set; }
	public string Remark { get; set; }
}


public class UserDetails
{
    public string ID { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string EmailId { get; set; }

    public string MobileNumber { get; set; }

    public List<string> Roles { get; set; } = new List<string>(); // e.g., Admin, Editor, User

    public List<string> Permissions { get; set; } = new List<string>(); // e.g., "Read", "Write", "Delete"

    public Address Address { get; set; }

    public Nullable<DateTime> LastLogin { get; set; }

    public TnC TnC { get; set; }
}