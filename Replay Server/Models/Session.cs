using LeagueReplayLibrary;
using System;
using System.Net;

namespace Replay_Server.Models
{
    public class Session : IEquatable<Session>
    {
        public IPAddress IP { get; set; }
        public GameKey GameKey { get; set; }

        public bool Equals(Session other) => (BitConverter.ToInt32(this.IP.GetAddressBytes()) == BitConverter.ToInt32(other.IP.GetAddressBytes())) && (GameKey.Equals(other.GameKey));
        public override int GetHashCode() => HashCode.Combine(IP, GameKey);

    }
}
