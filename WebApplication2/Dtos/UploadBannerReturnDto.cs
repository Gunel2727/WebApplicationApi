namespace WebApplication2.Dtos
{
    public class UploadBannerReturnDto
    {
        public string Message { get; set; } = "Banner uploaded";
        public string? FileName { get; set; }
        public string? Path { get; set; }
        public string? Url { get; set; }
    }
}
