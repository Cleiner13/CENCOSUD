using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq; // ✅ NUEVO
using CencosudBackend.DTOs;
using Microsoft.Extensions.Configuration;

namespace CencosudBackend.Repositories
{
    public class CencosudTiendaRepository : ICencosudTiendaRepository
    {
        private readonly string _connectionString;

        public CencosudTiendaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CencosudDb")
                               ?? throw new InvalidOperationException("Connection string 'CencosudDb' not found.");
        }

        public async Task<CencosudClienteResponseDto> GuardarClienteAsync(CencosudClienteRequestDto dto)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("USP_CENCOSUD_TIENDA_CLIENTE_MANTENIMIENTO", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // JSON de campos dinámicos
            string? jsonInfo = null;
            if (dto.InfoAdicional is not null && dto.InfoAdicional.Count > 0)
            {
                jsonInfo = JsonSerializer.Serialize(dto.InfoAdicional);
            }

            // @ID_CLIENTE (InputOutput)
            var pIdCliente = command.Parameters.Add("@ID_CLIENTE", SqlDbType.Int);
            pIdCliente.Direction = ParameterDirection.InputOutput;
            pIdCliente.Value = (object?)dto.IdCliente ?? DBNull.Value;

            // Datos fijos
            command.Parameters.Add("@DNI_CLIENTE", SqlDbType.VarChar, 20).Value = dto.DniCliente;
            command.Parameters.Add("@NOMBRE_CLIENTE", SqlDbType.VarChar, 70).Value = dto.NombreCliente;
            command.Parameters.Add("@TELEFONO", SqlDbType.VarChar, 20).Value = dto.Telefono;

            command.Parameters.Add("@ESTADO", SqlDbType.NVarChar, 40).Value =
                (object?)dto.Estado ?? DBNull.Value;

            command.Parameters.Add("@COMENTARIO", SqlDbType.NVarChar, 400).Value =
                (object?)dto.Comentario ?? DBNull.Value;

            command.Parameters.Add("@USUARIO_CENCOSUD", SqlDbType.NVarChar, 40).Value =
                (object?)dto.UsuarioCencosud ?? DBNull.Value;

            command.Parameters.Add("@CARGO", SqlDbType.NVarChar, 100).Value =
                (object?)dto.Cargo ?? DBNull.Value;

            command.Parameters.Add("@SUPERVISOR", SqlDbType.NVarChar, 100).Value =
                (object?)dto.Supervisor ?? DBNull.Value;

            command.Parameters.Add("@UUNN", SqlDbType.NVarChar, 100).Value =
                (object?)dto.Uunn ?? DBNull.Value;

            command.Parameters.Add("@NOM_VENDEDOR", SqlDbType.NVarChar, 100).Value =
                (object?)dto.NomVendedor ?? DBNull.Value;

            // JSON dinámico
            var pJsonInfo = command.Parameters.Add("@JsonInfo", SqlDbType.NVarChar, -1);
            pJsonInfo.Value = (object?)jsonInfo ?? DBNull.Value;

            // Imagen
            command.Parameters.Add("@TieneImagen", SqlDbType.Bit).Value = dto.TieneImagen;

            // Parámetros de salida
            var pCodigo = command.Parameters.Add("@CodigoResultado", SqlDbType.Int);
            pCodigo.Direction = ParameterDirection.Output;

            var pMensaje = command.Parameters.Add("@Mensaje", SqlDbType.NVarChar, 200);
            pMensaje.Direction = ParameterDirection.Output;

            await connection.OpenAsync().ConfigureAwait(false);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            var response = new CencosudClienteResponseDto
            {
                CodigoResultado = (int)(pCodigo.Value ?? 0),
                Mensaje = pMensaje.Value?.ToString() ?? string.Empty
            };

            if (command.Parameters["@ID_CLIENTE"].Value != DBNull.Value)
            {
                response.IdCliente = Convert.ToInt32(command.Parameters["@ID_CLIENTE"].Value);
            }

            return response;
        }

        // =================  NUEVO: RESUMEN MENSUAL  =================
        public async Task<List<CencosudResumenMensualDto>> ObtenerResumenMensualAsync(
            string rolApp,
            string usuario,
            DateTime? fechaIni,
            DateTime? fechaFin)
        {
            var lista = new List<CencosudResumenMensualDto>();

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.USP_CENCOSUD_TIENDA_RESUMEN_MENSUAL", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@RolApp", rolApp);
                cmd.Parameters.AddWithValue("@Usuario", usuario);

                cmd.Parameters.AddWithValue("@FechaIni",
                    (object?)fechaIni ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin",
                    (object?)fechaFin ?? DBNull.Value);

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    var item = new CencosudResumenMensualDto
                    {
                        Estado = dr["Estado"] as string ?? string.Empty,
                        Cantidad = dr["Cantidad"] as int? ?? Convert.ToInt32(dr["Cantidad"])
                    };

