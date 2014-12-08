﻿using System;
using SonarTools;
using System.Diagnostics;

namespace SonarConsole {
    class Program {
        static void Main(string[] args) {
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
