using System.ComponentModel.DataAnnotations;

namespace Core.Models.Entities
{
    public class FormRicHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdFormRic { get; set; }
        public Guid IdEditor { get; set; }

        public int Version { get; set; }
        public string Snapshot { get; set; } = default!;
        public string? EditedFields { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public FormRic? FormRic { get; set; }
        public User? Editor { get; set; }
    }
}
