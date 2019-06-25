using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;

namespace GoogleSynch
{
    public class GoogleSynchronizer
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Drive API .NET Quickstart";

        public string Authorize(Stream pathToFile, string tokenPath)
        {
            UserCredential credential;

            using (pathToFile)
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = tokenPath +"/token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(pathToFile).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            var filesString = "";
            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    filesString = filesString + file.Name;
                }
                return filesString;
            }
            else
            {
               return "No files found.";
            }
            //Console.Read();

            //var fileMetadata = new File()
            //{
            //    Name = "photo.jpg"
            //};
            //FilesResource.CreateMediaUpload request;
            //using (var stream = new System.IO.FileStream("files/photo.jpg",
            //                        System.IO.FileMode.Open))
            //{
            //    request = service.Files.Create(
            //        fileMetadata, stream, "image/jpeg");
            //    request.Fields = "id";
            //    request.Upload();
            //}
            //var file2 = request.ResponseBody;
            //Console.WriteLine("File ID: " + file2.Id);
        }
    }
}
