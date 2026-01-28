using System.Threading.Tasks;
using CencosudBackend.Models;

namespace CencosudBackend.Repositories
{
    public interface IAuthRepository
    {
        Task<LoginResult> ValidarLoginAsync(string usuario, string passwordHash);
    }
}
