using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[InitializeOnLoad]
public class SetupCharacterSelectionUI 
{
    static SetupCharacterSelectionUI()
    {
        EditorApplication.delayCall += () => {
            if (!EditorPrefs.GetBool("CharacterSelectionUIBuilt", false)) {
                BuildUI();
                EditorPrefs.SetBool("CharacterSelectionUIBuilt", true);
            }
        };
    }

    [MenuItem("Tools/Build Character Selection UI")]
    public static void BuildUI() 
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "GameScene") {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        }

        // 1. Assign Prefabs to CharacterManager
        CharacterManager cm = Object.FindAnyObjectByType<CharacterManager>();
        if (cm != null) {
            SerializedObject so = new SerializedObject(cm);
            
            GameObject kaelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HeroKnight.prefab");
            if (kaelPrefab == null) kaelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hero Knight - Pixel Art/Demo/HeroKnight.prefab");
            
            GameObject elaraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Medieval King Pack 2/Elara.prefab");

            if (kaelPrefab != null) so.FindProperty("kaelPrefab").objectReferenceValue = kaelPrefab;
            if (elaraPrefab != null) so.FindProperty("elaraPrefab").objectReferenceValue = elaraPrefab;
            
            so.ApplyModifiedProperties();
        }

        // 2. Delete existing instances from the scene
        GameObject existingKael = GameObject.Find("HeroKnight");
        if (existingKael != null) GameObject.DestroyImmediate(existingKael);

        GameObject existingElara = GameObject.Find("Elara");
        if (existingElara != null) GameObject.DestroyImmediate(existingElara);

        // 3. Find Canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        // 4. Create UI Panel
        GameObject panelObj = new GameObject("CharacterSelectionPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // 5. Create Title Text (using standard text if TMP is annoying to script, or just basic UI text)
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Choose Your Character";
        titleText.fontSize = 40;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 150);
        titleRect.sizeDelta = new Vector2(500, 100);

        // 6. Create Kael Button
        GameObject kaelBtnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        kaelBtnObj.name = "Btn_ChooseKael";
        kaelBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform kRect = kaelBtnObj.GetComponent<RectTransform>();
        kRect.anchoredPosition = new Vector2(-150, 0);
        kRect.sizeDelta = new Vector2(200, 60);
        kaelBtnObj.GetComponentInChildren<Text>().text = "Play as Kael (Hell)";
        
        // 7. Create Elara Button
        GameObject elaraBtnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        elaraBtnObj.name = "Btn_ChooseElara";
        elaraBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform eRect = elaraBtnObj.GetComponent<RectTransform>();
        eRect.anchoredPosition = new Vector2(150, 0);
        eRect.sizeDelta = new Vector2(200, 60);
        elaraBtnObj.GetComponentInChildren<Text>().text = "Play as Elara (Heaven)";

        // 8. Hook up buttons to a dedicated UI script since adding dynamic listeners with scene objects 
        // in editor scripts doesn't always serialize well. We will attach a simple runtime script.
        CharacterSelectionUI runtimeScript = panelObj.AddComponent<CharacterSelectionUI>();
        runtimeScript.manager = cm;
        runtimeScript.btnKael = kaelBtnObj.GetComponent<Button>();
        runtimeScript.btnElara = elaraBtnObj.GetComponent<Button>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Character Selection UI generated successfully!");
    }
}
