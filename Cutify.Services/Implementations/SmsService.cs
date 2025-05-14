using Cutify.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutify.Services.Implementations
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            // Burada real SMS göndərmə servisinə API inteqrasiyası edilə bilər
            _logger.LogInformation($"[MOCK SMS] To: {phoneNumber}, Code: {code}");
            return Task.CompletedTask;
        }
    }
}
