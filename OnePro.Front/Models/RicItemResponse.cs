using Core.Models.Enums;
using Newtonsoft.Json;

namespace OnePro.Front.Models
{
    public class RicItemResponse
    {
        public Guid Id { get; set; }
        public string Judul { get; set; } = default!;
        public string? Permasalahan { get; set; }
        public string? UserName { get; set; }
        public string? Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RicCreateViewModel
    {
        // Penting: tambahkan Id buat edit
        public Guid Id { get; set; }

        public string JudulPermintaan { get; set; } = default!;
        public List<string> Hashtags { get; set; } = new();

        public string Permasalahan { get; set; } = string.Empty;
        public string DampakMasalah { get; set; } = string.Empty;
        public string FaktorPenyebab { get; set; } = string.Empty;
        public string SolusiSaatIni { get; set; } = string.Empty;

        public List<string> Alternatifs { get; set; } = new();

        public IFormFile? AsIsRasciFile { get; set; }
        public IFormFile? ToBeProcessFile { get; set; }
        public IFormFile? ExpectedCompletionFile { get; set; }

        public string? AsIsRasciFileContent { get; set; }
        public string? ToBeProcessFileContent { get; set; }
        public string? ExpectedCompletionFileContent { get; set; }

        public string? PotentialValue { get; set; }
        public string? HasilSetelahPerbaikan { get; set; }

        // Buat nampung URL file lama (buat Edit)
        public List<string>? ExistingAsIsFileUrls { get; set; }
        public List<string>? ExistingToBeFileUrls { get; set; }
        public List<string>? ExistingExpectedCompletionFileUrls { get; set; }
    }

    public class FormRicCreateRequest
    {
        public string Judul { get; set; } = default!;
        public List<string>? Hastag { get; set; }
        public List<string>? AsIsProcessRasciFile { get; set; }
        public string? Permasalahan { get; set; }
        public string? DampakMasalah { get; set; }
        public string? FaktorPenyebabMasalah { get; set; }
        public string? SolusiSaatIni { get; set; }
        public List<string>? AlternatifSolusi { get; set; }
        public List<string>? ToBeProcessBusinessRasciKkiFile { get; set; }
        public string? PotensiValueCreation { get; set; }
        public List<string>? ExcpectedCompletionTargetFile { get; set; }
        public string? HasilSetelahPerbaikan { get; set; }
        public int Status { get; set; }
    }

    // === Tambahan ini yang lagi dicari compiler ===

    public class RicDetailResponse
    {
        public Guid Id { get; set; }

        public string Judul { get; set; } = default!;
        public List<string>? Hastag { get; set; }
        public List<string>? AsIsProcessRasciFile { get; set; }

        public string? Permasalahan { get; set; }
        public string? DampakMasalah { get; set; }
        public string? FaktorPenyebabMasalah { get; set; }
        public string? SolusiSaatIni { get; set; }

        public List<string>? AlternatifSolusi { get; set; }
        public List<string>? ToBeProcessBusinessRasciKkiFile { get; set; }
        public string? PotensiValueCreation { get; set; }

        public List<string>? ExcpectedCompletionTargetFile { get; set; }

        [JsonProperty("hasilSetelahPerbaikan")]
        public string? HasilSetelahPerbaikan { get; set; }

        public int Status { get; set; }
    }

    public class FormRicUpdateRequest
    {
        public Guid Id { get; set; }

        public string Judul { get; set; } = default!;
        public List<string>? Hastag { get; set; }
        public List<string>? AsIsProcessRasciFile { get; set; }
        public string? Permasalahan { get; set; }
        public string? DampakMasalah { get; set; }
        public string? FaktorPenyebabMasalah { get; set; }
        public string? SolusiSaatIni { get; set; }
        public List<string>? AlternatifSolusi { get; set; }
        public List<string>? ToBeProcessBusinessRasciKkiFile { get; set; }
        public string? PotensiValueCreation { get; set; }
        public List<string>? ExcpectedCompletionTargetFile { get; set; }
        public string? HasilSetelahPerbaikan { get; set; }
        public int Status { get; set; }
    }
}
