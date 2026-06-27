using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Central manager for handling Realm-specific environments.
/// You can assign your specific Tilemaps, Backgrounds, and Directional Light here.
/// </summary>
public class RealmEnvironmentManager : MonoBehaviour
{
    [Header("Hell Environment")]
    [Tooltip("Assign the Hell-Tilemap GameObject here.")]
    public GameObject hellTilemap;
    
    [Tooltip("Assign the Background Hell GameObject here.")]
    public GameObject hellBackground;
    
    [Header("Heaven Environment")]
    [Tooltip("Assign the Heven-Tilemap GameObject here.")]
    public GameObject heavenTilemap;
    
    [Tooltip("Assign the Background Heven GameObject here.")]
    public GameObject heavenBackground;

    [Header("Lighting")]
    [Tooltip("Assign the AmbientDarkness-Directional Light GameObject here.")]
    public GameObject directionalLight;
    
    public float hellLightIntensity = 0.5f;
    public float heavenLightIntensity = 1.0f;
    
    [Header("Settings")]
    [Tooltip("Alpha value when a realm is INACTIVE. 0 = invisible, 0.3 = faint silhouette.")]
    [Range(0f, 1f)]
    public float inactiveAlpha = 0.3f;

    private void OnEnable()
    {
        CharacterManager.OnRealmSwapped += HandleRealmSwapped;
    }

    private void OnDisable()
    {
        CharacterManager.OnRealmSwapped -= HandleRealmSwapped;
    }

    private void HandleRealmSwapped(bool isKaelActive)
    {
        // Kael active = Hell active
        UpdateEnvironment(hellTilemap, isKaelActive);
        if (hellBackground != null) hellBackground.SetActive(isKaelActive);
        
        // Elara active = Heaven active
        UpdateEnvironment(heavenTilemap, !isKaelActive);
        if (heavenBackground != null) heavenBackground.SetActive(!isKaelActive);

        UpdateLighting(isKaelActive);
    }

    private void UpdateEnvironment(GameObject envObj, bool isActiveRealm)
    {
        if (envObj == null) return;

        // Toggle colliders if any exist (e.g., TilemapCollider2D)
        Collider2D[] colliders = envObj.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = isActiveRealm;
        }

        // Adjust Opacity (Tilemap or SpriteRenderer)
        float targetAlpha = isActiveRealm ? 1f : inactiveAlpha;
        
        Tilemap tilemap = envObj.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            Color c = tilemap.color;
            c.a = targetAlpha;
            tilemap.color = c;
        }
        else
        {
            SpriteRenderer sr = envObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = targetAlpha;
                sr.color = c;
            }
        }
    }

    private void UpdateLighting(bool isKaelActive)
    {
        if (directionalLight == null) return;
        
        float targetIntensity = isKaelActive ? hellLightIntensity : heavenLightIntensity;

        // 1. Try Standard 3D Light
        Light standardLight = directionalLight.GetComponent<Light>();
        if (standardLight != null) standardLight.intensity = targetIntensity;

        // 2. Try URP 2D Light (using reflection to guarantee compilation even if URP isn't explicitly referenced)
        Behaviour urp2DLight = directionalLight.GetComponent("UnityEngine.Rendering.Universal.Light2D") as Behaviour;
        if (urp2DLight != null)
        {
            var prop = urp2DLight.GetType().GetProperty("intensity");
            if (prop != null)
            {
                prop.SetValue(urp2DLight, targetIntensity);
            }
            else
            {
                // Fallback for some Unity versions where it's a private field instead of a property
                var field = urp2DLight.GetType().GetField("m_Intensity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(urp2DLight, targetIntensity);
            }
        }
    }
}
