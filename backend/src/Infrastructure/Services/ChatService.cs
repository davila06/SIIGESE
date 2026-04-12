using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Enterprise chatbot service with intent-based NLP, domain data awareness, and rich responses.
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IPolizaRepository _polizaRepo;
        private readonly IReclamoRepository _reclamoRepo;
        private readonly ICobroRepository _cobroRepo;
        private readonly ILogger<ChatService> _logger;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ChatService(
            IChatRepository chatRepo,
            IPolizaRepository polizaRepo,
            IReclamoRepository reclamoRepo,
            ICobroRepository cobroRepo,
            ILogger<ChatService> logger)
        {
            _chatRepo = chatRepo;
            _polizaRepo = polizaRepo;
            _reclamoRepo = reclamoRepo;
            _cobroRepo = cobroRepo;
            _logger = logger;
        }

        // â”€â”€ Sessions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<ChatSessionDto> CreateSessionAsync(int userId, string? title = null)
        {
            var session = new ChatSession
            {
                UserId = userId,
                Title = title ?? "Nueva conversaciÃ³n",
                LastActivityAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            await _chatRepo.AddSessionAsync(session);
            return MapSessionToDto(session);
        }

        public async Task<IEnumerable<ChatSessionDto>> GetSessionsAsync(int userId)
        {
            var sessions = await _chatRepo.GetSessionsByUserAsync(userId);
            return sessions.Select(MapSessionToDto);
        }

        public async Task<ChatSessionDetailDto> GetSessionAsync(int userId, string sessionId)
        {
            var session = await _chatRepo.GetSessionBySessionIdAsync(sessionId, userId)
                ?? throw new KeyNotFoundException($"SesiÃ³n {sessionId} no encontrada");

            var messages = await _chatRepo.GetMessagesBySessionAsync(session.Id);
            return new ChatSessionDetailDto
            {
                Session = MapSessionToDto(session),
                Messages = messages.Select(MapMessageToDto).ToList()
            };
        }

        public async Task DeleteSessionAsync(int userId, string sessionId)
        {
            var session = await _chatRepo.GetSessionBySessionIdAsync(sessionId, userId)
                ?? throw new KeyNotFoundException($"SesiÃ³n {sessionId} no encontrada");

            await _chatRepo.DeleteSessionAsync(session);
        }

        public async Task MarkSessionAsReadAsync(int userId, string sessionId)
        {
            var session = await _chatRepo.GetSessionBySessionIdAsync(sessionId, userId)
                ?? throw new KeyNotFoundException($"SesiÃ³n {sessionId} no encontrada");

            await _chatRepo.MarkSessionMessagesReadAsync(session.Id, userId);
        }

        public async Task ReactToMessageAsync(int userId, int messageId, ReactToMessageDto dto)
        {
            var message = await _chatRepo.GetMessageByIdAsync(messageId)
                ?? throw new KeyNotFoundException($"Mensaje {messageId} no encontrado");

            if (dto.Score is not (1 or -1))
                throw new ArgumentException("Score must be 1 (like) or -1 (dislike)");

            message.ReactionScore = message.ReactionScore == dto.Score ? null : dto.Score;
            await _chatRepo.UpdateMessageAsync(message);
        }

        public async Task<ChatStatsDto> GetStatsAsync(int userId)
        {
            var sessions = (await _chatRepo.GetSessionsByUserAsync(userId)).ToList();
            var allMessages = new List<ChatMessage>();

            foreach (var s in sessions)
            {
                var msgs = await _chatRepo.GetMessagesBySessionAsync(s.Id, 0, int.MaxValue);
                allMessages.AddRange(msgs);
            }

            var botMessages = allMessages.Where(m => m.MessageType == ChatMessageType.Bot).ToList();

            return new ChatStatsDto
            {
                TotalSessions = sessions.Count,
                ActiveSessions = sessions.Count(s => s.Status == ChatSessionStatus.Active),
                TotalMessages = allMessages.Count,
                AvgMessagesPerSession = sessions.Count > 0
                    ? Math.Round((double)allMessages.Count / sessions.Count, 1)
                    : 0,
                AvgResponseTimeMs = botMessages.Count > 0
                    ? Math.Round(botMessages.Where(m => m.ProcessingTimeMs.HasValue).Average(m => m.ProcessingTimeMs!.Value), 0)
                    : 0,
                LikedResponses = botMessages.Count(m => m.ReactionScore == 1),
                DislikedResponses = botMessages.Count(m => m.ReactionScore == -1)
            };
        }

        // â”€â”€ Message handling â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        public async Task<SendMessageResponseDto> SendMessageAsync(int userId, string sessionId, SendMessageDto dto)
        {
            var session = await _chatRepo.GetSessionBySessionIdAsync(sessionId, userId)
                ?? throw new KeyNotFoundException($"SesiÃ³n {sessionId} no encontrada");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Persist user message
            var userMsg = new ChatMessage
            {
                ChatSessionId = session.Id,
                Content = dto.Content.Trim(),
                MessageType = ChatMessageType.User,
                IsRead = true,
                CreatedBy = userId.ToString()
            };
            await _chatRepo.AddMessageAsync(userMsg);

            // Generate AI response
            var (responseContent, richContent, quickReplies) = await GenerateResponseAsync(dto.Content, userId);

            stopwatch.Stop();

            var botMsg = new ChatMessage
            {
                ChatSessionId = session.Id,
                Content = responseContent,
                MessageType = ChatMessageType.Bot,
                RichContent = richContent != null ? JsonSerializer.Serialize(richContent, _jsonOpts) : null,
                QuickReplies = quickReplies is { Count: > 0 } ? JsonSerializer.Serialize(quickReplies, _jsonOpts) : null,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                IsRead = false,
                CreatedBy = "bot"
            };
            await _chatRepo.AddMessageAsync(botMsg);

            // Update session metadata
            session.MessageCount += 2;
            session.LastMessage = responseContent.Length > 100
                ? responseContent[..97] + "..."
                : responseContent;
            session.LastActivityAt = DateTime.UtcNow;

            // Auto-title session from first user message
            if (session.MessageCount == 2)
                session.Title = TruncateTitle(dto.Content);

            await _chatRepo.UpdateSessionAsync(session);

            return new SendMessageResponseDto
            {
                SessionId = session.SessionId,
                SessionTitle = session.Title,
                UserMessage = MapMessageToDto(userMsg),
                BotResponse = MapMessageToDto(botMsg)
            };
        }

        // â”€â”€ AI Engine â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private async Task<(string Response, object? RichContent, List<string>? QuickReplies)> GenerateResponseAsync(
            string input, int userId)
        {
            var intent = DetectIntent(input);
            _logger.LogDebug("Chat intent detected: {Intent} for input: {Input}", intent, input);

            return intent switch
            {
                "greeting" => BuildGreeting(),
                "help" => BuildHelp(),
                "poliza_query" => await BuildPolizaResponseAsync(input, userId),
                "cobro_query" => await BuildCobrosResponseAsync(userId),
                "reclamo_query" => await BuildReclamosResponseAsync(input, userId),
                "stats_query" => await BuildStatsResponseAsync(userId),
                "farewell" => BuildFarewell(),
                _ => BuildFallback(input)
            };
        }

        private static string DetectIntent(string input)
        {
            var text = input.ToLowerInvariant().RemoveDiacritics();

            // Domain-specific intents take priority over greetings
            if (AnyMatch(text, "poliza", "polizas", "asegurado", "seguro", "cobertura", "aseguradora", "vigencia", "prima"))
                return "poliza_query";

            if (AnyMatch(text, "cobro", "cobros", "pago", "pagos", "cuota", "cuotas", "deuda", "pendiente", "factura"))
                return "cobro_query";

            if (AnyMatch(text, "reclamo", "reclamos", "siniestro", "siniestros", "reclamacion", "incidente"))
                return "reclamo_query";

            if (AnyMatch(text, "estadistica", "estadisticas", "resumen", "total", "cuantos", "cuantas", "informe", "reporte", "dashboard", "dashboard"))
                return "stats_query";

            if (AnyMatch(text, "ayuda", "help", "que puedes", "como funcionas", "que haces", "opciones", "comandos"))
                return "help";

            if (AnyMatch(text, "adios", "chao", "hasta luego", "bye", "gracias", "muchas gracias"))
                return "farewell";

            if (AnyMatch(text, "hola", "buenos dias", "buenas", "saludos", "hey", "hi", "que tal", "buen dia"))
                return "greeting";

            return "unknown";
        }

        // â”€â”€ Intent handlers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static (string, object?, List<string>?) BuildGreeting()
        {
            var greetings = new[]
            {
                "Â¡Hola! Soy **OmnIA Assistant**, tu asistente inteligente de gestiÃ³n de seguros.",
                "Â¡Buenos dÃ­as! Soy tu asistente virtual de **OmnIA**.",
                "Â¡Hola! Estoy aquÃ­ para ayudarte con la gestiÃ³n de tus seguros."
            };

            var response = greetings[new Random().Next(greetings.Length)] +
                "\n\nÂ¿En quÃ© puedo ayudarte hoy?";

            var quickReplies = new List<string>
            {
                "Ver mis pÃ³lizas",
                "Consultar cobros",
                "Ver reclamos",
                "EstadÃ­sticas",
                "Â¿QuÃ© puedes hacer?"
            };

            return (response, null, quickReplies);
        }

        private static (string, object?, List<string>?) BuildHelp()
        {
            var response = "Puedo ayudarte con las siguientes tareas:\n\n" +
                           "ðŸ“‹ **PÃ³lizas** â€” Buscar pÃ³lizas por nÃºmero, asegurado o aseguradora\n" +
                           "ðŸ’° **Cobros** â€” Consultar estado de cobros y pagos pendientes\n" +
                           "âš ï¸ **Reclamos** â€” Ver reclamos activos, pendientes o vencidos\n" +
                           "ðŸ“Š **EstadÃ­sticas** â€” Resumen general del sistema\n\n" +
                           "Solo escrÃ­beme lo que necesitas en lenguaje natural. Por ejemplo:\n" +
                           "*\"Busca la pÃ³liza de Juan PÃ©rez\"* o *\"Â¿CuÃ¡ntos cobros tengo pendientes?\"*";

            var quickReplies = new List<string>
            {
                "Ver mis pÃ³lizas",
                "Cobros pendientes",
                "Reclamos activos",
                "Resumen general"
            };

            return (response, null, quickReplies);
        }

        private async Task<(string, object?, List<string>?)> BuildPolizaResponseAsync(string input, int userId)
        {
            try
            {
                var allPolizas = (await _polizaRepo.GetActivasAsync()).ToList();

                // Try to extract a poliza number or name from input
                var searchTerm = ExtractSearchTerm(input, "poliza", "polizas", "seguro", "asegurado",
                    "numero", "busca", "buscar", "muestra", "mostrar", "ver");

                List<Poliza> matches;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLowerInvariant();
                    matches = allPolizas
                        .Where(p =>
                            (p.NumeroPoliza?.ToLowerInvariant().Contains(term) ?? false) ||
                            (p.NombreAsegurado?.ToLowerInvariant().Contains(term) ?? false) ||
                            (p.Aseguradora?.ToLowerInvariant().Contains(term) ?? false) ||
                            (p.NumeroCedula?.ToLowerInvariant().Contains(term) ?? false))
                        .Take(10)
                        .ToList();
                }
                else
                {
                    matches = allPolizas.Take(5).ToList();
                }

                if (matches.Count == 0)
                    return ($"No encontrÃ© pÃ³lizas que coincidan con **\"{searchTerm}\"**. " +
                            "Intenta buscar por nÃºmero de pÃ³liza, nombre del asegurado o aseguradora.",
                            null,
                            new List<string> { "Ver todas las pÃ³lizas", "Buscar por aseguradora" });

                var richContent = new
                {
                    type = "polizas_table",
                    title = matches.Count == 1 ? "PÃ³liza encontrada" : $"{matches.Count} pÃ³lizas encontradas",
                    rows = matches.Select(p => new
                    {
                        numeroPoliza = p.NumeroPoliza,
                        nombreAsegurado = p.NombreAsegurado,
                        aseguradora = p.Aseguradora,
                        vigencia = p.FechaVigencia.ToString("dd-MM-yyyy"),
                        frecuencia = p.Frecuencia
                    }).ToList()
                };

                var summaryText = matches.Count == 1
                    ? $"EncontrÃ© la pÃ³liza **{matches[0].NumeroPoliza}** a nombre de **{matches[0].NombreAsegurado}**."
                    : $"EncontrÃ© **{matches.Count} pÃ³lizas** que coinciden con tu bÃºsqueda:";

                return (summaryText, richContent, new List<string> { "Ver detalle completo", "Cobros de esta pÃ³liza", "Reclamos asociados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying policies for chat");
                return ("OcurriÃ³ un error al consultar las pÃ³lizas. Por favor intenta de nuevo.", null, null);
            }
        }

        private async Task<(string, object?, List<string>?)> BuildCobrosResponseAsync(int userId)
        {
            try
            {
                var allCobros = (await _cobroRepo.GetAllAsync()).ToList();
                var pendientes = allCobros.Where(c => c.Estado == EstadoCobro.Pendiente).ToList();
                var vencidos = allCobros.Where(c => c.Estado == EstadoCobro.Vencido).ToList();

                var richContent = new
                {
                    type = "cobros_summary",
                    title = "Resumen de Cobros",
                    stats = new[]
                    {
                        new { label = "Total cobros", value = allCobros.Count.ToString(), color = "primary" },
                        new { label = "Pendientes", value = pendientes.Count.ToString(), color = "warn" },
                        new { label = "Vencidos", value = vencidos.Count.ToString(), color = "error" }
                    },
                    recentPendientes = pendientes.Take(5).Select(c => new
                    {
                        numeroPoliza = c.NumeroPoliza,
                        clienteNombreCompleto = c.ClienteNombreCompleto,
                        montoTotal = c.MontoTotal,
                        moneda = c.Moneda,
                        fechaVencimiento = c.FechaVencimiento.ToString("dd-MM-yyyy")
                    }).ToList()
                };

                var text = $"Hay **{pendientes.Count} cobros pendientes** y **{vencidos.Count} vencidos** en el sistema.";
                if (vencidos.Count > 0)
                    text += $"\n\nâš ï¸ Tienes **{vencidos.Count} cobros vencidos** que requieren atenciÃ³n inmediata.";

                return (text, richContent, new List<string>
                {
                    "Ver todos los cobros",
                    "Cobros vencidos",
                    "Registrar pago"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying cobros for chat");
                return ("OcurriÃ³ un error al consultar los cobros. Por favor intenta de nuevo.", null, null);
            }
        }

        private async Task<(string, object?, List<string>?)> BuildReclamosResponseAsync(string input, int userId)
        {
            try
            {
                var total = await _reclamoRepo.GetTotalCountAsync();
                var pendientes = await _reclamoRepo.GetCountByEstadoAsync(Domain.Entities.EstadoReclamo.Pendiente);
                var enProceso = await _reclamoRepo.GetCountByEstadoAsync(Domain.Entities.EstadoReclamo.EnProceso);
                var vencidos = (await _reclamoRepo.GetReclamosVencidosAsync()).Count();
                var montoTotal = await _reclamoRepo.GetMontoTotalReclamadoAsync();

                var richContent = new
                {
                    type = "reclamos_summary",
                    title = "Estado de Reclamos",
                    stats = new[]
                    {
                        new { label = "Total reclamos", value = total.ToString(), color = "primary" },
                        new { label = "Pendientes", value = pendientes.ToString(), color = "warn" },
                        new { label = "En proceso", value = enProceso.ToString(), color = "info" },
                        new { label = "Vencidos", value = vencidos.ToString(), color = "error" }
                    },
                    montoTotal = montoTotal.ToString("C0")
                };

                var text = $"Actualmente hay **{total} reclamos** en el sistema.\n\n" +
                           $"â€¢ **{pendientes}** pendientes de atenciÃ³n\n" +
                           $"â€¢ **{enProceso}** en proceso\n" +
                           $"â€¢ **{vencidos}** vencidos\n\n" +
                           $"El monto total reclamado es **{montoTotal:C0}**.";

                if (vencidos > 0)
                    text += $"\n\nâš ï¸ Hay **{vencidos} reclamos vencidos** que necesitan resoluciÃ³n urgente.";

                return (text, richContent, new List<string>
                {
                    "Ver reclamos pendientes",
                    "Reclamos vencidos",
                    "Crear nuevo reclamo"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying reclamos for chat");
                return ("OcurriÃ³ un error al consultar los reclamos. Por favor intenta de nuevo.", null, null);
            }
        }

        private async Task<(string, object?, List<string>?)> BuildStatsResponseAsync(int userId)
        {
            try
            {
                var totalPolizas = (await _polizaRepo.GetActivasAsync()).Count();
                var totalCobros = (await _cobroRepo.GetAllAsync()).Count();
                var totalReclamos = await _reclamoRepo.GetTotalCountAsync();
                var montoReclamado = await _reclamoRepo.GetMontoTotalReclamadoAsync();
                var montoAprobado = await _reclamoRepo.GetMontoTotalAprobadoAsync();

                var richContent = new
                {
                    type = "stats_dashboard",
                    title = "Resumen General del Sistema",
                    cards = new[]
                    {
                        new { icon = "business", label = "PÃ³lizas Activas", value = totalPolizas.ToString(), color = "purple" },
                        new { icon = "payment", label = "Cobros Totales", value = totalCobros.ToString(), color = "blue" },
                        new { icon = "gavel", label = "Reclamos Totales", value = totalReclamos.ToString(), color = "orange" },
                        new { icon = "attach_money", label = "Monto Reclamado", value = montoReclamado.ToString("C0"), color = "red" }
                    }
                };

                var approvalRate = montoReclamado > 0
                    ? Math.Round((double)montoAprobado / (double)montoReclamado * 100, 1)
                    : 0;

                var text = $"ðŸ“Š **Resumen del Sistema OmnIA**\n\n" +
                           $"â€¢ **{totalPolizas}** pÃ³lizas activas\n" +
                           $"â€¢ **{totalCobros}** cobros registrados\n" +
                           $"â€¢ **{totalReclamos}** reclamos en el sistema\n" +
                           $"â€¢ Monto total reclamado: **{montoReclamado:C0}**\n" +
                           $"â€¢ Tasa de aprobaciÃ³n: **{approvalRate}%**";

                return (text, richContent, new List<string>
                {
                    "Ver pÃ³lizas",
                    "Ver cobros",
                    "Ver reclamos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating stats for chat");
                return ("OcurriÃ³ un error al generar las estadÃ­sticas. Por favor intenta de nuevo.", null, null);
            }
        }

        private static (string, object?, List<string>?) BuildFarewell()
        {
            return ("Â¡Hasta luego! Ha sido un placer ayudarte. No dudes en volver cuando necesites asistencia. ðŸ‘‹",
                    null,
                    new List<string> { "Nueva consulta", "Ver mis pÃ³lizas" });
        }

        private static (string, object?, List<string>?) BuildFallback(string input)
        {
            var response = "No estoy seguro de entender tu consulta. Puedo ayudarte con:\n\n" +
                           "â€¢ **PÃ³lizas** â€” buscar o consultar pÃ³lizas\n" +
                           "â€¢ **Cobros** â€” estados y pagos pendientes\n" +
                           "â€¢ **Reclamos** â€” seguimiento de siniestros\n" +
                           "â€¢ **EstadÃ­sticas** â€” resumen general\n\n" +
                           "Â¿Puedes reformular tu pregunta?";

            return (response, null, new List<string>
            {
                "Ver mis pÃ³lizas",
                "Consultar cobros",
                "Ver reclamos",
                "EstadÃ­sticas generales",
                "Â¿QuÃ© puedes hacer?"
            });
        }

        // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static bool AnyMatch(string text, params string[] keywords)
            => keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

        private static string ExtractSearchTerm(string input, params string[] stopwords)
        {
            var words = Regex.Split(input.ToLowerInvariant().RemoveDiacritics(), @"\s+")
                .Where(w => w.Length > 2 && !stopwords.Contains(w))
                .Where(w => !new[] { "de", "la", "el", "los", "las", "del", "un", "una", "con", "por", "para", "que", "su" }.Contains(w))
                .ToList();

            return string.Join(" ", words).Trim();
        }

        private static string TruncateTitle(string content)
        {
            var clean = content.Trim();
            return clean.Length <= 40 ? clean : clean[..37] + "...";
        }

        private static ChatSessionDto MapSessionToDto(ChatSession s) => new()
        {
            Id = s.Id,
            SessionId = s.SessionId,
            Title = s.Title,
            Status = s.Status.ToString(),
            LastMessage = s.LastMessage,
            MessageCount = s.MessageCount,
            CreatedAt = s.CreatedAt,
            LastActivityAt = s.LastActivityAt ?? s.CreatedAt
        };

        private static ChatMessageDto MapMessageToDto(ChatMessage m)
        {
            object? richContent = null;
            if (!string.IsNullOrEmpty(m.RichContent))
            {
                try { richContent = JsonSerializer.Deserialize<object>(m.RichContent, _jsonOpts); }
                catch { /* ignore malformed JSON */ }
            }

            List<string>? quickReplies = null;
            if (!string.IsNullOrEmpty(m.QuickReplies))
            {
                try { quickReplies = JsonSerializer.Deserialize<List<string>>(m.QuickReplies, _jsonOpts); }
                catch { /* ignore malformed JSON */ }
            }

            return new ChatMessageDto
            {
                Id = m.Id,
                ChatSessionId = m.ChatSessionId,
                Content = m.Content,
                MessageType = m.MessageType.ToString(),
                Status = m.Status.ToString(),
                RichContent = richContent,
                QuickReplies = quickReplies,
                ReactionScore = m.ReactionScore,
                IsRead = m.IsRead,
                ProcessingTimeMs = m.ProcessingTimeMs,
                CreatedAt = m.CreatedAt
            };
        }
    }

    // â”€â”€ Extension helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    internal static class StringExtensions
    {
        private static readonly (string, string)[] _diacritics =
        {
            ("Ã¡Ã Ã¤Ã¢", "a"), ("Ã©Ã¨Ã«Ãª", "e"), ("Ã­Ã¬Ã¯Ã®", "i"), ("Ã³Ã²Ã¶Ã´", "o"), ("ÃºÃ¹Ã¼Ã»", "u"),
            ("ÃÃ€Ã„Ã‚", "A"), ("Ã‰ÃˆÃ‹ÃŠ", "E"), ("ÃÃŒÃÃŽ", "I"), ("Ã“Ã’Ã–Ã”", "O"), ("ÃšÃ™ÃœÃ›", "U"),
            ("Ã±", "n"), ("Ã‘", "N")
        };

        public static string RemoveDiacritics(this string text)
        {
            foreach (var (chars, replacement) in _diacritics)
                foreach (var c in chars)
                    text = text.Replace(c.ToString(), replacement);
            return text;
        }
    }
}

