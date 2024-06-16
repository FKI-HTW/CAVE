using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class KinectCopyPluginDataHelper
{
    private const string DataDirSuffix = "_Data";
    private const string PluginsDirName = "Plugins";
    private const string PackageName = "de.htw.cave";

    private static readonly string packagesDir = Path.Combine(Application.dataPath, "..", "Packages");
    private static readonly string kinectPluginPath = Path.Combine("Kinect", PluginsDirName);

    private static Dictionary<BuildTarget, string> TargetToDirName = new()
    {
        {BuildTarget.StandaloneWindows, "x86"},
        {BuildTarget.StandaloneWindows64, "x86_64"}
    };

    public static void CopyPluginData(BuildTarget target, string buildTargetPath, string subDirToCopy)
    {
        if (!TargetToDirName.TryGetValue (target, out var subDirName))
            return;
        
        var packageDir = FindPackageDirectory(packagesDir);
        if (packageDir == null)
            throw PackageDirectoryNotFoundException();

        // Get Required Paths
        var buildName = Path.GetFileNameWithoutExtension(buildTargetPath);
        var targetDir = Directory.GetParent(buildTargetPath);
        var separator = Path.DirectorySeparatorChar;
        var kinectDir = Path.Combine(packageDir, kinectPluginPath);

        var buildDataDir = targetDir.FullName + separator + buildName + DataDirSuffix + separator;
        var tgtPluginsDir = buildDataDir + separator + PluginsDirName + separator + subDirToCopy + separator;
        var srcPluginsDir = Path.Combine(kinectDir, subDirName, subDirToCopy);

        CopyAll(new(srcPluginsDir), new(tgtPluginsDir));
    }

    /// <summary>
    /// Recursive Copy Directory Method
    /// </summary>
    private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        // Check if the source directory exists, if not, don't do any work.
        if (!Directory.Exists(source.FullName))
        {
            return;
        }

        // Check if the target directory exists, if not, create it.
        if (!Directory.Exists(target.FullName))
        {
            Directory.CreateDirectory(target.FullName);
        }

        // Copy each file into it’s new directory.
        foreach (var fileInfo in source.GetFiles())
        {
            fileInfo.CopyTo (Path.Combine (target.ToString (), fileInfo.Name), true);
        }
        
        // Copy each subdirectory using recursion.
        foreach (var subDirInfo in source.GetDirectories())
        {
            var nextTargetSubDir = target.CreateSubdirectory(subDirInfo.Name);
            CopyAll(subDirInfo, nextTargetSubDir);
        }
    }
    
    private static string FindPackageDirectory(string path)
    {
        // Version control systems add the branch name to the package name sometimes.
        // So if the package cannot be found look for the substring.
			
        var packageDir = new DirectoryInfo(Path.Combine(path, PackageName));

        if (packageDir.Exists)
            return packageDir.FullName;

        foreach (var dir in Directory.GetDirectories(path))
            if (Path.GetDirectoryName(dir).Contains(PackageName))
                return dir;
			
        return null;
    }
    
    private static DirectoryNotFoundException PackageDirectoryNotFoundException() =>
        new("Unable to locate the package directory containing the Kinect Addin.");
}
