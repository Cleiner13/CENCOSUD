using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Linq;
using CencosudBackend.DTOs;
using Microsoft.Extensions.Configuration;

namespace CencosudBackend.Repositories
{
    public class MetasRepository : IMetasRepository
    {
        private readonly string _connectionString;

        public MetasRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CencosudDb")
                               ?? throw new InvalidOperationException("Connection string 'CencosudDb' not found.");
        }

        public async Task<List<string>> ListarAsesoresPorSupervisorAsync(string supervisor, string? uunn)
        {
            var list = new List<string>();

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TC_LISTAR_ASESORES_POR_SUPERVISOR", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Supervisor", supervisor);
            cmd.Parameters.AddWithValue("@UUNN", (object?)uunn ?? DBNull.Value);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(dr["ASESOR"]?.ToString() ?? string.Empty);

            return list.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
        }

        public async Task<List<string>> ListarSupervisoresPorUunnAsync(string uunn)
        {
            var list = new List<string>();

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TC_LISTAR_SUPERVISORES_POR_UUNN", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UUNN", uunn);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
                list.Add(dr["SUPERVISOR"]?.ToString() ?? string.Empty);

            return list.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
        }

        public async Task<List<MetaAsesorDto>> ListarMetasAsesoresAsync(string uunn, int periodo, string supervisor)
        {
            var list = new List<MetaAsesorDto>();

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TC_METAS_ASESOR_LISTAR", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UUNN", uunn);
            cmd.Parameters.AddWithValue("@Periodo", periodo);
            cmd.Parameters.AddWithValue("@Supervisor", supervisor);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new MetaAsesorDto
                {
                    Uunn = uunn,
                    Periodo = periodo,
                    Supervisor = supervisor,
                    Asesor = dr["ASESOR"]?.ToString() ?? string.Empty,
                    MetaWapeos = dr["META_WAPEOS"] == DBNull.Value ? null : Convert.ToInt32(dr["META_WAPEOS"]),
                    MetaVentas = dr["META_VENTAS"] == DBNull.Value ? null : Convert.ToInt32(dr["META_VENTAS"]),
                    UsuarioAccion = string.Empty
                });
            }

            return list;
        }

        public async Task<List<MetaSupervisorDto>> ListarMetasSupervisoresAsync(string uunn, int periodo)
        {
            var list = new List<MetaSupervisorDto>();

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TC_METAS_SUPERVISOR_LISTAR", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UUNN", uunn);
            cmd.Parameters.AddWithValue("@Periodo", periodo);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new MetaSupervisorDto
                {
                    Uunn = uunn,
                    Periodo = periodo,
                    Supervisor = dr["SUPERVISOR"]?.ToString() ?? string.Empty,
                    MetaWapeos = dr["META_WAPEOS"] == DBNull.Value ? null : Convert.ToInt32(dr["META_WAPEOS"]),
                    MetaVentas = dr["META_VENTAS"] == DBNull.Value ? null : Convert.ToInt32(dr["META_VENTAS"]),
                    UsuarioAccion = string.Empty
                });
            }

            return list;
        }

        public async Task UpsertMetaAsesorAsync(MetaAsesorDto dto)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TC_METAS_ASESOR_UPSERT", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UUNN", dto.Uunn);
            cmd.Parameters.AddWithValue("@Periodo", dto.Periodo);
            cmd.Parameters.AddWithValue("@Supervisor", dto.Supervisor);
            cmd.Parameters.AddWithValue("@Asesor", dto.Asesor);
            cmd.Parameters.AddWithValue("@MetaWapeos", (object?)dto.MetaWapeos ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MetaVentas", (object?)dto.MetaVentas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UsuarioAccion", dto.UsuarioAccion);

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpsertMetaSupervisorAsync(MetaSupervisorDto dto)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TC_METAS_SUPERVISOR_UPSERT", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UUNN", dto.Uunn);
            cmd.Parameters.AddWithValue("@Periodo", dto.Periodo);
            cmd.Parameters.AddWithValue("@Supervisor", dto.Supervisor);
            cmd.Parameters.AddWithValue("@MetaWapeos", (object?)dto.MetaWapeos ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MetaVentas", (object?)dto.MetaVentas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UsuarioAccion", dto.UsuarioAccion);

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
