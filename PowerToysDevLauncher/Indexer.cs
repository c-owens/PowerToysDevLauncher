namespace PowerToysDevLauncher.Plugin
{
    using System;
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
    using Lucene.Net.Analysis;
    using System.IO;

    public class IndexItem
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public int Score { get; set; }
	}

	public class Indexer : IDisposable
	{
		public Indexer( Settings settings )
		{
			m_settings = settings;
			SetupLuceneIndex();
		}

		private void SetupLuceneIndex()
		{
			if( !Utilities.Directory.Exists( m_settings.IndexPath ) )
				Utilities.Directory.CreateDirectory( m_settings.IndexPath );

			m_directory = FSDirectory.Open( m_settings.IndexPath );
			if( m_settings.CaseSensitive )
			{
				// This is some gross junk we need to delay load a class that uses something from Lucene.net as its base class.
				// See the comment on CaseSensitiveStandardAnalyzer for more explanation.
				Assembly assembly = Main.GetDynamicDependencyAssembly();
				Type[] types = assembly.GetTypes();
                Type type = types.First( o => o.IsClass && !o.IsAbstract && o.Name == "CaseSensitiveStandardAnalyzer" );
				m_analyzer = (Analyzer)Activator.CreateInstance( type, AppLuceneVersion );
			}
			else
			{
				m_analyzer = new StandardAnalyzer( AppLuceneVersion );
			}
		}

		public void Rebuild( Action<string> output = null )
		{
            if( output == null )
                output = s => Debug.WriteLine( s );

            var config = new IndexWriterConfig( AppLuceneVersion, m_analyzer ) { OpenMode = OpenMode.CREATE };
            using( var writer = new IndexWriter( m_directory, config ) )
			{
				foreach( KeyValuePair<string, List<string>> item in m_settings.IndexPathsAndExtensions )
				{
					string toIndex = item.Key;
					List<string> extensions = item.Value;

                    string extensionsStr = string.Join( ", ", extensions );
                    output( $"Indexing files under {toIndex} matching the extensions {extensionsStr}" );
					foreach( string file in Utilities.Directory.GetFiles( toIndex, "*.*", SearchOption.AllDirectories ).Where( p => extensions.Contains( Path.GetExtension( p ) ) ) )
					{
						string lower = file.ToLower();
						if( m_settings.IgnorePatterns.Any( pattern => lower.Contains( pattern ) ) )
							continue;

						output( $"Indexing {file}" );
						string name = Path.GetFileNameWithoutExtension( file );
						string extension = Path.GetExtension( file );
						string content = File.ReadAllText( file );
						var doc = new Document
						{
							new StringField( "name", name, Field.Store.YES ),
							new StringField( "path", file, Field.Store.YES ),
							new StringField( "extension", extension, Field.Store.YES )
						};

						if( m_settings.IndexFileContent )
							doc.Add( new TextField( "content", content, Field.Store.YES ) );

						writer.AddDocument( doc );
					}
				}
			}
		}

		public IEnumerable<IndexItem> Search( string[] terms )
		{
            string GetFormattedTerm( string t ) => m_settings.CaseSensitive ? t : t.ToLower();

            // TODO:
            // Query the name and path fields in addition to content
            // Score name matches the highest
            // Score path matches after that (maybe? this might be last, need to test)
            // Score content matches after that
            // Support exact match queries, e.g. "this is a string" using PhraseQuery instead of TermQuery
            //  Need to figure out how Wox gives us these (does it show up as a single term?)
            using( var reader = DirectoryReader.Open( m_directory ) )
			{
                Query query;
                if( terms.Length == 1 )
                {
                    // Create a simple "content:term" query.
                    query = new TermQuery( new Term( "content", GetFormattedTerm( terms.First() ) ) );
                }
                else
                {
                    // Create a query that ANDs all of the terms together. 
                    var boolQuery = new BooleanQuery(); //{ MinimumNumberShouldMatch = 1 };
                    foreach( string term in terms )
                        boolQuery.Add( new TermQuery( new Term( "content", GetFormattedTerm( term ) ) ), Occur.MUST );

                    query = boolQuery;
                }

				var searcher = new IndexSearcher( reader );
				ScoreDoc[] hits = searcher.Search( query, 5 ).ScoreDocs;
				foreach( ScoreDoc hit in hits )
				{
					Document found = searcher.Doc( hit.Doc );
					yield return new IndexItem
					{
						Name = Path.GetFileName( found.Get( "path" ) ),
						Path = found.Get( "path" ),
						Score = Convert.ToInt32( hit.Score * 1000 )
					};
				}
			}
		}

		public void Dispose()
		{
			m_analyzer.Dispose();
			m_directory.Dispose();
		}

		private FSDirectory m_directory;
		private Analyzer m_analyzer;
		private readonly Settings m_settings;
		private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
	}
}
