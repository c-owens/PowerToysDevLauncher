using System;
using System.IO.Abstractions;
using System.Reflection;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace PowerToysRunDevLauncher.Plugin
{
	internal class IndexItem
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public int Score { get; set; }
	}

	internal class Indexer : IDisposable
	{
		public Indexer()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder( codeBase );
			string path = Uri.UnescapeDataString( uri.Path );
			m_indexPath = Path.Combine( Path.GetDirectoryName( path ), "index" );
			SetupLuceneIndex();
		}

		private void SetupLuceneIndex()
		{
			if( !Directory.Exists( m_indexPath ) )
				Directory.CreateDirectory( m_indexPath );

			string lockFile = Path.Combine( m_indexPath, "write.lock" );
			if( File.Exists( lockFile ) )
				File.Delete( lockFile );

			m_directory = FSDirectory.Open( m_indexPath );
			m_analyzer = new StandardAnalyzer( AppLuceneVersion );
		}

		private FSDirectory m_directory;
		private StandardAnalyzer m_analyzer;

		public string IndexPath => m_indexPath;
		private readonly string m_indexPath;

		public void Rebuild()
		{
			var config = new IndexWriterConfig( AppLuceneVersion, m_analyzer );
			config.OpenMode = OpenMode.CREATE;
			using( var writer = new IndexWriter( m_directory, config ) )
			{
				foreach( string file in Directory.GetFiles( @"D:\git\EmsApi.Client", "*.cs", System.IO.SearchOption.AllDirectories ) )
				{
					Debug.WriteLine( $"Indexing {file}" );
					string extension = Path.GetExtension( file );
					string content = File.ReadAllText( file );
					var doc = new Document
					{
						new TextField( "content", content, Field.Store.YES ),
						new StringField( "path", file, Field.Store.YES ),
						new StringField( "extension", extension, Field.Store.YES )
					};

					writer.AddDocument( doc );
				}
			}
		}

		public IEnumerable<IndexItem> Search( string[] terms )
		{
			using( var reader = DirectoryReader.Open( m_directory ) )
			{
				var query = new PhraseQuery();
				foreach( string term in terms )
				{
					//query.Add( new Term( "path", term ) );
					query.Add( new Term( "content", term ) );
				}

				//reader.
				IndexSearcher searcher = new IndexSearcher( reader );
				ScoreDoc[] hits = searcher.Search( query, 5 ).ScoreDocs;
				foreach( var hit in hits )
				{
					Document found = searcher.Doc( hit.Doc );
					yield return new IndexItem
					{
						Name = Path.GetFileName( found.Get( "path" ) ),
						Path = found.Get( "path" ),
						Score = Convert.ToInt32( hit.Score )
					};
				}
			}
		}

		public void Dispose()
		{
			m_analyzer.Dispose();
			m_directory.Dispose();
		}

		private static readonly IFileSystem FileSystem = new FileSystem();
		private static readonly IPath Path = FileSystem.Path;
		private static readonly IFile File = FileSystem.File;
		private static readonly IDirectory Directory = FileSystem.Directory;
		private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
	}
}
