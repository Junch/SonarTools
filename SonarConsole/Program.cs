using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SonarTools;

namespace SonarConsole {
    class Program {
        static void Main(string[] args) {
            ParserManager pm = new ParserManager();
            pm.cppPlugType = PluginType.kCppCommunity;
            pm.Branch = "$/ACAD/R";
            pm.Version = "1.0.0.1";

            String file = @"D:\Github\wtl\example\NetworkDrive\NetworkDrive.vcxproj";
            var v = pm.Parser(file);
            v.Run();
        }
    }
}
