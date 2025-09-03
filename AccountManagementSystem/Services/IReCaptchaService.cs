namespace AccountManagementSystem.Services
{
    public interface IReCaptchaService
    {
        Task<bool> VerifyRecaptchaAsync(string recaptchaResponse);
    }
}

