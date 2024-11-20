using mylibrary.DTOs.ResponseDTOs;
using mylibrary.Models.User;
using mylibrary.Repositories.Interfaces;

namespace mylibrary.Services;

public class UserService
{
	private readonly IUserRepository _userRepo;
	public UserService( IUserRepository userRepository)
	{
		_userRepo = userRepository;
	}

	public CommonResponse<User> RegisterUser()
	{
		CommonResponse<User> response = new();

		return response;

    }
}

