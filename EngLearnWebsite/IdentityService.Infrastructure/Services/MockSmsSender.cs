using IdentityService.Domain;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services
{
    public class MockSmsSender : ISmsSender
    {
        private readonly ILogger<MockSmsSender> _logger;

        public MockSmsSender(ILogger<MockSmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string phoneNumber, params string[] args)
        {
            _logger.LogInformation("给{0}发短信, args: {1}", phoneNumber, string.Join(",", args));
            return Task.CompletedTask;
        }
    }
}
