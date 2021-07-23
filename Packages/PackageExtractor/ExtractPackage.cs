#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

#endregion

public static class ExtractPackage
{
    static string TargetPackage = "";
    static string source;
    static string location;

    static string packageLocation => Path.Combine(Path.GetFullPath($"Packages/{TargetPackage}"), source);
    static string LocalLocation => Path.Combine(Application.dataPath, location);

    static EmbedRequest Request;
    static ListRequest LRequest;

    static bool locked;

    public static void Extract(string packageName, string sourceLocation, string targetLocation)
    {
        if(locked)
        {
            return
        }
        locked = true;
        TargetPackage = packageName;
        source = sourceLocation;
        location = targetLocation;

        CopyLocalFiles();
    }
    
    
    static void CopyLocalFiles()
    {
        if (!Directory.Exists(LocalLocation) && Directory.Exists(packageLocation))
        {
            // First get the name of an installed package
            LRequest = Client.List(false, true);
            EditorApplication.update += LProgress;
        }
    }

    static void LProgress()
    {
        if (LRequest.IsCompleted)
        {
            if (LRequest.Status == StatusCode.Success)
            {
                foreach (PackageInfo package in LRequest.Result)
                {
                    if (package.name == TargetPackage)
                    {
                        Embed();
                    }
                }
            }
            else
            {
                //Debug.Log(LRequest.Error.message);
                locked = false;
            }

            EditorApplication.update -= LProgress;
        }
    }

    static void Embed()
    {
        Request = Client.Embed(TargetPackage);
        EditorApplication.update += Progress;
    }

    static void Progress()
    {
        if (Request.IsCompleted)
        {
            EditorApplication.update -= Progress;

            if (!Directory.Exists(LocalLocation))
            {
                Directory.CreateDirectory(LocalLocation);
                MoveDirectory(packageLocation, LocalLocation);
            }
            else
            {
                Directory.Delete(packageLocation);
            }
            
                        locked = false;
        }
    }

    public static void MoveDirectory(string source, string target)
    {
        string sourcePath = source.TrimEnd('\\', ' ').Replace("/", "\\");
        string targetPath = target.TrimEnd('\\', ' ').Replace("/", "\\");
        
        CopyFilesRecursively(sourcePath, targetPath);

        Directory.Move(source, source + "~");
        locked = false;
    }
    
    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}
