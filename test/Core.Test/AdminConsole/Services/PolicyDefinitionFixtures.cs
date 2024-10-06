﻿#nullable enable

using Bit.Core.AdminConsole.Entities;
using Bit.Core.AdminConsole.Enums;
using Bit.Core.AdminConsole.OrganizationFeatures.Policies;
using NSubstitute;

namespace Bit.Core.Test.AdminConsole.Services;

public class FakeSingleOrgPolicyDefinition : IPolicyDefinition
{
    public PolicyType Type => PolicyType.SingleOrg;
    public IEnumerable<PolicyType> RequiredPolicies => Array.Empty<PolicyType>();

    public readonly Func<Policy?, Policy, Task<string?>> ValidateAsyncMock = Substitute.For<Func<Policy?, Policy, Task<string?>>>();
    public readonly Action<Policy?, Policy> OnSaveSideEffectsAsyncMock = Substitute.For<Action<Policy?, Policy>>();

    public Task<string?> ValidateAsync(Policy? currentPolicy, Policy modifiedPolicy)
    {
        return ValidateAsyncMock(currentPolicy, modifiedPolicy);
    }

    public Task OnSaveSideEffectsAsync(Policy? currentPolicy, Policy modifiedPolicy)
    {
        OnSaveSideEffectsAsyncMock(currentPolicy, modifiedPolicy);
        return Task.FromResult(0);
    }
}
public class FakeRequireSsoPolicyDefinition : IPolicyDefinition
{
    public PolicyType Type => PolicyType.RequireSso;
    public IEnumerable<PolicyType> RequiredPolicies => [PolicyType.SingleOrg];

    public Task<string?> ValidateAsync(Policy? currentPolicy, Policy modifiedPolicy)
    {
        return Task.FromResult((string)null);
    }

    public Task OnSaveSideEffectsAsync(Policy? currentPolicy, Policy modifiedPolicy)
    {
        return Task.FromResult(0);
    }
}
