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
    private static readonly char[] EmailSeparators = { ',', ';' };

    /// <summary>
    /// This unsubscribe footer is needed because the lack of one will give us a higher
    /// spamminess score with some email services and might damage deliverability for
    /// everyone. Our mail is transactional, not commercial, so we do not need to comply
    /// with CAN-SPAM but email services won't know that. Please do not remove or change.
    /// (Note that this doesn't actually comply with CAN-SPAM since it doesn't auto-remove
    /// them. We can't do that since we sent most mail bcc and don't know who they are.)
    /// </summary>
    private const string MailUnsubscribeFooter = @"

--
This mail was sent by msph@puzzlehunt.org, Redmond WA 98005 because you registered
for a hunt and entered this email address on your profile or team page.
To unsubscribe: go to https://puzzlehunt.azurewebsites.net/Identity/Account/Manage and login.
Then remove your email address from your profile or delete your account. If you have entered
your email as the contact address for a team, then you also need to remove it on the team page.
";


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
            Enabled = PublicSecret != null && PrivateSecret != null;
            IsDev = isDev;
        }
        catch
        {
            Enabled = false;
        }
    }

    /// <summary>
    /// Send plaintext mail to a single person.
    /// </summary>
    public void SendPlaintextOneAddress(string recipient, string subject, string body)
    {
        SendInternal(new List<string> { recipient }, false, subject, body, false);
    }

    /// <summary>
    /// Send html mail to a single person.
    /// </summary>
    public void SendHtmlOneAddress(string recipient, string subject, string body)
    {
        SendInternal(new List<string> { recipient }, false, subject, body, true);
    }

    /// <summary>
    /// Send plaintext mail to a list of people using BCC.
    /// This avoids exposing people's email addresses and avoids the reply-all problem.
    /// Still does BCC even if there's just one person to avoid exposing the fact that
    /// there is just one person.
    /// </summary>
    public void SendPlaintextBcc(IEnumerable<string> recipients, string subject, string body)
    {
        SendInternal(recipients, true, subject, body, false);
    }

    /// <summary>
    /// Send html mail to a list of people using BCC.
    /// This avoids exposing people's email addresses and avoids the reply-all problem.
    /// Still does BCC even if there's just one person to avoid exposing the fact that
    /// there is just one person.
    /// </summary>
    public void SendHtmlBcc(IEnumerable<string> recipients, string subject, string body)
    {
        SendInternal(recipients, true, subject, body, true);
    }

    /// <summary>
    /// Send plaintext mail to the members of a team, without using BCC.
    /// Since team members already see each other's email addresses, it's OK to let them
    /// see it here, and this avoids the everyone-replies-separately problem.
    /// </summary>
    public void SendPlaintextWithoutBcc(IEnumerable<string> recipients, string subject, string body)
    {
        SendInternal(recipients, false, subject, body, false);
    }

    /// <summary>
    /// Send plaintext mail to the members of a team, without using BCC.
    /// Since team members already see each other's email addresses, it's OK to let them
    /// see it here, and this avoids the everyone-replies-separately problem.
    /// </summary>
    public void SendHtmlWithoutBcc(IEnumerable<string> recipients, string subject, string body)
    {
        SendInternal(recipients, false, subject, body, true);
    }

    private void SendInternal(IEnumerable<string> recipients, bool bcc, string subject, string body, bool isHtml)
    {
        // See https://www.mailjet.com/docs/code/c/c-sharp for sample code from Mailjet.

        MailMessage msg = new MailMessage();
        if (isHtml)
        {
            msg.IsBodyHtml = true;
            body += MailUnsubscribeFooter.Replace("\n", "<br />\n");
        }
        else
        {
            body += MailUnsubscribeFooter;
        }

        if (!Enabled)
        {
            if (!IsDev)
            {
                throw new InvalidOperationException("Mail was not configured.");
            }
            WriteMailToConsole(recipients, bcc, subject, body);
            return;
        }

        msg.From = new MailAddress(FromAddress);
        msg.Subject = subject;
        msg.Body = body;

        if (bcc)
        {
            msg.To.Add(new MailAddress(ToAddressForBCC));
            AddRecipients(msg.Bcc, recipients);
        }
        else
        {
            AddRecipients(msg.To, recipients);
        }

        SmtpClient client = new SmtpClient("in.mailjet.com", 587);
        // Unlike port 25 which is for general SMTP, port 587 is for
        // MSA (Message Submission Agent) use and is authenticated.
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.EnableSsl = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(PublicSecret, PrivateSecret);
        client.SendMailAsync(msg);
    }

    /// <summary>
    /// Add a list of recipients to an address collection.
    /// Each recipient may be a comma- or semicolon-separated list of addresses.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="recipients"></param>
    private static void AddRecipients(MailAddressCollection collection, IEnumerable<string> recipients)
    {
        foreach (string recipient in recipients)
        {
            string[] addresses = recipient.Split(EmailSeparators);
            foreach (string address in addresses)
            {
                try
                {
                    collection.Add(new MailAddress(address.Trim()));
                }
                catch (FormatException)
                {
                    ;  // ignore
                }
            }
        }
    }

    private void WriteMailToConsole(IEnumerable<string> recipients, bool bcc, string subject, string body)
    {
        // In dev environment, log the email instead of sending it if not configured.
        Debug.WriteLine("----------------------------------------");
        foreach (string recipient in recipients)
        {
            Debug.WriteLine((bcc ? "Bcc: " : "To: ") + recipient);
        }
        Debug.WriteLine("Subject: " + subject);
        Debug.WriteLine("Body: " + body);
        Debug.WriteLine("----------------------------------------");
    }
}
