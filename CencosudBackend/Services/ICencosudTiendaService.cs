using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CencosudBackend.DTOs;

namespace CencosudBackend.Services
{
    public interface ICencosudTiendaService
    {
        Task<CencosudClienteResponseDto> GuardarClienteAsync(
            CencosudClienteRequestDto dto,
            ClaimsPrincipal user);

        Task<List<CencosudResumenMensualDto>> ObtenerResumenMensualAsync(
            ClaimsPrincipal user,
            DateTime? fechaIni,
            DateTime? fechaFin);

        // ✅ NUEVO: vendedor
        Task<CencosudVentasListadoResponseDto> ObtenerListadoVentasAsync(
            ClaimsPrincipal user,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor,   // ✅
            int page,
            int pageSize);

        // ✅ NUEVO: vendedor
        Task<byte[]> GenerarExcelListadoVentasAsync(
            ClaimsPrincipal user,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor);  // ✅

        Task<CencosudClienteDetalleDto?> ObtenerClienteDetalleAsync(int idCliente);
        Task<CencosudClienteResponseDto> EliminarClienteAsync(int idCliente);
    }
}
