using MongoDB.Driver;
using mylibrary.DTOs;
using mylibrary.DTOs.LoginDtos;
using mylibrary.DTOs.ResponseDTOs;
using mylibrary.Helpers;
using mylibrary.IServices;
using mylibrary.Models;
using mylibrary.Models.CommonModel;
using mylibrary.Models.User;
using mylibrary.Repositories.Interfaces;
using mylibrary.Utility;

using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;

using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using System.Buffers.Text;

namespace mylibrary.Services;

public class AuthService: IAuthService
{
    private readonly IUserRepository _user;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private ICommunicationManager _communicationManager;
    public AuthService(IUserRepository userService, JwtTokenHelper jwtTokenHelper, ICommunicationManager communicationManager)
    {
        _user = userService;
        _jwtTokenHelper = jwtTokenHelper;
        _communicationManager = communicationManager;
    }

    public async Task<CommonResponse<LogInResponseDTOs>> Login(LoginRequestDto logInRequest, string userId)
    {
        CommonResponse<LogInResponseDTOs> response = new();

        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq(entry => entry.EmailId, logInRequest.EmailId) & Builders<User>.Filter.Eq(entry => entry.Status, Status.Active);
        User user = await _user.GetByIdAsync(filterUser);
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

                response.Data.Token = _jwtTokenHelper.GenerateToken(user.EmailId);

                updateUser = updateUser.Set(entry => entry.LastLogin, DateTime.Now);

                response.Data.LastLogin = user.LastLogin;
            }
            else
            {
                user.Password.LoginFailedAttemotCount += 1;

                Console.WriteLine("Password not matched.");
                return response;
            }

            updateUser = updateUser.Set(entry => entry.Password, user.Password);
            await _user.UpdateUserAsync(updateUser, filterUser);
        }


        response.Code = ErrorCode.Success;
        return response;
    }

    public async Task<CommonResponse<LogInResponseDTOs>> ResetPassword(LoginRequestDto forgetRequest, string userId)
    {
        CommonResponse<LogInResponseDTOs> response = new();

        FilterDefinition<User> filterDefinition = Builders<User>.Filter.Eq(entry => entry.EmailId, forgetRequest.EmailId) & Builders<User>.Filter.Eq(entry => entry.Status, Status.Active);
        User user = await _user.GetByIdAsync(filterDefinition);
        if (user != null)
        {
            if (user.Password.LoginBlockedTime > DateTime.Now)
            {
                Console.WriteLine("User's account is locked.");
                response.ErrorResponse = new(ErrorCode.UserAcountLocked, "User account is locked please try after sometime's");
                return response;
            }

            string newPassowrd = UtilityManager.ComputeSHA512Hash(UtilityManager.GeneratePassword(6));
            string salt = UtilityManager.ComputeSHA512Hash(UtilityManager.GeneratePassword(4));

            if (user.OldPasswords == null && user.Password != null)
            {
                user.OldPasswords = new() { user.Password };
            }
            else if (user.OldPasswords != null)
            {
                user.OldPasswords.Add(user.Password);
            }

            user.Password = new()
            {
                Password = AESCryptography.Encrypt(newPassowrd) + salt,
                Salt = salt,
                ExpiryDate = DateTime.Now.AddMinutes(30),
                IsAutoGenerated = true
            };


            
            MailRequest mailRequest = new()
            {
                ToEmail = AESCryptography.Decrypt(user.EmailId),
                Subject = "New Password Generated",
                Body = $"<p><h1>{newPassowrd}</h1></p>",
                Attachments = new()
            };

            _communicationManager.SendEmailAsync(mailRequest);
        }

        response.Code = ErrorCode.Success;
        return response;
    }

    public  async Task<CommonResponse<LogInResponseDTOs>> SetPassword(ChangePasswordRequestDto changePasswordRequest, string userId)
    {
        CommonResponse<LogInResponseDTOs> response = new();

        FilterDefinition<User> filterUser = Builders<User>.Filter.Eq(entry => entry.ID, userId);

        User user = await _user.GetByIdAsync(filterUser);
        if (user == null)
        {
            response.ErrorResponse = new(ErrorCode.NotFound, "User not found.");
            return response;
        }

        if (user.TnC.IsAcepted == false && changePasswordRequest.IsTnCAcepted == false)
        {
            response.ErrorResponse = new(ErrorCode.TnCNotAcepted, "User not acepted the TnC.");
            return response;
        }
        if (!user.Password.Password.Equals(AESCryptography.Encrypt(changePasswordRequest.OldPassword) + user.Password.Salt))
        {
            response.ErrorResponse = new(ErrorCode.OldPasswordNotMatched, "Old passowrd not correct.");
            return response;
        }

        if (user.OldPasswords == null)
        {
            user.OldPasswords = new() { user.Password };
        }
        else
        {
            user.OldPasswords.Add(user.Password);
        }

        if (user.OldPasswords.Count > 5)
        {
            user.OldPasswords.RemoveAt(0);
        }
        string salt = UtilityManager.ComputeSHA512Hash(UtilityManager.GeneratePassword(8));
        user.Password.Salt = salt;
        user.Password.Password = changePasswordRequest.NewPassword + salt;
        if (changePasswordRequest.IsTnCAcepted)
        {
            user.TnC = new()
            {
               IsAcepted = true,
               AceptedOn = DateTime.Now
            };
        }

        user.UpdatedBy = userId;
        user.UpdatedOn = DateTime.Now;

        await _user.ReplaceUserAsync( filterUser, user);

        response.Code = ErrorCode.Success;
        return response;
    }

    public async Task<CommonResponse<LoginRequestDto>> Logout(string token,string userId)
    {
        CommonResponse<LoginRequestDto> response = new();

        var filterUser = Builders<User>.Filter.Eq(entry => entry.ID, userId);
        var updateUser = Builders<User>.Update.Set(entry => entry.RefreshToken.RefreshTokenExpiry, DateTime.Now.AddHours(1));
        await _user.UpdateUserAsync(updateUser, filterUser);

        BlockedToken newBlockedToken = new()
        {
            Token = token,
            CreatedOn = DateTime.Now
        };

        await _user.AddBlockToken(newBlockedToken);
        response.Code = ErrorCode.Success;
        return response;
    }

    public async Task<string> GenerateQRCodeWithText()
    {
        string qrContent = "MSRTC40";
        string text = "Download app and use this code 'MSRTC40' to log in";
        int qrSize = 300;
        int padding = 20;

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q))
        {
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(qrSize);
            using (Image<Rgba32> qrImage = Image.Load<Rgba32>(qrCodeBytes))
            {
                int imageWidth = qrImage.Width;
                int imageHeight = qrImage.Height + 50; // Extra space for text

                using (Image<Rgba32> finalImage = new Image<Rgba32>(imageWidth, imageHeight))
                {
                    finalImage.Mutate(ctx => ctx.BackgroundColor(Color.White));
                    finalImage.Mutate(ctx => ctx.DrawImage(qrImage, new Point(0, 0), 1f));

                    FontFamily fontFamily = SystemFonts.Get("Arial");
                    Font font = new Font(fontFamily, 20, FontStyle.Regular);

                    TextOptions options = new TextOptions(font)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        WrappingLength = imageWidth - (2 * padding)
                    };

                    var textSize = TextMeasurer.MeasureSize(text, options);
                    int textX = (imageWidth - (int)textSize.Width) / 2;
                    int textY = qrSize + 10;

                    finalImage.Mutate(ctx => ctx.DrawText(new DrawingOptions(), text, font, Color.Black, new PointF(0, qrSize + 10)));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        finalImage.SaveAsPng(ms);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }
    }
}

