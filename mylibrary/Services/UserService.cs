using MongoDB.Driver;
using mylibrary.DTOs.ResponseDTOs;
using mylibrary.DTOs.UserDtos;
using mylibrary.IServices;
using mylibrary.Models;
using mylibrary.Models.CommonModel;
using mylibrary.Models.User;
using mylibrary.Repositories;
using mylibrary.Repositories.Interfaces;
using mylibrary.Utility;

namespace mylibrary.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly ICommunicationManager _communication;

    public UserService(IUserRepository userRepository, IEmailTemplateRepository emailTemplateRepository, ICommunicationManager communication)
    {
        _userRepo = userRepository;
        _emailTemplateRepository = emailTemplateRepository;
        _communication = communication;
    }

    #region User Registration
    public async Task<CommonResponse<UserResponseDto>> RegisterUser(UserRequestDto userRequest, string userId)
    {
        CommonResponse<UserResponseDto> response = new();
        response.Code = ErrorCode.Success;

        try
        {
            response = CheckDuplicate(userRequest).Result;
            if (response.ErrorResponse != null)
            {
                return response;
            }

            if (response.Data != null && response.Data.RegisterStep == RegisterStep.Address)
            {
                userRequest.ID = response.Data.ID;
                response = UpdateUser(userRequest, userId).Result;
                return response;
            }
            User newUser = new()
            {
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                EmailId = userRequest.EmailId,
                MobileNumber = userRequest.MobileNumber,
                Status = Status.Active,
                CreatedOn = DateTime.Now,
                CreatedBy = userId,
                RegisterStep = RegisterStep.Address
            };

            await _userRepo.AddUserAsync(newUser);

            response.Data = new()
            {
                ID = newUser.ID,
                RegisterStep = newUser.RegisterStep
            };

            //sending password
            SendOTP(newUser);
        }
        catch (Exception ex)
        {

        }


        return response;
    }

    public async Task<CommonResponse<UserResponseDto>> UpdateUser(UserRequestDto updateRequest, string userId)
    {
        CommonResponse<UserResponseDto> response = new();
        response.Code = ErrorCode.Success;
        try
        {
            response = CheckDuplicate(updateRequest).Result;
            if (response.ErrorResponse != null)
            {
                return response;
            }

            var filteruser = Builders<User>.Filter.Eq(x => x.ID, updateRequest.ID);
            User userdb = _userRepo.GetByIdAsync(filteruser).Result;

            if (userdb == null)
            {
                response.ErrorResponse = new(ErrorCode.NotFound, "User not found.");
                return response;

            }

            var updateUser = Builders<User>.Update.Set(x => x.FirstName, updateRequest.FirstName);
            updateUser = updateUser.Set(x => x.LastName, updateRequest.LastName);
            updateUser = updateUser.Set(x => x.MobileNumber, updateRequest.MobileNumber);
            updateUser = updateUser.Set(x => x.UpdatedOn, DateTime.Now);
            updateUser = updateUser.Set(x => x.UpdatedBy, userId);

            await _userRepo.UpdateUserAsync(updateUser, filteruser);

        }
        catch (Exception ex)
        {

        }
        return response;
    }

    public async Task<CommonResponse<List<UsersDto>>> GetUsers()
    {
        CommonResponse<List<UsersDto>> response = new();
        response.Code = ErrorCode.Success;

        response.Data = _userRepo.GetAllUserAsync().Result.Select(x => new UsersDto()
        {
            ID = x.ID,
            FirstName = x.FirstName,
            LastName = x.LastName,
            EmailId = x.EmailId,
            MobileNumber = x.MobileNumber,
            LastLogin = x.LastLogin,
            TnC = x.TnC,
            RegisterStep = x.RegisterStep

        }).ToList();

        return response;
    }
    public async Task<CommonResponse<UserDetails>> GetUserById(UserRequest userId)
    {
        CommonResponse<UserDetails> response = new();
        response.Code = ErrorCode.Success;

        User user = _userRepo.GetByIdAsync(Builders<User>.Filter.Eq(x => x.ID, userId.ID)).Result;

        response.Data = new()
        {
            ID = user.ID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailId = user.EmailId,
            MobileNumber = user.MobileNumber,
            Roles = user.Roles,
            Permissions = user.Permissions,
            Address = user.Address,
            LastLogin = user.LastLogin,
            TnC = user.TnC
        };

        return response;


    }

    public async Task<CommonResponse<UserResponseDto>> ChangesStatusUser(ChangeStatus changeStatus, string userId)
    {
        CommonResponse<UserResponseDto> response = new();
        response.Code = ErrorCode.Success;

        var filterUser = Builders<User>.Filter.Eq(x => x.ID, changeStatus.ID);
        var updateUser = Builders<User>.Update.Set(x => x.Status, changeStatus.Status);
        updateUser = updateUser.Set(x => x.UpdatedBy, userId);
        updateUser = updateUser.Set(x => x.UpdatedOn, DateTime.Now);

        await _userRepo.UpdateUserAsync(updateUser, filterUser);
        return response;


    }

    public async Task<CommonResponse<UserResponseDto>> CheckDuplicate(UserRequestDto userRequest)
    {
        CommonResponse<UserResponseDto> response = new();

        User user = _userRepo.GetAllUserAsync().Result.Where(x => x.EmailId.Equals(userRequest.EmailId) && x.Status != Status.Delete).FirstOrDefault();

        if (user != null)
        {
            if (user.RegisterStep == RegisterStep.Completed)
            {
                response.ErrorResponse = new(ErrorCode.AlreadyExist, "User Already Exist.");
                return response;
            }
            else if (user.RegisterStep == RegisterStep.Address)
            {
                response.Data = new()
                {
                    ID = user.ID,
                    RegisterStep = RegisterStep.Address
                };

                return response;
            }
            else if (user.RegisterStep == RegisterStep.Permssion)
            {
                response.Data = new()
                {
                    ID = user.ID,
                    RegisterStep = RegisterStep.Permssion
                };

                return response;
            }
        }

        return response;
    }

    #endregion

    #region Address
    public async Task<CommonResponse<UserResponseDto>> AddupdateAddress(AddressRequest address, string userId)
    {
        CommonResponse<UserResponseDto> response = new();
        response.Code = ErrorCode.Success;

        var filterUser = Builders<User>.Filter.Eq(x => x.ID, address.ID);
        var updateUser = Builders<User>.Update.Set(x => x.Address, address.Address);
        updateUser = updateUser.Set(x => x.UpdatedBy, userId);
        updateUser = updateUser.Set(x => x.UpdatedOn, DateTime.Now);
        updateUser = updateUser.Set(x => x.RegisterStep, RegisterStep.Permssion);

        await _userRepo.UpdateUserAsync(updateUser, filterUser);

        return response;
    }
    #endregion

    #region Add / Update User Permission

    public async Task<CommonResponse<UserResponseDto>> AddUserRole(UserRole userRole, string userId)
    {
        CommonResponse<UserResponseDto> response = new();
        response.Code = ErrorCode.Success;

        var filterUser = Builders<User>.Filter.Eq(x => x.ID, userRole.ID);
        var updateUser = Builders<User>.Update.Set(x => x.Roles, userRole.Roles);
        updateUser = updateUser.Set(x => x.UpdatedBy, userId);
        updateUser = updateUser.Set(x => x.UpdatedOn, DateTime.Now);
        updateUser = updateUser.Set(x => x.RegisterStep, RegisterStep.Permssion);

        await _userRepo.UpdateUserAsync(updateUser, filterUser);

        return response;
    }


    public async Task<CommonResponse<UserResponseDto>> AddUserPermission(UserPermission userPermission, string userId)
    {
        CommonResponse<UserResponseDto> response = new();
        response.Code = ErrorCode.Success;

        var filterUser = Builders<User>.Filter.Eq(x => x.ID, userPermission.ID);
        var updateUser = Builders<User>.Update.Set(x => x.Permissions, userPermission.Permssion);
        updateUser = updateUser.Set(x => x.UpdatedBy, userId);
        updateUser = updateUser.Set(x => x.UpdatedOn, DateTime.Now);
        updateUser = updateUser.Set(x => x.RegisterStep, RegisterStep.Completed);
        updateUser = updateUser.Set(x => x.Status, Status.Active);

        await _userRepo.UpdateUserAsync(updateUser, filterUser);

        return response;
    }
    #endregion

    public async void SendOTP(User user)
    {
        var filterTemplate = Builders<EmailTemplate>.Filter.Eq(x => x.Key, "OTPTemplate");
        EmailTemplate emailTemplate = await _emailTemplateRepository.GetByIdAsync(filterTemplate);

        var filterUser = Builders<User>.Filter.Eq(x => x.ID, user.ID);

        string otp = UtilityManager.GeneratePassword(8);

        PasswordDetails passwordDetails = new()
        {
            Password = UtilityManager.ComputeSHA512Hash(otp),
            CreatedOn = DateTime.Now,
            IsAutoGenerated = true,
            ExpiryDate = DateTime.Now.AddMinutes(10),

        };
        var updatePassword = Builders<User>.Update.Set(x => x.Password, passwordDetails);

        await _userRepo.UpdateUserAsync(updatePassword, filterUser);

        string template = emailTemplate.Template.Replace("{{Name}}", user.FirstName).Replace("{{otp}}", otp);

        _communication.SendEmailAsync(new()
        {
            ToEmail = user.EmailId,
            Subject = "You temprary password.",
            Body = template
        });
    }
 }

