using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CencosudBackend.DTOs;

namespace CencosudBackend.Services
{
    public interface IOpenAiVisionService
    {
        Task<(OcrCencosudResumenCamposDto Campos, string RawJson)> LeerResumenAsync(
            byte[] imageBytes,
            string contentType);
    }

    public class OpenAiVisionService : IOpenAiVisionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";
        private const string ModeloVision = "gpt-4o";

        public OpenAiVisionService(IHttpClientFactory httpClientFactory,
                                   IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<(OcrCencosudResumenCamposDto Campos, string RawJson)> LeerResumenAsync(
            byte[] imageBytes,
            string contentType)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Falta OpenAI:ApiKey en appsettings.json");

            // Data URL de la imagen
            var base64 = Convert.ToBase64String(imageBytes);
            var imageUrl = $"data:{contentType};base64,{base64}";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            // 🔹 Forzamos JSON usando response_format = json_object
            var body = new
            {
                model = ModeloVision,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "Eres un asistente que lee capturas de pantalla de la web Tarjeta Cencosud. " +
                                  "Solo te interesa el cuadro 'RESUMEN'. Siempre responde SOLO con un JSON y nada más."
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text =
                            @"Extrae únicamente la información del cuadro RESUMEN de la tarjeta Cencosud.

                            MAPEO ESTRICTO (muy importante):

                            - El campo ""fecha"" viene de la fila cuyo título sea ""Fecha:"".
                            - El campo ""tipo_doc"" viene de la fila ""Tipo Doc:"".
                            - El campo ""doc"" viene de la fila ""Doc:"".
                            - El campo ""nombre"" viene de la fila ""Nombre:"".
                            - El campo ""tipo_de_tramite"" viene de la fila ""Tipo de Trámite:"".
                            - El campo ""oferta"" viene de la fila ""Oferta:"".
                            - El campo ""incremento_de_linea"" viene de la fila ""Incremento de Línea:"".
                            - El campo ""superavance"" viene de una fila cuyo título contenga ""Superavance"".
                            - El campo ""superavance_plus"" viene de una fila cuyo título contenga ""Superavance Plus"".
                            - El campo ""avance_efectivo"" viene de la fila cuyo título contenga exactamente ""Avance Efectivo"".
                            - El campo ""cambio_de_producto"" SOLO debe usarse si existe una fila cuyo título contenga ""Cambio de Producto"" o ""Cambio de Prod."".

                            REGLAS IMPORTANTES:

                            1) Si una fila NO aparece en la imagen (por ejemplo no existe ""Cambio de Producto""), pon ese campo en null.
                            2) NUNCA copies el mismo valor en dos campos distintos.
                               Ejemplo: si solo ves ""Avance Efectivo: DE-TASA"" y NO ves ""Cambio de Producto"",
                               entonces:
                               - ""avance_efectivo"": ""DE-TASA""
                               - ""cambio_de_producto"": null
                            3) No inventes valores ni infieras productos a partir de otros textos.
                            3) No inventes valores ni infieras productos a partir de otros textos.

                            Devuélvela SIEMPRE en este JSON (usa null cuando no puedas leer un valor, no inventes nada):

                            {
                              ""fecha"": string | null,
                              ""tipo_doc"": string | null,
                              ""doc"": string | null,
                              ""nombre"": string | null,
                              ""tipo_de_tramite"": string | null,
                              ""oferta"": string | null,
                              ""incremento_de_linea"": string | null,
                              ""superavance"": string | null,
                              ""superavance_plus"": string | null,
                              ""avance_efectivo"": string | null,
                              ""cambio_de_producto"": string | null
                            }

                            No escribas comentarios ni texto fuera del JSON."

                            },
                            new
                            {
                                type = "image_url",
                                image_url = new
                                {
                                    url = imageUrl
                                }
                            }
                        }
                    }
                },
                temperature = 0.0,
                response_format = new   // 🔹 clave
                {
                    type = "json_object"
                }
            };

            var jsonBody = JsonSerializer.Serialize(body);
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var httpResponse = await client.PostAsync(OpenAiUrl, httpContent);
            var responseText = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                var status = (int)httpResponse.StatusCode;
                throw new Exception(
                    $"Error al llamar a OpenAI. Status={status} ({httpResponse.StatusCode}). Body={responseText}");
            }

            // 🔹 Sacamos el contenido de OpenAI
            string jsonCampos = ExtraerContenidoTexto(responseText);

            // 🔹 Limpiamos fences ``` y nos quedamos solo con el objeto {...}
            jsonCampos = LimpiarJsonDevuelto(jsonCampos);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var campos = JsonSerializer.Deserialize<OcrCencosudResumenCamposDto>(jsonCampos, options)
                         ?? new OcrCencosudResumenCamposDto();

            return (campos, jsonCampos);
        }

        /// <summary>
        /// Lee el JSON de OpenAI y devuelve el texto del primer bloque de contenido.
        /// Soporta tanto content como string como content como array de partes.
        /// </summary>
        private static string ExtraerContenidoTexto(string responseJson)
        {
            var root = JsonNode.Parse(responseJson)?.AsObject()
                       ?? throw new InvalidOperationException("Respuesta inválida de OpenAI.");

            var choices = root["choices"]?.AsArray();
            if (choices == null || choices.Count == 0)
                throw new InvalidOperationException("OpenAI no devolvió choices.");

            var message = choices[0]?["message"];
            var contentNode = message?["content"];

            if (contentNode == null)
                throw new InvalidOperationException("Respuesta sin message.content.");

            // Caso viejo: content es string
            if (contentNode is JsonValue)
            {
                return contentNode.ToString();
            }

            // Caso nuevo: content es array de partes [{type:"text", text:"..."}]
            if (contentNode is JsonArray arr)
            {
                foreach (var part in arr)
                {
                    var type = part?["type"]?.ToString();
                    if (string.Equals(type, "text", StringComparison.OrdinalIgnoreCase))
                    {
                        return part?["text"]?.ToString() ?? "";
                    }
                }
            }

            throw new InvalidOperationException("No se encontró texto en la respuesta de OpenAI.");
        }

        /// <summary>
        /// Quita ```json, ``` y recorta todo lo que no sea el objeto JSON principal.
        /// </summary>
        private static string LimpiarJsonDevuelto(string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                throw new InvalidOperationException("Respuesta vacía de OpenAI.");

            var s = contenido.Trim();

            // Si vino con fences ```json ... ```
            if (s.StartsWith("```"))
            {
                // quitar primera línea ```json o ```
                var firstNewLine = s.IndexOf('\n');
                if (firstNewLine >= 0)
                {
                    s = s.Substring(firstNewLine + 1);
                }

                // quitar última ``` si existe
                var lastFence = s.LastIndexOf("```", StringComparison.Ordinal);
                if (lastFence >= 0)
                {
                    s = s.Substring(0, lastFence);
                }

                s = s.Trim();
            }

            // Por seguridad, nos quedamos SOLO entre el primer '{' y el último '}'
            var first = s.IndexOf('{');
            var last = s.LastIndexOf('}');
            if (first >= 0 && last > first)
            {
                s = s.Substring(first, last - first + 1);
            }

            return s.Trim();
        }
    }
}
