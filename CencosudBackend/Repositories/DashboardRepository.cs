using System.Data;
using System.Data.SqlClient;
using CencosudBackend.DTOs;
using Microsoft.Extensions.Configuration;

namespace CencosudBackend.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CencosudDb")
                               ?? throw new InvalidOperationException("Connection string 'CencosudDb' not found.");
        }

        // ===== Helpers anti-NULL =====
        private static int GetInt(IDataRecord r, string col)
            => r[col] == DBNull.Value ? 0 : Convert.ToInt32(r[col]);

        private static int? GetNullableInt(IDataRecord r, string col)
            => r[col] == DBNull.Value ? (int?)null : Convert.ToInt32(r[col]);

        private static string GetString(IDataRecord r, string col)
            => r[col] == DBNull.Value ? string.Empty : r[col].ToString() ?? string.Empty;

        public async Task<(List<DashboardAsesorResponseDto>, DashboardAsesorTotalesDto)> GetDashboardAsesorAsync(
            string uunn, string asesor, DateTime? fechaIni, DateTime? fechaFin)
        {
            var estados = new List<DashboardAsesorResponseDto>();
            DashboardAsesorTotalesDto totales = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("USP_CENCOSUD_TC_DASHBOARD_ASESOR", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UUNN", uunn);
                cmd.Parameters.AddWithValue("@Asesor", asesor);
                cmd.Parameters.AddWithValue("@FechaIni", (object?)fechaIni ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object?)fechaFin ?? DBNull.Value);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        estados.Add(new DashboardAsesorResponseDto
                        {
                            Estado = GetString(reader, "ESTADO"),
                            Cantidad = GetInt(reader, "CANTIDAD")
                        });
                    }

                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        totales = new DashboardAsesorTotalesDto
                        {
                            TotalWapeos = GetInt(reader, "TOTAL_WAPEOS"),
                            TotalVentas = GetInt(reader, "TOTAL_VENTAS"),          // <- antes reventaba
                            MetaWapeos = GetNullableInt(reader, "META_WAPEOS"),
                            MetaVentas = GetNullableInt(reader, "META_VENTAS")
                        };
                    }
                    else
                    {
                        // por seguridad, si no vino segundo resultset
                        totales = new DashboardAsesorTotalesDto
                        {
                            TotalWapeos = 0,
                            TotalVentas = 0,
                            MetaWapeos = null,
                            MetaVentas = null
                        };
                    }
                }
            }

            return (estados, totales);
        }

        public async Task<(List<DashboardSupervisorDetalleDto>, DashboardSupervisorTotalesDto)> GetDashboardSupervisorAsync(
            string uunn, string supervisor, DateTime? fechaIni, DateTime? fechaFin)
        {
            var detalle = new List<DashboardSupervisorDetalleDto>();
            DashboardSupervisorTotalesDto totales = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("USP_CENCOSUD_TC_DASHBOARD_SUPERVISOR", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UUNN", uunn);
                cmd.Parameters.AddWithValue("@Supervisor", supervisor);
                cmd.Parameters.AddWithValue("@FechaIni", (object?)fechaIni ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object?)fechaFin ?? DBNull.Value);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        detalle.Add(new DashboardSupervisorDetalleDto
                        {
                            Asesor = GetString(reader, "ASESOR"),
                            Wapeos = GetInt(reader, "WAPEOS"),
                            Ventas = GetInt(reader, "VENTAS"),
                            MetaWapeos = GetNullableInt(reader, "META_WAPEOS"),
                            MetaVentas = GetNullableInt(reader, "META_VENTAS")
                        });
                    }

                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        totales = new DashboardSupervisorTotalesDto
                        {
                            TotalWapeos = GetInt(reader, "TOTAL_WAPEOS"),
                            TotalVentas = GetInt(reader, "TOTAL_VENTAS"),
                            MetaWapeosEquipo = GetNullableInt(reader, "META_WAPEOS_EQUIPO"),
                            MetaVentasEquipo = GetNullableInt(reader, "META_VENTAS_EQUIPO")
                        };
                    }
                    else
                    {
                        totales = new DashboardSupervisorTotalesDto
                        {
                            TotalWapeos = 0,
                            TotalVentas = 0,
                            MetaWapeosEquipo = null,
                            MetaVentasEquipo = null
                        };
                    }
                }
            }

            return (detalle, totales);
        }

        public async Task<(List<DashboardAdminDetalleDto>, DashboardAdminTotalesDto)> GetDashboardAdminAsync(
            string uunn, DateTime? fechaIni, DateTime? fechaFin)
        {
            var detalle = new List<DashboardAdminDetalleDto>();
            DashboardAdminTotalesDto totales = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("USP_CENCOSUD_TC_DASHBOARD_ADMIN", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UUNN", uunn);
                cmd.Parameters.AddWithValue("@FechaIni", (object?)fechaIni ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object?)fechaFin ?? DBNull.Value);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        detalle.Add(new DashboardAdminDetalleDto
                        {
                            Supervisor = GetString(reader, "SUPERVISOR"),
                            Wapeos = GetInt(reader, "WAPEOS"),
                            Ventas = GetInt(reader, "VENTAS"),
                            MetaWapeosEquipo = GetNullableInt(reader, "META_WAPEOS_EQUIPO"),
                            MetaVentasEquipo = GetNullableInt(reader, "META_VENTAS_EQUIPO")
                        });
                    }

                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        totales = new DashboardAdminTotalesDto
                        {
                            TotalWapeos = GetInt(reader, "TOTAL_WAPEOS"),
                            TotalVentas = GetInt(reader, "TOTAL_VENTAS")
                        };
                    }
                    else
                    {
                        totales = new DashboardAdminTotalesDto
                        {
                            TotalWapeos = 0,
                            TotalVentas = 0
                        };
                    }
                }
            }

            return (detalle, totales);
        }
    }
}
