using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RestaurantApp.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

            client.UseDefaultCredentials = false;

            //what credientials to use
            client.Credentials = new NetworkCredential("chowdhuryadnan956@gmail.com", "9566626231");

                    //enable ssl
            client.EnableSsl = true;

                    //composing email message
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("chowdhuryadnan956@gmail.com");  //sender
            mailMessage.To.Add(email);  //receiver

            mailMessage.Body = message;                      //message

            mailMessage.IsBodyHtml = true;                   //why true?otherwise if we send link it will display anchor tags <a></a>
                                                             //renders the html

            mailMessage.Subject = subject;                   //subject

            client.Send(mailMessage);                        //sending the composed mail


            return Task.CompletedTask;
        }
    }
}
