using Core.Models.Enums;
using OnePro.Front.Helpers;
using OnePro.Front.Models;

namespace OnePro.Front.Mappers
{
    public static class RicMapper
    {
        public static async Task<FormRicCreateRequest> MapToCreateRequestAsync(
            RicCreateViewModel model,
            string action,
            Func<IEnumerable<IFormFile>?, Task<List<string>>> saveFilesAsync
        )
        {
            var asIsUrls = await saveFilesAsync(model.AsIsRasciFiles);
            var toBeUrls = await saveFilesAsync(model.ToBeProcessFiles);
            var expectedUrls = await saveFilesAsync(model.ExpectedCompletionFiles);

            var status = action == "submit"
                ? StatusRic.Submitted_To_BR
                : StatusRic.Draft;

            return new FormRicCreateRequest
            {
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,
                AsIsProcessRasciFile = asIsUrls.Any() ? asIsUrls : null,
                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,
                AlternatifSolusi = model.Alternatifs,
                ToBeProcessBusinessRasciKkiFile = toBeUrls.Any() ? toBeUrls : null,
                PotensiValueCreation = model.PotentialValue,
                ExcpectedCompletionTargetFile = expectedUrls.Any() ? expectedUrls : null,
                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        public static async Task<FormRicUpdateRequest> MapToUpdateRequestAsync(
            Guid id,
            RicCreateViewModel model,
            string action,
            RicDetailResponse existing,
            Func<IEnumerable<IFormFile>?, Task<List<string>>> saveFilesAsync
        )
        {
            var asIsUrls = await saveFilesAsync(model.AsIsRasciFiles);
            var toBeUrls = await saveFilesAsync(model.ToBeProcessFiles);
            var expectedUrls = await saveFilesAsync(model.ExpectedCompletionFiles);

            var status = action == "submit"
                ? StatusRic.Submitted_To_BR
                : StatusRic.Draft;

            return new FormRicUpdateRequest
            {
                Id = id,
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,
                AsIsProcessRasciFile =
                    asIsUrls.Any() ? asIsUrls : existing.AsIsProcessRasciFile,
                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,
                AlternatifSolusi = model.Alternatifs,
                ToBeProcessBusinessRasciKkiFile =
                    toBeUrls.Any() ? toBeUrls : existing.ToBeProcessBusinessRasciKkiFile,
                PotensiValueCreation = model.PotentialValue,
                ExcpectedCompletionTargetFile =
                    expectedUrls.Any() ? expectedUrls : existing.ExcpectedCompletionTargetFile,
                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        public static RicCreateViewModel MapToEditViewModel(RicDetailResponse ric)
        {
            return new RicCreateViewModel
            {
                Id = ric.Id,
                JudulPermintaan = ric.Judul,
                Hashtags = ric.Hastag ?? new List<string>(),
                Permasalahan = ric.Permasalahan ?? string.Empty,
                DampakMasalah = ric.DampakMasalah ?? string.Empty,
                FaktorPenyebab = ric.FaktorPenyebabMasalah ?? string.Empty,
                SolusiSaatIni = ric.SolusiSaatIni ?? string.Empty,
                Alternatifs = ric.AlternatifSolusi ?? new List<string>(),
                PotentialValue = ric.PotensiValueCreation,
                HasilSetelahPerbaikan = ric.HasilSetelahPerbaikan ?? string.Empty,

                ExistingAsIsFileUrls = ric.AsIsProcessRasciFile,
                ExistingToBeFileUrls = ric.ToBeProcessBusinessRasciKkiFile,
                ExistingExpectedCompletionFileUrls = ric.ExcpectedCompletionTargetFile,
            };
        }
    }
}
