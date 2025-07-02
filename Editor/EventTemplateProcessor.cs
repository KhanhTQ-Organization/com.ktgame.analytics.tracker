using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;

namespace com.ktgame.analytics.tracker.editor
{
    public class EventTemplateProcessor : AssetPostprocessor
    {
        private const string PackageName = "com.ktgame.analytics.tracker";
        private const string GeneratePath = "GeneratedData/Analytics";
        private static EventGenerator _generator;
        private static EventImporter _importer;
        private static EventExporter _exporter;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var isAssetModified = importedAssets.Any(path => path.Contains(GeneratePath)) ||
                                        deletedAssets.Any(path => path.Contains(GeneratePath)) ||
                                        movedAssets.Any(path => path.Contains(GeneratePath)) ||
                                        movedFromAssetPaths.Any(path => path.Contains(GeneratePath));

            if (isAssetModified)
            {
                // GenerateTrackingData();
            }
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (listRequest.Error != null)
            {
                return;
            }

            var packages = listRequest.Result;
            foreach (var package in packages)
            {
                if (package.source != PackageSource.Registry) continue;
                if (package.name != PackageName) continue;
                // InitRequiredResources();
                break;
            }
        }

        private static void InitRequiredResources()
        {
            var destPath = Path.Combine(Application.dataPath, GeneratePath);
            if (Directory.Exists(destPath)) return;
            Directory.CreateDirectory(destPath);
            CopySampleContent(destPath);
        }

        private static void GenerateTrackingData()
        {
            if (_generator == null)
            {
                _importer = new EventImporter(Path.Combine(Application.dataPath, GeneratePath));
                _exporter = new EventExporter(Path.Combine(Application.dataPath, GeneratePath));
                _generator = new EventGenerator(_importer, _exporter);
            }

            _generator.GenerateStructEvent();
            _generator.GenerateStructUserProperties();
            _generator.GenerateStructMachineLearningEvent();
        }

        private static void CopySampleContent(string destPath)
        {
            string packagePath = GetPackagePath(PackageName);
            string packageTemplatePath = Path.Combine(packagePath, "Template");
            Copy(packageTemplatePath, destPath);
        }

        private static string GetPackagePath(string packageId)
        {
            var projectRootPath = GetProjectRootPath();
            var packageRootPath = Path.Combine(projectRootPath, "Library/PackageCache");
            var packageRootDir = new DirectoryInfo(packageRootPath);
            if (packageRootDir == null)
            {
                throw new DirectoryNotFoundException($"ERROR::: Path does not exist: {packageRootPath}.");
            }

            foreach (var dirInfo in packageRootDir.GetDirectories())
            {
                if (!dirInfo.Name.Contains(packageId)) continue;
                return dirInfo.FullName;
            }

            throw new DirectoryNotFoundException($"ERROR::: Package {packageId} not found at {packageRootPath}");
        }

        private static string GetProjectRootPath()
        {
            DirectoryInfo projectFolderDir = new DirectoryInfo(Application.dataPath).Parent;
            if (projectFolderDir == null)
            {
                throw new DirectoryNotFoundException($"ERROR::: Path does not exist: {Application.dataPath}.");
            }

            return projectFolderDir.FullName;
        }

        private static void Copy(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"Source directory does not exist or could not be found: {sourceDirName}");
            }

            Directory.CreateDirectory(destDirName);

            foreach (var file in dir.GetFiles())
            {
                File.Copy(file.FullName, Path.Combine(destDirName, file.Name));
            }

            foreach (var subDir in dir.GetDirectories())
            {
                Copy(subDir.FullName, Path.Combine(destDirName, Path.GetFileName(subDir.Name)));
            }
        }
    }
}