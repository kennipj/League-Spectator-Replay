using LeagueReplayLibrary;
using Replay_Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Replay_Server
{
    public class ReplayService
    {
        private Dictionary<GameKey, ReplayMeta> _replayLookup = new Dictionary<GameKey, ReplayMeta>();
        private Dictionary<GameKey, ReplayCache> _replayCache = new Dictionary<GameKey, ReplayCache>();
        private Dictionary<Session, LastChunkInfo> _chunkInfos = new Dictionary<Session, LastChunkInfo>();

        private readonly ReplayServiceSettings _replayServiceSettings;

        public ReplayService(ReplayServiceSettings replayServiceSettings)
        {
            _replayServiceSettings = replayServiceSettings;
        }

        public async Task<MetaData> GetMetaData(GameKey gameKey, string id)
        {
            if (!await CheckLookup(gameKey))
                return null;

            return _replayLookup[gameKey].MetaData;
        }

        public async Task<LastChunkInfo> GetLastChunkInfo(GameKey gameKey, int id, IPAddress ip)
        {
            var session = new Session() { IP = ip, GameKey = gameKey };

            if (!await CheckLookup(gameKey))
                return null;

            if (!_replayCache.ContainsKey(gameKey))
                await CacheReplay(_replayLookup[gameKey].Uri);

            if (!_chunkInfos.ContainsKey(session))
                _chunkInfos.Add(session, new LastChunkInfo()
                {
                    ChunkId = 1,
                    AvailableSince = 30000,
                    NextAvailableChunk = 100,
                    KeyFrameId = 1,
                    NextChunkId = 1,
                    EndStartupChunkId = _replayCache[gameKey].Replay.EndStartupChunkId,
                    StartGameChunkId = _replayCache[gameKey].Replay.StartGameChunkId,
                    EndGameChunkId = _replayCache[gameKey].Replay.EndGameChunkId,
                    Duration = 30000
                });
            else
                _chunkInfos[session].Update(_replayCache[gameKey].Replay.EndGameChunkId, _replayCache[gameKey].Replay.KeyFrames.Count);

            var chunkInfo = _chunkInfos[session];
            if (chunkInfo.EndGameChunkId == chunkInfo.ChunkId)
                _chunkInfos.Remove(session);

            return chunkInfo;
        }

        public async Task<byte[]> GetChunk(GameKey gameKey, int id)
        {
            if (!await CheckLookup(gameKey))
                return null;

            if (!_replayCache.ContainsKey(gameKey))
                await CacheReplay(_replayLookup[gameKey].Uri);

            return _replayCache[gameKey].Replay.Chunks.Find(x => x.id == id).data;

        }
        public async Task<byte[]> GetKeyFrame(GameKey gameKey, int id)
        {
            if (!await CheckLookup(gameKey))
                return null;

            if (!_replayCache.ContainsKey(gameKey))
                await CacheReplay(_replayLookup[gameKey].Uri);

            return _replayCache[gameKey].Replay.KeyFrames.Find(x => x.id == id).data;
        }

        public async Task<string> GetEndStats(GameKey gameKey, int id) => null;


        private async Task UpdateLookup()
        {
            var files = Directory.GetFiles(_replayServiceSettings.ReplayDirectory);
            foreach (var file in files)
            {
                if (Path.GetExtension(file).ToLower() == ".lsrp")
                {
                    using (var fs = new FileStream(file, FileMode.Open))
                    {
                        var header = await Packer.UnpackHeader(fs);
                        var key = new GameKey() { GameId = header.gameId, PlatformId = (Region)header.region };
                        this._replayLookup[key] = new ReplayMeta() { Uri = new Uri(file), MetaData = MetaData.CreateFromHeader(header) };
                    }
                }
            }
        }

        private async Task<bool> CheckLookup(GameKey gameKey)
        {
            if (!_replayLookup.ContainsKey(gameKey))
            {
                await UpdateLookup();
                if (!_replayLookup.ContainsKey(gameKey))
                    return false;
            }
            return true;
        }

        private async Task<ReplayCache> CacheReplay(Uri uri)
        {
            var replay = await Replay.ReadReplay(uri.LocalPath);
            if (!_replayCache.ContainsKey(replay.GameKey))
                _replayCache[replay.GameKey] = new ReplayCache() { Replay = replay, Requested = DateTime.Now };

            RefreshCache();

            return _replayCache[replay.GameKey];
        }

        private void RefreshCache()
        {
            foreach (var key in _replayCache.Keys.ToList())
            {
                if (DateTime.Now.Subtract(_replayCache[key].Requested).Seconds > _replayServiceSettings.CacheDuration)
                    _replayCache.Remove(key);
            }
        }
    }
}
