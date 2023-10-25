﻿using Microsoft.Azure.Databricks.Client.Models.UnityCatalog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Databricks.Client.UnityCatalog;

public class FunctionsApiClient : ApiClient, IFunctionsApi
{
    private string FunctionsApiUrl => $"{BaseUnityCatalogUri}/functions";

    public FunctionsApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<Function>> List(CancellationToken cancellationToken = default)
    {
        var functionsList = await HttpGet<JsonObject>(HttpClient, this.FunctionsApiUrl, cancellationToken).ConfigureAwait(false);
        functionsList.TryGetPropertyValue("functions", out var functions);

        return functions?.Deserialize<IEnumerable<Function>>(Options) ?? Enumerable.Empty<Function>();
    }

    public async Task<Function> Create(
        Function newFunction,
        CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.SerializeToNode(newFunction, Options).AsObject();
        return await HttpPost<JsonObject, Function>(HttpClient, this.FunctionsApiUrl, request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Function> Get(string name, CancellationToken cancellationToken = default)
    {
        var requestUri = $"{this.FunctionsApiUrl}/{name}";
        return await HttpGet<Function>(HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Function> Update(
        string functionName,
        string owner,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"{this.FunctionsApiUrl}/{functionName}";

        var request = new Dictionary<string, string>()
        {
            {"owner", owner }
        };

        var requestJson = JsonSerializer.SerializeToNode(request, Options).AsObject();
        return await HttpPatch<JsonObject, Function>(HttpClient, requestUri, requestJson, cancellationToken).ConfigureAwait(false);
    }

    public async Task Delete(string name, CancellationToken cancellationToken = default)
    {
        var requestUri = $"{this.FunctionsApiUrl}/{name}";
        await HttpDelete(HttpClient, requestUri, cancellationToken).ConfigureAwait(false);
    }
}
