using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs.Programmes;
using backend.Models;

namespace backend.Services;

public class ProgrammeService(AppDbContext db)
{
    // ─── Mapper ───────────────────────────────────────────────────────────────
    private static ProgrammeResponseDto ToDto(Programme p) => new(
        p.Id,
        p.Nom,
        p.Description,
        p.CreatedAt,
        p.ExpiresAt,
        p.Actif,
        p.MembreId,
        p.ProgrammeExercices
            .OrderBy(pe => pe.Ordre)
            .Select(pe => new ProgrammeExerciceDto(
                pe.ExerciceId,
                pe.Exercice!.Nom,
                pe.Exercice.Series,
                pe.Exercice.Repetitions,
                pe.Exercice.Mode.ToString(),
                pe.Exercice.Description,
                pe.Ordre
            )).ToList()
    );

    // ─── Purge automatique des programmes expirés ─────────────────────────────
    private async Task PurgerExpires()
    {
        var expires = await db.Programmes
            .Where(p => p.ExpiresAt < DateTime.UtcNow)
            .Include(p => p.ProgrammeExercices)
            .ToListAsync();

        foreach (var prog in expires)
        {
            // Supprimer les exercices liés
            var exerciceIds = prog.ProgrammeExercices.Select(pe => pe.ExerciceId).ToList();
            await db.Exercices
                .Where(e => exerciceIds.Contains(e.Id))
                .ExecuteDeleteAsync();

            db.Programmes.Remove(prog);
        }

        if (expires.Count > 0)
            await db.SaveChangesAsync();
    }

    // ─── GET ALL par membre ───────────────────────────────────────────────────
    public async Task<List<ProgrammeResponseDto>> GetAllByMembre(int membreId)
    {
        await PurgerExpires();
        return await db.Programmes
            .Where(p => p.MembreId == membreId && p.Actif)
            .Include(p => p.ProgrammeExercices)
                .ThenInclude(pe => pe.Exercice)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    // ─── GET BY ID ────────────────────────────────────────────────────────────
    public async Task<ProgrammeResponseDto> GetById(int id, int membreId)
    {
        var p = await db.Programmes
            .Include(p => p.ProgrammeExercices)
                .ThenInclude(pe => pe.Exercice)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Programme introuvable.");

        if (p.MembreId != membreId)
            throw new UnauthorizedAccessException("Accès refusé.");

        return ToDto(p);
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────
    public async Task<ProgrammeResponseDto> Create(CreateProgrammeDto dto, int membreId)
    {
        // 1. Créer le programme
        var programme = new Programme
        {
            Nom = dto.Nom.Trim(),
            Description = dto.Description?.Trim(),
            MembreId = membreId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(35), // 5 semaines
        };
        db.Programmes.Add(programme);
        await db.SaveChangesAsync(); // pour avoir l'Id

        // 2. Créer les exercices et les lier
        int ordre = 0;
        foreach (var exDto in dto.Exercices)
        {
            // Exercice A
            var exA = new Exercice
            {
                Nom = exDto.Nom.Trim(),
                Description = exDto.Description?.Trim(),
                Series = exDto.Series,
                Repetitions = exDto.Repetitions,
                Mode = exDto.Mode,
                MembreId = membreId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(35),
            };
            db.Exercices.Add(exA);
            await db.SaveChangesAsync();

            db.ProgrammeExercices.Add(new ProgrammeExercice
            {
                ProgrammeId = programme.Id,
                ExerciceId = exA.Id,
                Ordre = ordre++
            });

            // Exercice B (Superset uniquement)
            if (exDto.Mode == ModeExecution.Superset && !string.IsNullOrWhiteSpace(exDto.SupersetNom))
            {
                var exB = new Exercice
                {
                    Nom = exDto.SupersetNom.Trim(),
                    Description = exDto.SupersetDescription?.Trim(),
                    Series = exDto.SupersetSeries,
                    Repetitions = exDto.SupersetRepetitions,
                    Mode = ModeExecution.Superset,
                    MembreId = membreId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(35),
                };
                db.Exercices.Add(exB);
                await db.SaveChangesAsync();

                db.ProgrammeExercices.Add(new ProgrammeExercice
                {
                    ProgrammeId = programme.Id,
                    ExerciceId = exB.Id,
                    Ordre = ordre++
                });
            }
        }

        await db.SaveChangesAsync();

        // Recharger avec les includes
        return await GetById(programme.Id, membreId);
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────
    public async Task Delete(int id, int membreId)
    {
        var p = await db.Programmes
            .Include(p => p.ProgrammeExercices)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Programme introuvable.");

        if (p.MembreId != membreId)
            throw new UnauthorizedAccessException("Accès refusé.");

        // Supprimer les exercices liés
        var exerciceIds = p.ProgrammeExercices.Select(pe => pe.ExerciceId).ToList();
        await db.Exercices
            .Where(e => exerciceIds.Contains(e.Id))
            .ExecuteDeleteAsync();

        db.Programmes.Remove(p);
        await db.SaveChangesAsync();
    }
}