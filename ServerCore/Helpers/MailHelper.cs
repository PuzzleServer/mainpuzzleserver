using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Helper class for sending mail with Mailjet.
/// </summary>
public class MailHelper
{
    private string PublicSecret;
    private string PrivateSecret;

    public bool Enabled { get; private set; }
    private bool IsDev;

    private const string FromAddress = "msph@puzzlehunt.org";
    private const string ToAddressForBCC = "msph-bcc@puzzlehunt.org";

    public static MailHelper Singleton { get; private set; }

    public static void Initialize(IConfiguration configuration, bool isDev)
    {
        Singleton = new MailHelper(configuration, isDev);
    }

    private MailHelper(IConfiguration configuration, bool isDev)
    {
        try
        {
            PublicSecret = configuration["Mailjet-Public"];
            PrivateSecret = configuration["Mailjet-Private"];
            Enabled = true;
            IsDev = isDev;
        }
        catch
        {
            Enabled = false;
        }
        Enabled = false;
    }

    public void Send(string recipient, string subject, string body)
    {
        SendInternal(new List<string> { recipient }, false, subject, body);
    }

    public void Send(IEnumerable<string> recipients, string subject, string body)
    {
        // To avoid reply-all problems, any mail sent to a list is always sent BCC.
        // (We do this even if the list is just one person to avoid exposing whether or
        SendInternal(recipients, true, subject, body);
    }

    private void SendInternal(IEnumerable<string> recipients, bool bcc, string subject, string body)
    {
        // See https://www.mailjet.com/docs/code/c/c-sharp for sample code from Mailjet.
        if (!Enabled)
        {
            if (!IsDev)
            {
                throw new InvalidOperationException("Mail was not configured.");
            }
            WriteMailToConsole(recipients, subject, body);
            return;
        }
        MailMessage msg = new MailMessage();

        msg.From = new MailAddress(FromAddress);

        if (bcc)
        {
            msg.To.Add(new MailAddress(ToAddressForBCC));
            foreach (string recipient in recipients)
            {
                try
                {
                    msg.Bcc.Add(new MailAddress(recipient));
                }
                catch (FormatException)
                {
                    ;  // ignore
                }
            }
        }
        else
        {
            foreach (string recipient in recipients)
            {
                try
                {
                    msg.To.Add(new MailAddress(recipient));
                }
                catch (FormatException)
                {
                    ;  // ignore
                }
            }
        }

        msg.Subject = subject;
        msg.Body = body;

        SmtpClient client = new SmtpClient("in.mailjet.com", 587);
        // Unlike port 25 which is for general SMTP, port 587 is for
        // MSA (Message Submission Agent) use and is authenticated.
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.EnableSsl = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(PublicSecret, PrivateSecret);
        client.SendMailAsync(msg);
    }

    private void WriteMailToConsole(IEnumerable<string> recipients, string subject, string body)
    {
        // In dev environment, log the email instead of sending it if not configured.
        Debug.WriteLine("----------------------------------------");
        foreach (string recipient in recipients)
        {
            Console.WriteLine("To: " + recipient);
        }
        Debug.WriteLine("Subject: " + subject);
        Debug.WriteLine("Body: " + body);
        Debug.WriteLine("----------------------------------------");
    }
}
