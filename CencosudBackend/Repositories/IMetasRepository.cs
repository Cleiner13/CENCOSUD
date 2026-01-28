using CencosudBackend.DTOs;

namespace CencosudBackend.Repositories
{
    public interface IMetasRepository
    {
        Task<List<string>> ListarAsesoresPorSupervisorAsync(string supervisor, string? uunn);
        Task<List<string>> ListarSupervisoresPorUunnAsync(string uunn);

        Task<List<MetaAsesorDto>> ListarMetasAsesoresAsync(string uunn, int periodo, string supervisor);
        Task<List<MetaSupervisorDto>> ListarMetasSupervisoresAsync(string uunn, int periodo);

        Task UpsertMetaAsesorAsync(MetaAsesorDto dto);
        Task UpsertMetaSupervisorAsync(MetaSupervisorDto dto);
    }
}
