using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class Mission2AutoBuilder
{
    const string ScenePath = "Assets/_Unity Essentials/Scenes/2_KidsRoom_3D_Scene.unity";
    const string Marker = "Mission2_AutoBuilt_V3";

    static Mission2AutoBuilder()
    {
        EditorApplication.delayCall += Build;
    }

    static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.OpenScene(ScenePath);
        if (GameObject.Find(Marker) != null) return;
        foreach (var oldName in new[] { "Mission2_AutoBuilt", "Mission2_AutoBuilt_V2" })
        {
            var oldRoot = GameObject.Find(oldName);
            if (oldRoot != null) Object.DestroyImmediate(oldRoot);
        }

        var root = new GameObject(Marker);
        AddPrefab("Assets/_Unity Essentials/Prefabs/Rooms/01_Bedroom.prefab", Vector3.zero, Vector3.zero, root.transform);
        AddPrefab("Assets/_Unity Essentials/Prefabs/Bedroom/Bed_Twin_BR003.prefab", new Vector3(-2.7f, 0.05f, 1.8f), new Vector3(0, 90, 0), root.transform);
        AddPrefab("Assets/_Unity Essentials/Prefabs/Bedroom/RugRectangleLarge_Purple.prefab", new Vector3(0, 0.03f, 0), Vector3.zero, root.transform);
        AddPrefab("Assets/_Unity Essentials/Prefabs/Bedroom/Nightstand_25in_BR001.prefab", new Vector3(-2.8f, 0.05f, -0.1f), new Vector3(0, 90, 0), root.transform);
        AddPrefab("Assets/_Unity Essentials/Prefabs/Bedroom/Dresser_36in_BR001.prefab", new Vector3(2.8f, 0.05f, 1.8f), new Vector3(0, -90, 0), root.transform);
        AddPrefab("Assets/_Unity Essentials/Prefabs/Bedroom/Rocking Horse.prefab", new Vector3(1.8f, 0.05f, 0.2f), new Vector3(0, -25, 0), root.transform);

        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ball Ramp";
        ramp.transform.SetParent(root.transform);
        ramp.transform.SetPositionAndRotation(new Vector3(-1.8f, 1.0f, -1.9f), Quaternion.Euler(0, 0, -18));
        ramp.transform.localScale = new Vector3(3.3f, 0.18f, 1.0f);
        ramp.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Ramp Blue", new Color(0.15f, 0.45f, 0.85f));

        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Bouncy Ball";
        ball.transform.SetParent(root.transform);
        ball.transform.position = new Vector3(-2.8f, 2.15f, -1.9f);
        ball.transform.localScale = Vector3.one * 0.55f;
        ball.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Ball Orange", new Color(1f, 0.25f, 0.05f));
        var ballBody = ball.AddComponent<Rigidbody>();
        ballBody.mass = 1.2f;
        var bounce = new PhysicsMaterial("Bouncy Ball Physics") { bounciness = 0.82f, dynamicFriction = 0.25f, staticFriction = 0.25f, bounceCombine = PhysicsMaterialCombine.Maximum };
        ball.GetComponent<SphereCollider>().material = bounce;

        Color[] colors = { new Color(0.95f,0.18f,0.2f), new Color(1f,0.75f,0.1f), new Color(0.15f,0.7f,0.35f), new Color(0.15f,0.45f,0.9f), new Color(0.65f,0.25f,0.85f) };
        int index = 0;
        var towerBodies = new List<Rigidbody>();
        Vector3 towerCenter = new Vector3(1.65f, 0, -1.9f);
        for (int level = 0; level < 4; level++)
        {
            bool rotate = level % 2 == 1;
            for (int i = 0; i < 3; i++)
            {
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = $"Tower Block {level + 1}-{i + 1}";
                block.transform.SetParent(root.transform);
                float offset = (i - 1) * 0.43f;
                block.transform.position = towerCenter + new Vector3(rotate ? offset : 0, 0.255f + level * 0.515f, rotate ? 0 : offset);
                block.transform.rotation = Quaternion.Euler(0, rotate ? 90 : 0, 0);
                block.transform.localScale = new Vector3(0.8f, 0.5f, 0.38f);
                block.GetComponent<Renderer>().sharedMaterial = MakeMaterial($"Block Color {index}", colors[index++ % colors.Length]);
                var body = block.AddComponent<Rigidbody>();
                body.mass = 0.6f;
                body.linearDamping = 0.12f;
                body.angularDamping = 0.15f;
                body.sleepThreshold = 0.02f;
                body.isKinematic = true;
                towerBodies.Add(body);
            }
        }
        var trigger = ball.AddComponent<Mission2BallTrigger>();
        trigger.blocks = towerBodies.ToArray();

        var light = Object.FindFirstObjectByType<Light>();
        if (light != null) { light.intensity = 1.25f; light.color = new Color(1f, 0.92f, 0.82f); light.transform.rotation = Quaternion.Euler(48, -32, 0); }
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            camera.transform.position = new Vector3(0, 5.7f, -9.5f);
            camera.transform.rotation = Quaternion.Euler(24, 0, 0);
            camera.fieldOfView = 55;
            camera.backgroundColor = new Color(0.2f, 0.3f, 0.48f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = root;
        Debug.Log("Mission 2 scene built and saved successfully.");
    }

    static GameObject AddPrefab(string path, Vector3 position, Vector3 rotation, Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogWarning("Missing prefab: " + path); return null; }
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
        return obj;
    }

    static Material MakeMaterial(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = name, color = color };
        return material;
    }
}
