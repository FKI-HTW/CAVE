using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class KinectPostBuildPluginFix : IPostprocessBuildWithReport
{
    public int callbackOrder => 1000;

    public void OnPostprocessBuild(BuildReport report)
    {
        FixBuildPluginSubdirectory(report.summary.outputPath, report.summary.platform);
    }
    
    private const string DataDirSuffix = "_Data";
    private const string PluginsDirName = "Plugins";
    private static Dictionary<BuildTarget, string> TargetToDirName = new()
    {
        {BuildTarget.StandaloneWindows, "x86"},
        {BuildTarget.StandaloneWindows64, "x86_64"}
    };
    
    private static void FixBuildPluginSubdirectory(string executable, BuildTarget target)
    {
        // In older Unity versions the plugins are copied directly under the Plugins/ folder.
        // Newer Unity versions (2019.4 and higher) copy the plugins to a subfolder
        // based on the platform like Plugins/x86_64 which results in broken file path
        // lookups from the Kinect Addin.

        // The fix tries to restore the old directory structure.
        // In the future it would be better to replace the KinectCopyPluginDataHelper
        // with a variant that works without moving the Plugin folder everytime.
        
        if (!TargetToDirName.TryGetValue (target, out var subDirName))
            return;

        var buildName = Path.GetFileNameWithoutExtension(executable);
        var dataPath = Path.Combine(Path.GetDirectoryName(executable), buildName + DataDirSuffix);
        
        try
        {
            var pluginsDir = Path.Combine(dataPath, PluginsDirName);
            var plugins = Directory.GetFiles(Path.Combine(pluginsDir, subDirName));

            foreach (var plugin in plugins)
                File.Move(plugin, Path.Combine(pluginsDir, Path.GetFileName(plugin)));
        } catch (DirectoryNotFoundException) {}
    }
}
