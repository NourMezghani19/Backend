using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;

namespace backend.Services.Admin
{
    public class EmailService
    {
        private readonly IConfiguration config;
        private readonly ILogger<EmailService> logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async Task<bool> EnvoyerEmailInscription(
            string destinataire,
            string nomComplet,
            string motDePasseTemp)
        {
            var host = config["Email:SmtpHost"]!;
            var from = config["Email:From"]!;
            var password = config["Email:Password"]!;
            var display = config["Email:DisplayName"] ?? "QLF Gym";

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(display, from));
                message.To.Add(new MailboxAddress(nomComplet, destinataire));
                message.Subject = "Votre compte QLF Gym";

                var builder = new BodyBuilder();

                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "1.png"
                );

                var logo = builder.LinkedResources.Add(imagePath);
                logo.ContentId = MimeUtils.GenerateMessageId();

                builder.HtmlBody = BuildHtml(
                    nomComplet,
                    motDePasseTemp,
                    destinataire,
                    logo.ContentId
                );

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(from, password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur envoi email");
                return false;
            }
        }

        private static string BuildHtml(string nom, string mdp, string email, string logoCid) => $@"
            <!DOCTYPE html>
            <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                </head>
            <body style='margin:0;padding:0;background:#ffffff;font-family:Arial,sans-serif;'>
            <table width='100%' cellpadding='0' cellspacing='0' style='background:#ffffff;padding:40px 0;'>
            <tr>
                <td align='center'>
                    <table width='600' cellpadding='0' cellspacing='0' style='background:#000000;border-radius:8px;'>

            <!-- Header -->
            <tr>
                <td align='center' style='padding:40px 20px;'>

            <!-- logo de QLF gym -->
            <table align='center' cellpadding='0' cellspacing='0'>
            <tr>
                <td 
                       align='center' 
                       width='160'
                       height='160'
                       style='border-radius:50%;
                          border:5px solid #dc2626;
                          background:#000000;'>
                <img src='cid:{logoCid}'
                     width='130'
                     height='130'
                     style='display:block;border-radius:50%;margin:0 auto;'>
                </td></tr></table>
            <h1 
                style='color:#ffffff;
                   margin-top:20px;
                   font-size:26px;
                   letter-spacing:2px;'>
                       Bienvenue <span style='color:#dc2626;'>{nom}</span>
            </h1></td></tr>

            <!-- Body -->
            <tr>
            <td 
                style='padding:30px 40px;
                   color:#ffffff;
                   font-size:15px;
                   line-height:1.6;'>
            <p>Votre compte a été créé avec succès sur la plateforme QLF Gym.</p>

            <!-- La partie de l'email -->
            <p style='margin-top:25px;'>
            <strong style='color:#dc2626;'>Email :</strong><br>
            <span
                style='background:#ffffff;
                   color:#000000;
                   padding:8px 14px;
                   border-radius:4px;
                   display:inline-block;
                   margin-top:6px;'>
                   {email}
            </span></p>

            <!-- Mot de passe -->
            <p style='margin-top:20px;'>
            <strong style='color:#dc2626;'>Mot de passe temporaire :</strong><br>
            <span style='background:#dc2626;color:#ffffff;padding:10px 18px;border-radius:4px;display:inline-block;margin-top:6px;font-weight:bold;letter-spacing:2px;'>
            {mdp}
            </span></p></td></tr>

            <!-- La partie footer -->
            <tr>
            <td align='center' style='padding:20px;color:#ffffff;font-size:13px;border-top:2px solid #dc2626;'>
            QLF Gym - Sfax
            </td></tr></table></td></tr></table>
            </body></html>";
    }
}