Readme.md
=================

To aim is to run the sonar properly for the files in the vcxproj, csproj

##Todo
- Sync code from tfs or p4
- Commandline parameter process
- IncludePath in vcxproj need be analyzed. The proj NetworkDrive is a good example
- Use sonar.host.url to specify the host. The sonar-runner can run on a different computer.
- Clean the cache.
- Limits the numbers of sonar-sunner.bat to be run simultaneously.
- Support csharp

##Implemented
- Learn to use cppcheck to generate the .xml output.
- Run the sonar-runner bat to verify the usuage of the -Dxxx.
- Run several sonar-runners simultaneously

##Tips
- Cannot execute sonar-runner.bat under the drive root, such as C:\, D:\
- The -Dsonar.cxx.includeDirectories can include the relative paths, such as "..\..\include"