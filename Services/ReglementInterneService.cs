// Services/ReglementInterneService.cs
using backend.Data;
using backend.DTOs.ReglementInterne;
using backend.Models;
using Microsoft.EntityFrameworkCore;

public class ReglementInterneService(AppDbContext db)
{
    // GET — Récupérer le règlement actuel
    public async Task<ReglementInterneResponseDto> GetActuel()
    {
        var r = await db.ReglementsInternes
            .OrderByDescending(r => r.DateModification)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Aucun règlement trouvé.");

        return new ReglementInterneResponseDto(
            r.Id, r.Titre, r.Contenu, r.DateModification);
    }

    // PUT — SuperAdmin modifie le règlement
    public async Task<ReglementInterneResponseDto> Update(
        UpdateReglementInterneDto dto, int superAdminId)
    {
        var r = await db.ReglementsInternes
            .OrderByDescending(r => r.DateModification)
            .FirstOrDefaultAsync();

        if (r == null)
        {
            // Premier enregistrement
            r = new ReglementInterne();
            db.ReglementsInternes.Add(r);
        }

        r.Titre = dto.Titre;
        r.Contenu = dto.Contenu;
        r.DateModification = DateTime.UtcNow;
        r.ModifieParId = superAdminId;

        await db.SaveChangesAsync();
        return new ReglementInterneResponseDto(
            r.Id, r.Titre, r.Contenu, r.DateModification);
    }

    // Envoyer notification règlement à UN membre
    public async Task EnvoyerAMembre(int membreId)
    {
        var r = await db.ReglementsInternes
            .OrderByDescending(x => x.DateModification)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Règlement introuvable.");

        var notif = new Notification
        {
            UtilisateurId = membreId,
            Titre = "📋 " + r.Titre,
            Contenu = r.Contenu,
            Type = "ReglementInterne",
            DateEnvoi = DateTime.UtcNow,
            Lue = false
        };

        db.Notifications.Add(notif);
        await db.SaveChangesAsync();
    }

    // Envoyer rappel à TOUS les membres
    public async Task EnvoyerRappelATous()
    {
        var r = await db.ReglementsInternes
            .OrderByDescending(x => x.DateModification)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Règlement introuvable.");

        var membres = await db.Utilisateurs
            .Where(u => u.Role == "Membre")
            .Select(u => u.Id)
            .ToListAsync();

        var notifications = membres.Select(id => new Notification
        {
            UtilisateurId = id,
            Titre = "🔔 Rappel — " + r.Titre,
            Contenu = r.Contenu,
            Type = "ReglementInterne",
            DateEnvoi = DateTime.UtcNow,
            Lue = false
        }).ToList();

        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();
    }
}