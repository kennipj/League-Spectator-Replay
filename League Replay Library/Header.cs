using System;
using System.IO;
using System.Text;

namespace LeagueReplayLibrary.Structs
{

    public class Header
    {
        public ushort headerLength;
        public ulong gameId;
        public byte region;
        public ushort endStartupChunkId;
        public ushort startGameChunkId;
        public ushort endGameChunkId;
        public ushort chunkCount;
        public ushort keyframeCount;
        public ushort chunkHeaderLength;
        public ushort keyframekHeaderLength;
        public uint chunkHeaderOffset;
        public uint keyframeHeaderOffset;
        public byte encryptionKeyLength;
        public string encryptionKey;

        internal byte[] ToBytes()
        {
            using (var memStream = new MemoryStream())
            using (var writer = new BinaryWriter(memStream))
            {
                writer.Write(headerLength);
                writer.Write(gameId);
                writer.Write(region);
                writer.Write(endStartupChunkId);
                writer.Write(startGameChunkId);
                writer.Write(endGameChunkId);
                writer.Write(chunkCount);
                writer.Write(keyframeCount);
                writer.Write(chunkHeaderLength);
                writer.Write(keyframekHeaderLength);
                writer.Write(chunkHeaderOffset);
                writer.Write(keyframeHeaderOffset);
                writer.Write(encryptionKey);
                return memStream.ToArray();
            }
        }

        internal static Header FromBytes(byte[] bytes)
        {
            return new Header()
            {
                headerLength = BitConverter.ToUInt16(bytes[..2]),
                gameId = BitConverter.ToUInt64(bytes[2..10]),
                region = bytes[10],
                endStartupChunkId = BitConverter.ToUInt16(bytes[11..13]),
                startGameChunkId = BitConverter.ToUInt16(bytes[13..15]),
                endGameChunkId = BitConverter.ToUInt16(bytes[15..17]),
                chunkCount = BitConverter.ToUInt16(bytes[17..19]),
                keyframeCount = BitConverter.ToUInt16(bytes[19..21]),
                chunkHeaderLength = BitConverter.ToUInt16(bytes[21..23]),
                keyframekHeaderLength = BitConverter.ToUInt16(bytes[23..25]),
                chunkHeaderOffset = BitConverter.ToUInt32(bytes[25..29]),
                keyframeHeaderOffset = BitConverter.ToUInt32(bytes[29..33]),
                encryptionKeyLength = bytes[33],
                encryptionKey = Encoding.UTF8.GetString(bytes[34..])
            };
        }
    }

    internal class PayloadHeader
    {
        internal ushort id;
        internal uint size;
        internal uint offset;

        internal byte[] ToBytes()
        {
            using (var memStream = new MemoryStream())
            using (var writer = new BinaryWriter(memStream))
            {
                writer.Write(id);
                writer.Write(size);
                writer.Write(offset);
                return memStream.ToArray();
            }
        }

        internal static PayloadHeader FromBytes(byte[] bytes)
        {
            return new PayloadHeader()
            {
                id = BitConverter.ToUInt16(bytes[..2]),
                size = BitConverter.ToUInt32(bytes[2..6]),
                offset = BitConverter.ToUInt32(bytes[6..10])
            };
        }
    }
}
