using System;
using System.Collections.Generic;
using System.Text;

namespace LeagueReplayLibrary
{
    internal class MetaData
    {
        internal GameKey gameKey;
        internal string gameServerAddress = "";
        internal int port = 0;
        internal string encryptionKey = "";
        internal int chunkTimeInterval;
        internal string startTime = "";
        internal string endTime = "";
        internal bool gameEnded;
        internal int lastChunkId;
        internal int lastKeyFrameId;
        internal int endStartupChunkId;
        internal int delayTime;
        internal List<ChunkInfo> pendingAvailableChunkInfo;
        internal List<KeyframeInfo> pendingAvailableKeyFrameInfo;
        internal int keyFrameTimeInterval;
        internal string decodedEncryptionKey;
        internal int startGameChunkId;
        internal int gameLength;
        internal int clientAddedLag;
        internal bool clientBackFetchingEnabled;
        internal int clientBackFetchingFreq;
        internal int interestScore;
        internal bool featuredGame;
        internal string createTime;
        internal int endGameChunkId;
        internal int endGameKeyFrameId;
    }

    internal class GameKey
    {
        internal ulong gameId;
        internal Region platformId;
    }
}