                    lista.Add(item);
                }
            }

            return lista;
        }

        // =================  NUEVO: LISTADO VENTAS  =================
        public async Task<CencosudVentasListadoResponseDto> ObtenerListadoVentasAsync(
            string rolApp,
            string usuario,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor,
            int page,
            int pageSize)
        {
            var response = new CencosudVentasListadoResponseDto
            {
                Registros = new List<CencosudVentaListadoItemDto>()
            };

            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.USP_CENCOSUD_TIENDA_LISTADO", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@RolApp", rolApp);
                cmd.Parameters.AddWithValue("@Usuario", usuario);

                cmd.Parameters.AddWithValue("@FechaIni",
                    (object?)fechaIni ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin",
                    (object?)fechaFin ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Estado",
                    (object?)estado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DniCliente",
                    (object?)dniCliente ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Vendedor", (object?)vendedor ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                await cn.OpenAsync();
                using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    var item = new CencosudVentaListadoItemDto
                    {
                        IdCliente = dr["ID_CLIENTE"] as int? ?? Convert.ToInt32(dr["ID_CLIENTE"]),
                        DniCliente = dr["DNI_CLIENTE"] as string ?? string.Empty,
                        NombreCliente = dr["NOMBRE_CLIENTE"] as string ?? string.Empty,
                        Telefono = dr["TELEFONO"] as string ?? string.Empty,
                        Estado = dr["ESTADO"] as string ?? string.Empty,
                        Comentario = dr["COMENTARIO"] as string ?? string.Empty,
                        NomVendedor = dr["NOM_VENDEDOR"] as string ?? string.Empty,
                        Supervisor = dr["SUPERVISOR"] as string ?? string.Empty,
                        Uunn = dr["UUNN"] as string ?? string.Empty,
                        DateEntry = dr["DATE_ENTRY"] as DateTime? ?? Convert.ToDateTime(dr["DATE_ENTRY"])
                    };

                    response.Registros.Add(item);
                }
            }

            return response;
        }


        // DESCARGA EXCEL
        public async Task<DataTable> ObtenerListadoVentasExcelAsync(
            string rolApp,
            string usuario,
            DateTime? fechaIni,
            DateTime? fechaFin,
            string? estado,
            string? dniCliente,
            string? vendedor)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TIENDA_LISTADO_VENTAS_EXCEL", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@RolApp", (object?)rolApp ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Usuario", (object?)usuario ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaIni", (object?)fechaIni ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", (object?)estado ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DniCliente", (object?)dniCliente ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Vendedor", (object?)vendedor ?? DBNull.Value);

            await cn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        // Traer Informacion del Cliente
        public async Task<CencosudClienteDetalleDto?> ObtenerClienteDetalleAsync(int idCliente)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TIENDA_CLIENTE_DETALLE", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_CLIENTE", idCliente);

            await cn.OpenAsync();
            using var dr = await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
            {
                return null;
            }

            var dto = new CencosudClienteDetalleDto
            {
                IdCliente = dr["ID_CLIENTE"] as int? ?? Convert.ToInt32(dr["ID_CLIENTE"]),
                DniCliente = dr["DNI_CLIENTE"] as string ?? string.Empty,
                NombreCliente = dr["NOMBRE_CLIENTE"] as string ?? string.Empty,
                Telefono = dr["TELEFONO"] as string ?? string.Empty,

                Estado = dr["ESTADO"] as string,
                Comentario = dr["COMENTARIO"] as string,
                UsuarioCencosud = dr["USUARIO_CENCOSUD"] as string,
                Cargo = dr["CARGO"] as string,
                Supervisor = dr["SUPERVISOR"] as string,
                Uunn = dr["UUNN"] as string,
                NomVendedor = dr["NOM_VENDEDOR"] as string,

                DateEntry = dr["DATE_ENTRY"] as DateTime? ?? Convert.ToDateTime(dr["DATE_ENTRY"]),
                DateModify = dr["DATE_MODIFY"] as DateTime?,

                AvanceEfectivo = dr["AvanceEfectivo"] as string,
                TipoTramite = dr["TipoTramite"] as string,
                Oferta = dr["Oferta"] as string,
                Superavance = dr["Superavance"] as string,
                CambioProducto = dr["CambioProducto"] as string,
                IncrementoLinea = dr["IncrementoLinea"] as string,
                InfoAdicional = null

            };

            // Deserializar JSON a diccionario
            if (dr["JsonInfo"] is string jsonInfo && !string.IsNullOrWhiteSpace(jsonInfo))
            {
                dto.InfoAdicional = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonInfo);
            }

            return dto;
        }

        //Eliminar cliente 
        public async Task<CencosudClienteResponseDto> EliminarClienteAsync(int idCliente)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.USP_CENCOSUD_TIENDA_CLIENTE_ELIMINAR", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_CLIENTE", idCliente);

            var pCodigo = cmd.Parameters.Add("@CodigoResultado", SqlDbType.Int);
            pCodigo.Direction = ParameterDirection.Output;

            var pMensaje = cmd.Parameters.Add("@Mensaje", SqlDbType.NVarChar, 200);
            pMensaje.Direction = ParameterDirection.Output;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            var resp = new CencosudClienteResponseDto
            {
                CodigoResultado = (int)(pCodigo.Value ?? 0),
                Mensaje = pMensaje.Value?.ToString() ?? string.Empty,
                IdCliente = idCliente
            };

            return resp;
        }
    }
}
