namespace PowerToysDevLauncher.Indexer
{
    using System;
    using PowerToysDevLauncher.Plugin;

    /// <summary>
    /// Rebuilds the index for the launcher plugin.
    /// </summary>
    public class Program
	{
		public static void Main( string[] args )
		{
            string logFile = Utilities.Path.Combine( Utilities.Storage.DirectoryPath, "indexLog.txt" );
            using( var file = new System.IO.StreamWriter( logFile, append: false ) )
            {
                const int pad = 17;
                Settings s = Utilities.Settings;
                file.WriteLine( "Index location: ".PadRight( pad ) + s.IndexPath );
                file.WriteLine( "Case sensitive: ".PadRight( pad ) + s.CaseSensitive );
                file.WriteLine( "Index content: ".PadRight( pad ) + s.IndexFileContent );
                file.WriteLine( "Index paths: ".PadRight( pad ) + string.Join( ", ", s.IndexPathsAndExtensions.Keys ) );
                file.WriteLine( "Ignore patterns: ".PadRight( pad ) + string.Join( ", ", s.IgnorePatterns ) );
                file.WriteLine();

                new Indexer( s ).Rebuild( message =>
                {
                    Console.WriteLine( message );
                    file.WriteLine( message );
                } );
            }
		}
	}
}
