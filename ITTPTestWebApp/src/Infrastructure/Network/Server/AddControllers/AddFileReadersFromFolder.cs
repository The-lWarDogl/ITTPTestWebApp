using System.Text;
using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Routing;

using static ITTPTestWebApp.Adapters.AspNetCore.EndpointRouteBuilder.MapExt;

namespace ITTPTestWebApp.Network
{
    partial class Server
    {
        public void AddFileReadersFromFolder
        (
            string prefix,
            bool needAuthorization,
            string folderPath, 
            List<string>? permittedAddresses = null
        ) =>
        AddControllers
        (
            Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
            .Select(absoluteFilePath =>
            (IRequestController)
            new FileReaderController
            (
                prefix,
                needAuthorization,
                folderPath,
                Path.GetRelativePath(folderPath, absoluteFilePath).Replace('\\', '/'),
                permittedAddresses
            )).ToList()
        );
    }

    class FileReaderController : IRequestController
    {
        #region fields
        private readonly string _Prefix = string.Empty;
        private readonly bool _NeedAuthorization = false;
        private readonly string _FolderPath = string.Empty;
        private readonly string _FilePath = string.Empty;
        private readonly List<string>? _PermittedAddresses = null;
        #endregion

        public FileReaderController(string prefix, bool needAuthorization, string folderPath, string filePath, List<string>? permittedAddresses = null)
        {
            _Prefix = prefix;
            _NeedAuthorization = needAuthorization;
            _FolderPath = folderPath;
            _FilePath = filePath;
            _PermittedAddresses = permittedAddresses;
        }

        ~FileReaderController() { }

        public void Register(IEndpointRouteBuilder endpoints) =>
            endpoints.MapGetExt
            (
                $"{_Prefix}/{_FilePath}".Replace("\\", "/"),
                _NeedAuthorization,
                GetContentTypeFromExtension(_FilePath),
                Check,
                Handle,
                permittedAddresses: _PermittedAddresses
            );

        private async Task<(HttpStatusCode, string?)>
        Check
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            try
            {
                var fullPath = Path.Combine(_FolderPath, _FilePath);
                if (!File.Exists(fullPath)) { return (HttpStatusCode.NotFound, null); }

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        Handle
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            try
            {
                var fullPath = Path.Combine(_FolderPath, _FilePath);
                var bytes = await File.ReadAllBytesAsync(fullPath);
                var content = Encoding.UTF8.GetString(bytes);

                return ( HttpStatusCode.OK, content, null, null );
            }
            catch { return ( HttpStatusCode.InternalServerError, null, null, null ); }
            finally { await Task.CompletedTask; }
        }

        private static string GetContentTypeFromExtension(string filePath) =>
            Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
    }
}