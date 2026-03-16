using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Discord;
using NadekoBot.Medusa;

namespace NorthShirahebi;

public sealed class Shrine : Snek
{
    public new string Name = "Shrine";
    private static readonly HttpClient Client = new HttpClient();
    
    [svc(Lifetime.Singleton)]
    public sealed class ShrineService
    {
        private const string WeatherBaseUrl = "https://api.openweathermap.org/data/2.5";

        public async Task<string> GetWaifuPicsImage(ImageType imageType)
        {
            var img = await Client
                .GetFromJsonAsync<WaifuData>($"https://waifu.pics/api/sfw/{imageType}")
                .ConfigureAwait(false);
        
            return img.URL;
        }
        
        public async Task SendWaifuPicsEmbedAsync(AnyContext ctx, ImageType imageType, string text = null)
        {
            var emb = new EmbedBuilder();

            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            var img = await GetWaifuPicsImage(imageType);

            if (!string.IsNullOrWhiteSpace(img))
            {
                emb.WithImageUrl(img);
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                switch (imageType)
                {
                    case ImageType.hug:
                        emb.WithDescription($"{ctx.User.Mention} hugged {text}");
                        break;
                    case ImageType.pat:
                        emb.WithDescription($"{ctx.User.Mention} petted {text}");
                        break;
                    case ImageType.kiss:
                        emb.WithDescription($"{ctx.User.Mention} kissed {text}");
                        break;
                    case ImageType.wave:
                        emb.WithDescription($"{ctx.User.Mention} waved to {text}");
                        break;
                    case ImageType.cuddle:
                        emb.WithDescription($"{ctx.User.Mention} cuddled {text}");
                        break;
                }
            }
            await ctx.Channel.EmbedAsync(emb);
        }
        
        public async Task SendWaifuPicsEmbedAsync(AnyContext ctx, ImageType imageType)
        {
            var emb = new EmbedBuilder();

            await ctx.Channel.TriggerTypingAsync().ConfigureAwait(false);

            var img = await GetWaifuPicsImage(imageType);

            if (!string.IsNullOrWhiteSpace(img))
            {
                emb.WithImageUrl(img);
            }

            await ctx.Channel.EmbedAsync(emb);
        }

        public async Task SendWeatherEmbedAsync(AnyContext ctx, string location)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                await ctx.Channel.SendMessageAsync("API Key not found in environment variables (OPENWEATHERMAP_API_KEY).");
                return;
            }

