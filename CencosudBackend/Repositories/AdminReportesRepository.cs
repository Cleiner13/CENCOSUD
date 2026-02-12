using CencosudBackend.DTOs;
using CencosudBackend.DTOs.AdminReportes;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CencosudBackend.Repositories
{

    public class AdminReportesRepository : IAdminReportesRepository
    {
        private readonly string _cn;

        public AdminReportesRepository(IConfiguration configuration)
        {
            _cn = configuration.GetConnectionString("CencosudDb")
                  ?? throw new InvalidOperationException("Connection string 'CencosudDb' not found.");
        }

        public async Task<List<AdminTramitesMesRowDto>> ObtenerTramitesMesAsync(AdminTramitesMesRequestDto req)
        {
            var result = new List<AdminTramitesMesRowDto>();

            using var con = new SqlConnection(_cn);
            await con.OpenAsync();

            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_ADMIN_TRAMITES_MES", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FechaIni", req.FechaIni.Date);
            cmd.Parameters.AddWithValue("@FechaFin", req.FechaFin.Date);
            cmd.Parameters.AddWithValue("@FiltroTipo", req.FiltroTipo ?? "TODO");
            cmd.Parameters.AddWithValue("@UUNN", (object?)req.Uunn ?? DBNull.Value);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(new AdminTramitesMesRowDto
                {
                    TipoTramite = rd["TipoTramite"]?.ToString() ?? "",
                    Cantidad = rd["Cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Cantidad"]),
                    Porcentaje = rd["Porcentaje"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["Porcentaje"])
                });
            }

            return result;
        }

        public async Task<List<AdminHoraWapeoRowDto>> ObtenerHoraWapeoPorDiaAsync(AdminHoraWapeoPorDiaRequestDto req)
        {
            var result = new List<AdminHoraWapeoRowDto>();
            var horasCols = new[] { "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22" };

            using var con = new SqlConnection(_cn);
            await con.OpenAsync();

            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_ADMIN_HORA_WAPEO_POR_DIA", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FechaIni", req.FechaIni.Date);
            cmd.Parameters.AddWithValue("@FechaFin", req.FechaFin.Date);
            cmd.Parameters.AddWithValue("@Supervisor", req.Supervisor);
            cmd.Parameters.AddWithValue("@Asesor", (object?)req.Asesor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UUNN", (object?)req.Uunn ?? DBNull.Value);

            using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                var row = new AdminHoraWapeoRowDto
                {
                    Supervisor = rd["Supervisor"]?.ToString() ?? "",
                    Promotor = rd["Promotor"]?.ToString() ?? "",
                    NroTc = rd["NRO_TC"] == DBNull.Value ? 0 : Convert.ToInt32(rd["NRO_TC"]),
                };

                foreach (var h in horasCols)
                {
                    // columnas "08", "09"... vienen como nombres válidos en reader
                    row.Horas[h] = rd[h] == DBNull.Value ? 0 : Convert.ToInt32(rd[h]);
                }

                result.Add(row);
            }

            return result;
        }

        public async Task<AdminKpisResponseDto> ObtenerKpisAsync(DateTime fechaIni, DateTime fechaFin, string periodo, string? uunn)
        {
            using var cn = new SqlConnection(_cn);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_ADMIN_KPIS", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@FechaIni", fechaIni.Date);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
            cmd.Parameters.AddWithValue("@Periodo", periodo);
            cmd.Parameters.AddWithValue("@UUNN", (object?)uunn ?? DBNull.Value);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            var dto = new AdminKpisResponseDto { Periodo = periodo };

            if (await dr.ReadAsync())
            {
                dto.Ventas = dr["Ventas"] as int? ?? Convert.ToInt32(dr["Ventas"]);
                dto.Coordinado = dr["Coordinado"] as int? ?? Convert.ToInt32(dr["Coordinado"]);
                dto.SinOferta = dr["SinOferta"] as int? ?? Convert.ToInt32(dr["SinOferta"]);

                dto.MetaEquipoVentas = dr["MetaEquipoVentas"] as int? ?? Convert.ToInt32(dr["MetaEquipoVentas"]);
                dto.CumplimientoVentasPct = dr["CumplimientoVentasPct"] as decimal? ?? Convert.ToDecimal(dr["CumplimientoVentasPct"]);

                dto.MetaEquipoWapeos = dr["MetaEquipoWapeos"] as int? ?? Convert.ToInt32(dr["MetaEquipoWapeos"]);
                dto.CumplimientoWapeosPct = dr["CumplimientoWapeosPct"] as decimal? ?? Convert.ToDecimal(dr["CumplimientoWapeosPct"]);
            }

            return dto;
        }

        public async Task<List<AdminSerieDiariaItemDto>> ObtenerSerieDiariaAsync(DateTime fechaIni, DateTime fechaFin, string? supervisor, string? uunn)
        {
            var list = new List<AdminSerieDiariaItemDto>();

            using var cn = new SqlConnection(_cn);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_ADMIN_SERIE_DIARIA", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@FechaIni", fechaIni.Date);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
            cmd.Parameters.AddWithValue("@Supervisor", (object?)supervisor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UUNN", (object?)uunn ?? DBNull.Value);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new AdminSerieDiariaItemDto
                {
                    Fecha = dr["Fecha"] as DateTime? ?? Convert.ToDateTime(dr["Fecha"]),
                    Venta = dr["Venta"] as int? ?? Convert.ToInt32(dr["Venta"]),
                    Coordinado = dr["Coordinado"] as int? ?? Convert.ToInt32(dr["Coordinado"]),
                    SinOferta = dr["SinOferta"] as int? ?? Convert.ToInt32(dr["SinOferta"])
                });
            }

            return list;
        }

        public async Task<List<AdminRankingSupervisorDto>> ObtenerRankingSupervisoresAsync(DateTime fechaIni, DateTime fechaFin, string periodo, string? uunn)
        {
            var list = new List<AdminRankingSupervisorDto>();

            using var cn = new SqlConnection(_cn);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_ADMIN_RANKING_SUPERVISORES", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@FechaIni", fechaIni.Date);
            cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
            cmd.Parameters.AddWithValue("@Periodo", periodo);
            cmd.Parameters.AddWithValue("@UUNN", (object?)uunn ?? DBNull.Value);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new AdminRankingSupervisorDto
                {
                    Supervisor = dr["Supervisor"]?.ToString() ?? string.Empty,
                    Asesores = dr["Asesores"] as int? ?? Convert.ToInt32(dr["Asesores"]),
                    Ventas = dr["Ventas"] as int? ?? Convert.ToInt32(dr["Ventas"]),
                    MetaVentas = dr["MetaVentas"] as int? ?? Convert.ToInt32(dr["MetaVentas"]),
                    CumplimientoPct = dr["CumplimientoPct"] as decimal? ?? Convert.ToDecimal(dr["CumplimientoPct"]),
                });
            }

            return list;
        }
    }
}
