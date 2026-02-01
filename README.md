# WattRescue

A web application to backup and read Wattpad stories offline. WattRescue scrapes story content from Wattpad and stores it locally in a SQLite database for offline reading.

> [!IMPORTANT]  
> This project made for learning purposes only. I am not meant to make any profit from it. Please respect Wattpad's terms of service and only use this tool for personal use. If some how the project get some traction, I will take it down immediately. I made this project to learn web scraping and a way for my girlfriend to read her favorite stories offline and save those stories from getting lost forever.

## Features

- 📚 **Story Backup** - Scrape and save Wattpad stories locally
- 📖 **Offline Reader** - Read saved stories without internet connection
- 🔄 **Auto-sync** - Scheduled job checks for story updates every 30 minutes for only modified stories
- 📊 **Progress Tracking** - Real-time scraping progress updates

## Tech Stack

- **Framework**: ASP.NET Core 10 (Razor Pages + Web API)
- **Database**: SQLite with Entity Framework Core
- **Scraping**: PuppeteerSharp (headless Chrome) + HtmlAgilityPack
- **Scheduler**: Quartz.NET for background jobs
- **Frontend**: Vanilla JavaScript with custom CSS

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Chrome/Chromium (downloaded automatically by PuppeteerSharp)

## Getting Started

### Release version

1. **Download the latest release** from the [Releases](https://github.com/AlejandroAmayaIzquierdo/WattRescue/releases)
2. **Extract the ZIP file** to your desired location
3. **Run the application** by executing:

   ```bash
   dotnet WattRescue.dll
   ```

   Or simply double-click the `WattRescue.exe` file

### Docker

1. **Build and start the Docker containers**
   ```bash
   docker compose up --build -d
   ```
   - You can modify the `docker-compose.yml` file to use a different volume.
2. **Access the application** at `http://localhost:5000`

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/WattRescue.git
   cd WattRescue
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to `https://localhost:5001`

## Project Structure

```
WattRescue/
├── Controllers/        # API endpoints
├── Data/              # DbContext and database configuration
├── Dtos/              # Data transfer objects
├── Jobs/              # Quartz background jobs
├── Migrations/        # EF Core migrations
├── Models/            # Entity models (Story, Part, Paragraphs)
├── Pages/             # Razor Pages (UI)
│   ├── Index          # Home/Story list
│   ├── Reader/        # Story reader
│   └── Scrape/        # Scrape new stories
├── Services/          # Business logic
│   ├── ScrapperService.cs   # Web scraping logic
│   └── StoriesService.cs    # Story CRUD operations
└── wwwroot/           # Static files (CSS, JS)
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/scrape/start` | Start scraping a story |
| GET | `/api/scrape/progress/{storyId}` | Get scraping progress |

## Configuration

Database connection string is configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Local": "Data Source=wattrescue.db"
  }
}
```

## License

MIT License
