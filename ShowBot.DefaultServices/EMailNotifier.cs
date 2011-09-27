using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShowBot.Services;
using ShowBot.Model;
using System.Net.Mail;
using System.Net;

namespace ShowBot.DefaultServices {
	public class EMailNotifier : INotifier {

		dynamic settings;
		private readonly string smtpServer;
		private readonly string smtpUser;
		private readonly string smtpPassword;
		private readonly string from;
		private readonly string to;
		private readonly string subject;

		public EMailNotifier(IConfig config) {
			settings = config.GetConfigurationSettings();
			smtpServer = settings.SMTPServer;
			smtpUser = settings.SMTPUser;
			smtpPassword = settings.SMTPPassword;
			from = settings.From;
			to = settings.To;
			subject = settings.Subject;
		}

		public void Notify(string message) {
			var mailMessage = new MailMessage(from, to, subject, message);
			var smtp = new SmtpClient(smtpServer);
			smtp.Credentials = new NetworkCredential(smtpUser, smtpPassword);
			smtp.Send(mailMessage);
		}
	}
}
