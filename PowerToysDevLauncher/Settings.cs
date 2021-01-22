namespace PowerToysDevLauncher.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class Settings
	{
        /// <summary>
        /// The path to the index
        /// </summary>
        public string IndexPath { get; set; }

		/// <summary>
		/// Whether the index and search should be case sensitive.
		/// </summary>
		public bool CaseSensitive { get; set; } = true;

		/// <summary>
		/// Whether file contents should be indexed in addition to file paths and names.
		/// </summary>
		public bool IndexFileContent { get; set; } = true;

		/// <summary>
		/// The lists of paths to index (recursively), and the file extensions to index under each path.
		/// </summary>
		public Dictionary<string, List<string>> IndexPathsAndExtensions { get; set; }

		/// <summary>
		/// A list of patterns to ignore. If an ignore pattern is found in the full path to any file it is 
		/// not included in the index. These should not include wildcards.
		/// </summary>
		public List<string> IgnorePatterns { get; set; }

        [OnDeserialized]
        internal void OnDeserialized( StreamingContext context )
        {
            Validate();
            Normalize();
            SetIndexPath();
        }

        /// <summary>
        /// Makes sure any required fields are filled out or throws an exception.
        /// </summary>
        private void Validate()
        {
            if( IndexPathsAndExtensions == null )
                throw new InvalidOperationException( "No index paths specified" );
        }

        /// <summary>
        /// Run after loading settings to set some default values where necessary.
        /// </summary>
        private void Normalize()
        {
            if( IgnorePatterns == null )
                IgnorePatterns = new List<string>();

            // Make sure our ignore patterns are lower cased.
            for( int i = 0; i < IgnorePatterns.Count; ++i )
                IgnorePatterns[i] = IgnorePatterns[i].ToLower();

            // Make sure each of the extensions to index start with a period.
            foreach( KeyValuePair<string, List<string>> item in IndexPathsAndExtensions )
            {
                List<string> extensions = item.Value;
                for( int i = 0; i < extensions.Count; ++i )
                {
                    if( !extensions[i].StartsWith( "." ) )
                        extensions[i] = "." + extensions[i];
                }
            }
        }

        private void SetIndexPath()
        {
            if( !string.IsNullOrEmpty( IndexPath ) )
                return;

            IndexPath = Utilities.Path.Combine( Utilities.Storage.DirectoryPath, "index" );
        }
	}
}
