using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace ELearning.Models
{
    public class Email
    {


        public string To { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }


        public static string sendMail(Email email)
        {
            try
            {
                string MailSender = "mounafathallah17@gmail.com";
                string MailPw = "Mouna1503";

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 100000;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(MailSender, MailPw);

                MailMessage mailMessage = new MailMessage(MailSender, email.To, email.Subject, email.Message);
                // mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = System.Text.UTF8Encoding.UTF8;

                smtpClient.Send(mailMessage);

                return "true";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}