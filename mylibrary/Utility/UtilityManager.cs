using System;
using System.Security.Cryptography;
using System.Text;

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


    public static string GenerateOTP(int length)
    {
        if (length <= 0) throw new ArgumentException("Length must be greater than zero.");

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes);

            // Convert random bytes to a numeric OTP
            long otp = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
            string numericOtp = (otp % Math.Pow(10, length)).ToString($"D{length}");

            return numericOtp;
        }
    }

    public static string ComputeSHA512Hash(string input)
    {
        using (var sha512 = SHA512.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }
    }
}



