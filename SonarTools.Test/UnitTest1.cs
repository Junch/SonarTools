﻿using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;

namespace SonarTools.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void Generate_Setting_From_Properties() {
            RunnerConfig config = new RunnerConfig ();
            config["ProjectName"] = "Simple example";
            config["SourceEncoding"] = "UTF-8";
            config["Language"] = "c++";
            var setting = config.GetSettings();
            setting.Sort();

            Assert.AreEqual(3, setting.Count);

            Assert.AreEqual("-Dsonar.language=c++", setting[0]);
            Assert.AreEqual("-Dsonar.projectName=Simple example", setting[1]);
            Assert.AreEqual("-Dsonar.sourceEncoding=UTF-8", setting[2]);
        }

        [TestMethod]
        public void Generate_sonarcmd_From_Properties() {
            RunnerConfig config = new RunnerConfig();
            config["SourceEncoding"] = "UTF-8";
            config["Language"] = "c++";

            String cmd = config.SonarCmd;
            Assert.AreEqual("sonar-runner -Dsonar.sourceEncoding=UTF-8 -Dsonar.language=c++", cmd);
        }

        [TestMethod]
        public void Get_AdditionalInclude_Directories_From_XML() {
            // Arrange
            string text =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Project DefaultTargets=""Build"" ToolsVersion=""12.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
              <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
                <ClCompile>
                  <WarningLevel>Level3</WarningLevel>
                  <AdditionalIncludeDirectories>C:\Test\Include;$(Macro)</AdditionalIncludeDirectories>
                </ClCompile>
              </ItemDefinitionGroup>
              <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
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
            ParserManager pm = new ParserManager();
            pm.Branch = @"$/AutoCAD/M-Branches/R";

            String s = @"U:\components\global\src\coredll\accore.vcxproj";
            String r = pm.DepotName(s);

            Assert.AreEqual(@"$/AutoCAD/M-Branches/R/components/global/src/coredll/accore.vcxproj", r);
        }

        [TestMethod]
        public void Get_KeyName_From_Path() {
            ParserManager pm = new ParserManager();
            pm.Branch = @"$/AutoCAD/M-Branches/R";

            String s = @"U:\components\global\src\coredll\accore.vcxproj";
            String r = pm.KeyName(s);

            Assert.AreEqual(@"AutoCAD_M-Branches_R_components_global_src_coredll_accore_vcxproj", r);
        }


    }
}
