using Bc_exercise_and_healthy_nutrition.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Bc_exercise_and_healthy_nutrition.Services
{
    public class TurnstileVerificationService
    {
        private readonly HttpClient _httpClient;
        private readonly TurnstileSettings _settings;

        public TurnstileVerificationService(HttpClient httpClient, IOptions<TurnstileSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _settings.SecretKey },
                { "response", token }
            });

            var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<TurnstileVerificationResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Success == true;
        }

        private class TurnstileVerificationResponse
        {
            public bool Success { get; set; }
        }
    }
}