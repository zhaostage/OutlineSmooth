using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class OutLineBaker : AssetPostprocessor
{
    static bool bExecuting = false;
    [MenuItem("Tools/BakeNormal", false, 100)]
    
    public static void BakeNormal()
        {
        Object obj = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(obj);
        string ext = Path.GetExtension(path);

        if(ext == ".fbx")
        {
            bExecuting = true;
            string newPath = Path.GetDirectoryName(path) + "/copy_@@@" + Path.GetFileName(path);
            Debug.Log($"new Path= {newPath}");
            AssetDatabase.CopyAsset(path, newPath);
            //AssetDatabase.ImportAsset(newPath);


        }
        else
        {
            Debug.Log($"�����.fbx�ļ����д˲���");
        }
        Debug.Log($"EXT name = {ext}");

        }
    static void OnPostOver(string path,bool isCopy)
    {
        if (isCopy)
        {
            string srcPath = path.Replace("copy_@@@", "");
            Debug.Log($"���������󣬱����path={srcPath}");
            AssetDatabase.ImportAsset(srcPath);
        }

    }
    void OnPreprocessModel()
    {
        if (!bExecuting) return;
        //Input Settings

        ModelImporter importer = assetImporter as ModelImporter;

        if (!assetImporter.assetPath.Contains("copy_@@@")) return;

        importer.importNormals = ModelImporterNormals.Calculate;
        importer.normalCalculationMode = ModelImporterNormalCalculationMode.AngleWeighted;
        importer.normalSmoothingAngle = 180f;
        importer.importAnimation = false;//no animation imported
        importer.materialImportMode = ModelImporterMaterialImportMode.None;//no material imported
        Debug.Log($"1");
    }

    void OnPostprocessModel(GameObject go)
    {
        if (!bExecuting) return;
        //AfterInputing Settings

        if(go.name.Contains("copy_@@@"))
        {
            //Debug.Log($"Enter the copy obj postprocess");
            OnPostOver(assetPath, true);
        }
        else
        {
           // Debug.Log($"Self");
            string copyPath = Path.GetDirectoryName(assetPath) + "/copy_@@@" + Path.GetFileName(assetPath);
            Debug.Log($"�������Path = {copyPath}");
            GameObject cGo = AssetDatabase.LoadAssetAtPath<GameObject>(copyPath);

            Dictionary<string, Mesh> dic_name2mesh_src = GetMesh(go);//ԭ��ģ�͵ĸ�����Mesh,ͨ���ڵ�������
            Dictionary<string, Mesh> dic_name2mesh_copy = GetMesh(cGo);//ƽ��ģ�͸�����Mesh,ͨ���ڵ�������

            foreach (var item in dic_name2mesh_src) 
            {
                item.Value.colors = GetColor(item.Value,dic_name2mesh_copy[item.Key]);
            }


        }

      
    }
    Dictionary<string,Mesh>GetMesh(GameObject go)
    {
        Dictionary<string, Mesh> dic = new();
        foreach (var item in go.GetComponentsInChildren<MeshFilter>())
        {
            string n = item.name.Replace("copy_@@@", "");
            dic.Add(n, item.sharedMesh);
        }

        if (dic.Count == 0)
        {
            foreach (var item in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                string n = item.name.Replace("copy_@@@", "");
                dic.Add(n, item.sharedMesh);
            }
        }

        return dic;
    }

    Color[] GetColor(Mesh srcMesh,Mesh smthMesh)
    {
        //���ն���λ���������決��Ϣ
        int lenSrc = srcMesh.vertices.Length;
        int lenSmth = smthMesh.vertices.Length;
        int maxOverlap = 10;

        NativeArray<Vector3> arr_vert_smth = new(smthMesh.vertices,Allocator.Persistent);//AllocatorΪ��Դ����ķ�ʽ��һ��ѡ��Persistent
        NativeArray<Vector3> arr_normal_smth = new(smthMesh.normals, Allocator.Persistent);

        NativeArray<UnsafeHashMap<Vector3, Vector3>> arr_vert2normal = new(maxOverlap, Allocator.Persistent);










        arr_vert_smth.Dispose();//���е�Nativeϵ����������֧��GC������Ҫ�ֶ�Dispose

    }

    struct CollectNormalJob : IJobParallelFor
    {
        NativeArray<Vector3> verts;

        NativeArray<Vector3> normals;


        //����-���ߵ�ӳ��
        NativeArray<UnsafeHashMap<Vector3, Vector3>> arr_vert2normal;
        


        public void Execute(int index)
        {

        }
    }
}
