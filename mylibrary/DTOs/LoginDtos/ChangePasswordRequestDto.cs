namespace mylibrary.DTOs.LoginDtos;

public class ChangePasswordRequestDto
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public bool IsTnCAcepted { get; set; }

}

