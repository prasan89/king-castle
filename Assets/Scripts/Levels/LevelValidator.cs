using System;
using System.Collections.Generic;
namespace KingSmash.Levels
{
    public class LevelValidationReport
    {
        public int LevelIndex;
        public bool IsValid;
        public List<string> Errors   = new();
        public List<string> Warnings = new();

        public void AddError(string msg)   { Errors.Add(msg);   IsValid = false; }
        public void AddWarning(string msg) { Warnings.Add(msg); }

        public override string ToString()
        {
            if (IsValid && Warnings.Count == 0) return $"Level {LevelIndex + 1}: OK";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Level {LevelIndex + 1}:");
            foreach (var e in Errors)   sb.AppendLine($"  ERROR: {e}");
            foreach (var w in Warnings) sb.AppendLine($"  WARN:  {w}");
            return sb.ToString();
        }
    }

    public static class LevelValidator
    {
        public static LevelValidationReport Validate(LevelDefinition def)
        {
            var report = new LevelValidationReport { LevelIndex = def.LevelIndex, IsValid = true };

            if (def.LevelIndex < 0 || def.LevelIndex > 99)
                report.AddError($"LevelIndex {def.LevelIndex} out of range 0-99");

            if (def.WorldIndex < 0 || def.WorldIndex > 4)
                report.AddError($"WorldIndex {def.WorldIndex} out of range 0-4");

            if (string.IsNullOrEmpty(def.DisplayName))
                report.AddError("DisplayName is empty");

            if (def.KingLaunches < 1)
                report.AddError($"KingLaunches={def.KingLaunches} must be >= 1");

            if (def.RequiredScore <= 0)
                report.AddError($"RequiredScore={def.RequiredScore} must be > 0");

            if (def.TwoStarScore <= def.RequiredScore)
                report.AddError($"TwoStarScore({def.TwoStarScore}) must be > RequiredScore({def.RequiredScore})");

            if (def.ThreeStarScore <= def.TwoStarScore)
                report.AddError($"ThreeStarScore({def.ThreeStarScore}) must be > TwoStarScore({def.TwoStarScore})");

            if (!def.HasQueen)
                report.AddError("HasQueen must be true (Queen must be present in every level)");

            if (def.DifficultyRating < 1 || def.DifficultyRating > 10)
                report.AddError($"DifficultyRating={def.DifficultyRating} out of range 1-10");

            if (def.OneStar_DestructionMin < 0f || def.OneStar_DestructionMin > 1f)
                report.AddError($"OneStar_DestructionMin={def.OneStar_DestructionMin} out of 0-1");

            if (def.TwoStar_DestructionMin < def.OneStar_DestructionMin)
                report.AddError("TwoStar_DestructionMin must be >= OneStar_DestructionMin");

            if (def.ThreeStar_DestructionMin < def.TwoStar_DestructionMin)
                report.AddError("ThreeStar_DestructionMin must be >= TwoStar_DestructionMin");

            // World/level index cross-check
            var world = WorldRegistry.GetWorld(def.WorldIndex);
            if (world != null && !world.ContainsLevel(def.LevelIndex))
                report.AddError($"LevelIndex {def.LevelIndex} not in WorldIndex {def.WorldIndex} range [{world.LevelStart},{world.LevelEnd}]");

            // IsBossLevel consistency
            bool registryBoss = WorldRegistry.IsBossLevel(def.LevelIndex);
            if (def.IsBossLevel != registryBoss)
                report.AddWarning($"IsBossLevel={def.IsBossLevel} but WorldRegistry.IsBossLevel={registryBoss}");

            // Warnings
            if (def.GuardCount + def.ShieldGuardCount + def.ArcherCount == 0 && !def.HasEnemyKing)
                report.AddWarning("Level has no enemies");

            if (def.CastleFloors < 1)
                report.AddWarning($"CastleFloors={def.CastleFloors} seems low");

            return report;
        }

        public static List<LevelValidationReport> ValidateAll()
        {
            var results = new List<LevelValidationReport>();
            var seenIds = new HashSet<int>();

            foreach (var def in LevelConfigFactory.All)
            {
                var report = Validate(def);
                if (!seenIds.Add(def.LevelIndex))
                    report.AddError($"Duplicate LevelIndex {def.LevelIndex}");
                results.Add(report);
            }

            if (LevelConfigFactory.All.Count != 100)
            {
                var synthetic = new LevelValidationReport { LevelIndex = -1, IsValid = false };
                synthetic.AddError($"Expected 100 levels, found {LevelConfigFactory.All.Count}");
                results.Add(synthetic);
            }

            return results;
        }

        public static bool ValidateAllAndLog()
        {
            var reports = ValidateAll();
            bool allValid = true;
            foreach (var r in reports)
            {
                if (!r.IsValid || r.Warnings.Count > 0)
                {
                    KingSmash.Core.GameLogger.Warning("LevelValidator", r.ToString());
                    if (!r.IsValid) allValid = false;
                }
            }
            if (allValid)
                KingSmash.Core.GameLogger.Info("LevelValidator", "All 100 levels validated successfully.");
            return allValid;
        }
    }
}
