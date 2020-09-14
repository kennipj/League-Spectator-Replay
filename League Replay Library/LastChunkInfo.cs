namespace LeagueReplayLibrary
{
    public class LastChunkInfo
    {
        public int ChunkId { get; set; }
        public int AvailableSince { get; set; }
        public int NextAvailableChunk { get; set; }
        public int KeyFrameId { get; set; }
        public int NextChunkId { get; set; }
        public int EndStartupChunkId { get; set; }
        public int StartGameChunkId { get; set; }
        public int EndGameChunkId { get; set; }
        public int Duration { get; set; }

        public void Update(int lastChunkId, int lastKeyFrameId)
        {
            this.ChunkId = NextChunkId;
            this.NextChunkId = this.ChunkId == lastChunkId ? this.ChunkId : this.ChunkId + 1;
            this.KeyFrameId = this.ChunkId / 2 <= lastKeyFrameId ? this.ChunkId / 2 : lastKeyFrameId;
        }
    }
}