            try 
            {
                var response = await Client.GetAsync($"{WeatherBaseUrl}/weather?q={Uri.EscapeDataString(location)}&APPID={apiKey}&units=metric");
                if (!response.IsSuccessStatusCode)
                {
                    await HandleApiError(ctx, response, location, "weather");
                    return;
                }
                
                var data = await response.Content.ReadFromJsonAsync<WeatherResponse>();
                if (data == null || data.main == null) throw new Exception("Invalid data received");

                var embed = new EmbedBuilder()
                    .WithTitle($"Weather in {data.name}, {data.sys.country} {CountryCodeToFlag(data.sys.country)}")
                    .WithDescription($"{GetWeatherIcon(data.weather[0].icon)} {data.weather[0].main} - {data.weather[0].description}")
                    .WithThumbnailUrl($"https://openweathermap.org/img/wn/{data.weather[0].icon}@2x.png")
                    .AddField("🌡️ Temperature", $"{Math.Round(data.main.temp)}°C / {Math.Round(CelsiusToFahrenheit(data.main.temp))}°F", true)
                    .AddField("🤔 Feels Like", $"{Math.Round(data.main.feels_like)}°C / {Math.Round(CelsiusToFahrenheit(data.main.feels_like))}°F", true)
                    .AddField("📊 Min/Max", $"{Math.Round(data.main.temp_min)}°C / {Math.Round(data.main.temp_max)}°C", true)
                    .AddField("💨 Wind", $"{Math.Round(data.wind.speed)} m/s ({Math.Round(MpsToMph(data.wind.speed))} mph) {DegToCompass(data.wind.deg)} ({data.wind.deg}°)", true)
                    .AddField("💧 Humidity", $"{data.main.humidity}%", true)
                    .AddField("☁️ Cloudiness", $"{data.clouds.all}%", true)
                    .AddField("👁️ Visibility", $"{data.visibility / 1000.0:F1} km", true)
                    .AddField("🔽 Pressure", $"{data.main.pressure} hPa", true)
                    .AddField("🌅 Sunrise/Sunset", $"{UnixTimeStampToDateTime(data.sys.sunrise):HH:mm} / {UnixTimeStampToDateTime(data.sys.sunset):HH:mm}", true);

                if (data.rain != null)
                {
                    var val = data.rain.OneHour != 0 ? data.rain.OneHour : data.rain.ThreeHour;
                    var tf = data.rain.OneHour != 0 ? "1h" : "3h";
                    if (val != 0) embed.AddField("🌧️ Rain", $"{val} mm ({tf})", true);
                }
                 if (data.snow != null)
                {
                    var val = data.snow.OneHour != 0 ? data.snow.OneHour : data.snow.ThreeHour;
                    var tf = data.snow.OneHour != 0 ? "1h" : "3h";
                    if (val != 0) embed.AddField("❄️ Snow", $"{val} mm ({tf})", true);
                }

                embed.WithFooter("Weather data provided by OpenWeatherMap")
                     .WithColor(Color.Green);

                await ctx.Channel.EmbedAsync(embed);
            }
            catch (Exception ex)
            {
                 await ctx.Channel.SendMessageAsync("Error fetching weather: " + ex.Message);
            }
        }

        public async Task SendForecastEmbedAsync(AnyContext ctx, string location)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                await ctx.Channel.SendMessageAsync("API Key not found.");
                return;
            }

            try
            {
                var response = await Client.GetAsync($"{WeatherBaseUrl}/forecast?q={Uri.EscapeDataString(location)}&APPID={apiKey}&units=metric");
                 if (!response.IsSuccessStatusCode)
                {
                    await HandleApiError(ctx, response, location, "forecast");
                    return;
                }

                var data = await response.Content.ReadFromJsonAsync<ForecastResponse>();
                if (data == null || data.list == null || data.list.Count == 0) throw new Exception("Invalid forecast data received");

                var dailyForecasts = data.list
                    .GroupBy(x => x.dt_txt.Split(' ')[0])
                    .ToDictionary(g => g.Key, g => g.ToList());

                var embed = new EmbedBuilder()
                    .WithTitle($"Forecast for {data.city.name}, {data.city.country} {CountryCodeToFlag(data.city.country)}")
                    .WithFooter("Forecast provided by OpenWeatherMap")
                    .WithColor(Color.Green);

                if (dailyForecasts.Any())
                {
                    var firstDayItems = dailyForecasts.First().Value;
                    if(firstDayItems.Count > 0)
                    {
                        var iconCode = firstDayItems[firstDayItems.Count / 2].weather[0].icon;
                        embed.WithThumbnailUrl($"https://openweathermap.org/img/wn/{iconCode}@2x.png");
                    }
                }

                foreach(var kvp in dailyForecasts)
                {
                    var dateStr = kvp.Key;
                    var items = kvp.Value;
                    
                    var targets = new int[] { 9, 15, 21 };
                    var selectedItems = new ForecastItem[3];

                    for(int i=0; i<3; i++)
                    {
                         var target = targets[i];
                         var candidate = items.OrderBy(item => Math.Abs(int.Parse(item.dt_txt.Split(' ')[1].Split(':')[0]) - target)).FirstOrDefault();
                         if (candidate != null && Math.Abs(int.Parse(candidate.dt_txt.Split(' ')[1].Split(':')[0]) - target) <= 3)
                         {
                             selectedItems[i] = candidate;
                         }
                    }

                    var seen = new HashSet<long>();
                    for(int i=0; i<3; i++)
                    {
                        if(selectedItems[i] != null)
                        {
                            if(seen.Contains(selectedItems[i].dt)) selectedItems[i] = null;
                            else seen.Add(selectedItems[i].dt);
                        }
                    }

                    if(DateTime.TryParse(dateStr, out var dateObj))
                    {
                        var displayDate = $"{dateObj:ddd} {dateObj.Day}";
                        var dailyMax = Math.Round(items.Max(x => x.main.temp_max));
                        var dailyMin = Math.Round(items.Min(x => x.main.temp_min));
                        
                        for(int i=0; i<3; i++)
                        {
                            var colName = (i == 0) ? $"**{displayDate}** ({dailyMax}°C / {dailyMin}°C)" : "\u200b";
                            var item = selectedItems[i];
                            
                            if (item != null)
                            {
                                var temp = Math.Round(item.main.temp);
                                var time = item.dt_txt.Split(' ')[1].Substring(0, 5);
                                var weatherDesc = item.weather[0].main;
                                var icon = GetWeatherIcon(item.weather[0].icon);
                                var pop = Math.Round(item.pop * 100);
                                var popStr = pop > 0 ? $" 💧{pop}%" : "";

                                embed.AddField(colName, $"`{time}` **{temp}°C** {icon}{popStr}\n*{weatherDesc}*", true);
                            }
                            else
                            {
                                embed.AddField(colName, "\u200b", true);
                            }
                        }
                    }
                }
                
                await ctx.Channel.EmbedAsync(embed);
            }
            catch(Exception ex)
            {
                await ctx.Channel.SendMessageAsync("Error fetching forecast: " + ex.Message);
            }
        }

        private async Task HandleApiError(AnyContext ctx, HttpResponseMessage response, string location, string type)
        {
             string errorMsg = "Unknown error";
             try {
                 var errData = await response.Content.ReadFromJsonAsync<WeatherErrorResponse>();
                 if (errData != null) errorMsg = errData.message;
             } catch {}

             var desc = $"Error {response.StatusCode}: {errorMsg}";
             if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                 desc = $"Location **{location}** not found.";
             else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                 desc = "Invalid API key.";

             var embed = new EmbedBuilder()
                .WithTitle("Error")
                .WithDescription(desc)
                .WithColor(Color.Red);
             
             await ctx.Channel.EmbedAsync(embed);
        }

        private double CelsiusToFahrenheit(double c) => (c * 9) / 5 + 32;
        private double MpsToMph(double mps) => mps * 2.23694;
        private string DegToCompass(double deg)
        {
            string[] directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            return directions[(int)Math.Round(((double)deg % 360) / 45) % 8];
        }
        private string CountryCodeToFlag(string countryCode)
        {
            return string.Concat(countryCode.ToUpper().Select(c => char.ConvertFromUtf32(c + 127397)));
        }
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private string GetWeatherIcon(string iconCode)
        {
            var icons = new Dictionary<string, string>
            {
                {"01d", "☀️"}, {"01n", "🌙"}, {"02d", "⛅"}, {"02n", "☁️"},
                {"03d", "☁️"}, {"03n", "☁️"}, {"04d", "☁️"}, {"04n", "☁️"},
                {"09d", "🌧️"}, {"09n", "🌧️"}, {"10d", "🌦️"}, {"10n", "🌧️"},
                {"11d", "⛈️"}, {"11n", "⛈️"}, {"13d", "❄️"}, {"13n", "❄️"},
                {"50d", "🌫️"}, {"50n", "🌫️"}
            };
            return icons.ContainsKey(iconCode) ? icons[iconCode] : "";
        }
    }
    
    public enum ImageType  
    {
        hug,
        pat,
        kiss,
        wave,
        cuddle,
        waifu,
        neko,
        shinobu,
        megumin
    }
    
    public sealed class SocialInteractions(ShrineService service) : Snek
    {
        [cmd]
        public async Task Hug(AnyContext ctx, [leftover] string text = null)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.hug, text);
        }

        [cmd]
        public async Task Pat(AnyContext ctx, [leftover] string text = null)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.pat, text);
        }

        [cmd]
        public async Task Kiss(AnyContext ctx, [leftover] string text = null)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.kiss, text);
        }

        [cmd]
        public async Task Wave(AnyContext ctx, [leftover] string text = null)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.wave, text);
        }

        [cmd]
        public async Task Cuddle(AnyContext ctx, [leftover] string text = null)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.cuddle, text);
        }
    }

    public sealed class Images(ShrineService service) : Snek
    {
        [cmd]
        public async Task Waifu(AnyContext ctx)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.waifu);
        }

        [cmd]
        public async Task Neko(AnyContext ctx)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.neko);
        }

        [cmd]
        public async Task Shinobu(AnyContext ctx)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.shinobu);
        }
        
        [cmd]
        public async Task Megumin(AnyContext ctx)
        {
            await service.SendWaifuPicsEmbedAsync(ctx, ImageType.megumin);
        }

    }

    public sealed class WeatherCommands(ShrineService service) : Snek
    {
        [cmd]
        public async Task Weather(AnyContext ctx, [leftover] string query)
        {
             if (string.IsNullOrWhiteSpace(query)) return;
             await service.SendWeatherEmbedAsync(ctx, query);
        }

        [cmd]
        public async Task Forecast(AnyContext ctx, [leftover] string query)
        {
             if (string.IsNullOrWhiteSpace(query)) return;
             await service.SendForecastEmbedAsync(ctx, query);
        }
    }
}