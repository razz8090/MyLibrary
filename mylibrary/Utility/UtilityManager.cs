using System;
using System.Security.Cryptography;

namespace mylibrary.Utility;

public class UtilityManager
{
    public static string GeneratePassword(int length)
    {
        using (var cryptRNG = RandomNumberGenerator.Create())
        {
            byte[] tokenBuffer = new byte[length];
            cryptRNG.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer).Remove(length).ToUpper();
        }
    }
}

