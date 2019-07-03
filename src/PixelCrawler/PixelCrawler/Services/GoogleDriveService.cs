using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PixelCrawler.Services
{
    public class GoogleDriveService : DriveService
    {
        private NLog.Logger _logger;

        public GoogleDriveService(NLog.Logger logger, string key)
            : base(new Initializer() {
                HttpClientInitializer = GoogleCredential.FromJson(key).CreateScoped(DriveService.Scope.Drive),
            }) {
            _logger = logger;
        }


        public async Task<long> AvailableSpaceGoogleDrive()
        {
            var get = this.About.Get();
            get.Fields = "storageQuota";
            var about = await get.ExecuteAsync();
            var sq = about.StorageQuota;
            return sq.Limit.Value - sq.Usage.Value;
        }

        public async Task<bool> CheckIfFileUploadedToGoogle(string id)
        {
            var listRequest = this.Files.List();
            listRequest.Q = $"appProperties has  {{ key = 'id' and value='{id}' }}";
            var files = await listRequest.ExecuteAsync();

            return files.Files.Count > 0;
        }

        public async Task<List<Google.Apis.Drive.v3.Data.File>> 
            ListAllFiles(string search=null) {

            var files = new List<Google.Apis.Drive.v3.Data.File>();
            var list = this.Files.List();
            list.Q = search;

            FileList fileList = null;

            do {
                fileList = await list.ExecuteAsync();
                files.AddRange(fileList.Files);
                list.PageToken = fileList.NextPageToken;
            }
            while (fileList.NextPageToken != null);
        
            return files;
        }

        public async Task<string> UploadToGoogle(Stream stream, string name)
        {
            var file = new Google.Apis.Drive.v3.Data.File {
                Name = name,
                
            };
            var request = this.Files.Create(file, stream, "image/jpg");
            request.Fields = "id";

            await request.UploadAsync();
            return request.ResponseBody.Id;
        }

        public async Task AddPermissionsAnyoneReader(string id) {
            await this.Permissions.Create(new Permission()
            {
                Kind = "drive#permission",
                Type = "anyone",
                Role = "reader",
            }, id).ExecuteAsync();
        }
    }
}
