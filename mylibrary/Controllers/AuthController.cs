using Microsoft.AspNetCore.Mvc;
using mylibrary.DTOs;
using mylibrary.DTOs.LoginDtos;
using mylibrary.Helpers;
using mylibrary.IServices;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mylibrary.Controllers;

public class AuthController : ControllerBase
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IAuthService _authService;
    public AuthController(JwtTokenHelper jwtTokenHelper, IAuthService authService)
    {
        _jwtTokenHelper = jwtTokenHelper;
        _authService = authService;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {

        if (ModelState.IsValid)
        {
            return Ok(await _authService.Login(request, string.Empty));
        }

        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("setpassword")]
    public async Task<IActionResult> SetPassword([FromBody] ChangePasswordRequestDto request)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _authService.SetPassword(request, string.Empty));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword([FromBody] LoginRequestDto request)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _authService.ResetPassword(request, string.Empty));
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        return Ok();
    }

    [HttpPost("generatecode")]
    public async Task<IActionResult> GetQrCode()
    {
        return Ok(await _authService.GenerateQRCodeWithText());
    }
}

