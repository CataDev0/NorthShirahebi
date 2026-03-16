# NorthShirahebi

A [Medusa](https://nadekobot.readthedocs.io/en/latest/medusa/creating-a-medusa/) module (plugin) for [NadekoBot](https://gitlab.com/kwoth/nadekobot).

## Features

- **Waifu Pics**: Fetches anime images and reaction gifs from [waifu.pics](https://waifu.pics/).
- **Weather**: Provides current weather and 5-day forecasts using [OpenWeatherMap](https://openweathermap.org/).

## Commands

### Social & Images
- `.hug [@user]` - Hug someone.
- `.kiss [@user]` - Kiss someone.
- `.pat [@user]` - Pat someone.
- `.cuddle [@user]` - Cuddle someone.
- `.wave [@user]` - Wave at someone.
- `.waifu` - Get a random waifu image.
- `.neko` - Get a random catgirl image.
- `.shinobu` - Get a random Shinobu image.
- `.megumin` - Get a random Megumin image.

### Weather
- `.weather <location>` - Top level command shows the current weather for a specified location.
- `.forecast <location>` - Shows a 5-day weather forecast for a specified location.

## Configuration

To use the weather commands, you must obtain an API key from [OpenWeatherMap](https://openweathermap.org/) and set it as an environment variable named `OPENWEATHERMAP_API_KEY` before starting your bot.

```bash
export OPENWEATHERMAP_API_KEY="your_api_key_here"
```

## Installation

1. Open your terminal and navigate to the directory containing your bot's root folder.
2. Run the following command to download and extract the module:

    ```bash
    mkdir -p nadekobot/output/data/medusae/NorthShirahebi && curl -sL https://github.com/cataclym/NorthShirahebi/releases/latest/download/NorthShirahebi.tar.gz \
   | tar xzv -C nadekobot/output/data/medusae/NorthShirahebi
    ```

3. Start your bot.
4. Load the module by running:
    `.meload NorthShirahebi`
