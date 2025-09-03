using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountManagementSystem.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
        private readonly ReCaptchaSettings _recaptchaSettings;
        private readonly HttpClient _httpClient;

        public ReCaptchaService(IOptions<ReCaptchaSettings> recaptchaSettings, HttpClient httpClient)
        {
            _recaptchaSettings = recaptchaSettings.Value;
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse)
        {
            if (string.IsNullOrEmpty(recaptchaResponse))
            {
                return false;
            }

            try
            {
                var parameters = new Dictionary<string, string>
                {
                    {"secret", _recaptchaSettings.SecretKey},
                    {"response", recaptchaResponse}
                };

                var encodedContent = new FormUrlEncodedContent(parameters);
                var response = await _httpClient.PostAsync(_recaptchaSettings.VerifyUrl, encodedContent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"reCAPTCHA verification failed with status: {response.StatusCode}");
                    return false;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var verificationResult = JsonSerializer.Deserialize<ReCaptchaVerificationResponse>(jsonResponse);
                return verificationResult?.Success == true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error during reCAPTCHA verification: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error during reCAPTCHA verification: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during reCAPTCHA verification: {ex.Message}");
                return false;
            }
        }
    }

    public class ReCaptchaVerificationResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("challenge_ts")]
        public DateTime? ChallengeTs { get; set; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}