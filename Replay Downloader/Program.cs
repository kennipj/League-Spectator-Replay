using EntryPoint;
using LeagueReplayLibrary;
using ReplayDownloader;
using System;

namespace Replay_Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = Cli.Parse<AppSettings>(args);
            if (arguments.eKey != default &&
                      arguments.GameId != default &&
                      arguments.Region != default)
            {
                var replay = Replay.DownloadReplay(new GameKey() { GameId = arguments.GameId, PlatformId = Enum.Parse<Region>(arguments.Region) }, arguments.eKey).GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(arguments.Output))
                    replay.PackReplay(arguments.Output).GetAwaiter().GetResult();
                else
                    replay.PackReplay($"{arguments.Region}_{arguments.GameId}").GetAwaiter().GetResult();
            }
            else
            {
                var downloader = new Downloader();
                downloader.Start().Wait();
            }

        }

    }
}
