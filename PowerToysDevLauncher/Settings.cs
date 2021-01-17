namespace PowerToysDevLauncher.Plugin
{
    using System.Collections.Generic;

    public class Settings
	{
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
	}
}
