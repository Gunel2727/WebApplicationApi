namespace WebApplication2.Dtos
{
    public class UploadLogoReturnDto
    {
        public string Message { get; set; } = "Logo uploaded";
        public string? FileName { get; set; }
        public string? Path { get; set; }
        public string? Url { get; set; }
    }
}
