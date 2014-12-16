Readme.md
=================

https://github.com/wenns/sonar-cxx/wiki

http://www.sonarsource.com/products/plugins/languages/c-cpp-objectivec/installation-and-usage/

http://docs.sonarqube.org/display/SONAR/Analysis+Parameters

The aim is to run the sonar properly for the files in the vcxproj, csproj

##Todo
- Add a csharp

##Implemented
- Learn to use cppcheck to generate the .xml output.
- Run the sonar-runner bat to verify the usuage of the -Dxxx.
- Run several sonar-runners simultaneously
- Limits the numbers of sonar-sunner.bat to be run simultaneously.
- Add symbol link to the log files
- IncludePath in vcxproj need be analyzed. The proj NetworkDrive is a good example
- Configuration file could contain folder name. All projects in the folder need be analyzed. Use char '-' to escape.
- Support csharp
- Commandline parameter process

##Defer
- Sync code from tfs or p4
- Analyze the .log file to calculate how many files are analyzed, how many files are with "parser error".
- For commerical build. set the sonar.cfamily.cppcheck.path property to the path of the Cppcheck executable.

##Tips
- Cannot execute sonar-runner.bat under the drive root, such as C:\, D:\
- The -Dsonar.cxx.includeDirectories can include the relative paths, such as "..\..\include"
- Use sonar.host.url to specify the host. The sonar-runner can run on a different computer.
- Install the C# plugin through the Update Center. Also install Analysis Bootstrapper for Visual Studio Projects Plugin which configure automatically many of the required analysis parameters