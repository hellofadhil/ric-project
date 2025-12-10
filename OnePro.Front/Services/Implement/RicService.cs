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
            var apiUrl = $"{_config["ApiUrl"]}/api/Ric/my";

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
            var apiUrl = $"{_config["ApiUrl"]}/api/Ric";

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

        public async Task<OnePro.Front.Models.RicDetailResponse?> GetRicByIdAsync(Guid id, string token)
        {
            var apiUrl = $"{_config["ApiUrl"]}/api/Ric/{id}";
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
            var apiUrl = $"{_config["ApiUrl"]}/api/Ric/{id}";
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
    }
}
