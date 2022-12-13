#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

class BuildIos: IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("BuildIos OnPostProcessBuild");

        if (report.summary.platform == BuildTarget.iOS)
        {
            string projectPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            //Disabling Bitcode on all targets
            //Main
            string target = pbxProject.GetUnityMainTargetGuid();
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            //Unity Tests
            target = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            //Unity Framework
            target = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            // libz.tbd for grpc ios build
            pbxProject.AddFrameworkToProject(target, "libz.tbd", false);

            pbxProject.WriteToFile(projectPath);
        }
    }
}
#endif