using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public static class EmbedPackage
{
    static ListRequest LRequest;
    static EmbedRequest Request;

    static List<string> noneEmbeded = new List<string>();
    
    [InitializeOnLoadMethod]
    static void GetItems()
    {
        LRequest = Client.List(false, true);
        EditorApplication.update += LProgress;
    }
    
    static void LProgress()
    {
        if (LRequest.IsCompleted)
        {
            if (LRequest.Status == StatusCode.Success)
            {
                
                noneEmbeded.Clear();
                foreach (PackageInfo package in LRequest.Result)
                {
                    if (package.isDirectDependency && (package.source != PackageSource.Embedded && package.source != PackageSource.Local))
                    {
                        noneEmbeded.Add(package.name);
                    }
                }
            }

            EditorApplication.update -= LProgress;
        }
    }
    

    [MenuItem("Assets/Embed Package", false, 99999999)]
    static void EmbedPackageMenuItem()
    {
        Object selection = Selection.activeObject;
        string packageName = Path.GetFileName(AssetDatabase.GetAssetPath(selection));
        
        Request = Client.Embed(packageName);
        EditorApplication.update += EmbedProcess;
    }

    [MenuItem("Assets/Embed Package", true)]
    static bool EmbedPackageValidation()
    {
        Object selection = Selection.activeObject;

        if (selection == null)
        {
            return false;
        }

        return noneEmbeded.Contains(selection.name);
    }
    
    static void EmbedProcess()
    {
        if (Request.IsCompleted)
        {
            Debug.Log("Package Embeded");
        }
    }
}