using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueReplayLibrary
{
    public class Replay
    {
        internal List<Keyframe> KeyFrames { get; set; }
        internal List<Chunk> Chunks { get; set; }

        public ulong gameId { get; set; }
        public string encryptionKey { get; set; }


        public async Task PackReplay(string outdir)
        {
            await Packer.PackReplay(this, outdir);
        }
    }
}
