namespace Dating_App_Backend.Entities
{
    public class AppFile
    {
        public AppFile()
        {
            
        }
        public AppFile(IFormFile file)
        {
            FileName = file.FileName;
            FileType = file.ContentType;
            FileExtension = Path.GetExtension(file.FileName);
            Length = file.Length;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        //from cloudinary
        public string FileUrl { get; set; }
        public string PublicId { get; set; }
        ///
        public string FileType { get; set; }
        public string FileExtension { get; set; }
        public long Length { get; set; }
        public string FileName { get; set; }
    }
}
