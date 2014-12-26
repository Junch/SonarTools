using System;
using System.IO;

namespace SonarTools.Runner {
    public class ViewRunner: SonarRunner{
        public ViewRunner(String views): base(null, null) {
            this["views.list"] = views;
        }

        public override String LogFilepath {
            get {
                String viewLog = String.Format("{0}/sonarview.log", SymbolLinkLogFolder);
                return viewLog;
            }
        }

        protected override bool IsView() {
            return true;
        }

        public override void Run(String runnerHome) {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/aa365247(v=vs.85).aspx
            // In the Windows API, the maximum length for a path is MAX_PATH, which is defined as 260 characters.
            const int MAX_PATH = 260;
            if (LogFilepath.Length > MAX_PATH) {
                Console.WriteLine("Error: The path {0} is too long", LogFilepath);
                return;
            }

            using (logWriter = File.CreateText(LogFilepath)) {
                System.Console.WriteLine("==> Views {0}", this["views.list"]);
                bool bSuccess = RunSonarCmd(runnerHome);
                if (bSuccess) {
                    System.Console.WriteLine("<== Views {0}", this["views.list"]);
                } else {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    System.Console.WriteLine("x== Views {0}", this["views.list"]);
                    Console.ResetColor();
                }
            }
        }
    }
}
