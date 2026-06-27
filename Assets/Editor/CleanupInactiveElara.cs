using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class CleanupInactiveElara 
{
    static CleanupInactiveElara()
    {
        EditorApplication.delayCall += () => {
            if (!EditorPrefs.GetBool("InactiveElaraCleaned", false)) {
                Scene activeScene = EditorSceneManager.GetActiveScene();
                var roots = activeScene.GetRootGameObjects();
                foreach(var go in roots) {
                    if (go.name == "Elara" || go.name == "Elara Place Holder" || go.name == "HeroKnight") {
                        GameObject.DestroyImmediate(go);
                    }
                }
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveOpenScenes();
                EditorPrefs.SetBool("InactiveElaraCleaned", true);
                Debug.Log("Inactive characters successfully destroyed from scene hierarchy!");
            }
        };
    }
}
