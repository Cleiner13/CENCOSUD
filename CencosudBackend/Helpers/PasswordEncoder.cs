using System.Security.Cryptography;
using System.Text;

namespace CencosudBackend.Helpers
{
    public static class PasswordEncoder
    {
        // Debe generar EXACTAMENTE el mismo hash que tu función Python encode_password
        public static string EncodePassword(string password)
        {
            var cadena = "ALMPES" + password;

            // Python usa 'utf-16le' -> en .NET es Encoding.Unicode
            byte[] bytes = Encoding.Unicode.GetBytes(cadena);

            using var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}
