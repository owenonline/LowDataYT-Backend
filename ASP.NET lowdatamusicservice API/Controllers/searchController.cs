using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ASP.NET_lowdatamusicservice_API.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using LibVLCSharp.Shared;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NAudio.Wave;
using WrapYoutubeDl;
namespace ASP.NET_lowdatamusicservice_API.Controllers
{
    [RoutePrefix("api/search")]
    public class SearchController : ApiController
    {
        public List<Models.SearchResult> searcheslist = new List<Models.SearchResult>();

        //step 1
        [HttpGet]
        [Route("{search}")]
        public IHttpActionResult GetSearch(string search)
        {
            getyoutuberesults(search);
            var results = searcheslist;
            return Ok(results);
        }

        //step 2
        [HttpGet]
        [Route("{vidid},{title}")]
        public IHttpActionResult streamvideo(string vidid, string title)
        {
            //var mp3OutputFolder = HttpContext.Current.Server.MapPath("/mp3s");
            var mp3OutputFolder = @"C:/Users/Administrator/Desktop/lowdatayt/mp3s/";
            //Models.SearchResult selectedvideo
            Models.SearchResult selectedvideo = new Models.SearchResult() { vidid = vidid, title = title};
            //List<Models.SearchResult> searcheslist
            Boolean alreadydown = false;
            //var di = new DirectoryInfo("C:/Users/Administrator/Desktop/lowdatayt/mp3s");
            var di = new DirectoryInfo("C:/Users/Administrator/Desktop/lowdatayt/mp3s");
            foreach (FileInfo file in di.GetFiles())
            {
                if (selectedvideo.title +".mp3" == file.Name)
                {
                    alreadydown = true;
                    break;
                }
            }
            if (alreadydown!=true)
            {
                var urlToDownload = "https://www.youtube.com/watch?v=" + selectedvideo.vidid;
                var newFilename = selectedvideo.title;
                //"C:/Users/Administrator/Desktop/lowdatayt/mp3s"
                var downloader = new AudioDownloader(urlToDownload, newFilename, mp3OutputFolder);
                downloader.ProgressDownload += downloader_ProgressDownload;
                downloader.FinishedDownload += downloader_FinishedDownload;
                downloader.Download();
                
                //var inputFile = new MediaFile { Filename = @"C:/Users/Owen Burns/Desktop/lowdatayt/mp3s/"+selectedvideo.title+".mp3" };
                //var outputFile = new MediaFile { Filename = @"C:/Users/Owen Burns/Desktop/lowdatayt/wavs/" + selectedvideo.title + ".wav" };
                //using (var engine = new Engine())
                //{
                //    engine.Convert(inputFile, outputFile);
                //}
            }
            //"C:/Users/Administrator/Desktop/lowdatayt/mp3s"
            Stream audiostream = File.Open(mp3OutputFolder + selectedvideo.title + ".mp3", FileMode.Open);

            //new code
            MemoryStream memoryStream = new MemoryStream();
            audiostream.CopyTo(memoryStream);
            audiostream.Close();
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(memoryStream.GetBuffer())
            };
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = selectedvideo.title + ".mp3"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = ResponseMessage(result);
            //end new code
            return response;
        }
        static void downloader_FinishedDownload(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("Finished!");
        }

        static void downloader_ProgressDownload(object sender, ProgressEventArgs e)
        {
            Console.WriteLine(e.Percentage);
        }

        private async Task getyoutuberesults(string search)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyD0AlGww8qpR2MOHVyWV9YaExr-YhFCbY0",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = search; 
            searchListRequest.MaxResults = 50;

            
            var searchListResponse = searchListRequest.ExecuteAsync().Result;

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        searcheslist.Add(new Models.SearchResult { vidid = searchResult.Id.VideoId, title = searchResult.Snippet.Title });
                        break;

                    case "youtube#channel":
                        break;

                    case "youtube#playlist":
                        break;
                }
            }
        }
    }
}
