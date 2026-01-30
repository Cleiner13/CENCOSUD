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
                                  "Solo te interesa la cabecera (Documento de identidad y Nombre Completo) y las tarjetas de productos. " +
                                  "Siempre responde SOLO con un JSON y nada más."
                    }
                    ,
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = @"Extrae únicamente la información visible de la cabecera y de las tarjetas de productos en la pantalla de consulta de Tarjeta Cencosud.

                                MAPEO ESTRICTO (muy importante):

                                - El campo ""dni"" viene del texto después de ""Documento de identidad:"".
                                - El campo ""nombre"" viene del texto después de ""Nombre Completo:"".
                                - El campo ""tipo_tramite"" viene del título de la tarjeta ""Tarjeta de Crédito"": por ejemplo ""Preevaluado"", ""Regular"".
                                - El campo ""oferta"" viene del monto mostrado debajo de esa misma tarjeta (ej. ""S/ 3,000.00"", sin duplicar).
                                - El campo ""avance_efectivo"" viene del monto de la tarjeta ""Avance Efectivo"" (ej. ""S/ 124.00"").
                                - El campo ""incremento_de_linea"" viene del monto de la tarjeta ""Incremento de Línea"" (ej. ""S/ 2,200.00"").
                                - El campo ""adicionales"" viene del texto de la tarjeta ""Tarjeta de Crédito Adicionales"" (por ejemplo ""Hasta 3 TC"").
                                - El campo ""efectivo_cencosud"" viene del monto de la tarjeta ""Efectivo Cencosud"" (ej. ""S/ 1,500.00"").
                                - El campo ""ec_pct"" viene del valor de la línea ""Pct"" en la tarjeta ""Efectivo Cencosud"" (por ejemplo ""O45"").
                                - El campo ""ec_tasa"" viene del valor de la línea ""Tasa"" en la tarjeta ""Efectivo Cencosud"" (ej. ""70.90%"").
                                - El campo ""ae_pct"" viene del valor de la línea ""Pct"" en la tarjeta ""Avance Efectivo"" (por ejemplo ""O54"").
                                - El campo ""ae_tasa"" viene del valor de la línea ""Tasa"" en la tarjeta ""Avance Efectivo"" (por ejemplo ""101.86%"").
                                - Por ahora todas las demás tarjetas que aparezcan no las tomes en cuenta para nada.

                                REGLAS IMPORTANTES:
                                1) Si alguno de estos datos no aparece en la captura, pon ese campo en null.
                                2) Nunca copies el mismo valor en dos campos distintos.
                                3) No inventes valores ni intentes inferir información que no esté explícitamente en la captura.

                                Devuelve SIEMPRE un JSON EXACTAMENTE con esta estructura (usa null cuando no puedas leer un valor):

                                {
                                  ""dni"": string | null,
                                  ""nombre"": string | null,
                                  ""tipo_tramite"": string | null,
                                  ""oferta"": string | null,
                                  ""avance_efectivo"": string | null,
                                  ""incremento_de_linea"": string | null,
                                  ""adicionales"": string | null,
                                  ""efectivo_cencosud"": string | null,
                                  ""ec_pct"": string | null,
                                  ""ec_tasa"": string | null,
                                  ""ae_pct"": string | null,
                                  ""ae_tasa"": string | null
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
