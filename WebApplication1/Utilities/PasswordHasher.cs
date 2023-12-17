namespace WebApplication1.Utilities
{
    using System;
    using System.Security.Cryptography;

    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            const int SaltSize = 16; // 128 bit
            const int KeySize = 32; // 256 bit
            const int Iterations = 10000; // Number of iterations

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256))
            {
                var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
                var salt = Convert.ToBase64String(algorithm.Salt);

                return $"{Iterations}.{salt}.{key}";
            }
        }

        public static bool VerifyPassword(string hashedPassword, string password)
        {
            var parts = hashedPassword.Split('.', 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. Should be formatted as '{iterations}.{salt}.{hash}'");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(key.Length);
                var verified = keyToCheck.SequenceEqual(key);

                return verified;
            }
        }
    }

}
