# PowerToysDevLauncher

## About

This is an *unofficial* plugin for the PowerToys Run utility that indexes and opens source code files and projects. The index and behavior for opening files is fully configurable to support many different languages and modes of working. The plugin uses the search prefix `;` and results are not included in global searches that don't include the prefix.

There is currently no release or installer, the plugin must be built from source.

## Rationale

The intention of this project is to provide a more configurable index for code files that is not based on the Windows Search API. As opposed to the default indexer for PowerToys Run, this avoids forcing the user to manually filter out results for paths and file types that may not be relevant (e.g. files in the user's Documents folder that are typically indexed by default in Windows).

In addition this plugin should allow for more effective methods of opening files instead of using the shell's default program for the file type, e.g.: 
- Opening the nearest `.sln` file above a `*.cs` file in Visual Studio
- Opening the nearest folder containing a `.git` directory above a `*.py` file in Visual Studio Code
- Focusing the window where the file is already open instead of re-opening it

## Developing

Due to the fact that external plugins aren't currently supported for the PowerToys Run utility, we include the full PowerToys repository as a submodule in the [PowerToys](/PowerToys) directory. Any build dependencies for [that project](https://github.com/microsoft/PowerToys) apply here. Because it includes some C++ dependencies the project must be built with Visual Studio 2019 and other editors are not currently supported.

The main [solution file](/PowerToysDevLauncher.sln) contains projects for both the dependencies required from the PowerToys project and the plugin's code, so building the solution file will build everything required to run and debug the plugin. For debugging, set the PowerLauncher project in the PowerToys solution folder as the startup project.
