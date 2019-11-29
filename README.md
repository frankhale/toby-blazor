# Toby (Blazor)

This is a rewrite of [Toby](https://github.com/frankhale/toby) using C# and
Blazor. This project is using the server-side hosting model.

This is a simple YouTube player.

## Status

This is an effort to learn more about Blazor and how to build apps in it. I'm
definitely doing some funky stuff in here as I get my footing squared away. Some
refactoring is definitely in order as my initial assumptions are not making it
as easy as I'd like to implement new features.

We are using SQLite with Entity Framework Core which makes it drop dead easy to
interface with our database. I'm using [DB Browser](https://sqlitebrowser.org/)
as a way to manage the db graphically.

I'm using the latest Visual Studio 2019 Preview
with .NET Core 3.1 preview3 to compile and run this.

## Usage

NOTE: Currently the database shipping in the repo is a slighly modified one from
[Toby](https://github.com/frankhale/toby). It's pre-populated with videos I like
(LOL!). Use the command `/all` to list them or `/manage` to delete them all.

NOTE: If you want to search YouTube you'll need a YouTube Data API key. This
code needs to be added to an environment variable on your machine called
`YOUTUBE_API_KEY`.

To obtain a YouTube Data API key you need a Google account and need to go to
[Google's developer console](https://console.developers.google.com/) to obtain
one.

Search commands:

- `/all` or `/ls`: lists all videos in the database
- `/clear`: clear search results
- `/crp` or `/clear-recently-played`: clear recently played
- `/group [name]` or `/g [name]`: lists videos in a specific group
- `/favorites` or `/fav`: list favorite videos
- `/manage` or `/mg`: video management
- `/recently-played` or `/rp`: list all recently played videos
- `[search term]`: search locally for video
- `/youtube [search term]` or `/yt [search term]`: search YouTube for videos

**NOTE:** When using `manage` there is a search box to search within the videos
you are managing. You can use `/all` to show all videos after a narrow search.

The Recently Played videos are limited to 30 videos.

## Screenshots

![Basic UI](screenshots/one.PNG)

![Video Playback](screenshots/two.PNG)

![Video Management](screenshots/three.PNG)

## Folder Layout

- server: Blazor server-side project
- client: Electron and NW.js drivers (soon [Web Window support](https://blog.stevensanderson.com/2019/11/18/2019-11-18-webwindow-a-cross-platform-webview-for-dotnet-core/))

## Author(s)

Frank Hale &lt;frankhale@gmail.com&gt;

## Date

29 November 2019

## License

MIT - see [LICENSE](LICENSE.txt)
