using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class FixElaraSetup 
{
    static FixElaraSetup()
    {
        EditorApplication.delayCall += () => {
            if (!EditorPrefs.GetBool("ElaraFixed", false)) {
                FixSetup();
                EditorPrefs.SetBool("ElaraFixed", true);
            }
        };
    }

    [MenuItem("Tools/Fix Elara Setup")]
    public static void FixSetup() 
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "GameScene") {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        }

        // 1. Create Player Spawn Point
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint == null) {
            spawnPoint = new GameObject("PlayerSpawnPoint");
            // Find Kael to use his position as initial spawn
            GameObject kael = GameObject.Find("HeroKnight");
            if (kael != null) {
                spawnPoint.transform.position = kael.transform.position;
            } else {
                spawnPoint.transform.position = new Vector3(0, 0, 0);
            }
        }

        // 2. Assign to CharacterManager
        CharacterManager cm = Object.FindAnyObjectByType<CharacterManager>();
        if (cm != null) {
            SerializedObject so = new SerializedObject(cm);
            SerializedProperty propSpawn = so.FindProperty("playerSpawnPoint");
            if (propSpawn != null) {
                propSpawn.objectReferenceValue = spawnPoint.transform;
            }
            so.ApplyModifiedProperties();
        }

        // 3. Fix Elara's scale, collider, and groundCheck
        GameObject elara = GameObject.Find("Elara");
        if (elara != null) {
            // Apply user requested scale
            elara.transform.localScale = new Vector3(3f, 3f, 3f);

            // Fix Collider (scale 3 makes it huge if not adjusted)
            BoxCollider2D bc = elara.GetComponent<BoxCollider2D>();
            if (bc != null) {
                // Approximate size based on King sprite. The king sprite is quite large.
                // You can manually tweak this later.
                bc.size = new Vector2(0.25f, 0.5f);
                bc.offset = new Vector2(0, 0.25f);
            }

            // Create ground check child
            Transform groundCheck = elara.transform.Find("GroundCheck");
            if (groundCheck == null) {
                GameObject gcObj = new GameObject("GroundCheck");
                gcObj.transform.SetParent(elara.transform);
                // Position at the bottom of the collider
                gcObj.transform.localPosition = new Vector3(0, 0, 0); 
                groundCheck = gcObj.transform;
            }

            // Assign groundCheck and whatIsGround in HeroKnight script
            HeroKnight hk = elara.GetComponent<HeroKnight>();
            if (hk != null) {
                SerializedObject so = new SerializedObject(hk);
                SerializedProperty propGC = so.FindProperty("groundCheck");
                if (propGC != null) propGC.objectReferenceValue = groundCheck;

                SerializedProperty propMask = so.FindProperty("whatIsGround");
                if (propMask != null) propMask.intValue = LayerMask.GetMask("Ground");

                so.ApplyModifiedProperties();
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Elara Setup Fixed: Added GroundCheck, adjusted scale/colliders, created PlayerSpawnPoint.");
    }
}
