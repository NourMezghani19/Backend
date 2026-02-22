using System.Net;
using System.Net.Mail;

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
            try
            {
                var smtpClient = new SmtpClient(config["Email:SmtpHost"]!)
                {
                    Port = int.Parse(config["Email:SmtpPort"] ?? "587"),
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        config["Email:From"],
                        config["Email:Password"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        config["Email:From"]!,
                        "PFA Project — Équipe Admin"),
                    Subject = "🎉 Bienvenue sur PFA — Votre compte a été créé",
                    IsBodyHtml = true,
                    Body = BuildEmailBody(nomComplet, motDePasseTemp, destinataire)
                };
                mailMessage.To.Add(new MailAddress(destinataire, nomComplet));

                await smtpClient.SendMailAsync(mailMessage);
                logger.LogInformation($"Email inscription envoyé à {destinataire}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Erreur envoi email à {destinataire}: {ex.Message}");
                return false;
            }
        }

        private static string BuildEmailBody(
            string nomComplet,
            string motDePasseTemp,
            string email)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{ font-family: Arial, sans-serif; background: #f5f5f5; }}
                .container {{ max-width: 550px; margin: 30px auto;
                             background: white; border-radius: 12px;
                             padding: 32px; box-shadow: 0 2px 12px rgba(0,0,0,.1); }}
                .header {{ background: #0284c7; border-radius: 8px;
                           padding: 20px; text-align: center; margin-bottom: 24px; }}
                .header h1 {{ color: white; font-size: 1.4rem; margin: 0; }}
                .credentials {{ background: #f0f9ff; border: 1px solid #bae6fd;
                                border-radius: 8px; padding: 16px; margin: 20px 0; }}
                .cred-label {{ font-size: 0.8rem; color: #64748b; margin-bottom: 4px; }}
                .cred-value {{ font-size: 1rem; font-weight: bold; color: #0f172a;
                               font-family: monospace; background: #e0f2fe;
                               padding: 6px 12px; border-radius: 6px; display: inline-block; }}
                .warning {{ background: #fef3c7; border: 1px solid #fcd34d;
                            border-radius: 8px; padding: 12px; margin-top: 16px;
                            font-size: 0.85rem; color: #92400e; }}
                .footer {{ text-align: center; margin-top: 24px;
                           color: #94a3b8; font-size: 0.78rem; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎉 Bienvenue, {nomComplet} !</h1>
                </div>
                <p>Votre compte membre sur la plateforme PFA a été créé par un administrateur.</p>
                <p>Voici vos identifiants de connexion :</p>

                <div class='credentials'>
                    <div class='cred-label'>📧 Email (identifiant)</div>
                    <div class='cred-value'>{email}</div>
                    <br><br>
                    <div class='cred-label'>🔑 Mot de passe temporaire</div>
                    <div class='cred-value'>{motDePasseTemp}</div>
                </div>

                <div class='warning'>
                    ⚠️ <strong>Important :</strong> Veuillez changer votre mot de passe
                    dès votre première connexion pour sécuriser votre compte.
                </div>

                <div class='footer'>
                    <p>PFA Project — Ne pas répondre à cet email</p>
                </div>
            </div>
        </body>
        </html>";
        }
    }
}
