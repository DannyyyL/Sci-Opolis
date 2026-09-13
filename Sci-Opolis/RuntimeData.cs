using System;
using System.IO;

namespace SciOpolis
{
    // Keep saves beside the executable, including when launched from another directory.
    internal static class RuntimeData
    {
        private static readonly string[] DefaultFileNames =
        {
            "Stage.txt", "Statistics.txt", "Upgrades.txt"
        };

        internal static string ResolvePath(string filePath)
        {
            return Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
        }

        internal static void EnsureDefaults(string directory)
        {
            Directory.CreateDirectory(directory);

            foreach (string fileName in DefaultFileNames)
            {
                string destination = Path.Combine(directory, fileName);
                if (File.Exists(destination))
                {
                    continue;
                }

                string resourceName = "SciOpolis.DefaultData." + fileName;
                using (Stream source = typeof(RuntimeData).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (source == null)
                    {
                        throw new InvalidOperationException("Missing embedded default data: " + resourceName);
                    }

                    FileStream output;
                    try
                    {
                        // Never replace an existing save, even if another process just created it.
                        output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    }
                    catch (IOException)
                    {
                        if (File.Exists(destination))
                        {
                            continue;
                        }

                        throw;
                    }

                    using (output)
                    {
                        source.CopyTo(output);
                    }
                }
            }
        }
    }
}
