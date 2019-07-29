using System;
using System.IO;
namespace Tool.Copy
{
    class Program
    {
        static void Main(string[] args)
        {
           CopyDirectory();
        }
        protected static   void CopyDirectory()
        {
            var path = AppContext.BaseDirectory;
            path = path.Substring(0, path.IndexOf("\\Tool"));
            var sourcepath = $"{path}\\ConfigurationFile";//资源路径
            string[] sourcefiles = Directory.GetFiles(sourcepath, "*.json");
            var destinationpath = $"{path}\\Web";
            if (Directory.Exists(path))
            {
                DirectoryInfo root = new DirectoryInfo(destinationpath);
                DirectoryInfo[] dics = root.GetDirectories();
                foreach (var dic in dics)
                {
                    var newpath = dic.FullName;
                    DirectoryInfo files = new DirectoryInfo(newpath);
                    var directories = files.GetDirectories();
                    foreach (var directory in directories)
                    {
                        var finalpath = $"{directory.FullName}\\bin\\Debug\\netcoreapp2.1";//目标路径
                        ReplaceFiles(sourcepath, finalpath);
                     }
                }
            }
        }

        private static void ReplaceFiles(string orgPath, string desPath)
        {
            if (!Directory.Exists(desPath))
                Directory.CreateDirectory(desPath);
            var orgDirectory = new DirectoryInfo(orgPath);
            var files = orgDirectory.GetFiles("*.json");
            foreach (var file in files)
            {
                var desFileName = Path.Combine(desPath, file.Name);
                var desFile = new FileInfo(desFileName);
                if (!desFile.Exists || file.LastWriteTime > desFile.LastWriteTime)
                    file.CopyTo(desFileName, desFile.Exists);
            }
            var directories = orgDirectory.GetDirectories();
            foreach (var directory in directories)
            {
                if (directory.Name == "bin" || directory.Name == "obj")
                    continue;
                ReplaceFiles(Path.Combine(orgPath, directory.Name),
                    Path.Combine(desPath, directory.Name));
            }
        }
    }
}
