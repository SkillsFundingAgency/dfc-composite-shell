namespace DFC.Composite.Shell.Models
{
    public class PostResponseModel
    {
        public string Html { get; set; }
        public bool IsFileDownload => FileDownloadModel != null;
        public FileDownloadModel FileDownloadModel { get; set; }
    }
}
