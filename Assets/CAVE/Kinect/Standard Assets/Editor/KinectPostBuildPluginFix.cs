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
    
    private const string DATA_DIR_SUFFIX = "_Data";
    private const string PLUGINS_DIR_NAME = "Plugins";
    private static readonly Dictionary<BuildTarget, string> TargetToDirName = new()
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
        var dataPath = Path.Combine(Path.GetDirectoryName(executable), buildName + DATA_DIR_SUFFIX);
        
        try
        {
            var pluginsDir = Path.Combine(dataPath, PLUGINS_DIR_NAME);
            var plugins = Directory.GetFiles(Path.Combine(pluginsDir, subDirName));

            foreach (var plugin in plugins)
            {
                var destFile = Path.Combine(pluginsDir, Path.GetFileName(plugin));
                if (File.Exists(destFile))
                    File.Delete(destFile);
                File.Move(plugin, destFile);
            }
        } catch (DirectoryNotFoundException) {}
    }
}
