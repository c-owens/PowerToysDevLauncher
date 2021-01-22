namespace PowerToysDevLauncher.Plugin
{
    using System.IO.Abstractions;
    using System.Reflection;
    using Wox.Infrastructure.Storage;

    public static class Utilities
    {
        static Utilities()
        {
            Storage = new PluginJsonStorage<Settings>();

            FileSystem = new FileSystem();
            Path = FileSystem.Path;
            File = FileSystem.File;
            Directory = FileSystem.Directory;
            PluginDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            Settings = Storage.Load();
        }

        public static void SaveSettings() => Storage.Save();

        public static Settings Settings { get; set; }

        public static string PluginDirectory { get; set; }

        public static IFileSystem FileSystem { get; }

        public static IPath Path { get; }

        public static IFile File { get; }

        public static IDirectory Directory { get; }

        public static PluginJsonStorage<Settings> Storage { get; }
    }
}
