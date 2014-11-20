using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools.Runner {
    public class CommunityCppRunner: SonarRunner {
        StreamWriter cppcheckErrorWriter;
        
        public CommunityCppRunner(String fullFilePath, String branch): base(fullFilePath, branch) {
            this["ProjectDescription"] = "\"Last run by community version\"";
            this["Language"] = "c++";
            this["ProjectBaseDir"] = DirectoryName;
            this["cxx.cppcheck.reportPath"] = ProjectKey + ".xml";
            this["cxx.suffixes.headers"]= ".x"; // Don't parse the header files as the commerical does
        }

        public String CppcheckErrorLogFile {
            get {
                return String.Format("{0}\\{1}.xml", DirectoryName, ProjectKey);
            }
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

            using (cppcheckErrorWriter = File.CreateText(CppcheckErrorLogFile))
            using (Process proc = Process.Start(psi)) {
                proc.OutputDataReceived += proc_DataReceived;
                proc.ErrorDataReceived += proc_ErrorDataReceived;
                proc.EnableRaisingEvents = true;

                WriteLog("############################### Cppcheck begin ###############################");
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                WriteLog("################################ Cppcheck end ################################\n");
            }
        }

        void proc_DataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null)
                WriteLog(e.Data);
        }

        void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null && cppcheckErrorWriter != null)
                cppcheckErrorWriter.WriteLine(e.Data);
        }

        protected override void PreRun() {
            RunCppCheckCmd();
        }
    }
}
