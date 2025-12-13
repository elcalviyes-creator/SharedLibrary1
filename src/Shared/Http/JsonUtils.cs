namespace Shared.Http;
using System.Collections;
using System.Net;
using System.Text.Json;
public static class JsonUtils
{

public static dynamic GetPaginatedResponse<T>(HttpListenerRequest req,
    HttpListenerResponse res, Hashtable props, PagedResult<T> pagedResult,
    int page, int size)
{
    var baseUrl =
        $"{req.Url!.Scheme}://{req.Url.Authority}{req.Url.AbsolutePath}";
    int totalPages = Math.Max(1, (int) Math.Ceiling((double)
        pagedResult.TotalCount / size));

    string self =
        $"{baseUrl}?page={page}&size={size}";
    string? first = page == 1 ? null :
        $"{baseUrl}?page=1&size={size}";
    string? last = page == totalPages ? null :
        $"{baseUrl}?page={totalPages}&size={size}";
    string? prev = page > 1 ? $"{baseUrl}?page={page - 1}&size={size}" : null;
string? next = page < totalPages ? $"{baseUrl}?page={page + 1}&size={size}" : null;

var jsonApiPayload = new
{
    data = pagedResult.Values,
    meta = new { pagedResult.TotalCount, page, size, totalPages },
    links = new { self, first, prev, next, last }
};

return jsonApiPayload;
}
public static async Task SendPagedResultResponse<T>(HttpListenerRequest req,
    HttpListenerResponse res, Hashtable props, Result<PagedResult<T>> result,
    int page, int size)
{
    if(result.IsError)
    {
        res.Headers["Cache-Control"] = "no-store";

        var jsonApiError = new { errors = new[] { result.Error! } };

        await HttpUtils.SendResponse(req, res, props, result.StatusCode,
            JsonSerializer.Serialize(jsonApiError, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
    else
    {
        var pagedResult = result.Payload!;
        var jsonApiPayload = JsonUtils.GetPaginatedResponse(req, res, props,
            pagedResult, page, size);

        HttpUtils.AddPaginationHeaders(req, res, props, pagedResult, page, size);

        await HttpUtils.SendResponse(req, res, props, result.StatusCode,
            JsonSerializer.Serialize(jsonApiPayload, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
public static async Task SendResultResponse<T>(HttpListenerRequest req,
    HttpListenerResponse res, Hashtable props, Result<T> result)
{
    if(result.IsError)
    {
        res.Headers["Cache-Control"] = "no-store";
        await HttpUtils.SendResponse(req, res, props, result.StatusCode,
            JsonSerializer.Serialize(result.Error!, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
    else
    {
        await HttpUtils.SendResponse(req, res, props, result.StatusCode,
            JsonSerializer.Serialize(result.Payload!, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
}