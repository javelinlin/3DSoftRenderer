using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

// https://answers.unity.com/questions/722507/making-mesh-non-readable.html
// Select objects in scene, then press the button to apply needed configuration 
// to all mesh-assets at once.
// Igor Aherne 05 oct 2017  facebook.com/igor.aherne
public class TweakMeshAssets : EditorWindow
{

    bool _isMeshReadable;

    HashSet<GameObject> _gameObjectsWithNoAssets = new HashSet<GameObject>();


    [MenuItem("Tools/Tweak Mesh Assets from selection")]
    static void Init()
    {
        TweakMeshAssets window = EditorWindow.CreateInstance<TweakMeshAssets>() as TweakMeshAssets;
        window.Show();
    }


    public void OnGUI()
    {

        _isMeshReadable = EditorGUILayout.Toggle("is mesh readable", _isMeshReadable);

        if (GUILayout.Button("apply configuration to mesh assets of selected Objects"))
        {
            _gameObjectsWithNoAssets.Clear();
            //shared meshes, coupled with the gameObjects that carry them:
            Dictionary<Mesh, GameObject> meshesOfSelectedObjects = allMeshesOfSeletion();
            ToggleIsReadable(meshesOfSelectedObjects);
        }

        if (GUILayout.Button("select all gameObjects that didn't have assets from the previous Apply"))
        {
            Selection.objects = _gameObjectsWithNoAssets.ToArray();
        }


        //MiscTools.Editor_UIFunctions_Misc.DrawMany_GUI_Spaces(2);
        EditorGUILayout.LabelField("Bonus: sometimes the meshes can have a bug, where they are showing as Instance");
        EditorGUILayout.LabelField("And don't seem to actually be linked to the actual mesh asset. Use folowing:");
        EditorGUILayout.LabelField("NOTICE: BACK UP YOUR SCENE FILE SO U CAN ROLL BACK when using this bonus!!!!!!");
        if (GUILayout.Button("revert Selected gameObjects to prefab, but preserve transfrom values (Non-recursive!)"))
        {
            revertSelectedToPrefab_keepTransformVals();
        }
    }//end OnGUI()




    void ToggleIsReadable(Dictionary<Mesh, GameObject> meshesOfSelectedObjects)
    {
        foreach (var kvp in meshesOfSelectedObjects)
        {
            Mesh sharedMesh = kvp.Key;
            GameObject gameObj = kvp.Value;

            string asset_path = AssetDatabase.GetAssetPath(sharedMesh);
            ModelImporter importerForAsset = ModelImporter.GetAtPath(asset_path) as ModelImporter;

            if (importerForAsset == null)
            {
                Debug.Log("couldn't find an asset for the mesh belonging to " + gameObj);
                _gameObjectsWithNoAssets.Add(gameObj);
                continue;
            }


            importerForAsset.isReadable = _isMeshReadable;
            EditorUtility.SetDirty(sharedMesh);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }



    void revertSelectedToPrefab_keepTransformVals()
    {
        foreach (GameObject go in Selection.gameObjects)
        {

            GameObject go_prefabRoot = PrefabUtility.FindPrefabRoot(go);

            Vector3 pos = go_prefabRoot.transform.position;
            Quaternion rot = go_prefabRoot.transform.rotation;
            Vector3 scale = go_prefabRoot.transform.localScale;

            Undo.RegisterCompleteObjectUndo(go_prefabRoot, "reverting selected Gos to prefabs, keeping transforms");
            PrefabUtility.RevertPrefabInstance(go_prefabRoot);
            go_prefabRoot.transform.position = pos;
            go_prefabRoot.transform.rotation = rot;
            go_prefabRoot.transform.localScale = scale;

            EditorUtility.SetDirty(go_prefabRoot);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
    }



    Dictionary<Mesh, GameObject> allMeshesOfSeletion()
    {
        Dictionary<Mesh, GameObject> meshesOfSelectedObjects = new Dictionary<Mesh, GameObject>();

        foreach (object obj in Selection.objects)
        {
            GameObject obj_asGo = obj as GameObject;

            if (obj_asGo != null)
            {
                sharedMeshes_fromChildren(obj_asGo.transform, meshesOfSelectedObjects);
                continue;
            }

        }//end foreach obj in selection

        return meshesOfSelectedObjects;
    }


    //recursive, will extract all meshes (will auto-exclude duplicates) from the sub-children.
    //Returns shared meshes, coupled with their gameObjects
    void sharedMeshes_fromChildren(Transform currTransform, Dictionary<Mesh, GameObject> resultingMeshes)
    {

        for (int i = 0; i < currTransform.childCount; i++)
        {
            Transform child = currTransform.GetChild(i);
            sharedMeshes_fromChildren(child, resultingMeshes);
        }//end for child transforms



        MeshFilter mf = currTransform.GetComponent<MeshFilter>();
        if (mf == null)
        {
            return;
        }

        if (mf.sharedMesh == null)
        {
            return;
        }

        if (resultingMeshes.ContainsKey(mf.sharedMesh) == true)
        {
            return;
        }
        resultingMeshes.Add(mf.sharedMesh, currTransform.gameObject);
    }


}
