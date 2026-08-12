namespace IIIF.POC.PostgreSqlRelationalV3Store.Models;

public sealed record ManifestFinding(string RuleId, string Severity, string Path, string Message);