using LeagueReplayLibrary.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueReplayLibrary
{
    internal class Packer
    {
        private static byte[] StructToBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        private static T BytesToStruct<T>(byte[] bytes) where T : new()
        {
            var str = new T();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return str;
        }

        internal static async Task<Replay> UnpackReplay(string inpath)
        {
            var fs = new FileStream(inpath, FileMode.Open);
            var header = UnpackHeader(fs);
            var dataHeaders = UnpackDataHeaders(fs, header);

            var chunks = UnpackChunks(fs, dataHeaders.chunkHeaders);
            var keyframes = UnpackKeyframes(fs, dataHeaders.keyframeHeaders);

            await Task.WhenAll(chunks, keyframes);

            var replay = new Replay()
            {
                Chunks = chunks.Result,
                KeyFrames = keyframes.Result,
                encryptionKey = header.encryptionKey,
                gameId = header.gameId
            };

            return replay;
        }

        private static async Task<List<Keyframe>> UnpackKeyframes(FileStream fs, List<DataHeader> keyframeHeaders)
        {
            var keyframes = new List<Keyframe>();

            foreach(var header in keyframeHeaders)
            {
                var buffer = new byte[header.size];
                fs.Seek(header.offset, SeekOrigin.Begin);
                await fs.ReadAsync(buffer);
                keyframes.Add(new Keyframe() { data = buffer, id = header.id });
            }

            return keyframes;
        }

        private static async Task<List<Chunk>> UnpackChunks(FileStream fs, List<DataHeader> chunkHeaders)
        {
            var chunks = new List<Chunk>();

            foreach (var header in chunkHeaders)
            {
                var buffer = new byte[header.size];
                fs.Seek(header.offset, SeekOrigin.Begin);
                await fs.ReadAsync(buffer);
                chunks.Add(new Chunk() { data = buffer, id = header.id });
            }

            return chunks;
        }

        internal static Header UnpackHeader(FileStream fs)
        {
            var sizeBuffer = new byte[2];
            fs.Read(sizeBuffer);

            var buffer = new byte[BitConverter.ToUInt16(sizeBuffer)];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer);

            return BytesToStruct<Header>(buffer);
        }

        private static (List<DataHeader> chunkHeaders, List<DataHeader> keyframeHeaders) UnpackDataHeaders(FileStream fs, Header header)
        {
            fs.Seek(header.headerLength, SeekOrigin.Begin);
            var chunkHeaders = new List<DataHeader>();
            var keyframeHeaders = new List<DataHeader>();
            var buffer = new byte[10];

            for(var k = 0; k < header.chunkCount; k++)
            {
                fs.Read(buffer);
                chunkHeaders.Add(new DataHeader() 
                {
                    id = BitConverter.ToUInt16(buffer[..2]),
                    size = BitConverter.ToUInt32(buffer[2..6]),
                    offset = BitConverter.ToUInt32(buffer[6..])
                });
            }

            for (var k = 0; k < header.keyframeCount; k++)
            {
                fs.Read(buffer);
                keyframeHeaders.Add(new DataHeader()
                {
                    id = BitConverter.ToUInt16(buffer[..2]),
                    size = BitConverter.ToUInt32(buffer[2..6]),
                    offset = BitConverter.ToUInt32(buffer[6..])
                });
            }
            return (chunkHeaders, keyframeHeaders);
        }

        internal static async Task PackReplay(Replay replay, string outpath)
        {
            var fs = new FileStream(outpath, FileMode.CreateNew);
            var headers = PackHeaders(replay);
            var chunkData = PackChunkData(replay.Chunks);
            var keyframeData = PackKeyframeData(replay.KeyFrames);

            await Task.WhenAll(headers, chunkData, keyframeData);

            await fs.WriteAsync(headers.Result);
            await fs.WriteAsync(chunkData.Result);
            await fs.WriteAsync(keyframeData.Result);
        }

        private static async Task<byte[]> PackHeaders(Replay replay)
        {
            var memStream = new MemoryStream();

            var keyLength = (ushort)Encoding.ASCII.GetByteCount(replay.encryptionKey);
            var headerLength = (ushort)(keyLength + 32);
            var chunkHeaderLength = (ushort)(headerLength + (replay.Chunks.Count * 10));
            var keyframeHeaderLength = (ushort)(headerLength + chunkHeaderLength + (replay.KeyFrames.Count * 10));

            await memStream.WriteAsync(StructToBytes(new Header()
            {
                chunkCount = (ushort)replay.Chunks.Count,
                keyframeCount = (ushort)replay.KeyFrames.Count,
                gameId = replay.gameId,
                encryptionKey = replay.encryptionKey,
                encryptionKeyLength = keyLength,
                headerLength = headerLength,
                chunkHeaderLength = chunkHeaderLength,
                keyframekHeaderLength = keyframeHeaderLength,
                chunkHeaderOffset = headerLength,
                keyframeHeaderOffset = (ushort)(chunkHeaderLength + headerLength)
            }));

            uint pos = 0;
            foreach (var chunk in replay.Chunks)
            {
                var chunkLength = (uint)chunk.data.Length;
                await memStream.WriteAsync(StructToBytes(new DataHeader()
                {
                    id = chunk.id,
                    size = (uint)chunk.data.Length,
                    offset = (uint)(headerLength + chunkHeaderLength + keyframeHeaderLength + pos)
                }));
                pos += chunkLength;
            }

            foreach (var keyframe in replay.KeyFrames)
            {
                var keyframeLength = (uint)keyframe.data.Length;
                await memStream.WriteAsync(StructToBytes(new DataHeader()
                {
                    id = keyframe.id,
                    size = (uint)keyframe.data.Length,
                    offset = (uint)(headerLength + chunkHeaderLength + keyframeHeaderLength + pos)
                }));
                pos += keyframeLength;
            }

            return memStream.ToArray();
        }

        private static async Task<byte[]> PackChunkData(List<Chunk> chunks)
        {
            var memStream = new MemoryStream();
            foreach(var chunk in chunks)
                await memStream.WriteAsync(chunk.data);

            return memStream.ToArray();
        }

        private static async Task<byte[]> PackKeyframeData(List<Keyframe> keyframes)
        {
            var memStream = new MemoryStream();
            foreach (var keyframe in keyframes)
                await memStream.WriteAsync(keyframe.data);

            return memStream.ToArray();
        }
    }
}
