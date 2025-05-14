using Cutify.Data;
using Cutify.Domain.Entities;
using Cutify.Domain.Enums;
using Cutify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cutify.Services.Implementations;

public class LogService : ILogService
{
    private readonly CutifyDbContext _context;
    private const int MaxMessageLength = 4000;

    public LogService(CutifyDbContext context)
    {
        _context = context;
    }

    private string TruncateMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return message;
        return message.Length <= MaxMessageLength ? message : message.Substring(0, MaxMessageLength - 3) + "...";
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? userName = null, string? requestPath = null, string? requestMethod = null)
    {
        var fullMessage = exception != null ? $"{message}: {exception.Message}" : message;
        var log = new Log
        {
            Message = TruncateMessage(fullMessage),
            Level = LogLevel.Error.ToString(),
            Timestamp = DateTime.UtcNow,
            Source = source,
            UserName = userName,
            RequestPath = requestPath,
            RequestMethod = requestMethod,
            Exception = exception?.ToString()
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogInfoAsync(string message, string? source = null, string? userName = null, string? requestPath = null, string? requestMethod = null)
    {
        var log = new Log
        {
            Message = TruncateMessage(message),
            Level = LogLevel.Info.ToString(),
            Timestamp = DateTime.UtcNow,
            Source = source,
            UserName = userName,
            RequestPath = requestPath,
            RequestMethod = requestMethod
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Log>> GetLogsAsync(DateTime startDate, DateTime endDate, string? level = null)
    {
        var query = _context.Logs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(l => l.Level == level);
        }

        return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
    }

    public async Task LogAsync(string message, LogLevel level = LogLevel.Info)
    {
        var log = new Log
        {
            Message = TruncateMessage(message),
            Level = level.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Log>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, LogLevel? level = null)
    {
        var query = _context.Logs.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= endDate.Value);
        }

        if (level.HasValue)
        {
            query = query.Where(l => l.Level == level.Value.ToString());
        }

        return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
    }
} 