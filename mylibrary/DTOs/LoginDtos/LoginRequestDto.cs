using System;
using mylibrary.Utility;

namespace mylibrary.DTOs;

public class LoginRequestDto
{
    private string emailId;
    public string EmailId
    {
        get { return this.emailId; }
        set { this.emailId = AESCryptography.Encrypt(value?.Trim().ToLower()); }
    }

    private string password;
    public string Password
    {
        get { return this.emailId; }
        set { this.emailId = AESCryptography.Encrypt(value?.Trim().ToLower()); }

    }
}