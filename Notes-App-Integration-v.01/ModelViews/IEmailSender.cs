namespace Notes_App_Integration_v._01.ModelViews
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
