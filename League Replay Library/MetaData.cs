using LeagueReplayLibrary.Structs;
using System.Collections.Generic;

namespace LeagueReplayLibrary
{
    public class MetaData
    {
        public GameKey GameKey { get; set; }
        public string gameServerAddress { get; set; }
        public int Port { get; set; }
        public string EncryptionKey { get; set; }
        public int ChunkTimeInterval { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool GameEnded { get; set; }
        public int LastChunkId { get; set; }
        public int LastKeyFrameId { get; set; }
        public int EndStartupChunkId { get; set; }
        public int DelayTime { get; set; }
        public List<ChunkInfo> PendingAvailableChunkInfo { get; set; }
        public List<KeyframeInfo> PendingAvailableKeyFrameInfo { get; set; }
        public int KeyFrameTimeInterval { get; set; }
        public string DecodedEncryptionKey { get; set; }
        public int StartGameChunkId { get; set; }
        public int GameLength { get; set; }
        public int ClientAddedLag { get; set; }
        public bool ClientBackFetchingEnabled { get; set; }
        public int ClientBackFetchingFreq { get; set; }
        public int InterestScore { get; set; }
        public bool FeaturedGame { get; set; }
        public string CreateTime { get; set; }
        public int EndGameChunkId { get; set; }
        public int EndGameKeyFrameId { get; set; }


        public static MetaData CreateFromHeader(Header header)
        {
            var meta = new MetaData()
            {
                GameKey = new GameKey() { GameId = header.gameId, PlatformId = (Region)header.region },
                ChunkTimeInterval = 30000,
                GameEnded = true,
                LastChunkId = header.chunkCount,
                LastKeyFrameId = header.keyframeCount,
                EndStartupChunkId = header.endStartupChunkId,
                DelayTime = 180000,
                KeyFrameTimeInterval = 60000,
                StartGameChunkId = header.startGameChunkId,
                ClientAddedLag = 0,
                ClientBackFetchingEnabled = true,
                ClientBackFetchingFreq = 100,
                InterestScore = 0,
                FeaturedGame = false,
                EndGameChunkId = header.endGameChunkId,
                EndGameKeyFrameId = header.keyframeCount,
                PendingAvailableChunkInfo = new List<ChunkInfo>(),
                PendingAvailableKeyFrameInfo = new List<KeyframeInfo>(),
            };

            for (ushort k = 1; k <= header.chunkCount; k++)
                meta.PendingAvailableChunkInfo.Add(new ChunkInfo()
                {
                    Duration = 30000,
                    Id = k,
                    ReceivedTime = ""
                });

            for (ushort k = 1; k <= header.keyframeCount; k++)
                meta.PendingAvailableKeyFrameInfo.Add(new KeyframeInfo()
                {
                    NextChunkId = (uint)(k - 1) * 2 + header.startGameChunkId,
                    Id = k,
                    ReceivedTime = ""
                });

            return meta;
        }
    }
}
