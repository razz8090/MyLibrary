using MongoDB.Driver;
using mylibrary.DTOs;
using mylibrary.DTOs.LoginDtos;
using mylibrary.DTOs.ResponseDTOs;
using mylibrary.Helpers;
using mylibrary.Models.CommonModel;
using mylibrary.Models.User;
using mylibrary.Repositories.Interfaces;
using mylibrary.Utility;

namespace mylibrary.Services;

public class AuthService
{
    private readonly IUserRepository _user;
    private readonly JwtTokenHelper _jwtTokenHelper;
    public AuthService(IUserRepository userService, JwtTokenHelper jwtTokenHelper)
    {
        _user = userService;
        _jwtTokenHelper = jwtTokenHelper;
    }
    public async Task<CommonResponse<LogInResponseDTOs>> Login(LoginRequestDto logInRequest, string userId)
    {
        CommonResponse<LogInResponseDTOs> response = new();

        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq(entry => entry.EmailId, logInRequest.EmailId) & Builders<User>.Filter.Eq(entry => entry.Status, Status.Active);
        User user =  await _user.GetByIdAsync(filterUser);
        if (user == null)
        {
            Console.WriteLine("User not found.");
            return response;
        }
        else
        {
            var updateUser = Builders<User>.Update.Set(entry => entry.UpdatedBy, userId);
            updateUser = updateUser.Set(entry => entry.UpdatedOn, DateTime.Now);

            if (user.Password.LoginBlockedTime > DateTime.Now)
            {
                Console.WriteLine("User's account is locked.");
                response.ErrorResponse = new(ErrorCode.UserAcountLocked, "User account is locked please try after sometime's");
                return response;
            }
            else if (user.Password.LoginFailedAttemotCount > 3)
            {
                user.Password.LoginFailedAttemotCount += 1;
                user.Password.LoginBlockedTime = DateTime.Now.AddMinutes(15);

                Console.WriteLine("Login attemot exeeded and your acount is going to locked for 15 min.");
                response.ErrorResponse = new(ErrorCode.LoginAttemptExeeded, "Login attemot exeeded and your acount is going to locked for 15 min.");
                //return response;
            }
            else if (user.Password.Password.Equals(logInRequest.Password + user.Password.Salt))
            {
                user.Password.OTPFailedCount = 0;
                user.Password.LoginFailedAttemotCount = 0;
                user.Password.LoginBlockedTime = null;

                response.Data.Token= _jwtTokenHelper.GenerateToken(user.EmailId); 

                updateUser = updateUser.Set(entry => entry.LastLogin, DateTime.Now);

                response.Data.LastLogin = user.LastLogin;
            }
            else
            {
                user.LoginDetails.LoginFailedAttemotCount += 1;
                if (user.Remarks == null)
                {
                    user.Remarks = new();
                }
                user.Remarks.Add(new()
                {
                    UpdatedBy = userId,
                    Reamrk = "Password is wrong.",
                    UodatedOn = DateTime.Now

                });
                _user.Replace(user);

                Console.WriteLine("Password not matched.");
                return response;
            }

        }


        response.Status = ErrorCode.Success;
        return response;
    }
    public CommonResponse<LogInResponse> ResetPassword(ForgetRequest forgetRequest, string userId)
    {
        CommonResponse<LogInResponse> response = new();

        FilterDefinition<User> filterDefinition = Builders<User>.Filter.Eq(entry => entry.EmailId, forgetRequest.Email) & Builders<User>.Filter.Eq(entry => entry.Status, StatusCode.Active);
        User user = _user.Get(filterDefinition);
        if (user != null)
        {
            if (user.LoginDetails.LoginBlockedTime > DateTime.Now)
            {
                Console.WriteLine("User's account is locked.");
                response.ErrorResponse = new(ErrorCode.UserAcountLocked, "User account is locked please try after sometime's");
                return response;
            }
            string newPassowrd = UtilityManager.GeneratePassword(6);
            string salt = AESCryptography.Encrypt(UtilityManager.GeneratePassword(4));
            user.PasswordDetails = new()
            {
                Password = AESCryptography.Encrypt(newPassowrd) + salt,
                Salt = salt,
                PasswordExpiryTime = DateTime.Now.AddMinutes(30),
                IsSystemGeneratedPassword = true
            };

            user.LoginDetails.LoginFailedAttemotCount = 0;
            user.LoginDetails.OTPFailedCount = 0;
            user.LoginDetails.LoginBlockedTime = null;


            if (user.Remarks == null)
            {
                user.Remarks = new();
            }
            user.Remarks.Add(new()
            {
                UpdatedBy = userId,
                Reamrk = "User requested for new password.",
                UodatedOn = DateTime.Now

            });
            _user.Replace(user);
            MailRequest mailRequest = new()
            {
                ToEmail = AESCryptography.Decrypt(user.EmailId),
                Subject = "New Password Generated",
                Body = $"<p><h1>{newPassowrd}</h1></p>",
                Attachments = new()
            };

            _communicationManager.SendEmailAsync(mailRequest).GetAwaiter().GetResult();
        }
        response.Status = ErrorCode.Success;
        return response;
    }

    public CommonResponse<LogInResponse> SetPassword(ChangePasswordRequest changePasswordRequest, string userId)
    {
        CommonResponse<LogInResponse> response = new();

        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq(entry => entry.ID, userId);

        User user = _user.Get(filterUser);
        if (user == null)
        {
            response.ErrorResponse = new(ErrorCode.NotFound, "User not found.");
            return response;
        }

        if (user.IsTnCAcepted == false && changePasswordRequest.IsTnCAcepted == false)
        {
            response.ErrorResponse = new(ErrorCode.TnCNotAcepted, "User not acepted the TnC.");
            return response;
        }
        if (!user.PasswordDetails.Password.Equals(AESCryptography.Encrypt(changePasswordRequest.OldPassword) + user.PasswordDetails.Salt))
        {
            response.ErrorResponse = new(ErrorCode.OldPasswordNotMatched, "Old passowrd not correct.");
            return response;
        }

        user.PasswordDetails.Password = AESCryptography.Encrypt(changePasswordRequest.NewPassword) + AESCryptography.Encrypt(UtilityManager.GeneratePassword(4));
        if (changePasswordRequest.IsTnCAcepted)
        {
            user.IsTnCAcepted = true;
            user.TnCAceptedTime = DateTime.Now;
        }

        user.CreatedBy = userId;

        _user.Replace(user);

        response.Status = ErrorCode.Success;
        return response;
    }
}

