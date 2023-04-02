using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Senders
{
    /// <summary>
    ///  Класс для отправки email
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private string _senderEmail;
        public string _password;

        public string SenderEmail { get => _senderEmail; set => _senderEmail = value; }
        public string Password { get => _password; set => _password = value; }


        /// <summary>
        /// Отправляет email при помощи SMTPClient
        /// </summary>
        /// <param name="email">Адрес получателя</param>
        /// <param name="subject">Тема сообщения</param>
        /// <param name="message">Содержимое сообщения</param>
        public Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(SenderEmail, Password)
                };


                return client.SendMailAsync(
                    new MailMessage(
                        from: SenderEmail,
                        to: email,
                        subject: subject,
                        body: message
                        )
                    );
            }
            catch(ArgumentException ex )
            {
                throw new ArgumentException();
            }
            catch(SmtpException ex)
            {
                throw new SmtpException();
            }
        }
    }
}
