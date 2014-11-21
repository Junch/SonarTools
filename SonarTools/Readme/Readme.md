Readme.md
=================

https://github.com/wenns/sonar-cxx/wiki

http://www.sonarsource.com/products/plugins/languages/c-cpp-objectivec/installation-and-usage/

To aim is to run the sonar properly for the files in the vcxproj, csproj

##Todo
- Sync code from tfs or p4
- Commandline parameter process
- IncludePath in vcxproj need be analyzed. The proj NetworkDrive is a good example
- Use sonar.host.url to specify the host. The sonar-runner can run on a different computer.
- Clean the cache.
- Support csharp
- Configuration file could contain folder name. All projects in the folder need be analyzed. Use char '-' to escape.
- Analyze the .log file to calculate how many files are analyzed, how many files are with "parser error".
- For commerical build. set the sonar.cfamily.cppcheck.path property to the path of the Cppcheck executable.
- ParserManager need pass many parameters to the SonarRunner. such as symbollinkfolder, sonar-runner folder. A struct can be added.

##Implemented
- Learn to use cppcheck to generate the .xml output.
- Run the sonar-runner bat to verify the usuage of the -Dxxx.
- Run several sonar-runners simultaneously
- Limits the numbers of sonar-sunner.bat to be run simultaneously.
- Add symbol link to the log files

##Tips
- Cannot execute sonar-runner.bat under the drive root, such as C:\, D:\
- The -Dsonar.cxx.includeDirectories can include the relative paths, such as "..\..\include"