using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueReplayLibrary
{
    public class Replay
    {
        private static Dictionary<Region, string> Servers = new Dictionary<Region, string>()
        {
            { Region.EUW1, "185.40.64.163:80" },
            { Region.EUN1, "185.40.64.164:8088" },
            { Region.NA1, "192.64.174.163:80" },
            { Region.KR, "45.250.208.30:80" },
            { Region.BR1, "66.151.33.19:80" },
            { Region.LA1, "66.151.33.19:80" },
            { Region.LA2, "66.151.33.19:80" },
            { Region.TR1, "185.40.64.165:80" },
            { Region.RU, "185.40.64.165:80" },
            { Region.JP1, "104.160.154.200:80" },
            { Region.PBE1, "192.64.168.88:80" },
            { Region.OC1, "192.64.169.29:80" },
        };

        private ushort startGameChunkId { get; set; }
        private ushort endStartupChunkId { get; set; }
        private ushort chunkId { get; set; }

        internal List<Keyframe> KeyFrames { get; set; }
        internal List<Chunk> Chunks { get; set; }
        internal LastChunkInfo ChunkInfo { get; set; }


        public Region Region { get; set; }
        public ulong GameId { get; set; }
        public string eKey { get; set; }

        public static async Task<Replay> DownloadReplay(Region region, ulong gameid, string encryptionKey)
        {
            var replay = new Replay()
            {
                eKey = encryptionKey,
                GameId = gameid,
                Region = region,
                Chunks = new List<Chunk>(),
                KeyFrames = new List<Keyframe>(),
            };
            //replay.metaData = await ReplayDownloader.GetMetaData(region, gameid);
            await replay.GetPast();
            await replay.GetCurrent();
            return replay;
        }

        public static async Task<Replay> ReadReplay(string path) => await Packer.UnpackReplay(path);


        public JObject CreateLastChunkInfo()
        {
            int nextChunkId;

            if (this.chunkId == this.Chunks[^1].id)
                nextChunkId = this.chunkId;
            else
                nextChunkId = this.chunkId + 1;

            return JObject.FromObject(new LastChunkInfo()
            {
                chunkId = this.chunkId,
                availableSince = 30000,
                nextAvailableChunk = 100,
                keyFrameId = this.chunkId / 2,
                nextChunkId = nextChunkId,
                endStartupChunkId = this.endStartupChunkId,
                startGameChunkId = this.startGameChunkId,
                endGameChunkId = this.Chunks[^1].id,
                duration = 30000
            });
        }

        public JObject CreateMeta()
        {
            var meta = new MetaData()
            {
                gameKey = new GameKey() { gameId = this.GameId, platformId = this.Region },
                chunkTimeInterval = 30000,
                gameEnded = true,
                lastChunkId = this.Chunks[^1].id,
                lastKeyFrameId = this.KeyFrames[^1].id,
                endStartupChunkId = 1,
                delayTime = 180000,
                keyFrameTimeInterval = 60000,
                startGameChunkId = 2,
                clientAddedLag = 0,
                clientBackFetchingEnabled = false,
                clientBackFetchingFreq = 1000,
                interestScore = 0,
                featuredGame = false,
                endGameChunkId = this.Chunks[^1].id,
                endGameKeyFrameId = this.KeyFrames[^1].id,
            };

            foreach(var chunk in this.Chunks)
                meta.pendingAvailableChunkInfo.Add(new ChunkInfo()
                {
                    duration = 30000,
                    id = chunk.id,
                    receivedTime = ""
                });

            foreach (var keyframe in this.KeyFrames)
                meta.pendingAvailableKeyFrameInfo.Add(new KeyframeInfo()
                {
                    nextChunkId = (uint) (keyframe.id - 1) * 2 + this.startGameChunkId,
                    id = keyframe.id,
                    receivedTime = ""
                });

            return JObject.FromObject(meta);
        }

        public async Task PackReplay(string outdir) => await Packer.PackReplay(this, outdir);

        private async Task<Chunk> GetChunk(int id) => new Chunk() { id = (ushort)id, data = await ReplayDownloader.GetChunkData(this.Region, this.GameId, id) };
        private async Task<Keyframe> GetKeyFrame(int id) => new Keyframe() { id = (ushort)id, data = await ReplayDownloader.GetChunkData(this.Region, this.GameId, id) };

        private async Task GetCurrent()
        {
            this.ChunkInfo = (await ReplayDownloader.GetChunkInfo(this.Region, this.GameId)).ToObject<LastChunkInfo>();
            if(this.ChunkInfo.endGameChunkId > 0 && this.ChunkInfo.endGameChunkId <= this.ChunkInfo.chunkId)
            {
                this.endStartupChunkId = (ushort) this.ChunkInfo.endStartupChunkId;
                this.startGameChunkId = (ushort) this.ChunkInfo.startGameChunkId;
                this.Chunks.Add(await GetChunk(this.ChunkInfo.chunkId));
                return;
            }

            if(this.Chunks[^1].id < this.ChunkInfo.chunkId)
            {
                Console.WriteLine($"Downloading Chunk {this.ChunkInfo.chunkId}");
                this.Chunks.Add(await GetChunk(this.ChunkInfo.chunkId));
            }

            if (this.KeyFrames[^1].id < this.ChunkInfo.keyFrameId)
            {
                Console.WriteLine($"Downloading Keyframe {this.ChunkInfo.keyFrameId}");
                this.KeyFrames.Add(await GetKeyFrame(this.ChunkInfo.keyFrameId));
            }

            await Task.Delay(this.ChunkInfo.nextAvailableChunk + 500);
            await GetCurrent();
        }

        private async Task GetPast()
        {
            this.ChunkInfo = (await ReplayDownloader.GetChunkInfo(this.Region, this.GameId)).ToObject<LastChunkInfo>();
            for(ushort k = 1; k <= this.ChunkInfo.chunkId; k++)
            {
                Console.WriteLine($"Downloading Chunk {k}");
                this.Chunks.Add(await GetChunk(k));
            }
            
            for(ushort k = 1; k <= this.ChunkInfo.keyFrameId; k++)
            {
                Console.WriteLine($"Downloading Keyframe {k}");
                this.KeyFrames.Add(await GetKeyFrame(k));
            }
        }
    }

    public enum Region
    {
        EUW1,
        EUN1,
        NA1,
        KR,
        BR1,
        LA1,
        LA2,
        TR1,
        RU,
        JP1,
        PBE1,
        OC1
    }
}
