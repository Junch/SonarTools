using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools.Runner {
    public class CSharpRunner: SonarRunner {
        public CSharpRunner(String filePath, String branch): base(filePath, branch) {
            this["ProjectBaseDir"] = DirectoryName;
            this["Visualstudio.enable"] = "false";
            this["exclusions"] = "obj/**/*,bin/**/*,**/*.h,**/*.cpp,UnitTest*/**/*";
            String fileName = Path.GetFileName(filePath);
            this["working.directory"] = fileName + "_work";
        }
    }
}
