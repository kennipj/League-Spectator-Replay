using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueReplayLibrary
{
    internal class Keyframe
    {
        internal ushort id;
        internal byte[] data;
    }

    internal class KeyframeInfo
    {
        internal uint nextChunkId;
        internal ushort id;
        internal string receivedTime;
    }
}
