using Microsoft.AspNetCore.Mvc;
using mylibrary.DTOs.UserDtos;
using mylibrary.IServices;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mylibrary.Controllers;

public class UserController : Controller
{
    public readonly IUserService _user;
    public UserController(IUserService user)
    {
        _user = user;
    }

    [HttpPost("registeruser")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRequestDto userRequest)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _user.RegisterUser(userRequest, null));

        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("updateuser")]
    public async Task<IActionResult> UpdateUser([FromBody] UserRequestDto updateUser)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _user.UpdateUser(updateUser, null));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("Users")]
    public async Task<IActionResult> GetUserList()
    {
        return Ok(await _user.GetUsers());
    }

    [HttpPost("userdetails")]
    public async Task<IActionResult> GetUserDetails([FromBody] UserRequest userId)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _user.GetUserById(userId));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("changestatus")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatus changeStatus)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _user.ChangesStatusUser(changeStatus, null));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    
    [HttpPost("updateaddress")]
    public async Task<IActionResult> UpdateUserAddress(AddressRequest addressRequest)
    {
        if (ModelState.IsValid)
        {
            return Ok( await _user.AddupdateAddress(addressRequest, null));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("updaterole")]
    public async Task<IActionResult> UpdateUserRole(UserRole userRole)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _user.AddUserRole(userRole, null));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("updatepermission")]
    public async Task<IActionResult> UpdateUserPermission(UserPermission userPermission)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _user.AddUserPermission(userPermission, null));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }
}

