using System;
using System.Collections.Generic;
using Wox.Plugin;

namespace PowerToysRunDevLauncher
{
	public class Main : IPlugin
	{
		public void Init( PluginInitContext context )
		{
		}

		public List<Result> Query( Query query )
		{
			if( string.IsNullOrEmpty( query.Search ) )
				return GetDefaultResults();

			return new List<Result>();
		}

		private List<Result> GetDefaultResults()
		{
			return new List<Result>
			{
				new Result
				{
					Action = c =>
					{
						System.Diagnostics.Debug.WriteLine( "Rebuild Index" );
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

		private static string PluginName = "DevLauncher";
	}
}