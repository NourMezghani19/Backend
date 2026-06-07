using backend.Data;
using backend.DTOs.Exercices;
using backend.Models;
using Microsoft.EntityFrameworkCore;

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
                pe.Ordre,
                pe.SupersetNom,
                pe.SupersetDescription,
                pe.SupersetSeries,
                pe.SupersetRepetitions
            )).ToList()
    );

    // ─── Helper : créer et lier les exercices ─────────────────────────────────
    private async Task CreerExercices(
        int programmeId,
        int membreId,
        List<CreateExerciceProgrammeDto> exercices)
    {
        int ordre = 0;
        foreach (var exDto in exercices)
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

            var pe = new ProgrammeExercice
            {
                ProgrammeId = programmeId,
                ExerciceId = exA.Id,
                Ordre = ordre++,
            };

            // Stocker les infos superset sur le lien de jointure
            if (exDto.Mode == ModeExecution.Superset
                && !string.IsNullOrWhiteSpace(exDto.SupersetNom))
            {
                pe.SupersetNom = exDto.SupersetNom.Trim();
                pe.SupersetDescription = exDto.SupersetDescription?.Trim();
                pe.SupersetSeries = exDto.SupersetSeries;
                pe.SupersetRepetitions = exDto.SupersetRepetitions;
            }

            db.ProgrammeExercices.Add(pe);
        }

        await db.SaveChangesAsync();
    }

    // ─── Purge automatique des programmes expirés ─────────────────────────────
    private async Task PurgerExpires()
    {
        var expires = await db.Programmes
            .Where(p => p.ExpiresAt < DateTime.UtcNow)
            .Include(p => p.ProgrammeExercices)
            .ToListAsync();

        foreach (var prog in expires)
        {
            db.ProgrammeExercices.RemoveRange(prog.ProgrammeExercices);
            db.Programmes.Remove(prog);
        }

        if (expires.Count > 0)
            await db.SaveChangesAsync();
    }

    // ─── GET ALL ──────────────────────────────────────────────────────────────
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
        var programme = new Programme
        {
            Nom = dto.Nom.Trim(),
            Description = dto.Description?.Trim(),
            MembreId = membreId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(35),
        };
        db.Programmes.Add(programme);
        await db.SaveChangesAsync();

        await CreerExercices(programme.Id, membreId, dto.Exercices);

        return await GetById(programme.Id, membreId);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────
    public async Task<ProgrammeResponseDto> Update(int id, UpdateProgrammeDto dto, int membreId)
    {
        var programme = await db.Programmes
            .Include(p => p.ProgrammeExercices)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Programme introuvable.");

        if (programme.MembreId != membreId)
            throw new UnauthorizedAccessException("Accès refusé.");

        programme.Nom = dto.Nom.Trim();
        programme.Description = dto.Description?.Trim();

        // Supprimer anciens liens puis anciens exercices
        var ancienIds = programme.ProgrammeExercices
            .Select(pe => pe.ExerciceId).ToList();

        db.ProgrammeExercices.RemoveRange(programme.ProgrammeExercices);
        await db.SaveChangesAsync();

        await db.Exercices
            .Where(e => ancienIds.Contains(e.Id))
            .ExecuteDeleteAsync();

        await CreerExercices(programme.Id, membreId, dto.Exercices);

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

        var exerciceIds = p.ProgrammeExercices
            .Select(pe => pe.ExerciceId).ToList();

        db.ProgrammeExercices.RemoveRange(p.ProgrammeExercices);
        await db.SaveChangesAsync();

        await db.Exercices
            .Where(e => exerciceIds.Contains(e.Id))
            .ExecuteDeleteAsync();

        db.Programmes.Remove(p);
        await db.SaveChangesAsync();
    }
}