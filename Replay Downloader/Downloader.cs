using LeagueReplayLibrary;
using System;
using System.ComponentModel;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ReplayDownloader
{
    public class Downloader : INotifyPropertyChanged
    {
        private LeagueArgs _leagueArgs;
        public LeagueArgs LeagueArgs
        {
            get => _leagueArgs;
            set
            {
                if (_leagueArgs == null || !_leagueArgs.Equals(value))
                {
                    _leagueArgs = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Property Management

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public async Task Start()
        {
            PropertyChanged += LeagueArgsChanged;
            for (; ; )
            {
                this.LeagueArgs = GetLeagueArgs() ?? this.LeagueArgs;
                await Task.Delay(5000);
            }
        }

        private async void LeagueArgsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LeagueArgs")
            {
                Console.WriteLine($"Starting download of replay: {LeagueArgs.GameKey.PlatformId} - {LeagueArgs.GameKey.GameId} with key {LeagueArgs.eKey}");
                var replay = await Replay.DownloadReplay(LeagueArgs.GameKey, LeagueArgs.eKey);
                await replay.PackReplay(@$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/League of Legends/Spectator Replays/{LeagueArgs.GameKey.PlatformId}_{LeagueArgs.GameKey.GameId}");
                Console.WriteLine($"Finished downloading replay:  {LeagueArgs.GameKey.PlatformId} - {LeagueArgs.GameKey.GameId} with key {LeagueArgs.eKey}");
            }
        }

        private LeagueArgs GetLeagueArgs()
        {
            string query = "SELECT Name, CommandLine FROM Win32_Process";
            string wmiScope = @$"\\{Environment.MachineName}/root/cimv2";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiScope, query);
            foreach (var mo in searcher.Get())
            {
                if ((string)mo["Name"] == "League of Legends.exe")
                {
                    var args = (string)mo["CommandLine"];
                    var idx = args.IndexOf("spectator");
                    var endIdx = args[idx..].IndexOf('\"');
                    var spectatorArgs = args[idx..(endIdx + idx)].Split(" ");
                    return new LeagueArgs() { eKey = spectatorArgs[2], GameKey = new GameKey() { GameId = ulong.Parse(spectatorArgs[3]), PlatformId = Enum.Parse<Region>(spectatorArgs[4]) } };
                }
            }
            return null;
        }

    }
}
