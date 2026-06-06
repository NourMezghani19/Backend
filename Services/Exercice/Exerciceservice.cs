using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Exercices;
using backend.Models;

namespace backend.Services;

public class ExerciceService(AppDbContext db)
{
    // ─── Mapper ──────────────────────────────────────────────────────────────
    private static ExerciceResponseDto ToDto(Exercice e) => new(
        e.Id,
        e.Nom,
        e.Repetitions,
        e.Series,
        e.Mode.ToString(),
        e.Description,
        e.Actif,
        e.CreatedAt,
        e.ExpiresAt,
        e.MembreId
    );

    // ─── Nettoyage auto des exercices expirés ─────────────────────────────────
    private async Task PurgerExpires() =>
        await db.Exercices
            .Where(e => e.ExpiresAt.HasValue && e.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync();

    // ─── GET ALL par membre ───────────────────────────────────────────────────
    public async Task<List<ExerciceResponseDto>> GetAllByMembre(int membreId)
    {
        await PurgerExpires();
        return await db.Exercices
            .Where(e => e.MembreId == membreId && e.Actif)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => ToDto(e))
            .ToListAsync();
    }

    // ─── GET BY ID (avec vérification ownership) ──────────────────────────────
    public async Task<ExerciceResponseDto> GetById(int id, int membreId)
    {
        var e = await db.Exercices.FindAsync(id)
            ?? throw new KeyNotFoundException("Exercice introuvable.");

        if (e.MembreId != membreId)
            throw new UnauthorizedAccessException("Accès refusé.");

        return ToDto(e);
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────
    public async Task<ExerciceResponseDto> Create(CreateExerciceDto dto, int membreId)
    {
        var e = new Exercice
        {
            Nom = dto.Nom,
            Repetitions = dto.Repetitions,
            Series = dto.Series,
            Mode = dto.Mode,
            Description = dto.Description,
            MembreId = membreId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(42) // 6 semaines
        };
        db.Exercices.Add(e);
        await db.SaveChangesAsync();
        return ToDto(e);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────
    public async Task<ExerciceResponseDto> Update(int id, UpdateExerciceDto dto, int membreId)
    {
        var e = await db.Exercices.FindAsync(id)
            ?? throw new KeyNotFoundException("Exercice introuvable.");

        if (e.MembreId != membreId)
            throw new UnauthorizedAccessException("Accès refusé.");

        if (dto.Nom is not null) e.Nom = dto.Nom;
        if (dto.Repetitions is not null) e.Repetitions = dto.Repetitions.Value;
        if (dto.Series is not null) e.Series = dto.Series.Value;
        if (dto.Mode is not null) e.Mode = dto.Mode.Value;
        if (dto.Description is not null) e.Description = dto.Description;
        if (dto.Actif is not null) e.Actif = dto.Actif.Value;

        await db.SaveChangesAsync();
        return ToDto(e);
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────
    public async Task Delete(int id, int membreId)
    {
        var e = await db.Exercices.FindAsync(id)
            ?? throw new KeyNotFoundException("Exercice introuvable.");

        if (e.MembreId != membreId)
            throw new UnauthorizedAccessException("Accès refusé.");

        db.Exercices.Remove(e);
        await db.SaveChangesAsync();
    }

    // ─── GÉNÉRER UN PROGRAMME (texte structuré) ───────────────────────────────
    public async Task<ProgrammeResponseDto> GenererProgramme(int membreId)
    {
        var exercices = await db.Exercices
            .Where(e => e.MembreId == membreId && e.Actif)
            .ToListAsync();

        if (exercices.Count == 0)
            throw new InvalidOperationException("Aucun exercice disponible pour générer un programme.");

        // Répartition intelligente sur 3 jours
        var grouped = exercices
            .Select((e, i) => (exercice: e, jour: i % 3))
            .GroupBy(x => x.jour)
            .ToList();

        string[] noms = ["Lundi", "Mercredi", "Vendredi"];
        string[] focus = ["Force & Hypertrophie", "Endurance & Volume", "Puissance & Finition"];

        var jours = grouped.Select(g => new JourProgrammeDto(
            noms[g.Key],
            focus[g.Key],
            g.Select(x => new ExerciceProgrammeDto(
                x.exercice.Nom,
                x.exercice.Series,
                x.exercice.Repetitions,
                x.exercice.Mode.ToString(),
                x.exercice.Mode == ModeExecution.Dropset
                    ? "Réduire le poids de 20% entre chaque série"
                    : null
            )).ToList()
        )).ToList();

        return new ProgrammeResponseDto(
            "Programme Personnalisé",
            $"Programme généré à partir de vos {exercices.Count} exercices sur 3 séances par semaine.",
            jours
        );
    }
}