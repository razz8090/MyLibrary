using System;
using MongoDB.Driver;
using mylibrary.Models;
using mylibrary.Repositories.Interfaces;

namespace mylibrary.Middlewares;

public class TokenBlocklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IUserRepository _userRepo;
    public TokenBlocklistMiddleware(RequestDelegate next, IUserRepository userRepository)
    {
        _next = next;
        _userRepo = userRepository;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token) && IsTokenBlocked(token))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Token is blocked.");
            return;
        }

        await _next(context);
    }

    private bool IsTokenBlocked(string token)
    {
        var filterUser = Builders<BlockedToken>.Filter.Eq(entry => entry.Token, token);
        BlockedToken blockedToken = _userRepo.GetBlockTokenByToken(filterUser).GetAwaiter().GetResult();
        if (blockedToken != null)
        {
            // Remove token if it has expired
            if (blockedToken.CreatedOn.AddMinutes(30) < DateTime.UtcNow)
            {
                var tokenFilterRemove = Builders<BlockedToken>.Filter.Lte(entry => entry.CreatedOn, DateTime.UtcNow.AddMinutes(30));

                _userRepo.GetBlockTokenByToken(tokenFilterRemove);
                return false;
            }

            return true; // Token is blocked
        }

        return false;
    }
}

