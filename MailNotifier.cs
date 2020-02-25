using System;
using System.Net;
using System.Net.Mail;

namespace SteamGiveawaysAutoBot
{
    public sealed class MailNotifier
    {
        public static void SendMail(string profile, int giftsCount)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential("vasili.lefko@gmail.com", "stayback");

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("vasili.lefko@gmail.com", $"SteamGiveawaysAutoBot");
                mail.Subject = $"SGAB - Unclaimed gifts for {profile}";
                mail.Body = $"You have {giftsCount} unclaimed gifts for the {profile} profile!";
                mail.To.Add("Hori873Games@GMail.com");

                smtp.Send(mail);
            }
        }
    }
}
