using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LeagueReplayLibrary.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    internal class Header
    {
        [FieldOffset(0)] internal ushort headerLength;
        [FieldOffset(2)] internal uint fileLength;
        [FieldOffset(6)] internal ulong gameId;
        [FieldOffset(14)] internal ushort region;
        [FieldOffset(16)] internal ushort endStartupChunkId;
        [FieldOffset(18)] internal ushort startGameChunkId;
        [FieldOffset(20)] internal ushort endGameChunkId;
        [FieldOffset(22)] internal ushort chunkCount;
        [FieldOffset(24)] internal ushort keyframeCount;
        [FieldOffset(26)] internal ushort chunkHeaderLength;
        [FieldOffset(28)] internal ushort keyframekHeaderLength;
        [FieldOffset(30)] internal uint chunkHeaderOffset;
        [FieldOffset(34)] internal uint keyframeHeaderOffset;
        [FieldOffset(38)] internal ushort encryptionKeyLength;
        [FieldOffset(40)] internal string encryptionKey;
    }

    [StructLayout(LayoutKind.Explicit, Size = 10)]
    internal class DataHeader
    {
        [FieldOffset(0)] internal ushort id;
        [FieldOffset(2)] internal uint size;
        [FieldOffset(6)] internal uint offset;
    }

    [StructLayout(LayoutKind.Explicit, Size = 10)]
    internal class KeyframeHeader
    {
        [FieldOffset(0)] internal ushort keframeId;
        [FieldOffset(2)] internal uint keyframeLength;
        [FieldOffset(6)] internal uint offset;
    }

}
