using System;
using System.Threading.Tasks;
using mylibrary.DTOs;
using mylibrary.DTOs.LoginDtos;
using mylibrary.DTOs.ResponseDTOs;

namespace mylibrary.IServices;

public interface IAuthService
{
    Task<CommonResponse<LogInResponseDTOs>> Login(LoginRequestDto logInRequest, string userId);
    Task<CommonResponse<LogInResponseDTOs>> ResetPassword(LoginRequestDto forgetRequest, string userId);
    Task<CommonResponse<LogInResponseDTOs>> SetPassword(ChangePasswordRequestDto changePasswordRequest, string userId);
    Task<CommonResponse<LoginRequestDto>> Logout(string token, string userId);
    public Task<string> GenerateQRCodeWithText();

}

