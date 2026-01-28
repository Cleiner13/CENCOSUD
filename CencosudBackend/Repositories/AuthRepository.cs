using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CencosudBackend.Models;
using Microsoft.Extensions.Configuration;

namespace CencosudBackend.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CencosudDb")
                               ?? throw new InvalidOperationException("Connection string 'CencosudDb' not found.");
        }

        public async Task<LoginResult> ValidarLoginAsync(string usuario, string passwordHash)
        {
            var result = new LoginResult();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_VALIDAR_LOGIN_CENCOSUD", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Usuario", usuario);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.CodigoResultado = reader.GetInt32(reader.GetOrdinal("CodigoResultado"));
                result.Mensaje = reader["Mensaje"]?.ToString() ?? string.Empty;

                if (result.CodigoResultado == 0)
                {
                    result.IdUsuario = reader["IdUsuario"] as int?;
                    result.Usuario = reader["Usuario"]?.ToString();
                    result.Estado = reader["Estado"]?.ToString();
                    result.Cargo = reader["CARGO"]?.ToString();
                    result.Supervisor = reader["SUPERVISOR"]?.ToString();
                    result.UUNN = reader["UUNN"]?.ToString();
                    result.RolApp = reader["RolApp"]?.ToString();
                }
            }
            else
            {
                result.CodigoResultado = 1;
                result.Mensaje = "No se obtuvo resultado del procedimiento almacenado.";
            }

            return result;
        }
    }
}
