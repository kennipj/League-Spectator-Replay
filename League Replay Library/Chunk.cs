using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueReplayLibrary
{
    internal class Chunk
    {
        internal ushort id;
        internal byte[] data; 
    }

    internal class ChunkInfo
    {
        internal uint duration;
        internal ushort id;
        internal string receivedTime;
    }

    public class LastChunkInfo
    {
        public int chunkId;
        public int availableSince;
        public int nextAvailableChunk;
        public int keyFrameId;
        public int nextChunkId;
        public int endStartupChunkId;
        public int startGameChunkId;
        public int endGameChunkId;
        public int duration;
    }
}
