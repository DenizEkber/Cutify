using Cutify.Domain.Entities;
using Cutify.Domain.Enums;

namespace Cutify.Services.Interfaces;

public interface ILogService
{
    Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? userName = null, string? requestPath = null, string? requestMethod = null);
    Task LogInfoAsync(string message, string? source = null, string? userName = null, string? requestPath = null, string? requestMethod = null);
    Task<IEnumerable<Log>> GetLogsAsync(DateTime startDate, DateTime endDate, string? level = null);
    Task LogAsync(string message, LogLevel level = LogLevel.Info);
    Task<IEnumerable<Log>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, LogLevel? level = null);
} 