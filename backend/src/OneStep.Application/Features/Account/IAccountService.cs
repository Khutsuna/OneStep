namespace OneStep.Application.Features.Account;

public interface IAccountService
{
    Task<ProfileDto> GetProfileAsync(CancellationToken cancellationToken);
    Task<ProfileDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}
