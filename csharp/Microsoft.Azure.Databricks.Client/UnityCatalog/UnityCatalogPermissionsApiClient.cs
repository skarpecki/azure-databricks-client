﻿using Microsoft.Azure.Databricks.Client.Models.UnityCatalog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Azure.Databricks.Client.UnityCatalog;

public class UnityCatalogPermissionsApiClient : ApiClient, IUnityCatalogPermissionsApi
{
    public UnityCatalogPermissionsApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

  
    public async Task<IEnumerable<Permission>> Get(
        SecurableType securableType,
        string securableFullName,
        string principal = default,
        CancellationToken cancellationToken = default)
    {
        var requestUriSb = new StringBuilder(
            $"{BaseUnityCatalogUri}/permissions/{securableType.ToString().ToLower()}/{securableFullName}");
        
        if (principal != null)
        {
            requestUriSb.Append($"?principal={principal}");
        }

        var requestUri = requestUriSb.ToString();
        var permissionsList = await HttpGet<JsonObject>(HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
        permissionsList.TryGetPropertyValue("privilege_assignments", out var permissions);

        return permissions.Deserialize<IEnumerable<Permission>>(Options) ?? Enumerable.Empty<Permission>();
    }

    public async Task<IEnumerable<Permission>> Update(
        SecurableType securableType,
        string securableFullName,
        IEnumerable<PermissionsUpdate> permisionsUpdates,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            changes = permisionsUpdates,
        };

        var requestUri = $"{BaseUnityCatalogUri}/permissions/{securableType.ToString().ToLower()}/{securableFullName}";
        var requestJson = JsonSerializer.SerializeToNode(request, Options).AsObject();

        var permissionsList = await HttpPatch<JsonObject, JsonObject>(HttpClient, requestUri, requestJson, cancellationToken).ConfigureAwait(false);
        permissionsList.TryGetPropertyValue("privilege_assignments", out var permissions);

        return permissions.Deserialize<IEnumerable<Permission>>(Options) ?? Enumerable.Empty<Permission>();
    }

    public async Task<IEnumerable<EffectivePermission>> GetEffective(
        SecurableType securableType,
        string securableFullName,
        string principal = default,
        CancellationToken cancellationToken = default)
    {
        var requestUriSb = new StringBuilder(
            $"{BaseUnityCatalogUri}/effective-permissions/{securableType.ToString().ToLower()}/{securableFullName}");

        if (principal != null)
        {
            requestUriSb.Append($"?principal={principal}");
        }

        var requestUri = requestUriSb.ToString();
        var effectivePermissionsList = await HttpGet<JsonObject>(HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
        effectivePermissionsList.TryGetPropertyValue("privilege_assignments", out var effectivePermissions);

        return effectivePermissions.Deserialize<IEnumerable<EffectivePermission>>(Options) ?? Enumerable.Empty<EffectivePermission>();
    }
}
