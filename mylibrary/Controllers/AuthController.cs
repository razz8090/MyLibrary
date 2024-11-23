using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mylibrary.DTOs;
using mylibrary.Helpers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace mylibrary.Controllers;

public class AuthController : Controller
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    public AuthController(JwtTokenHelper jwtTokenHelper)
    {
        _jwtTokenHelper = jwtTokenHelper;
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        // Replace with actual user validation logic
        if (request.UserName == "admin" && request.Password == "password")
        {
            var token = _jwtTokenHelper.GenerateToken(request.UserName);
            return Ok(new { Token = token });
        }

        return Unauthorized();
    }
}

