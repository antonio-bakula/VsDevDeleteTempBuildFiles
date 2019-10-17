# VsDevDeleteTempBuildFiles
Little tool that will move Visual Studio temp build files in folder and child folders to another temp folder. It will retain original folder structure so you can just move back files in case of panic :)

The tool will generate a csv file with the moved files informations (name, size etc.)

Usage (console app):
VsDevDeleteTempBuildFiles <root_foolder_with_vs_projects>
