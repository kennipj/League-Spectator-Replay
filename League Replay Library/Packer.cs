using LeagueReplayLibrary.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueReplayLibrary
{
    public class Packer
    {

        internal static async Task<Replay> UnpackReplay(string inpath)
        {
            using (var fs = new FileStream(inpath, FileMode.Open))
            {
                var magic = new byte[4];
                await fs.ReadAsync(magic);

                if (BitConverter.ToUInt32(magic) != BitConverter.ToUInt32(new byte[4] { 0x4C, 0x53, 0x52, 0x50 })) //LSRP
                    return null;

                var header = await UnpackHeader(fs);
                var dataHeaders = await UnpackDataHeaders(fs, header);

                var chunks = await UnpackChunks(fs, dataHeaders.chunkHeaders);
                var keyframes = await UnpackKeyframes(fs, dataHeaders.keyframeHeaders);

                var replay = new Replay()
                {
                    GameKey = new GameKey() { PlatformId = (Region)header.region, GameId = header.gameId },
                    Chunks = chunks.OrderBy(x => x.id).ToList(),
                    KeyFrames = keyframes.OrderBy(x => x.id).ToList(),
                    eKey = header.encryptionKey,
                    EndStartupChunkId = header.endStartupChunkId,
                    StartGameChunkId = header.startGameChunkId,
                    EndGameChunkId = header.endGameChunkId,
                    GameEnded = true,
                };

                return replay;
            }
        }

        private static async Task<List<Payload>> UnpackKeyframes(FileStream fs, List<PayloadHeader> keyframeHeaders)
        {
            var keyframes = new List<Payload>();

            foreach (var header in keyframeHeaders)
            {
                var buffer = new byte[header.size];
                fs.Seek(header.offset, SeekOrigin.Begin);
                await fs.ReadAsync(buffer);
                keyframes.Add(new Payload() { data = buffer, id = header.id });
            }

            return keyframes;
        }

        private static async Task<List<Payload>> UnpackChunks(FileStream fs, List<PayloadHeader> chunkHeaders)
        {
            var chunks = new List<Payload>();

            foreach (var header in chunkHeaders)
            {
                var buffer = new byte[header.size];
                fs.Seek(header.offset, SeekOrigin.Begin);
                await fs.ReadAsync(buffer);
                chunks.Add(new Payload() { data = buffer, id = header.id });
            }

            return chunks;
        }

        public static async Task<Header> UnpackHeader(FileStream fs)
        {
            fs.Seek(4, SeekOrigin.Begin);
            var sizeBuffer = new byte[2];
            await fs.ReadAsync(sizeBuffer);

            var buffer = new byte[BitConverter.ToUInt16(sizeBuffer)];
            fs.Seek(4, SeekOrigin.Begin);
            await fs.ReadAsync(buffer);

            return Header.FromBytes(buffer);
        }

        private static async Task<(List<PayloadHeader> chunkHeaders, List<PayloadHeader> keyframeHeaders)> UnpackDataHeaders(FileStream fs, Header header)
        {
            fs.Seek(header.headerLength + 4, SeekOrigin.Begin);
            var chunkHeaders = new List<PayloadHeader>();
            var keyframeHeaders = new List<PayloadHeader>();
            var buffer = new byte[10];

            for (var k = 0; k < header.chunkCount; k++)
            {

                await fs.ReadAsync(buffer);
                chunkHeaders.Add(PayloadHeader.FromBytes(buffer));
            }

            for (var k = 0; k < header.keyframeCount; k++)
            {
                await fs.ReadAsync(buffer);
                keyframeHeaders.Add(PayloadHeader.FromBytes(buffer));
            }
            return (chunkHeaders, keyframeHeaders);
        }

        internal static async Task PackReplay(Replay replay, string outpath, bool overwrite = false)
        {

            var chunkData = PackChunkData(replay.Chunks);
            var keyframeData = PackKeyframeData(replay.KeyFrames);
            var headers = PackHeaders(replay);

            await Task.WhenAll(headers, chunkData, keyframeData);

            var dir = Directory.GetParent(outpath);
            Directory.CreateDirectory(dir.FullName);

            if (overwrite)
                File.Delete(@$"{outpath}.lsrp");

            using (var fs = new FileStream(@$"{outpath}.lsrp", FileMode.CreateNew))
            {
                await fs.WriteAsync(new byte[4] { 0x4C, 0x53, 0x52, 0x50 }); // LSRP

                await fs.WriteAsync(headers.Result);
                await fs.WriteAsync(chunkData.Result);
                await fs.WriteAsync(keyframeData.Result);
            }
        }

        private static async Task<byte[]> PackHeaders(Replay replay)
        {
            var memStream = new MemoryStream();

            var keyLength = (byte)Encoding.UTF8.GetByteCount(replay.eKey);
            var headerLength = (ushort)(keyLength + 34);
            var chunkHeaderLength = (ushort)(replay.Chunks.Count * 10);
            var keyframeHeaderLength = (ushort)(replay.KeyFrames.Count * 10);

            await memStream.WriteAsync(new Header()
            {
                chunkCount = (ushort)replay.Chunks.Count,
                keyframeCount = (ushort)replay.KeyFrames.Count,
                gameId = replay.GameKey.GameId,
                region = (byte)replay.GameKey.PlatformId,
                endStartupChunkId = (ushort)replay.ChunkInfo.EndStartupChunkId,
                startGameChunkId = (ushort)replay.ChunkInfo.StartGameChunkId,
                endGameChunkId = (ushort)replay.ChunkInfo.EndGameChunkId,
                encryptionKey = replay.eKey,
                encryptionKeyLength = keyLength,
                headerLength = headerLength,
                chunkHeaderLength = chunkHeaderLength,
                keyframekHeaderLength = keyframeHeaderLength,
                chunkHeaderOffset = headerLength,
                keyframeHeaderOffset = (ushort)(chunkHeaderLength + headerLength)
            }.ToBytes());

            uint pos = 0;
            foreach (var chunk in replay.Chunks)
            {
                var chunkLength = (uint)chunk.data.Length;
                await memStream.WriteAsync(new PayloadHeader()
                {
                    id = chunk.id,
                    size = (uint)chunk.data.Length,
                    offset = (uint)(headerLength + chunkHeaderLength + keyframeHeaderLength + pos + 4)
                }.ToBytes());
                pos += chunkLength;
            }

            foreach (var keyframe in replay.KeyFrames)
            {
                var keyframeLength = (uint)keyframe.data.Length;
                await memStream.WriteAsync(new PayloadHeader()
                {
                    id = keyframe.id,
                    size = (uint)keyframe.data.Length,
                    offset = (uint)(headerLength + chunkHeaderLength + keyframeHeaderLength + pos + 4)
                }.ToBytes());
                pos += keyframeLength;
            }

            return memStream.ToArray();
        }

        private static async Task<byte[]> PackChunkData(List<Payload> chunks)
        {
            var memStream = new MemoryStream();
            foreach (var chunk in chunks)
                await memStream.WriteAsync(chunk.data);

            return memStream.ToArray();
        }

        private static async Task<byte[]> PackKeyframeData(List<Payload> keyframes)
        {
            var memStream = new MemoryStream();
            foreach (var keyframe in keyframes)
                await memStream.WriteAsync(keyframe.data);

            return memStream.ToArray();
        }
    }
}
