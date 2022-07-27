namespace VoiceAssistantBackend.Commands
{
    public static class FoobarControl
    {
        public static string FoobarPath { get; private set; } = "C:\\Program Files (x86)\\foobar2000\\foobar2000.exe";
        public static string MusicDirectory { get; private set; } = "D:\\Muzyka";
        public static bool FoobarExists { get; private set; } = true;
        public static bool MusicDirectoryExists { get; private set; } = true;

        private static readonly string[] allowedExtensions = new string[] { ".mp3", ".flac", ".wav" };


        static FoobarControl()
        {
            if (!File.Exists(FoobarPath))
            {
                FoobarExists = false;
            }

            if (!Directory.Exists(MusicDirectory))
            {
                MusicDirectoryExists = false;
            }
        }

        public static bool ChangeFoobarPathIfExists(string path)
        {
            if (!File.Exists(path))
            {
                FoobarExists = false;
            }
            else
            {
                FoobarExists = true;
                FoobarPath = path;
            }

            return FoobarExists;
        }

        public static bool ChangeMusicDirectoryIfExists(string path)
        {
            if (!Directory.Exists(path))
            {
                MusicDirectoryExists = false;
            }
            else
            {
                MusicDirectoryExists = true;
                MusicDirectory = path;
            }

            return MusicDirectoryExists;
        }

        public static string[] GetArtistsFromMusicDirectory()
        {
            if (!MusicDirectoryExists)
                return new string[] { "No music directory" };

            var artistDirectories = Directory.GetDirectories(MusicDirectory);
            for (int i = 0; i < artistDirectories.Length; i++)
            {
                artistDirectories[i] = artistDirectories[i].Split('\\').Last().ToLower();
            }
            return artistDirectories;
        }

        public static string[] GetSongsDirectoriesFromMusicDirectory(string musicDirectory = "")
        {
            if (!MusicDirectoryExists)
                return new string[] { "No music directory" };

            if (musicDirectory.Length == 0)
                musicDirectory = MusicDirectory;

            List<string> songs = new List<string>();

            foreach (var directory in Directory.GetDirectories(musicDirectory))
            {
                var songFromDirectory = GetSongsDirectoriesFromMusicDirectory(directory);
                var songFiles = Directory.GetFiles(directory).Where(f => allowedExtensions.Any(f.ToLower().EndsWith)).ToArray();

                songs.AddRange(songFiles);
                songs.AddRange(songFromDirectory);
            }

            return songs.ToArray();
        }

        public static string[] GetSongsTitlesFromMusicDirectory()
        {
            if (!MusicDirectoryExists)
                return new string[] { "No music directory" };

            var songFiles = GetSongsDirectoriesFromMusicDirectory();

            for (int i = 0; i < songFiles.Count(); i++)
            {
                var song = songFiles[i];

                song = song.Split('\\').Last().ToLower(); // Get song file name
                song = song.Substring(0, song.LastIndexOf('.')); // Get name without extension

                int dotIndex = song.IndexOf('.');
                if (dotIndex >= 0)
                    song = song.Substring(dotIndex + 1); // Get text after first dot (which is usually track number dot)

                int dashIndex = song.IndexOf('-');
                if (dashIndex >= 0)
                    song = song.Substring(dashIndex + 1); // Get text after first - (which is usually track number dot)

                if (char.IsDigit(song[0]) && char.IsDigit(song[1]))
                    song = song.Substring(2);

                song = song.Trim(' ');

                if (song.Length == 0)
                    song = songFiles[i];

                songFiles[i] = song;
            }

            return songFiles;
        }

        private static string FindSong(object title, object artist)
        {
            string songPath = string.Empty;
            string sTitle = (string)title;
            string sArtist = (string)artist;

            songPath = GetSongsDirectoriesFromMusicDirectory(MusicDirectory).Where(f => f.ToLower().Contains(sTitle) && f.ToLower().Contains(sArtist)).FirstOrDefault();

            return songPath;
        }

        private static string FindSong(object title)
        {
            string songPath = string.Empty;
            string sTitle = (string)title;

            songPath = GetSongsDirectoriesFromMusicDirectory(MusicDirectory).Where(f => f.ToLower().Contains(sTitle)).FirstOrDefault();

            return songPath;
        }

        public static void FoobarPlaySong(object title, object artist)
        {
            var song = FindSong(title, artist);

            if (song is null || song.Length == 0)
                return;

            PlaySong(song);
        }

        public static void FoobarPlaySong(object title)
        {
            var song = FindSong(title);

            if (song is null || song.Length == 0)
                return;

            PlaySong(song);
        }

        private static void PlaySong(string songPath)
        {
            string strCmdText = $"/c C:\\\"Program Files (x86)\"\\foobar2000\\foobar2000.exe /context_command:\"Add to playback queue\" \"{songPath}\" /next";
            Misc.RunCMDCommand(strCmdText);
        }

        public static void FoobarVolumeUp()
        {
            string upVolumeCommand = "C:\\\"Program Files (x86)\"\\foobar2000\\foobar2000.exe /command:Up";
            string strCmdText = $"/c {upVolumeCommand}&{upVolumeCommand}&{upVolumeCommand}&{upVolumeCommand}&{upVolumeCommand}";
            Misc.RunCMDCommand(strCmdText);
        }

        public static void FoobarVolumeDown()
        {
            string downVolumeCommand = "C:\\\"Program Files (x86)\"\\foobar2000\\foobar2000.exe /command:Down";
            string strCmdText = $"/c {downVolumeCommand}&{downVolumeCommand}&{downVolumeCommand}&{downVolumeCommand}&{downVolumeCommand}";
            Misc.RunCMDCommand(strCmdText);
        }

        public static void FoobarVolumeUp(object value)
        {
            if (!int.TryParse(value.ToString(), out int correctValue)) return;

            string upVolumeCommand = "C:\\\"Program Files (x86)\"\\foobar2000\\foobar2000.exe /command:Up";
            string strCmdText = $"/c {upVolumeCommand}";

            for (int i = 0; i < correctValue; i++)
            {
                Misc.RunCMDCommand(strCmdText);
            }
        }

        public static void FoobarVolumeDown(object value)
        {
            if (!int.TryParse(value.ToString(), out int correctValue)) return;

            string downVolumeCommand = "C:\\\"Program Files (x86)\"\\foobar2000\\foobar2000.exe /command:Down";
            string strCmdText = $"/c {downVolumeCommand}";

            for (int i = 0; i < correctValue; i++)
            {
                Misc.RunCMDCommand(strCmdText);
            }
        }
    }
}
