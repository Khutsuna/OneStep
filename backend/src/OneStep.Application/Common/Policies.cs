namespace OneStep.Application.Common;

/// <summary>
/// Authorization policy names, used on controllers like
/// [Authorize(Policy = Policies.StaffOnly)].
/// The rules behind each name are registered in Infrastructure/DependencyInjection.cs.
/// </summary>
public static class Policies
{
    public const string CustomerOnly = "CustomerOnly";   // Customer
    public const string StaffOnly = "StaffOnly";         // Admin or StoreManager
    public const string AdminOnly = "AdminOnly";         // Admin
}
