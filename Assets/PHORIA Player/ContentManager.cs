using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;
using SimpleDiskUtils;
using UnityEngine;

using static Place;

public static class ContentManager
{
    [Serializable]
    public class Folder
    {
        public Folder parent;
        public string name;
        public Dictionary<string, Folder> subFolders = new Dictionary<string, Folder>();
        public Dictionary<string, File> files = new Dictionary<string, File>();

        public Folder() { }

        public Folder(Folder parent, string name)
        {
            this.name = name;
            this.parent = parent;
        }

        public ulong GetBytes()
        {
            ulong bytes = 0;

            foreach (File file in files.Values) bytes += file.bytes;
            foreach (Folder subFolder in subFolders.Values) bytes += subFolder.GetBytes();

            return bytes;
        }

        public Folder GetRelativeFolder(string relativePath)
        {
            string[] path = relativePath.Split(Path.DirectorySeparatorChar);
            
            Folder currentFolder = this;

            foreach (string folder in path)
            {
                Debug.LogWarning($"Looking for subdirectory {folder} in {currentFolder.GetAbsolutePath()}");
                
                if (currentFolder.subFolders.ContainsKey(folder))
                {
                    currentFolder = currentFolder.subFolders[folder];
                }
                else
                {
                    Debug.LogWarning($"Failed to find relative folder \"{relativePath}\" at path segment \"{folder}\" in \"{GetAbsolutePath()}\"");

                    return null;
                }
            }

            return currentFolder;
        }

        public File GetRelativeFile(string relativePath, string fileName)
        {
            Folder folder = GetRelativeFolder(relativePath);

            if (folder != null && folder.files.ContainsKey(fileName)) return folder.files[fileName];
            else { Debug.LogWarning($"Faled to find file {fileName} in folder {relativePath}"); return null; }
        }

        public File GetFile(string name) => files[name];

        public string GetAbsolutePath()
        {
            StringBuilder stringBuilder = new StringBuilder();
            Folder currentFolder = this;

            while (currentFolder.parent != null) { stringBuilder.Insert(0, currentFolder.name); if (currentFolder.parent.parent != null) stringBuilder.Insert(0, Path.DirectorySeparatorChar); currentFolder = currentFolder.parent; }

            return stringBuilder.ToString();
        }

        public string GetHierarchyString(int depth = 0)
        {
            Folder currentFolder = this;

            StringBuilder stringBuilder = new StringBuilder();

            foreach(File file in files.Values)
            {
                stringBuilder.Append(System.Environment.NewLine);
                for (int i = 0; i < depth; i++) stringBuilder.Append("-");
                stringBuilder.Append(file.name);
            }
            foreach (Folder folder in subFolders.Values)
            {
                stringBuilder.Append(System.Environment.NewLine);
                for (int i = 0; i < depth; i++) stringBuilder.Append("-");
                stringBuilder.Append(folder.name);
                stringBuilder.Append(folder.GetHierarchyString(depth + 1));
            }
            
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return GetAbsolutePath();
        }
    }

    [Serializable]
    public class File
    {
        public Folder parent;
        public string name;
        public string extension;
        public ulong bytes;

        public File(Folder parent, string name, string extension, ulong bytes)
        {
            this.name = name;
            this.extension = extension;
            this.bytes = bytes;
            this.parent = parent;
        }

        public string GetAbsolutePath()
        {
            return $"{parent.GetAbsolutePath()}{Path.DirectorySeparatorChar}{name}";
        }
    }

//    public static string localFolderPath;
    
    public static Folder localDirectory;
    
    public static Folder remoteDirectory;
    
//    public static async Task BuildFolderLists(string localFolderPath)
//    {
//        localDirectory = new Folder();
//        remoteDirectory = new Folder();
//
//        ContentManager.localFolderPath = localFolderPath;
//
//        BuildLocalDirectory("", localDirectory);
//        await BuildRemoteDirectoryAsync("", remoteDirectory);
//    }


    public static void BuildRootFolder()
    {
        localDirectory = new Folder(parent: null,
                                    name: Paths.Local.Root);


    }

    public static async Task BuildFolderLists()
    {
        localDirectory  = new Folder();
        remoteDirectory = new Folder();

//        ContentManager.localFolderPath = localFolderPath;

        BuildLocalDirectory("", localDirectory);
        await BuildRemoteDirectoryAsync("", remoteDirectory);
    }

    static void BuildLocalDirectory(string path, Folder folder)
    {
        Debug.LogWarning("Building local directory at\n" + Paths.Local.Root);

        if (!Directory.Exists(Paths.Local.Root))
        {
            Directory.CreateDirectory(Paths.Local.Root);
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Paths.Local.Root,
                                                                     path));

        foreach (FileInfo file in directoryInfo.GetFiles())
            folder.files.Add(file.Name,
                             new File(folder,
                                      file.Name,
                                      file.Extension,
                                      (ulong)file.Length));

        foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            folder.subFolders.Add(directory.Name,
                                  new Folder(folder,
                                             directory.Name));

        foreach (Folder subFolder in folder.subFolders.Values) BuildLocalDirectory(Path.Combine(path, subFolder.name), subFolder);
    }

    static async Task BuildRemoteDirectoryAsync(string key, Folder folder)
    {
        var results = await AWS.S3.ListObjectsV2Async(key);
        
        string[] path;
        string fileName;
        int extensionIndex;
        Folder currentFolder = folder;

        foreach (S3Object s3Object in results.S3Objects)
        {
            path = s3Object.Key.Split('/');
            fileName = path[path.Length - 1];
            extensionIndex = fileName.IndexOf('.');
            currentFolder = folder;

            for (int i = 0; i < path.Length - 1; i++)
            {
                if (!currentFolder.subFolders.ContainsKey(path[i])) currentFolder.subFolders.Add(path[i], new Folder(currentFolder, path[i]));
                currentFolder = currentFolder.subFolders[path[i]];
            }

            if (extensionIndex >= 0) currentFolder.files.Add(fileName, new File(currentFolder, fileName, fileName.Substring(extensionIndex), (ulong)s3Object.Size));
        }
    }


    public static List<string> GetMissingFileKeys(string path)
    {
        List<string> missingFileKeys = new List<string>();
        Debug.Log(path);
        Folder currentFolder = remoteDirectory.GetRelativeFolder(path);

        foreach (File file in currentFolder.files.Values)
            if (localDirectory.GetRelativeFile(path,
                                               file.name) ==
                null)
                missingFileKeys.Add($"{path}/{file.name}");

        foreach (Folder subFolder in currentFolder.subFolders.Values) missingFileKeys.AddRange(GetMissingFileKeys(subFolder.GetAbsolutePath()));

        return missingFileKeys;
    }


    public static long GetAvailableDiscSpace()
    {
        return ((long)DiskUtils.CheckAvailableSpace() * 1048576) - 104857600;  //104857600 is 100 mb in bytes
    }
}
