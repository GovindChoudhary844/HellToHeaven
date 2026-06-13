using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public class CreateElaraPrefab 
{
    static CreateElaraPrefab()
    {
        EditorApplication.delayCall += () => {
            if (!EditorPrefs.GetBool("ElaraSetupComplete", false)) {
                CreateAndWireElara();
                EditorPrefs.SetBool("ElaraSetupComplete", true);
            }
        };
    }

    [MenuItem("Tools/Setup Elara (Medieval King)")]
    public static void CreateAndWireElara() 
    {
        // 1. Copy Kael's Animator Controller
        string sourceControllerPath = "Assets/Hero Knight - Pixel Art/Animations/HeroKnight_AnimController.controller";
        string targetControllerPath = "Assets/Medieval King Pack 2/Animations/ElaraAnimator.controller";
        
        AssetDatabase.DeleteAsset(targetControllerPath);
        AssetDatabase.CopyAsset(sourceControllerPath, targetControllerPath);
        
        AnimatorController elaraController = AssetDatabase.LoadAssetAtPath<AnimatorController>(targetControllerPath);
        
        // 2. Load Elara's Clips
        Dictionary<string, AnimationClip> elaraClips = new Dictionary<string, AnimationClip>
        {
            { "HeroKnight_Idle", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Idle.anim") },
            { "HeroKnight_Run", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Run.anim") },
            { "HeroKnight_Jump", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Jump.anim") },
            { "HeroKnight_Fall", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Fall.anim") },
            { "HeroKnight_Attack1", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Attack1.anim") },
            { "HeroKnight_Attack2", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Attack2.anim") },
            { "HeroKnight_Attack3", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Attack3anim.anim") },
            { "HeroKnight_Hurt", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Take Hit.anim") },
            { "HeroKnight_Death", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Death.anim") },
            // Fallbacks for missing animations
            { "HeroKnight_Roll", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Idle.anim") },
            { "HeroKnight_Block", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Idle.anim") },
            { "HeroKnight_IdleBlock", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Idle.anim") },
            { "HeroKnight_WallSlide", AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Medieval King Pack 2/Animations/Fall.anim") }
        };

        // 3. Replace Clips in Controller
        if (elaraController != null) 
        {
            foreach (var layer in elaraController.layers) 
            {
                ReplaceClipsInStateMachine(layer.stateMachine, elaraClips);
            }
            EditorUtility.SetDirty(elaraController);
        }

        // 4. Create Elara Prefab
        GameObject go = new GameObject("Elara");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        
        // Assign default sprite
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Medieval King Pack 2/Sprites/Idle.png");
        foreach(var s in sprites) {
            if (s is Sprite) {
                sr.sprite = s as Sprite;
                break;
            }
        }
        
        // Cyan tint for Heaven realm
        sr.color = new Color(0.8f, 1f, 1f, 1f); 
        
        Animator anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = elaraController;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 1.2f);
        col.offset = new Vector2(0, 0.6f);

        go.AddComponent<HeroKnight>();
        go.tag = "Player";
        go.layer = LayerMask.NameToLayer("Player");

        string prefabPath = "Assets/Medieval King Pack 2/Elara.prefab";
        GameObject elaraPrefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go);
        AssetDatabase.SaveAssets();

        // 5. Wire into Scene
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "GameScene") {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        }

        CharacterManager cm = Object.FindAnyObjectByType<CharacterManager>();
        if (cm != null && elaraPrefab != null) {
            SerializedObject so = new SerializedObject(cm);
            SerializedProperty propA = so.FindProperty("characterA");
            HeroKnight kael = propA.objectReferenceValue as HeroKnight;
            
            Vector3 spawnPos = kael != null ? kael.transform.position + new Vector3(2f, 0, 0) : Vector3.zero;

            GameObject elaraInst = (GameObject)PrefabUtility.InstantiatePrefab(elaraPrefab);
            elaraInst.transform.position = spawnPos;
            HeroKnight elaraScript = elaraInst.GetComponent<HeroKnight>();

            SerializedProperty propB = so.FindProperty("characterB");
            propB.objectReferenceValue = elaraScript;
            
            SerializedProperty propPlaceholder = so.FindProperty("elaraPlaceholder");
            if (propPlaceholder.objectReferenceValue != null) {
                GameObject ph = propPlaceholder.objectReferenceValue as GameObject;
                if (ph != null) GameObject.DestroyImmediate(ph);
                propPlaceholder.objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();

            elaraInst.SetActive(false); // Kael starts

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("Elara successfully wired to CharacterManager in GameScene!");
        }
    }

    private static void ReplaceClipsInStateMachine(AnimatorStateMachine sm, Dictionary<string, AnimationClip> clipMap)
    {
        foreach (var state in sm.states)
        {
            if (state.state.motion is AnimationClip clip)
            {
                if (clipMap.TryGetValue(clip.name, out AnimationClip newClip))
                {
                    state.state.motion = newClip;
                }
            }
        }
        foreach (var childSm in sm.stateMachines)
        {
            ReplaceClipsInStateMachine(childSm.stateMachine, clipMap);
        }
    }
}
