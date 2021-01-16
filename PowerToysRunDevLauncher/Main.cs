using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using Wox.Plugin;

namespace PowerToysRunDevLauncher.Plugin
{
	public class Main : IPlugin, IDisposable
	{
		public Main()
		{
			m_pluginDirectory = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
			AppDomain.CurrentDomain.AssemblyResolve += LoadFromPluginFolder;
		}

		public void Init( PluginInitContext context )
		{
			m_context = context;
			m_index = new Indexer();
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
						Process.Start( item.Path );
						return true;
					},
					IcoPath = "Images\\rocket.png",
					Title = item.Name,
					SubTitle = $"{PluginName}: {item.Path}",
					Score = item.Score
				} );
			}
			return results;
		}

		private List<Result> GetDefaultResults()
		{
			return new List<Result>
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
						System.Diagnostics.Debug.WriteLine( "Test" );
						return true;
					},
					IcoPath = "Images\\rocket.png",
					Title = "Edit Settings",
					SubTitle = $"{PluginName}: Command",
					Score = 11
				},
			};
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= LoadFromPluginFolder;
		}

		private Indexer m_index;
		private readonly string m_pluginDirectory;
		private static string PluginName = "DevLauncher";
		private static readonly IFileSystem FileSystem = new FileSystem();
		private static readonly IPath Path = FileSystem.Path;
		private static readonly IFile File = FileSystem.File;
		private static readonly IDirectory Directory = FileSystem.Directory;
	}
}