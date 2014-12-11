/*
   Tuner Data -  Read Static Data in Game Development.
   e-mail : dongliang17@126.com
*/
using System.Collections.Generic;
using System.IO;
namespace Data.Excel
{
    /// <summary>
    /// field type
    /// </summary>
    public enum FIELD_TYPE
    {
        T_INT = 0,
        T_FLOAT = 1,
        T_STRING = 2,
        T_INVALID = -1
    };
    public class Util
    {
        /// <summary>
        /// make sure path Has been created.
        /// </summary>
        /// <param name="localPath">path</param>
        /// <returns>path</returns>
        static public string CreateDirectory(string localPath)
        {
            string path = localPath;
            if (System.IO.Directory.Exists(path) == false)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetFileName(string path)
        {
            path = path.Replace(@"\", @"/");
            string[] sArr1 = path.Split(new char[] { '/' });
            string str1 = sArr1[sArr1.Length - 1];
            string[] sArr2 = str1.Split(new char[] { '.' });
            string FileName = sArr2[0];
            return FileName; 
        }

        public static string GetFileSuffix(string path)
        {
            path = path.Replace(@"\", @"/");

            string[] sArr1 = path.Split(new char[] { '/' });
            string str1 = sArr1[sArr1.Length - 1];
            string[] sArr2 = str1.Split(new char[] { '.' });
            string suffix = sArr2[sArr2.Length - 1];
            return suffix;
        }

        public static string[] GetAllFilePath(string folder, string a_suffix, bool recursive)
        {
            List<string> folders = new List<string>();
            List<string> paths = new List<string>();
            folders.Add(folder);
            if (recursive)
            {
                DirectoryInfo sourceInfo = new DirectoryInfo(folder);
                foreach (DirectoryInfo item in sourceInfo.GetDirectories())
                {
                    folders.Add(item.FullName);
                }
            }

            foreach (string item in folders)
            {
                DirectoryInfo FolderInfo = new DirectoryInfo(item);

                foreach (FileInfo fileInfo in FolderInfo.GetFiles())
                {
                    string path = fileInfo.FullName;
                    string suffix = GetFileSuffix(path);
                    if (suffix == a_suffix)
                    {
                        path = path.Replace(@"\", @"/");
                        paths.Add(path);
                    }
                }
            }
           return  paths.ToArray();
        }

    }
}