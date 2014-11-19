Readme.md
=================

To aim is to run the sonar properly for the files in the vcxproj, csproj

##Todo
- Sync code from tfs or p4
- Commandline parameter process
- Run several sonar-runners simultaneously
- IncludePath in vcxproj need be analyzed. The proj NetworkDrive is a good example

##Implemented
- Learn to use cppcheck to generate the .xml output.
- Run the sonar-runner bat to verify the usuage of the -Dxxx.

##Tips
- Cannot execute sonar-runner.bat under the drive root, such as C:\, D:\
- The -Dsonar.cxx.includeDirectories can include the relative paths, such as "..\..\include"