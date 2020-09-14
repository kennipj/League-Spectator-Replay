using LeagueReplayLibrary;
using System;

namespace Replay_Server.Models
{
    public class ReplayCache
    {
        public Replay Replay { get; set; }
        public DateTime Requested { get; set; }
    }
}
