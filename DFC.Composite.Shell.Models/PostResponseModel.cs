namespace DFC.Composite.Shell.Models
{
    public class PostResponseModel
    {
        public string HTML { get; set; }
        public bool IsFileDownload => FileDownloadModel != null;
        public FileDownloadModel FileDownloadModel { get; set; }
    }
}
