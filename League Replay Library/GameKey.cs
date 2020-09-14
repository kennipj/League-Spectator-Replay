using System;

namespace LeagueReplayLibrary
{
    public class GameKey : IEquatable<GameKey>
    {
        public ulong GameId { get; set; }
        public Region PlatformId { get; set; }

        public bool Equals(GameKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return (this.GameId == other.GameId) && (this.PlatformId == other.PlatformId);
        }

        public override int GetHashCode() => HashCode.Combine(this.GameId, this.PlatformId);
    }
}
