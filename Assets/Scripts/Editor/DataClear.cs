using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DataClear
{
    [MenuItem("Tools/Clear Data")]
    static void Clear()
    {
        PlayerPrefs.DeleteAll();
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
        foreach (string filePath in filePaths)
            File.Delete(filePath);
    }
}
