using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using MudBlazorEntra.Options;

namespace MudBlazorEntra.Services;

public class SmtpRegistrationEmailSender(IOptions<EmailDeliveryOptions> options, IHostEnvironment hostEnvironment) : IRegistrationEmailSender
{
    private readonly EmailDeliveryOptions _settings = options.Value;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;

    public async Task SendTemporaryPasswordAsync(string emailAddress, string displayName, string temporaryPassword, string loginUrl, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(emailAddress));
        message.Subject = "Your temporary sign-in details";
        message.Body = new TextPart("plain")
        {
            Text = BuildBody(displayName, temporaryPassword, loginUrl)
        };

        using var smtpClient = new SmtpClient();
        if (_hostEnvironment.IsDevelopment())
        {
            smtpClient.CheckCertificateRevocation = false;
        }

        try
        {
            await smtpClient.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                ParseSecureSocketOption(_settings.SecureSocketOption),
                cancellationToken);

            await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await smtpClient.SendAsync(message, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"SMTP send failed for {_settings.SmtpHost}:{_settings.SmtpPort} using {_settings.SecureSocketOption}. {BuildExceptionMessage(ex)}",
                ex);
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            throw new InvalidOperationException("Email delivery configuration is incomplete. Set SMTP host, credentials, and from address.");
        }
    }

    private static SecureSocketOptions ParseSecureSocketOption(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "none" => SecureSocketOptions.None,
            "auto" => SecureSocketOptions.Auto,
            "ssl" => SecureSocketOptions.SslOnConnect,
            "sslonconnect" => SecureSocketOptions.SslOnConnect,
            "starttls" => SecureSocketOptions.StartTls,
            "starttlswhenavailable" => SecureSocketOptions.StartTlsWhenAvailable,
            _ => throw new InvalidOperationException("EmailDelivery:SecureSocketOption must be one of None, Auto, SslOnConnect, StartTls, or StartTlsWhenAvailable.")
        };
    }

    private static string BuildBody(string displayName, string temporaryPassword, string loginUrl)
    {
        return
            $"Hello {displayName},{Environment.NewLine}{Environment.NewLine}" +
            $"Your account has been created.{Environment.NewLine}{Environment.NewLine}" +
            $"Temporary password: {temporaryPassword}{Environment.NewLine}{Environment.NewLine}" +
            $"Sign in here: {loginUrl}{Environment.NewLine}{Environment.NewLine}" +
            "You will be asked to change this password after you sign in.";
    }

    private static string BuildExceptionMessage(Exception exception)
    {
        var messages = new List<string>();
        Exception? current = exception;

        while (current is not null)
        {
            messages.Add(current.Message);
            current = current.InnerException;
        }

        return string.Join(" | ", messages);
    }
}
