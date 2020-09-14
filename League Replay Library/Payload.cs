namespace LeagueReplayLibrary
{
    public class Payload
    {
        public ushort id;
        public byte[] data;
    }

    public class ChunkInfo
    {
        public uint Duration { get; set; }
        public ushort Id { get; set; }
        public string ReceivedTime { get; set; }
    }

    public class KeyframeInfo
    {
        public uint NextChunkId { get; set; }
        public ushort Id { get; set; }
        public string ReceivedTime { get; set; }
    }
}
