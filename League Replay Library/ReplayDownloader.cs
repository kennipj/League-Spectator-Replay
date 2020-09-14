using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeagueReplayLibrary
{
    internal class ReplayDownloader
    {
        private static readonly string UriPrefix = "/observer-mode/rest/consumer";
        private static readonly string UriVersion = "/version";
        private static readonly string UriMeta = "/getGameMetaData";
        private static readonly string UriChunkInfo = "/getLastChunkInfo";
        private static readonly string UriChunkData = "/getGameDataChunk";
        private static readonly string UriKeyframeData = "/getKeyFrame";
        private static readonly string UriEndStats = "/endOfGameStats";

        private static readonly HttpClient HttpClient = new HttpClient();

        private static readonly Dictionary<Region, string> Servers = new Dictionary<Region, string>()
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

        private static async Task<JObject> GetJson(Uri path)
        {
            var response = await HttpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
                return JObject.Parse(await response.Content.ReadAsStringAsync());
            else
                throw new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}. Failed to retrieve {path}");
        }

        private static async Task<byte[]> GetBytes(Uri path)
        {
            var response = await HttpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            else
                throw new HttpRequestException($"{response.StatusCode} - {response.ReasonPhrase}. Failed to retrieve {path}");
        }

        internal static async Task<JObject> GetVersion(GameKey gameKey) =>
            await GetJson(new Uri($"http://{Servers[gameKey.PlatformId]}{UriPrefix}{UriVersion}"));

        internal static async Task<JObject> GetMetaData(GameKey gameKey) =>
            await GetJson(new Uri($"http://{Servers[gameKey.PlatformId]}{UriPrefix}{UriMeta}/{gameKey.PlatformId}/{gameKey.GameId}/0/token"));

        internal static async Task<JObject> GetChunkInfo(GameKey gameKey) =>
            await GetJson(new Uri($"http://{Servers[gameKey.PlatformId]}{UriPrefix}{UriChunkInfo}/{gameKey.PlatformId}/{gameKey.GameId}/0/token"));

        internal static async Task<JObject> GetEndStats(GameKey gameKey) =>
            await GetJson(new Uri($"http://{Servers[gameKey.PlatformId]}{UriPrefix}{UriEndStats}/{gameKey.PlatformId}/{gameKey.GameId}/0/token"));

        internal static async Task<byte[]> GetChunkData(GameKey gameKey, int chunkId) =>
            await GetBytes(new Uri($"http://{Servers[gameKey.PlatformId]}{UriPrefix}{UriChunkData}/{gameKey.PlatformId}/{gameKey.GameId}/{chunkId}/token"));

        internal static async Task<byte[]> GetKeyframeData(GameKey gameKey, int keyframeId) =>
            await GetBytes(new Uri($"http://{Servers[gameKey.PlatformId]}{UriPrefix}{UriKeyframeData}/{gameKey.PlatformId}/{gameKey.GameId}/{keyframeId}/token"));

    }
}
