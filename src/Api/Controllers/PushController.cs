﻿using Bit.Core.Context;
using Bit.Core.Exceptions;
using Bit.Core.Models.Api;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bit.Api.Controllers;

[Route("push")]
[Authorize("Push")]
[SelfHosted(NotSelfHostedOnly = true)]
public class PushController : Controller
{
    private readonly IPushRegistrationService _pushRegistrationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentContext _currentContext;
    private readonly GlobalSettings _globalSettings;

    public PushController(
        IPushRegistrationService pushRegistrationService,
        IPushNotificationService pushNotificationService,
        IWebHostEnvironment environment,
        ICurrentContext currentContext,
        GlobalSettings globalSettings)
    {
        _currentContext = currentContext;
        _environment = environment;
        _pushRegistrationService = pushRegistrationService;
        _pushNotificationService = pushNotificationService;
        _globalSettings = globalSettings;
    }

    [HttpPost("register")]
    public async Task PostRegister([FromBody] PushRegistrationRequestModel model)
    {
        CheckUsage();
        await _pushRegistrationService.CreateOrUpdateRegistrationAsync(model.PushToken, Prefix(model.DeviceId),
            Prefix(model.UserId), Prefix(model.Identifier), model.Type);
    }

    [HttpPost("delete")]
    public async Task PostDelete([FromBody] PushDeviceRequestModel model)
    {
        CheckUsage();
        await _pushRegistrationService.DeleteRegistrationAsync(Prefix(model.Id), model.Type);
    }

    [HttpPut("add-organization")]
    public async Task PutAddOrganization([FromBody] PushUpdateRequestModel model)
    {
        CheckUsage();
        await _pushRegistrationService.AddUserRegistrationOrganizationAsync(
            model.Devices.Select(d => new KeyValuePair<string, Core.Enums.DeviceType>(Prefix(d.Id), d.Type)),
            Prefix(model.OrganizationId));
    }

    [HttpPut("delete-organization")]
    public async Task PutDeleteOrganization([FromBody] PushUpdateRequestModel model)
    {
        CheckUsage();
        await _pushRegistrationService.DeleteUserRegistrationOrganizationAsync(
            model.Devices.Select(d => new KeyValuePair<string, Core.Enums.DeviceType>(Prefix(d.Id), d.Type)),
            Prefix(model.OrganizationId));
    }

    [HttpPost("send")]
    public async Task PostSend([FromBody] PushSendRequestModel model)
    {
        CheckUsage();

        if (model.Global)
        {
            await _pushNotificationService.SendPayloadToEveryoneAsync(model.Type, model.Payload,
                Prefix(model.Identifier), Prefix(model.DeviceId), model.ClientType);
        }
        else if (!string.IsNullOrWhiteSpace(model.UserId))
        {
            await _pushNotificationService.SendPayloadToUserAsync(Prefix(model.UserId),
                model.Type, model.Payload, Prefix(model.Identifier), Prefix(model.DeviceId), model.ClientType);
        }
        else if (!string.IsNullOrWhiteSpace(model.OrganizationId))
        {
            await _pushNotificationService.SendPayloadToOrganizationAsync(Prefix(model.OrganizationId),
                model.Type, model.Payload, Prefix(model.Identifier), Prefix(model.DeviceId), model.ClientType);
        }
    }

    private string Prefix(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return $"{_currentContext.InstallationId.Value}_{value}";
    }

    private void CheckUsage()
    {
        if (CanUse())
        {
            return;
        }

        throw new BadRequestException("Not correctly configured for push relays.");
    }

    private bool CanUse()
    {
        if (_environment.IsDevelopment())
        {
            return true;
        }

        return _currentContext.InstallationId.HasValue && !_globalSettings.SelfHosted;
    }
}
