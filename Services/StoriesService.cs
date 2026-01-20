using Data;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services;

public class StoriesService(IHttpClientFactory httpClientFactory, WattDbContext dbContext)
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private readonly WattDbContext _dbContext = dbContext;

    public async Task<IList<Story>> GetAllStories()
    {
        return [.. await _dbContext.Stories.ToListAsync()];
    }

    public async Task<Story?> GetStoryById(string id)
    {
        try
        {
            Story? existingStory = await _dbContext
                .Stories.Include(s => s.Parts)
                    .ThenInclude(p => p.Paragraphs)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);

            Story? storyFromApi = await FetchStoryFromApi(id);

            if (storyFromApi is null)
                return null;

            if (existingStory is null)
            {
                existingStory = storyFromApi;
                _dbContext.Stories.Add(existingStory);
            }
            else
            {
                existingStory.Title = storyFromApi.Title;
                existingStory.Description = storyFromApi.Description;
                existingStory.Cover = storyFromApi.Cover;
                existingStory.Url = storyFromApi.Url;
                existingStory.NumParts = storyFromApi.NumParts;
                existingStory.ModifyDate = storyFromApi.ModifyDate;
                // Update parts
                foreach (var apiPart in storyFromApi.Parts)
                {
                    var existingPart = existingStory.Parts.FirstOrDefault(p => p.Id == apiPart.Id);
                    if (existingPart is null)
                        existingStory.Parts.Add(apiPart);
                    else
                    {
                        existingPart.ModifyDate = apiPart.ModifyDate;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            return existingStory;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching story by ID: {ex.Message}");
            return null;
        }
    }

    public async Task<Story?> FetchStoryFromApi(string id)
    {
        try
        {
            var _httpClient = _httpClientFactory.CreateClient("Wattpad");
            Story? story = await _httpClient.GetFromJsonAsync<Story>($"stories/{id}");

            return story;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching story from API: {ex.Message}");
            return null;
        }
    }

    public async Task UpdateStoryAsync(Story story)
    {
        try
        {
            // If the entity is already tracked, just save changes
            // Otherwise, attach it and mark as modified
            var entry = _dbContext.Stories.Entry(story);
            if (entry.State == EntityState.Detached)
            {
                _dbContext.Stories.Update(story);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating story: {ex.Message}");
        }
    }

    public async Task UpdatePartAsync(Part part)
    {
        var existingPart = await _dbContext
            .Parts.Include(p => p.Paragraphs)
            .FirstOrDefaultAsync(p => p.Id == part.Id);

        if (existingPart is null)
        {
            _dbContext.Parts.Add(part);
        }
        else
        {
            existingPart.RawContent = part.RawContent;
            existingPart.LastScrapedDate = part.LastScrapedDate;

            // Reemplazar párrafos
            existingPart.Paragraphs.Clear();
            foreach (var paragraph in part.Paragraphs)
            {
                existingPart.Paragraphs.Add(
                    new Paragraphs { PartId = existingPart.Id, Content = paragraph.Content }
                );
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateStoryLastScrapedAsync(string storyId)
    {
        var story = await _dbContext.Stories.FirstOrDefaultAsync(s => s.Id == storyId);
        if (story is not null)
        {
            story.LastScrapedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    public string ExtractStoryId(string url)
    {
        try
        {
            Uri uri = new(url);
            string[] segments = uri.Segments;
            if (segments.Length >= 3 && segments[1].TrimEnd('/') == "story")
            {
                string storynameUrl = segments[2];
                string storyId = storynameUrl.Split('-')[0];
                return storyId;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting story ID: {ex.Message}");
        }
        return string.Empty;
    }
}
