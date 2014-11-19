using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools.Runner {
    public class CommunityCppRunner: SonarRunner {
        StreamWriter cppcheckWriter;
        
        public CommunityCppRunner(String fullFilePath, String branch): base(fullFilePath, branch) {
            this["ProjectDescription"] = "\"Last run by community version\"";
            this["Language"] = "c++";
            this["ProjectBaseDir"] = DirectoryName;
            this["cxx.cppcheck.reportPath"] = ProjectKey + ".xml";
            this["cxx.suffixes.headers"]= ".x"; // Don't parse the header files as the commerical does
            cppcheckWriter = File.CreateText(DirectoryName + "\\" + ProjectKey + ".xml");
        }

        public String CppCheckCmdArguments {
            get {
                return String.Format("-j 8 {0} --xml-version=2", DirectoryName);
            }
        }

        public void RunCppCheckCmd() {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cppcheck.exe";
            psi.Arguments = CppCheckCmdArguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.ErrorDialog = false;
            psi.WorkingDirectory = DirectoryName;

            using (Process proc = Process.Start(psi)) {
                proc.OutputDataReceived += proc_DataReceived;
                proc.ErrorDataReceived += proc_ErrorDataReceived;
                proc.EnableRaisingEvents = true;

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
            }

            if (cppcheckWriter != null)
                cppcheckWriter.Close();
        }

        void proc_DataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null)
                System.Console.WriteLine(e.Data);
        }

        void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null && cppcheckWriter != null)
                cppcheckWriter.WriteLine(e.Data);
        }

        protected override void PreRun() {
            RunCppCheckCmd();
        }
    }
}
