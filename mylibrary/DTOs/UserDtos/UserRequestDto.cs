using System;
using mylibrary.Utility;

namespace mylibrary.DTOs.UserDtos;

public class UserRequestDto
{
    private string firstName;
    public string FirstName
    {
        get { return this.firstName; }
        set { this.firstName = value.Trim(); }
    }

    private string lastName;
    public string LastName
    {
        get { return this.lastName; }
        set { this.lastName = value?.Trim(); }
    }

    private string emailId;
    public string EmailId
    {
        get { return this.emailId; }
        set { this.emailId = AESCryptography.Encrypt(value?.Trim().ToLower()); }
    }

    private string mobileNumber;
    public string MobileNumber
    {
        get { return this.mobileNumber; }
        set { this.mobileNumber = AESCryptography.Encrypt(value?.Trim()); }
    }
}

