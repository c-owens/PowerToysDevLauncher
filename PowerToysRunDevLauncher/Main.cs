namespace PowerToysRunDevLauncher.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Abstractions;
    using System.Reflection;
    using Wox.Plugin;
    using Wox.Infrastructure.Storage;

    public class Main : IPlugin, ISavable, IDisposable
	{
		public Main()
		{
			m_pluginDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
			AppDomain.CurrentDomain.AssemblyResolve += LoadFromPluginFolder;
		}

		public void Init( PluginInitContext context )
		{
			m_storage = new PluginJsonStorage<Settings>();
			m_settings = LoadSettings();
			m_context = context;
			m_index = new Indexer( m_settings );
		}

		private PluginInitContext m_context;

		private Assembly LoadFromPluginFolder( object sender, ResolveEventArgs args )
		{
			string assemblyName = string.Format( "{0}.dll", new AssemblyName( args.Name ).Name );
			string assemblyPath = Path.Combine( m_pluginDirectory, assemblyName );
			if( !File.Exists( assemblyPath ) )
				return null;

			return Assembly.LoadFrom( assemblyPath );
		}

		public static Assembly GetDynamicDependencyAssembly()
		{
			if( m_dynamicAssembly != null )
				return m_dynamicAssembly;

			string dynamicDllPath = Path.Combine( m_pluginDirectory, "PowerToysRunDevLauncher.Dynamic.dll" );
			m_dynamicAssembly = Assembly.LoadFrom( dynamicDllPath );
			return m_dynamicAssembly;
		}

		public List<Result> Query( Query query )
		{
			if( string.IsNullOrEmpty( query.Search ) )
				return GetDefaultResults();

			var results = new List<Result>();
			string[] terms = query.Terms.ToArray();
			foreach( IndexItem item in m_index.Search( terms ) )
			{
				results.Add( new Result
				{
					Action = c =>
					{
                        var start = new ProcessStartInfo( item.Path ) { UseShellExecute = true };
                        Process.Start( start );
						return true;
					},
					IcoPath = item.Path,
					Title = item.Name,
					SubTitle = $"{PluginName}: {item.Path}",
					Score = item.Score
				} );
			}
			return results;
		}

        private List<Result> GetDefaultResults() => new List<Result>
            {
                new Result
                {
                    Action = c =>
                    {
                        m_index.Rebuild();
                        return true;
                    },
                    IcoPath = "Images\\rocket.png",
                    Title = "Rebuild Index",
                    SubTitle = $"{PluginName}: Command",
                    Score = 10
                },
                new Result
                {
                    Action = c =>
                    {
                        Debug.WriteLine( "Test" );
                        return true;
                    },
                    IcoPath = "Images\\rocket.png",
                    Title = "Edit Settings",
                    SubTitle = $"{PluginName}: Command",
                    Score = 11
                },
            };

        public void Dispose() => AppDomain.CurrentDomain.AssemblyResolve -= LoadFromPluginFolder;

        public Settings LoadSettings()
		{
			Settings settings = m_storage.Load();
			if( settings.IgnorePatterns == null )
				settings.IgnorePatterns = new List<string>();

			// Make sure our ignore patterns are lower cased.
			for( int i = 0; i < settings.IgnorePatterns.Count; ++i )
				settings.IgnorePatterns[i] = settings.IgnorePatterns[i].ToLower();

			if( settings.IndexPathsAndExtensions == null )
				throw new InvalidOperationException( "No index paths specified" );

			// Make sure each of the extensions to index start with a period.
			foreach( KeyValuePair<string, List<string>> item in settings.IndexPathsAndExtensions )
			{
				List<string> extensions = item.Value;
				for( int i = 0; i < extensions.Count; ++i )
				{
					if( !extensions[i].StartsWith( "." ) )
						extensions[i] = "." + extensions[i];
				}
			}

			return settings;
		}

        public void Save() => m_storage.Save();

        private Indexer m_index;
		private Settings m_settings;
		private PluginJsonStorage<Settings> m_storage;
		private static string m_pluginDirectory;
		private const string PluginName = "DevLauncher";
		private static readonly IFileSystem FileSystem = new FileSystem();
		private static readonly IPath Path = FileSystem.Path;
		private static readonly IFile File = FileSystem.File;

		private static Assembly m_dynamicAssembly;
	}
}
