using LeagueReplayLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Replay_Server
{
    [Route("/observer-mode/rest/consumer")]
    [ApiController]
    public class ReplayController : ControllerBase
    {
        private readonly ReplayService _replayService;
        public ReplayController(ReplayService replayService)
        {
            _replayService = replayService;
        }

        [HttpGet("version")]
        public string GetVersion()
        {
            return "2.0.0";
        }

        [HttpGet("getGameMetaData/{region}/{gameid}/{id}/token")]
        public async Task<MetaData> GetMetaData(Region region, ulong gameid, string id)
        {
            return await _replayService.GetMetaData(new GameKey() { PlatformId = region, GameId = gameid }, id);
        }

        [HttpGet("getLastChunkInfo/{region}/{gameid}/{id}/token")]
        public async Task<LastChunkInfo> GetLastChunkInfo(Region region, ulong gameid, int id)
        {
            return await _replayService.GetLastChunkInfo(new GameKey() { PlatformId = region, GameId = gameid }, id, HttpContext.Connection.RemoteIpAddress);
        }

        [HttpGet("getGameDataChunk/{region}/{gameid}/{id}/token")]
        public async Task GetChunk(Region region, ulong gameid, int id)
        {
            var bytes = await _replayService.GetChunk(new GameKey() { PlatformId = region, GameId = gameid }, id);
            Response.ContentType = "application/octet-stream";
            Response.ContentLength = bytes.Length;
            await Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        [HttpGet("getKeyFrame/{region}/{gameid}/{id}/token")]
        public async Task GetKeyFrame(Region region, ulong gameid, int id)
        {
            var bytes = await _replayService.GetKeyFrame(new GameKey() { PlatformId = region, GameId = gameid }, id);
            Response.ContentType = "application/octet-stream";
            Response.ContentLength = bytes.Length;
            await Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        [HttpGet("endOfGameStats/{region}/{gameid}/{id}/token")]
        public async Task<string> GetEndStats(Region region, ulong gameid, int id)
        {
            return await _replayService.GetEndStats(new GameKey() { PlatformId = region, GameId = gameid }, id);
        }
    }
}
