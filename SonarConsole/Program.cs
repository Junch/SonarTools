using System;
using SonarTools;
using System.Diagnostics;
using SonarTools.Util;

namespace SonarConsole {
    class Program {
        static RunnerSetting GetSettingWithouConfigFile() {
            RunnerSetting setting = new RunnerSetting{
                Branch = "$/ACAD/R",
                RunnerHome = "D:/sonar-runner-2.4",
                ThreadNumber = 4,
            };

            setting.Filepaths = new String[] {
                @"U:\components\global\src\AcBrowser\AcHelpWrapper\AcHelpWrapper.vcxproj",
                @"U:\components\global\src\crxapps\rect\rectang.vcxproj",
                //@"D:\Github\Cplusplus\c11test\c11test.vcxproj",
                //@"U:\develop\global\src\coreapps\textfind\TextFind.vcxproj",
                //@"D:\Github\wtl\example\NetworkDrive\NetworkDrive.vcxproj",
                //@"U:\components\global\src\objectdbx\dbxapps\AcPointCloud\AcDbPointCloudDbx\AcDbPointCloudDbx.vcxproj"
            };

            return setting;
        }

        static RunnerSetting GetSettingWithConfigFile(String[] args) {
            if (args.Length != 2) {
                System.Console.WriteLine("Useage: SonarSonsole.exe demo.xml branchName");
                Environment.Exit(1);
                return null;
            }
            
            SonarConfig config = new SonarConfig();
            config.Read(args[0], args[1]);

            RunnerSetting setting = new RunnerSetting();
            setting.Branch = config.Depot;
            setting.CppType = config.CppType;
            setting.ThreadNumber = config.ThreadNumber;
            setting.RunnerHome = config.RunnerHome;
            setting.BuildWrapper = config.BuildWrapper;
            setting.Filepaths = config.Projects.ToArray();

            return setting;
        }

        static void Main(string[] args) {
            RunnerSetting setting = GetSettingWithConfigFile(args);
            
            SonarRunnerManager pm = new SonarRunnerManager(setting);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            pm.Run();
            timer.Stop();

            TimeSpan ts = timer.Elapsed;
            String output = string.Format("Elapsed: {0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"));
            Console.WriteLine(output);
        }
    }
}
