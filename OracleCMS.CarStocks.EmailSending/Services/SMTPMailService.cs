using OracleCMS.Common.Services.Shared.Interfaces;
using OracleCMS.Common.Services.Shared.Models.Mail;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
namespace OracleCMS.CarStocks.EmailSending.Services
{
    public class SMTPEmailService(IOptions<MailSettings> settings, IConfiguration Configuration) : IMailService
	{
		private readonly MailSettings _settings = settings.Value;
		public async Task SendAsync(MailRequest request, CancellationToken cancellationToken = default)
		{  
			var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(_settings.TimeOutMinute));
			if (Configuration.GetValue<bool>("IsTest"))
			{
				if (!string.IsNullOrEmpty(_settings.TestEmailRecipient))
				{
					string[] testEmails = _settings.TestEmailRecipient!.Split(',');
					if (testEmails.Length > 0)
					{
						request.To = testEmails[0];  // Set the first email as the 'To' recipient
						if (testEmails.Length > 1)
						{
							// If there are more than one test emails, add the rest to 'Cc'
							request.Ccs = testEmails.Skip(1).ToArray();
						}
						else
						{
							request.Ccs = null;
						}
					}
					else
					{
						request.To = "";  // Handle the case where the split result is empty
						request.Ccs = null;
					}
				}
				else
				{
					request.To = "";
					request.Ccs = null;
				}
				request.Bcc = null;
				request.Subject += " - Test";
			}
			var decryptedPassword = Common.Utility.Helpers.EncryptionHelper.DecryptPassword(_settings.SMTPEmailPassword!, _settings.SMTPEmail!);
			var mailMessage = new MailMessage(_settings.SMTPEmail!, request.To, request.Subject, request.Body)
			{
				IsBodyHtml = true,
				BodyEncoding = System.Text.Encoding.UTF8,
				SubjectEncoding = System.Text.Encoding.UTF8
			};
			if (request.Attachments != null && request.Attachments.Count > 0)
			{
				foreach (var item in request.Attachments)
				{
					if (File.Exists(item))
					{
						mailMessage.Attachments.Add(new Attachment(item));
					}
				}
			}
			// Add Cc recipients
			if (request.Ccs != null)
			{
				foreach (string cc in request.Ccs)
				{
					mailMessage.CC.Add(cc);
				}
			}
			// Add Bcc recipients
			if (request.Bcc != null)
			{
				foreach (string bcc in request.Bcc)
				{
					mailMessage.Bcc.Add(bcc);
				}
			}
			try
			{
				using var client = new SmtpClient();
				client.Host = _settings.SMTPHost!;
				client.Port = _settings.SMTPPort;
				client.DeliveryMethod = SmtpDeliveryMethod.Network;
				client.UseDefaultCredentials = false;
				if (!string.IsNullOrEmpty(_settings.SMTPEmailPassword))
				{
					client.EnableSsl = true;
					client.Credentials = new System.Net.NetworkCredential(_settings.SMTPEmail!, decryptedPassword);
				}
				else
				{
					client.EnableSsl = false;
				}
				await client.SendMailAsync(mailMessage, timeoutCancellationTokenSource.Token);
			}
			catch (OperationCanceledException)
			{
				if (timeoutCancellationTokenSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
				{
					throw new TimeoutException($"The email sending operation timed out after {_settings.TimeOutMinute} minute(s).");
				}
				throw; // If the cancellationToken parameter was cancelled, rethrow the original OperationCanceledException
			}
		}  		
	}
}
