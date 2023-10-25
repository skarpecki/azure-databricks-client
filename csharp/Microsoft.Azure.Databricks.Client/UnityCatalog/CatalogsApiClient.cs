﻿using Microsoft.Azure.Databricks.Client.Models.UnityCatalog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Databricks.Client.UnityCatalog;

public class CatalogsApiClient : ApiClient, ICatalogsApi
{
    public CatalogsApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<Catalog>> List(CancellationToken cancellationToken = default)
    {
        var requestUri = $"{BaseUnityCatalogUri}/catalogs";
        var catalogsList = await HttpGet<JsonObject>(this.HttpClient, requestUri, cancellationToken).ConfigureAwait(false);

        catalogsList.TryGetPropertyValue("catalogs", out var catalogs);

        return catalogs?.Deserialize<IEnumerable<Catalog>>(Options) ?? Enumerable.Empty<Catalog>();
    }

    public async Task<Catalog> Create(Catalog catalog, CancellationToken cancellationToken = default)
    {
        var requestUri = $"{BaseUnityCatalogUri}/catalogs";
        var request = JsonSerializer.SerializeToNode(catalog, Options).AsObject();
        var response = await HttpPost<JsonObject, JsonObject>
                (this.HttpClient, requestUri, request, cancellationToken)
            .ConfigureAwait(false);

        return JsonSerializer.Deserialize<Catalog>(response, Options);
    }

    public async Task<Catalog> Get(string catalogName, CancellationToken cancellationToken = default)
    {
        var requestUri = $"{BaseUnityCatalogUri}/catalogs/{catalogName}";
        return await HttpGet<Catalog>(this.HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Catalog> Update(
        string catalogName,
        string name = default,
        string owner = default,
        string comment = default,
        Dictionary<string, string> properties = default,
        IsolationMode? isolationMode = IsolationMode.OPEN,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"{BaseUnityCatalogUri}/catalogs/{catalogName}";

        var request = new
        {
            name,
            owner,
            comment,
            isolation_mode = isolationMode
        };

        var requestJson = JsonSerializer.SerializeToNode(request, Options).AsObject();

        if (properties != null)
        {
            var propertiesJson = JsonSerializer.SerializeToNode(properties, Options);
            requestJson.Add("properties", propertiesJson);
        }

        return await HttpPatch<JsonObject, Catalog>(HttpClient, requestUri, requestJson, cancellationToken).ConfigureAwait(false);
    }

    public async Task Delete(string catalogName, bool forceDeletion = false, CancellationToken cancellationToken = default)
    {
        var requestUri = $"{BaseUnityCatalogUri}/catalogs/{catalogName}?force={forceDeletion.ToString().ToLower()}";

        await HttpDelete(HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
    }
}
