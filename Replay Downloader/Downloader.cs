using LeagueReplayLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReplayDownloader
{
    public class Downloader : INotifyPropertyChanged
    {
        private LeagueArgs _leagueArgs;
        public LeagueArgs LeagueArgs
        {
            get => _leagueArgs;
            set {
                if (_leagueArgs?.eKey != value.eKey ||
                   _leagueArgs?.GameId != value.GameId ||
                   _leagueArgs?.Region != value.Region)
                {
                    _leagueArgs = value;
                    OnPropertyChanged("LeagueArgs");
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
            for(; ; )
            {
                this.LeagueArgs = GetLeagueArgs() ?? this.LeagueArgs;
                await Task.Delay(10000);
            }
        }

        private async void LeagueArgsChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "LeagueArgs")
            {
                Console.WriteLine($"Starting download of replay: {LeagueArgs.Region} - {LeagueArgs.GameId} with key {LeagueArgs.eKey}");
                var replay = await Replay.DownloadReplay(LeagueArgs.Region, LeagueArgs.GameId, LeagueArgs.eKey);
                await replay.PackReplay(@$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/League of Legends/Spectator Replays/{LeagueArgs.Region}_{LeagueArgs.GameId}");
                Console.WriteLine($"Finished downloading replay:  {LeagueArgs.Region} - {LeagueArgs.GameId} with key {LeagueArgs.eKey}");
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
                    return new LeagueArgs() { eKey = spectatorArgs[2], GameId = ulong.Parse(spectatorArgs[3]), Region = Enum.Parse<Region>(spectatorArgs[4]) };
                }
            }
            return null;
        }

    }
}
