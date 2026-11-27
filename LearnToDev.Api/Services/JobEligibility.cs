namespace LearnToDev.Api.Services;

// The answer for one job: can the player take it, and if not, why not, and is it the player's job now
public record JobEligibility(int JobId, string JobName, string Description, List<string> RequiredSchools, bool IsOpen, string? LockReason, bool IsCurrent);
