using System.ComponentModel.DataAnnotations.Schema;

namespace CreateExcel.Web.Models
{
    public enum FileStatus
    {
        Created = 0,
        Completed = 1
    }

    public class UserFile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public FileStatus FileStatus { get; set; }

        [NotMapped]
        public string GetCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToShortDateString() : "-";
    }
}
