using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a menu item to create a prefab of the selected game object.
/// 
/// adapted from https://docs.unity3d.com/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html
/// </summary>
public class PrefabCreator : MonoBehaviour
{
    [Tooltip("Exported prefab filename. Defaults to GameObject's name.")]
    public string creatureExportName = "";
    [Tooltip("Path where the prefab will be saved. Must end with a \"/\". Defaults to \"Assets/Prefabs\".")]
    public string creatureExportPath = "";

    // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
    [MenuItem("PG649/Save Prefab")]
    private static void CreatePrefab()
    {
        // Keep track of the currently selected GameObject
        GameObject gameObject = Selection.activeObject as GameObject;
        if (gameObject == null)
        {
            EditorUtility.DisplayDialog(
                            "Select GameObject",
                            "Thou shalt select a gameobject!",
                            "Sure thing!");
            return;
        }

        string path = "Assets/Prefabs/";
        string name = gameObject.name;
        // if (gameObject.TryGetComponent(out PrefabCreator creator))
        // {
        //     path = creator.creatureExportPath;
        //     name = creator.creatureExportName;
        // }

        // // set default values if custom values were not set
        // if (path == "") 
        // if (name == "") 

        string localPathToPrefabFolder = Path.Combine(path, name);
        // Create folder if it does not exist and set path for exported prefab.
        if (!Directory.Exists(localPathToPrefabFolder)) CreateFoldersRecursively(localPathToPrefabFolder);

        string localPathToPrefab = Path.Combine(localPathToPrefabFolder, name + ".prefab");
        // Make sure the file name is unique, in case an existing Prefab has the same name.
        localPathToPrefab = AssetDatabase.GenerateUniqueAssetPath(localPathToPrefab);


        var projectpath = Application.dataPath[..Application.dataPath.LastIndexOf("/")];
        string globalPathToFolder = Path.Combine(projectpath, localPathToPrefabFolder);

        if (!globalPathToFolder.StartsWith(projectpath))
        {
            EditorUtility.DisplayDialog(
                        "Wrong Prefab path",
                        "Thou shalt select a path below the project root!",
                        "Sure thing!");
            return;
        }

        // Create the new Prefab and log whether Prefab was saved successfully.
        PrefabUtility.SaveAsPrefabAsset(gameObject, localPathToPrefab, out bool prefabSuccess);
        if (prefabSuccess == true)
            Debug.Log("Prefab was saved successfully at: " + localPathToPrefab);
        else
            Debug.Log("Prefab failed to save" + prefabSuccess);
    }

    // Disable the menu item if no selection is in place or editor is not in play mode.
    [MenuItem("PG649/Save Prefab", true)]
    private static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject) && EditorApplication.isPlaying;
    }

    // Creates a new menu item 'PG649 > Create Prefab' in the main menu.
    [MenuItem("PG649/Save Prefab As")]
    private static void CreatePrefabAs()
    {
        // Keep track of the currently selected GameObject
        GameObject gameObject = Selection.activeObject as GameObject;
        if (gameObject == null)
        {
            EditorUtility.DisplayDialog(
                            "Select GameObject",
                            "Thou shalt select a gameobject!",
                            "Sure thing!");
            return;
        }

        string path = EditorUtility.SaveFilePanel(
                    "Save Prefab",
                    "Packages/com.pg649.creaturegenerator",
                    gameObject.name,
                    "prefab");// absolute path


        var projectpath = Application.dataPath[..Application.dataPath.LastIndexOf("/")];
        if (!path.StartsWith(projectpath))
        {
            EditorUtility.DisplayDialog(
                        "Wrong Prefab path",
                        "Thou shalt select a path below the project root!",
                        "Sure thing!");
            return;
        }
        var localPath = path[(projectpath.Length + 1)..];

        string globalPathToLSystem = Path.ChangeExtension(path, ".json");
        string localPathToLSystem = globalPathToLSystem[(projectpath.Length + 1)..];

        // Create the new Prefab and log whether Prefab was saved successfully.
        PrefabUtility.SaveAsPrefabAsset(gameObject, localPath, out bool prefabSuccess);
        if (prefabSuccess == true)
            Debug.Log("Prefab was saved successfully at: " + localPath);
        else
            Debug.Log("Prefab failed to save" + prefabSuccess);

    }
    // Disable the menu item if no selection is in place or editor is not in play mode.
    [MenuItem("PG649/Save Prefab As", true)]
    private static bool ValidateCreatePrefabAs()
    {
        return Selection.activeGameObject != null && !EditorUtility.IsPersistent(Selection.activeGameObject) && EditorApplication.isPlaying;
    }

    /// <summary>
    /// Create folders recursively specified by path, separated by /
    /// </summary>
    /// <param name="path">Path alongside which the folders will be created. Separated by /</param>
    /// <returns></returns>
    private static void CreateFoldersRecursively(string path)
    {
        path = path.Trim();
        if (path.EndsWith("/")) path = path[..^1]; // remove trailing /
        string[] folders = path.Split("/");

        for (int i = 1; i < folders.Length; i++)
        {
            // create parent folder path
            string parent = folders[0];
            for (int j = 1; j < i; j++) parent += "/" + folders[j];
            if (!Directory.Exists(parent + "/" + folders[i])) AssetDatabase.CreateFolder(parent, folders[i]);
        }
    }

}