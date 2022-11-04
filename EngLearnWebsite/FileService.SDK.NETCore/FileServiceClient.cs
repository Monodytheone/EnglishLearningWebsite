using System.Net.Http;
using System.Security.Claims;
using Zack.Commons;
using Zack.JWT;

namespace FileService.SDK.NETCore;

public class FileServiceClient
{
    private readonly Uri _serverRoot;
    private readonly IHttpClientFactory _httpClienFactory;
    private readonly ITokenService _tokenService;
    private readonly JWTOptions _jwTOptionsSnacpshot;

    public FileServiceClient(Uri serverRoot, IHttpClientFactory httpClienFactory, ITokenService tokenService, JWTOptions jwTOptionsSnacpshot)
    {
        _serverRoot = serverRoot;
        _httpClienFactory = httpClienFactory;
        _tokenService = tokenService;
        _jwTOptionsSnacpshot = jwTOptionsSnacpshot;
    }

    public Task<FileExistResponse> FileExistsAsync(long fileSize, string sha256Hash, CancellationToken cancellationToken = default)
    {
        string relativeUrl = FormattableStringHelper.BuildUrl($"api/Uploader/FileExists?fileSize={fileSize}&sha256Hash={sha256Hash}");
        Uri requestUri = new(_serverRoot, relativeUrl);
        HttpClient httpClient = _httpClienFactory.CreateClient();
        return httpClient.GetJsonAsync<FileExistResponse>(requestUri, cancellationToken)!;  // 发送Get请求，得到Json响应体
    }

    /// <summary>
    /// 创建有Admin身份的JWT，以满足访问UploaderController中对应方法的要求
    /// </summary>
    private string BuildToken()
    {
        List<Claim> claims = new();
        Claim claim = new(ClaimTypes.Role, "Admin");
        claims.Add(claim);
        return _tokenService.BuildToken(claims, _jwTOptionsSnacpshot);
    }

    public async Task<Uri> UploadAsync(FileInfo file, CancellationToken stoppingToken = default)
    {
        string token = BuildToken();
        using MultipartFormDataContent content = new();
        using var fileContent = new StreamContent(file.OpenRead());
        content.Add(fileContent, "file", file.Name);
        HttpClient httpClient = _httpClienFactory.CreateClient();
        //Uri requestUrl = new($"{_serverRoot}/api/Uploader/Upload");
        Uri requestUrl = new($"{_serverRoot}api/Uploader/Upload");

        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);  // 添加Authorization请求头
        HttpResponseMessage responseMsg = await httpClient.PostAsync(requestUrl, content, stoppingToken);  // 发送Post请求，得到响应
        if (responseMsg.IsSuccessStatusCode == false)
        {
            string respString = await responseMsg.Content.ReadAsStringAsync(stoppingToken);
            throw new HttpRequestException($"上传失败，状态码：{responseMsg.StatusCode}，响应报文：{respString}");
        }
        else
        {
            string respString = await responseMsg.Content.ReadAsStringAsync(stoppingToken);
            return respString.ParseJson<Uri>()!;
        }
    }

}
