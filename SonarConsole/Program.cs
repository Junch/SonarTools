using System;
using SonarTools;
using System.Diagnostics;

namespace SonarConsole {
    class Program {
        static void Main(string[] args) {
            ParserManager pm = new ParserManager();
            pm.cppPlugType = PluginType.kCppCommunity;
            pm.Branch = "$/ACAD/R";
            pm.Version = "1.0.0.1";
            pm.SonarRunnerHome = "D:/sonar-runner-2.4";
            pm.ThreadNumber = 4;
            pm.Filepaths = new String[] {
                @"U:\components\global\src\AcBrowser\AcHelpWrapper\AcHelpWrapper.vcxproj",
                @"U:\components\global\src\crxapps\rect\rectang.vcxproj",
                //@"U:\develop\global\src\coreapps\textfind\TextFind.vcxproj",
                //@"D:\Github\wtl\example\NetworkDrive\NetworkDrive.vcxproj",
                //@"U:\components\global\src\objectdbx\dbxapps\AcPointCloud\AcDbPointCloudDbx\AcDbPointCloudDbx.vcxproj"
            };

            Stopwatch timer = new Stopwatch();
            timer.Start();
            pm.Run();
            timer.Stop();

            double elapsedSeconds = (double)timer.ElapsedTicks / (double)Stopwatch.Frequency;
            System.Console.WriteLine("Seconds: {0:0.00}", elapsedSeconds);
        }
    }
}
