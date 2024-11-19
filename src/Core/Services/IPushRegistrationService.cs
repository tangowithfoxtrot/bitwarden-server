﻿using Bit.Core.Enums;

namespace Bit.Core.Services;

public interface IPushRegistrationService
{
    Task CreateOrUpdateRegistrationAsync(string pushToken, string deviceId, string userId,
        string identifier, DeviceType type, string installationId, IEnumerable<string> organizationIds);
    Task DeleteRegistrationAsync(string deviceId);
    Task AddUserRegistrationOrganizationAsync(IEnumerable<string> deviceIds, string organizationId);
    Task DeleteUserRegistrationOrganizationAsync(IEnumerable<string> deviceIds, string organizationId);
}
