using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Robot.Flashscore
{
    public class Emails
    {
        public void SendEmail(string to, string subject, string body)
        {
            SmtpClient smtp = new SmtpClient("127.0.0.1", 1025);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = false;
            smtp.UseDefaultCredentials = true;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("orkiestrator@local.com");
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;

            smtp.Send(mail);
        }
    }
}
