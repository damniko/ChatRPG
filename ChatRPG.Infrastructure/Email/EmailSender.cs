using ChatRPG.Application.Abstractions;

namespace ChatRPG.Infrastructure.Email;

public class EmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string html, CancellationToken ct = default) => throw new NotImplementedException();
}
