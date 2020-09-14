
using LeagueReplayLibrary;
using System;

namespace ReplayDownloader
{
    public class LeagueArgs : IEquatable<LeagueArgs>
    {
        public string eKey { get; set; }
        public GameKey GameKey { get; set; }

        public bool Equals(LeagueArgs other) => (this.eKey == other.eKey) && (this.GameKey.Equals(other.GameKey));

        public override int GetHashCode() => HashCode.Combine(this.eKey, this.GameKey);
    }
}
