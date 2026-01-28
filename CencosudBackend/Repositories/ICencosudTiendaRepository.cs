using CencosudBackend.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CencosudBackend.Repositories
{
    public interface ICencosudTiendaRepository
    {
        Task<CencosudClienteResponseDto> GuardarClienteAsync(CencosudClienteRequestDto dto);

        Task<List<CencosudResumenMensualDto>> ObtenerResumenMensualAsync(
            string rolApp,
            string usuario,
            DateTime? fechaIni,
            DateTime? fechaFin);

        // ✅ NUEVO: vendedor (asesor a filtrar)
        Task<CencosudVentasListadoResponseDto> ObtenerListadoVentasAsync(
            string rolApp,
            string usuario,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor,   // ✅
            int page,
            int pageSize);

        // ✅ NUEVO: vendedor también en excel (para consistencia)
        Task<DataTable> ObtenerListadoVentasExcelAsync(
            string rolApp,
            string usuario,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor);  // ✅

        Task<CencosudClienteDetalleDto?> ObtenerClienteDetalleAsync(int idCliente);
        Task<CencosudClienteResponseDto> EliminarClienteAsync(int idCliente);
    }
}
