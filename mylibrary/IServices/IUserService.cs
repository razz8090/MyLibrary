using mylibrary.DTOs.ResponseDTOs;
using mylibrary.DTOs.UserDtos;

namespace mylibrary.IServices;

public interface IUserService
{
    public Task<CommonResponse<UserResponseDto>> RegisterUser(UserRequestDto userRequest, string userId);
    public Task<CommonResponse<UserResponseDto>> UpdateUser(UserRequestDto updateRequest, string userId);
    public Task<CommonResponse<List<UsersDto>>> GetUsers();
    public Task<CommonResponse<UserDetails>> GetUserById(UserRequest userId);
    public Task<CommonResponse<UserResponseDto>> ChangesStatusUser(ChangeStatus changeStatus, string userId);
    public Task<CommonResponse<UserResponseDto>> CheckDuplicate(UserRequestDto userRequest);

    public Task<CommonResponse<UserResponseDto>> AddupdateAddress(AddressRequest address, string userId);
    public Task<CommonResponse<UserResponseDto>> AddUserRole(UserRole userRole, string userId);
    public Task<CommonResponse<UserResponseDto>> AddUserPermission(UserPermission userPermission, string userId);
}

