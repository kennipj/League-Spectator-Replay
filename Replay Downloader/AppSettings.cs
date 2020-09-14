using EntryPoint;

namespace ReplayDownloader
{
    [Help("Optional launch arguments")]
    public class AppSettings : BaseCliArguments
    {
        public AppSettings() : base("ReplayDownloader") { }

        [Option(ShortName: 'e', LongName: "key")]
        public string eKey { get; set; }

        [Option(ShortName: 'g', LongName: "id")]
        public ulong GameId { get; set; }

        [Option(ShortName: 'r', LongName: "region")]
        public string Region { get; set; }

        [Option(ShortName: 'o', LongName: "output")]
        public string Output { get; set; }
    }
}
