using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using SonarTools.Runner;

namespace SonarTools.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void Generate_Setting_From_Properties() {
            SonarRunner runner = new SonarRunner(@"C:\D\Test.vcxproj", @"$/ACAD/R");
            var setting = runner.GetProperties();
            setting.Sort();

            Assert.AreEqual(4, setting.Count);
            Assert.AreEqual("-Dsonar.fullFilePath=C:\\D\\Test.vcxproj", setting[0]);
            Assert.AreEqual("-Dsonar.projectKey=ACAD_R_D_Test_vcxproj", setting[1]);
            Assert.AreEqual("-Dsonar.projectName=$/ACAD/R/D/Test.vcxproj", setting[2]);
            //Assert.AreEqual("-Dsonar.sources=C:\\D", setting[3]); for 0.9
            Assert.AreEqual("-Dsonar.sources=.", setting[3]);
        }

        [TestMethod]
        public void Generate_sonarcmd_From_Properties() {
            SonarRunner runner = new SonarRunner(@"U:\a.vcxproj", @"$/A/R");
            runner["Language"] = "c++";

            String cmd = runner.SonarCmdArguments;
            //Assert.AreEqual(@"-Dsonar.language=c++ -Dsonar.fullFilePath=U:\a.vcxproj -Dsonar.projectKey=A_R_a_vcxproj -Dsonar.projectName=$/A/R/a.vcxproj -Dsonar.sources=U:\", cmd);
            Assert.AreEqual(@"-Dsonar.language=c++ -Dsonar.fullFilePath=U:\a.vcxproj -Dsonar.projectKey=A_R_a_vcxproj -Dsonar.projectName=$/A/R/a.vcxproj -Dsonar.sources=.", cmd);
        }

        [TestMethod]
        public void Get_AdditionalInclude_Directories_From_XML() {
            // Arrange
            string text =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Project DefaultTargets=""Build"" ToolsVersion=""12.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
              <ItemDefinitionGroup Condition=""'$(runneruration)|$(Platform)'=='Debug|Win32'"">
                <ClCompile>
                  <WarningLevel>Level3</WarningLevel>
                  <AdditionalIncludeDirectories>C:\Test\Include;$(Macro)</AdditionalIncludeDirectories>
                </ClCompile>
              </ItemDefinitionGroup>
              <ItemDefinitionGroup Condition=""'$(runneruration)|$(Platform)'=='Release|Win32'"">
                <ClCompile>
                  <RuntimeLibrary>MultiThreaded</RuntimeLibrary>
                  <AdditionalIncludeDirectories>D:\Test\Include</AdditionalIncludeDirectories>
                </ClCompile>
              </ItemDefinitionGroup>
            </Project>";

            // Action
            XElement xmlTree = XElement.Parse(text);
            VcxprojParser parser = new VcxprojParser();
            List<String> dirs = new List<string>();
            parser.AddAdditionalIncludeDirectories(xmlTree, dirs);

            // Assert
            Assert.AreEqual(3, dirs.Count);
            Assert.AreEqual(@"C:\Test\Include", dirs[0]);
            Assert.AreEqual(@"$(Macro)", dirs[1]);
            Assert.AreEqual(@"D:\Test\Include", dirs[2]);
        }

        [TestMethod]
        public void Replace_One_Macro_InPaths() {
            Project proj = new Project();
            proj.SetProperty("P1", "Property 1;Property 2");

            VcxprojParser parser = new VcxprojParser(proj);
            List<String> rawPaths = new List<string> {
                "1234",
                "$(P1)"
            };

            var paths = parser.EvaluateDirectories(rawPaths);

            Assert.AreEqual(3, paths.Count);
            Assert.AreEqual("1234", paths[0]);
            Assert.AreEqual("Property 1", paths[1]);
            Assert.AreEqual("Property 2", paths[2]);
        }

        [TestMethod]
        public void Replace_Two_Macro_InPaths() {
            Project proj = new Project();
            proj.SetProperty("P1", "Property 1");
            proj.SetProperty("P2", "Property 2");

            VcxprojParser parser = new VcxprojParser(proj);
            List<String> rawPaths = new List<string> {
                @"$(P1)\$(P2)\$(P1)"
            };

            var paths = parser.EvaluateDirectories(rawPaths);

            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual(@"Property 1\Property 2\Property 1", paths[0]);
        }

        [TestMethod]
        public void Get_DepotName_From_Path() {
            String fullFilePath = @"U:\components\global\src\coredll\accore.vcxproj";
            String branch = @"$/AutoCAD/M-Branches/R";
            SonarRunner runner = new SonarRunner(fullFilePath, branch);
 
            String r = runner.DepotName;
            Assert.AreEqual(@"$/AutoCAD/M-Branches/R/components/global/src/coredll/accore.vcxproj", r);
        }

        [TestMethod]
        public void Get_KeyName_From_Path() {
            String fullFilePath = @"U:\components\global\src\coredll\accore.vcxproj";
            String branch = @"$/AutoCAD/M-Branches/R";
            SonarRunner runner = new SonarRunner(fullFilePath, branch);

            String r = runner.ProjectKey;
            Assert.AreEqual(@"AutoCAD_M-Branches_R_components_global_src_coredll_accore_vcxproj", r);
        }

        [TestMethod]
        public void Create_CommunityCppRunner() {
            String fullFilePath = @"U:\accore.vcxproj";
            String branch = @"$/AutoCAD/R";
            CommunityCppRunner p = new CommunityCppRunner(fullFilePath, branch);
            var setting = p.GetProperties();
            setting.Sort();

            Assert.AreEqual(8, setting.Count);
            Assert.AreEqual("-Dsonar.cxx.cppcheck.reportPath=AutoCAD_R_accore_vcxproj.xml", setting[0]);
            Assert.AreEqual("-Dsonar.fullFilePath=U:\\accore.vcxproj", setting[1]);
            Assert.AreEqual("-Dsonar.language=c++", setting[2]);
            Assert.AreEqual("-Dsonar.projectBaseDir=U:\\", setting[3]);
            Assert.AreEqual("-Dsonar.projectDescription=\"Last run by community version\"", setting[4]);
            Assert.AreEqual("-Dsonar.projectKey=AutoCAD_R_accore_vcxproj", setting[5]);
            Assert.AreEqual("-Dsonar.projectName=$/AutoCAD/R/accore.vcxproj", setting[6]);
            Assert.AreEqual("-Dsonar.sources=.", setting[7]);
        }
    }
}
