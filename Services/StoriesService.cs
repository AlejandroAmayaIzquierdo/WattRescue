using Models;

namespace Services;

public class StoriesService(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<Story?> GetStoryById(string id)
    {
        try
        {
            var _httpClient = _httpClientFactory.CreateClient("Wattpad");
            Story? story = await _httpClient.GetFromJsonAsync<Story>($"stories/{id}");

            return story;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching story by ID: {ex.Message}");
            return null;
        }
    }
}
