// using UnityEngine;
// using UnityEditor;

// public static class CopyUIToolkitAssetPath
// {
//     [MenuItem("Assets/Copy UI Toolkit USS Path", false, 2000)]
//     public static void CopyUSSPath()
//     {
//         Object selected = Selection.activeObject;
//         if (selected == null)
//         {
//             Debug.LogWarning("No asset selected.");
//             return;
//         }

//         // Get asset path and GUID
//         string assetPath = AssetDatabase.GetAssetPath(selected);
//         string guid = AssetDatabase.AssetPathToGUID(assetPath);

//         // Escape spaces
//         string escapedPath = assetPath.Replace(" ", "%20");

//         // Get name without extension (for the # part)
//         string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

//         // Guess fileID based on asset type (textures usually 21300000)
//         long fileId;
//         AssetDatabase.TryGetGUIDAndLocalFileIdentifier(selected, out guid, out fileId);

//         // Construct the UI Toolkit USS URL
//         string ussPath = $"project://database/{escapedPath}?fileID={fileId}&guid={guid}&type=3#{assetName}";

//         // Copy to clipboard
//         EditorGUIUtility.systemCopyBuffer = ussPath;
//         Debug.Log($"Copied UI Toolkit USS path:\n{ussPath}");
//     }

//     // Enable menu only when an asset is selected
//     [MenuItem("Assets/Copy UI Toolkit USS Path", true)]
//     public static bool ValidateCopyUSSPath()
//     {
//         return Selection.activeObject != null;
//     }
// }
