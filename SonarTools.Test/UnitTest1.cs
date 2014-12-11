using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using SonarTools.Runner;
using SonarTools.Parser;
using SonarTools.Util;
using Moq;

namespace SonarTools.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void Generate_Setting_From_Properties() {
            SonarRunner runner = new SonarRunner(@"C:\D\Test.vcxproj", @"$/ACAD/R");
            var setting = runner.GetProperties();
            setting.Sort();

            Assert.AreEqual(3, setting.Count);
            Assert.AreEqual("-Dsonar.projectKey=ACAD_R_D_Test_vcxproj", setting[0]);
            Assert.AreEqual("-Dsonar.projectName=$/ACAD/R/D/Test.vcxproj", setting[1]);
            Assert.AreEqual("-Dsonar.sources=.", setting[2]);
        }

        [TestMethod]
        public void Generate_sonarcmd_From_Properties() {
            SonarRunner runner = new SonarRunner(@"U:\a.vcxproj", @"$/A/R");
            runner["Language"] = "c++";

            String cmd = runner.SonarCmdArguments;
            Assert.AreEqual(@"-Dsonar.language=c++ -Dsonar.projectKey=A_R_a_vcxproj -Dsonar.projectName=$/A/R/a.vcxproj -Dsonar.sources=.", cmd);
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
        public void Remove_Empty_String_InPaths() {
            Project proj = new Project();
            VcxprojParser parser = new VcxprojParser(proj);
            List<String> rawPaths = new List<string> {
                "1234; ;abcd"
            };

            var paths = parser.EvaluateDirectories(rawPaths);

            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("1234", paths[0]);
            Assert.AreEqual("abcd", paths[1]);
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

            Assert.AreEqual(6, setting.Count);
            Assert.AreEqual("-Dsonar.language=c++", setting[0]);
            Assert.AreEqual("-Dsonar.projectBaseDir=U:\\", setting[1]);
            Assert.AreEqual("-Dsonar.projectDescription=\"Last run by community version\"", setting[2]);
            Assert.AreEqual("-Dsonar.projectKey=AutoCAD_R_accore_vcxproj", setting[3]);
            Assert.AreEqual("-Dsonar.projectName=$/AutoCAD/R/accore.vcxproj", setting[4]);
            Assert.AreEqual("-Dsonar.sources=.", setting[5]);
        }

        [TestMethod]
        public void Include_Directories_Joined() {
            Mock<VcxprojParser> parser = new Mock<VcxprojParser>();
            IEnumerable<String> includes = new String[]{
                @"C:\dir\include",
                @"..\include"
            };

            parser.Setup(m => m.IncludeDirectories).Returns(includes);
            Assert.AreEqual("\"C:/dir/include\",\"../include\"", parser.Object.IncludeDirectoriesJoined);
        }

        [TestMethod]
        public void RunnerManager_With_4_Threads() {
            RunnerSetting setting = new RunnerSetting{
                RunnerHome = "home",
                ThreadNumber = 4,
                Filepaths = new String[]{
                    "file1.vcxproj", "file2.vcxproj"
                }
            };

            Mock<SonarRunner> runner = new Mock<SonarRunner>("temp.vcxproj", "");
            Mock<ProjectParser> parser = new Mock<ProjectParser>(setting);
            parser.Setup(m => m.Parse(It.IsAny<String>())).Returns(runner.Object);

            SonarRunnerManager manager = new SonarRunnerManager(setting, parser.Object);
            manager.Run();

            runner.Verify(m => m.Run(It.Is<String>(arg => arg == "home")), Times.Exactly(2));

            parser.Verify(m => m.Parse(It.IsAny<String>()), Times.Exactly(2));
            parser.Verify(m => m.Parse(It.Is<String>(arg => arg == "file1.vcxproj")), Times.Once);
            parser.Verify(m => m.Parse(It.Is<String>(arg => arg == "file2.vcxproj")), Times.Once);
        }

        [TestMethod]
        public void ParseCpp_Commercial() {
            // Mock the VcxProjParser
            Project pj = new Project();
            pj.FullPath = @"test.vcxproj";
            Mock<VcxprojParser> vcParser = new Mock<VcxprojParser>(pj);
            vcParser.Setup(m => m.IncludeDirectories).Returns(new String[]{
                @"C:\Test\Include"
            });

            // Create an ProjectParser instance
            RunnerSetting setting = new RunnerSetting() {
                CppType = CppPluginType.kCppCommercial
            };
            ProjectParser parser = new ProjectParser(setting);
            PrivateObject po = new PrivateObject(parser);            

            var r = po.Invoke("ParseCpp", vcParser.Object) as CommercialCppRunner;

            Assert.AreEqual(vcParser.Object.IncludeDirectoriesJoined, r["cfamily.library.directories"]);
        }

        [TestMethod]
        public void ParseCpp_Commercial_With_BuildWrap() {
            // Mock the VcxProjParser
            Project pj = new Project();
            pj.FullPath = @"test2.vcxproj";
            Mock<VcxprojParser> vcParser = new Mock<VcxprojParser>(pj);
            vcParser.Setup(m => m.IncludeDirectories).Returns(new String[]{
                @"C:\Test\Include"
            });

            // Create an ProjectParser instance
            RunnerSetting setting = new RunnerSetting() {
                CppType = CppPluginType.kCppCommercial,
                BuildWrapper = "sonarbuild"
            };
            ProjectParser parser = new ProjectParser(setting);
            PrivateObject po = new PrivateObject(parser);

            var r = po.Invoke("ParseCpp", vcParser.Object) as CommercialCppRunner;

            Assert.AreEqual("sonarbuild", r["cfamily.build-wrapper-output"]);
        }

        [TestMethod]
        public void ParseCpp_Community() {
            // Mock the VcxProjParser
            Project pj = new Project();
            pj.FullPath = @"test3.vcxproj";
            Mock<VcxprojParser> vcParser = new Mock<VcxprojParser>(pj);
            vcParser.Setup(m => m.IncludeDirectories).Returns(new String[]{
                @"C:\Test\Include"
            });

            // Create an ProjectParser instance
            RunnerSetting setting = new RunnerSetting() {
                CppType = CppPluginType.kCppCommunity,
            };
            ProjectParser parser = new ProjectParser(setting);
            PrivateObject po = new PrivateObject(parser);

            var r = po.Invoke("ParseCpp", vcParser.Object) as CommunityCppRunner;

            Assert.AreEqual(vcParser.Object.IncludeDirectoriesJoined, r["cxx.includeDirectories"]);
        }

        [TestMethod]
        public void ParseCSharp() {
            RunnerSetting setting = new RunnerSetting();
            ProjectParser parser = new ProjectParser(setting);
            PrivateObject po = new PrivateObject(parser);

            Project pj = new Project();
            pj.FullPath = @"test4.csproj";
            pj.SetProperty("Language", "C#");

            var r = po.Invoke("Parse", pj) as CSharpRunner;
            Assert.IsNotNull(r);
        }

        [TestMethod]
        public void Read_Normal_XML_Config_File() {
            String text =
            @"<Settings>
              <Branch Id=""Main"" Depot=""$AutoCAD/main"" RunnerHome=""D:/Bin"" ThreadNumber=""4"" CppType=""Commerical"">
                  <Projects>
                    <Project>C:\project1.vcxproj</Project>
                    <Project>D:\project2.csproj</Project>
                  </Projects>
              </Branch>
              <Branch></Branch>
            </Settings>";
            XElement tree = XElement.Parse(text);

            SonarConfig cfg = new SonarConfig();
            cfg.Read(tree, "Main");

            Assert.AreEqual("$AutoCAD/main", cfg.Depot);
            Assert.AreEqual("D:/Bin", cfg.RunnerHome);
            Assert.AreEqual(4, cfg.ThreadNumber);
            Assert.AreEqual(CppPluginType.kCppCommercial, cfg.CppType);
            Assert.AreEqual(@"C:\project1.vcxproj", cfg.Projects[0]);
            Assert.AreEqual(@"D:\project2.csproj", cfg.Projects[1]);
        }

        [TestMethod]
        public void Read_XML_Config_File_Without_Attributes() {
            String text =
            @"<Settings>
              <Branch Id=""Main"">
                  <Projects>
                    <Project>C:\project1.vcxproj</Project>
                    <Project>D:\project2.csproj</Project>
                  </Projects>
              </Branch>
            </Settings>";
            XElement tree = XElement.Parse(text);

            SonarConfig cfg = new SonarConfig();
            cfg.Read(tree, "Main");

            Assert.AreEqual("", cfg.Depot);
            Assert.AreEqual("", cfg.RunnerHome);
            Assert.AreEqual(0, cfg.ThreadNumber);
            Assert.AreEqual(CppPluginType.kCppNotSpecified, cfg.CppType);
            Assert.AreEqual("", cfg.BuildWrapper);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void Read_XML_Config_File_Name_Not_Found() {
            String text =
            @"<Settings>
              <Branch Id=""Main"">
                  <Projects>
                    <Project>C:\project1.vcxproj</Project>
                    <Project>D:\project2.csproj</Project>
                  </Projects>
              </Branch>
            </Settings>";
            XElement tree = XElement.Parse(text);

            SonarConfig cfg = new SonarConfig();
            cfg.Read(tree, "Main2");
        }

        [TestMethod]
        public void Read_XML_Config_File_With_Skip_Attr() {
            String text =
            @"<Settings>
              <Branch Id=""Main"">
                  <Projects>
                    <Project Skip=""true"">C:\project1.vcxproj</Project>
                    <Project>D:\project2.csproj</Project>
                  </Projects>
              </Branch>
            </Settings>";
            XElement tree = XElement.Parse(text);

            SonarConfig cfg = new SonarConfig();
            cfg.Read(tree, "Main");

            Assert.AreEqual(1, cfg.Projects.Count);
            Assert.AreEqual(@"D:\project2.csproj", cfg.Projects[0]);
        }
    }
}
