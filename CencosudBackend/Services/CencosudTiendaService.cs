using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CencosudBackend.DTOs;
using CencosudBackend.Repositories;
using ClosedXML.Excel;

namespace CencosudBackend.Services
{
    public class CencosudTiendaService : ICencosudTiendaService
    {
        private readonly ICencosudTiendaRepository _repository;

        public CencosudTiendaService(ICencosudTiendaRepository repository)
        {
            _repository = repository;
        }

        // ====================  HELPER PRIVADO  ====================

        /// <summary>
        /// Saca el rol de la app (ASESOR/SUPERVISOR/ADMIN)
        /// y el usuario (login) desde el JWT del usuario logueado.
        /// </summary>
        private static (string RolApp, string Usuario) ExtraerDatosUsuario(ClaimsPrincipal user)
        {
            // Rol que pusimos en el token con ClaimTypes.Role
            var rolApp = user.FindFirst(ClaimTypes.Role)?.Value ?? "ASESOR";

            // Usuario: primero intento Name, luego UniqueName
            var usuario =
                user.Identity?.Name ??
                user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                string.Empty;

            return (rolApp, usuario);
        }

        // ====================  MÉTODOS EXISTENTES  ====================

        public async Task<CencosudClienteResponseDto> GuardarClienteAsync(
            CencosudClienteRequestDto dto,
            ClaimsPrincipal user)
        {
            return await _repository.GuardarClienteAsync(dto);
        }

        public async Task<List<CencosudResumenMensualDto>> ObtenerResumenMensualAsync(
            ClaimsPrincipal user,
            DateTime? fechaIni,
            DateTime? fechaFin)
        {
            var (rolApp, usuario) = ExtraerDatosUsuario(user);

            return await _repository.ObtenerResumenMensualAsync(
                rolApp,
                usuario,
                fechaIni,
                fechaFin);
        }

        public async Task<CencosudVentasListadoResponseDto> ObtenerListadoVentasAsync(
    ClaimsPrincipal user,
    DateTime? fechaIni,
    DateTime? fechaFin,
    string? estado,
    string? dniCliente,
    string? vendedor,
    int page,
    int pageSize)
        {
            var (rolApp, usuario) = ExtraerDatosUsuario(user);

            return await _repository.ObtenerListadoVentasAsync(
                rolApp,
                usuario,
                fechaIni,
                fechaFin,
                estado,
                dniCliente,
                vendedor,   // ✅
                page,
                pageSize);
        }

        public async Task<byte[]> GenerarExcelListadoVentasAsync(
            ClaimsPrincipal user,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor)
        {
            var (rolApp, usuario) = ExtraerDatosUsuario(user);

            var tabla = await _repository.ObtenerListadoVentasExcelAsync(
                rolApp,
                usuario,
                fechaIni,
                fechaFin,
                estado,
                dniCliente,
                vendedor); // ✅

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Ventas");
            ws.Cell(1, 1).InsertTable(tabla, true);
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return stream.ToArray();
        }


        public Task<CencosudClienteDetalleDto?> ObtenerClienteDetalleAsync(int idCliente)
            => _repository.ObtenerClienteDetalleAsync(idCliente);

        public async Task<CencosudClienteResponseDto> EliminarClienteAsync(int idCliente)
        {
            return await _repository.EliminarClienteAsync(idCliente);
        }
    }
}
