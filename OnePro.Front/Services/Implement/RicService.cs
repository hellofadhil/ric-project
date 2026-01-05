using Newtonsoft.Json;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;
using RestSharp;

namespace OnePro.Front.Services.Implement
{
    public class RicService : IRicService
    {
        private readonly IConfiguration _config;

        public RicService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<RicItemResponse>> GetMyRicsAsync(string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric/my";

            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token}");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                return new List<RicItemResponse>();

            var result =
                JsonConvert.DeserializeObject<List<RicItemResponse>>(response.Content)
                ?? new List<RicItemResponse>();

            return result;
        }

        public async Task CreateRicAsync(FormRicCreateRequest requestDto, string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric";

            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(requestDto);

            var response = await client.ExecuteAsync(request);

            int code = (int)response.StatusCode;

            if (code < 200 || code > 299)
            {
                throw new Exception(
                    $"Create RIC gagal: {response.StatusCode} - {response.Content}"
                );
            }
        }

        public async Task<RicDetailResponse?> GetRicByIdAsync(Guid id, string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric/{id}/detail";
            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token}");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                return null;

            return JsonConvert.DeserializeObject<RicDetailResponse>(response.Content);
        }

        public async Task UpdateRicAsync(Guid id, FormRicUpdateRequest requestDto, string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric/{id}";
            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Put);

            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(requestDto);

            var response = await client.ExecuteAsync(request);
            int code = (int)response.StatusCode;

            if (code < 200 || code > 299)
            {
                throw new Exception(
                    $"Update RIC gagal: {response.StatusCode} - {response.Content}"
                );
            }
        }

        // NEW
        // public async Task ReviewRicAsync(Guid id, RicReviewRequest requestDto, string token)
        // {
        //     var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric/{id}/review";
        //     var client = new RestClient(apiUrl);
        //     var request = new RestRequest("", Method.Put);

        //     request.AddHeader("Authorization", $"Bearer {token}");
        //     request.AddJsonBody(requestDto);

        //     var response = await client.ExecuteAsync(request);
        //     int code = (int)response.StatusCode;

        //     if (code < 200 || code > 299)
        //     {
        //         throw new Exception(
        //             $"Review RIC gagal: {response.StatusCode} - {response.Content}"
        //         );
        //     }
        // }

        public async Task ResubmitRicAsync(Guid id, FormRicResubmitRequest requestDto, string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric/{id}/resubmit";
            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Put);

            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(requestDto);

            var response = await client.ExecuteAsync(request);
            int code = (int)response.StatusCode;

            if (code < 200 || code > 299)
            {
                throw new Exception(
                    $"Resubmit RIC gagal: {response.StatusCode} - {response.Content}"
                );
            }
        }

        public async Task RejectAsync(Guid id, string? note, string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/v1/Ric/{id}/reject";

            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Put);

            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(new { catatan = note });

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception(
                    $"Reject RIC gagal: {(int)response.StatusCode} - {response.Content}"
                );
            }
        }
    }
}
