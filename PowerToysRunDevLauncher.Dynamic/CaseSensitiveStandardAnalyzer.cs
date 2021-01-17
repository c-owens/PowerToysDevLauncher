namespace PowerToysRunDevLauncher.Lucene
{
    using System.IO;
    using global::Lucene.Net.Analysis;
    using global::Lucene.Net.Analysis.Core;
    using global::Lucene.Net.Analysis.Standard;
    using global::Lucene.Net.Util;

    /// <summary>
    /// This class is separated into a different assmbly so that it can be loaded via reflection after PowerToysRunDevLauncher.Plugin.Main
    /// is initialized. This is required because the powertoys launcher currently has no support for external plugins, so in order to inject
    /// our plugin's dependent assemblies into the process (specifically Lucene.net here) we have to hook the AssemblyResolve event in the plugin's
    /// constructor. If this class is part of the plugin assembly then when it's loaded by the launcher Lucene.Net.Analysis.Analyzer has to be resolved
    /// before our AssemblyResolve event handler is in place (which fails).
    /// </summary>
    public class CaseSensitiveStandardAnalyzer : Analyzer
	{
        public CaseSensitiveStandardAnalyzer( LuceneVersion version ) => m_version = version;
        private readonly LuceneVersion m_version;

		protected override TokenStreamComponents CreateComponents( string fieldName, TextReader reader )
		{
			Tokenizer tokenizer = new StandardTokenizer( m_version, reader );
			TokenStream filterStream = new StandardFilter( m_version, tokenizer );
			TokenStream stream = new StopFilter( m_version, filterStream, StopAnalyzer.ENGLISH_STOP_WORDS_SET );
			return new TokenStreamComponents( tokenizer, stream );
		}
	}
}
