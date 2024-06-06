namespace RealtorHubAPI;

public static class MimeTypesHelper
{
    /// <summary>
    /// Gets Correct MIME For File Type
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static string GetContentType(string extension)
    {
        switch (extension)
        {
            case StringConstants.Jpeg:
            case StringConstants.Jpg:
                return "image/jpeg";
            case StringConstants.Gif:
                return "image/gif";
            case StringConstants.PNG:
                return "image/png";
            case StringConstants.PDF:
                return "application/pdf";
            case StringConstants.CSV:
                return "text/csv";
            case StringConstants.Word:
                return "application/msword";
            case StringConstants.Wordx:
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case StringConstants.Zip:
                return "application/zip";
            case StringConstants.MP4:
                return "video/mp4";
            case StringConstants.MPEG4:
                return "video/mpeg";
            case StringConstants.MPEG:
                return "video/mpeg";
            case StringConstants.MPG:
                return "video/mpeg";
            case StringConstants.WEBM:
                return "video/webm"; 
            case StringConstants.WMV:
                return "video/x-ms-wmv"; 
            case StringConstants.MKV:
                return "video/x-matroska";
            case StringConstants.MOV:
                return "video/quicktime";
            case StringConstants.OGV:
                return "video/ogg";
            case StringConstants.AVI:
                return "video/x-msvideo";
            case StringConstants.FLV:
                return "video/x-msvideo";

            case StringConstants.ThreeGP:
                return "video/3gpp";
            default:
                return "application/octet-stream";
        }
    }
}

public class StringConstants
{
    /// <summary>
    /// Constants For File Types
    /// </summary>
    #region File Types
    public const string PNG = ".png";
    public const string Jpeg = ".jpeg";
    public const string Jpg = ".jpg";
    public const string Gif = ".gif";
    public const string PDF = ".pdf";
    public const string CSV = ".csv";
    public const string Word = ".doc";
    public const string Wordx = ".docx";
    public const string Zip = ".zip";
    public const string MP4 = ".mp4";
    public const string AVI = ".avi";
    public const string MKV = ".mkv";
    public const string MOV = ".mov";
    public const string WMV = ".wmv";
    public const string FLV = ".flv";
    public const string MPEG = ".mpeg";
    public const string MPEG4 = ".mpeg-4";
    public const string MPG = ".mpg";
    public const string WEBM = ".webm";
    public const string ThreeGP = ".3gp";
    public const string OGV = ".ogv";

    public static readonly string[] PermittedFileExtensions = { PNG, Jpeg, Jpg, Gif, PDF, CSV, Word, Wordx, Zip };
    #endregion
}