# League Spectator Replay
This library allows for an easy way of packing spectator data gathered  from the league spectator endpoint, into League Spectator Replay files (.lsrp). The library targets .NET Standard 2.1

#### NOTE: The spectator endpoint doesnt keep the Chunks/Keyframes for long, so make sure the game has recently started (4-5 minutes ago), to avoid data loss.

### Usage:
Downloading and packing a replay into a League Spectator Replay:
```cs
GameKey gameKey = new GameKey() { GameId = 123456789, PlatformId = Region.EUW1 };
string encryptionKey = "123456789";

Replay replay = await Replay.DownloadReplay(gameKey, encryptionKey);
await replay.PackReplay("C:/Path/To/Directory/Filename");
```
Reading a spectator replay:
```cs
Replay replay = await Replay.ReadReplay("C:/Path/To/Directory/Filename.lsrp");
```
### Sample applications

The repository also includes two sample applications, and a third soon to follow. A Downloader and a server. 

The downloader will automatically begin to download the spectator data for any game you spectate, and pack it into a League Spectator Replay and save it to `Documents/League of Legends/Spectator Replays`. (Only Windows support for now)

The server will serve any `.lsrp` files in located in `ReplayDirectory` specified in the `appsettings.json` file. When starting League, set the IP address parameter in the spectator arg to `127.0.0.1:3030`.

### TODO:

Handle missing chunks/keyframes.

Cross-platform GUI app for launching of .lsrp files.

Better memory management on the replay server.
