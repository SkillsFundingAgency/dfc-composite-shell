using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Composite.Shell.Models
{
    public class FileDownloadModel
    {
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
        public string FileContentType { get; set; }
    }
}
