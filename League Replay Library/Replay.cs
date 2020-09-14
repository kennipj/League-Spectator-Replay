using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeagueReplayLibrary
{
    public class Replay
    {

        public ushort StartGameChunkId { get; set; }
        public ushort EndStartupChunkId { get; set; }
        public ushort EndGameChunkId { get; set; }
        public bool GameEnded { get; set; }

        public List<Payload> KeyFrames { get; set; }
        public List<Payload> Chunks { get; set; }
        public LastChunkInfo ChunkInfo { get; set; }


        public GameKey GameKey { get; set; }

        public string eKey { get; set; }

        public static async Task<Replay> DownloadReplay(GameKey gameKey, string encryptionKey)
        {
            var replay = new Replay()
            {
                eKey = encryptionKey,
                GameKey = gameKey,
                Chunks = new List<Payload>(),
                KeyFrames = new List<Payload>(),
            };
            await replay.GetPast();
            await replay.GetCurrent();
            return replay;
        }

        public static async Task<Replay> ReadReplay(string path) => await Packer.UnpackReplay(path);

        public LastChunkInfo CreateLastChunkInfo()
        {
            return new LastChunkInfo()
            {
                ChunkId = 1,
                AvailableSince = 30000,
                NextAvailableChunk = 100,
                KeyFrameId = 1,
                NextChunkId = 1,
                EndStartupChunkId = this.EndStartupChunkId,
                StartGameChunkId = this.StartGameChunkId,
                EndGameChunkId = this.EndGameChunkId,
                Duration = 30000
            };
        }

        public MetaData CreateMeta()
        {
            var meta = new MetaData()
            {
                GameKey = this.GameKey,
                ChunkTimeInterval = 30000,
                GameEnded = GameEnded,
                LastChunkId = this.Chunks[^1].id,
                LastKeyFrameId = this.KeyFrames[^1].id,
                EndStartupChunkId = 1,
                DelayTime = 180000,
                KeyFrameTimeInterval = 60000,
                StartGameChunkId = 2,
                ClientAddedLag = 0,
                ClientBackFetchingEnabled = false,
                ClientBackFetchingFreq = 1000,
                FeaturedGame = false,
                EndGameChunkId = this.EndGameChunkId,
                EndGameKeyFrameId = this.KeyFrames[^1].id,
                PendingAvailableChunkInfo = new List<ChunkInfo>(),
                PendingAvailableKeyFrameInfo = new List<KeyframeInfo>()
            };

            foreach (var chunk in this.Chunks)
                meta.PendingAvailableChunkInfo.Add(new ChunkInfo()
                {
                    Duration = 30000,
                    Id = chunk.id,
                    ReceivedTime = ""
                });

            foreach (var keyframe in this.KeyFrames)
                meta.PendingAvailableKeyFrameInfo.Add(new KeyframeInfo()
                {
                    NextChunkId = (uint)(keyframe.id - 1) * 2 + this.StartGameChunkId,
                    Id = keyframe.id,
                    ReceivedTime = ""
                });

            return meta;
        }

        public async Task PackReplay(string outdir, bool overwrite = false) => await Packer.PackReplay(this, outdir, overwrite);

        private async Task<Payload> GetChunk(int id) => new Payload() { id = (ushort)id, data = await ReplayDownloader.GetChunkData(this.GameKey, id) };
        private async Task<Payload> GetKeyFrame(int id) => new Payload() { id = (ushort)id, data = await ReplayDownloader.GetKeyframeData(this.GameKey, id) };

        private async Task GetCurrent()
        {
            this.ChunkInfo = (await ReplayDownloader.GetChunkInfo(this.GameKey)).ToObject<LastChunkInfo>();
            if (this.ChunkInfo.EndGameChunkId > 0 && this.ChunkInfo.EndGameChunkId <= this.ChunkInfo.ChunkId)
            {
                this.EndStartupChunkId = (ushort)this.ChunkInfo.EndStartupChunkId;
                this.StartGameChunkId = (ushort)this.ChunkInfo.StartGameChunkId;
                this.Chunks.Add(await GetChunk(this.ChunkInfo.ChunkId));
                return;
            }

            if ((!this.Chunks.Any() ? 0 : this.Chunks[^1].id) < this.ChunkInfo.ChunkId)
                this.Chunks.Add(await GetChunk(this.ChunkInfo.ChunkId));

            if ((!this.KeyFrames.Any() ? 0 : this.KeyFrames[^1].id) < this.ChunkInfo.KeyFrameId)
                this.KeyFrames.Add(await GetKeyFrame(this.ChunkInfo.KeyFrameId));

            await Task.Delay(this.ChunkInfo.NextAvailableChunk + 500);
            await GetCurrent();
        }

        private async Task GetPast()
        {
            this.ChunkInfo = (await ReplayDownloader.GetChunkInfo(this.GameKey)).ToObject<LastChunkInfo>();
            for (ushort k = 1; k <= this.ChunkInfo.ChunkId; k++)
                this.Chunks.Add(await GetChunk(k));

            for (ushort k = 1; k <= this.ChunkInfo.KeyFrameId; k++)
                this.KeyFrames.Add(await GetKeyFrame(k));
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
