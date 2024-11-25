using mylibrary.Models.User;
using mylibrary.Utility;

namespace mylibrary.DTOs.UserDtos;

public class    UserRequestDto
{
    public string ID { get; set; }
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

public class UserRequest
{
    public string ID { get; set; }
}

public class AddressRequest
{
    public string ID { get; set; }
    public Address Address { get; set; }
}

public class UserRole
{
    public string ID { get; set; }
    public List<string> Roles { get; set; }
}

public class UserPermission
{
    public string ID { get; set; }
    public List<string> Permssion { get; set; }
}