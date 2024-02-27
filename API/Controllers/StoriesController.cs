using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers
{
    public class StoriesController : BaseApiController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ActionResult<Stories>> StoryList()
        {
            try
            {
                var ListOfStory = new List<StoryDto>();
                Stories stories = new Stories();
               
                var ids = await GetStoryIdsAsync();

                // Parallelize fetching story details using IDs
                var tasks = ids.Select(id => GetStoryDetailsAsync(id));
                var storyDetails = await Task.WhenAll(tasks);

                ListOfStory.AddRange(storyDetails);

                stories.ListOfStory = ListOfStory.OrderByDescending(x => x.score).ToList();

                return stories;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            return null;

        }

        private async Task<List<int>> GetStoryIdsAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            string ApiIdsUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";
            var idsResponse = await httpClient.GetStringAsync(ApiIdsUrl);
            return JsonSerializer.Deserialize<List<int>>(idsResponse);
        }

        private async Task<StoryDto> GetStoryDetailsAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient();
            string ApiSoryDetailsUrl = $"https://hacker-news.firebaseio.com/v0/item/{id}.json";
            var storyDetailsResponse = await httpClient.GetStringAsync(ApiSoryDetailsUrl);
            return JsonSerializer.Deserialize<StoryDto>(storyDetailsResponse);
        }
    }
}
