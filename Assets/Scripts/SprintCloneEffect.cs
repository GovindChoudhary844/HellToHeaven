using UnityEngine;

/// <summary>
/// SprintCloneEffect — High-performance ghost trail for sprinting.
/// Uses a fixed object pool to prevent instantiation stutter.
/// </summary>
public class SprintCloneEffect : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("How often a ghost clone is dropped (in seconds).")]
    [SerializeField] private float spawnRate = 0.05f;
    
    [Tooltip("How long the clone stays on screen before disappearing.")]
    [SerializeField] private float lifeTime = 0.3f;
    
    [Tooltip("Color and base opacity of the ghost clone.")]
    [SerializeField] private Color trailColor = new Color(0.5f, 0.8f, 1f, 0.6f);
    
    [Header("Pool Settings")]
    [Tooltip("Max clones allowed on screen at once. 10 is usually plenty.")]
    [SerializeField] private int poolSize = 10;
    
    private float spawnTimer;
    private SpriteRenderer playerRenderer;
    
    // Internal struct to track pool objects
    private class CloneData
    {
        public GameObject obj;
        public SpriteRenderer sr;
        public float lifeTimer;
        public bool active;
    }
    
    private CloneData[] pool;

    private void Awake()
    {
        playerRenderer = GetComponent<SpriteRenderer>();
        pool = new CloneData[poolSize];
        
        // Initialize the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"GhostClone_{gameObject.name}_{i}");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            
            // Match the player's material and rendering properties
            sr.material = playerRenderer.material;
            sr.sortingLayerID = playerRenderer.sortingLayerID;
            sr.sortingOrder = playerRenderer.sortingOrder - 1; // Render behind the player
            
            go.SetActive(false);
            
            pool[i] = new CloneData { obj = go, sr = sr, lifeTimer = 0f, active = false };
        }
    }

    private void Update()
    {
        // Process fading for active clones
        for (int i = 0; i < poolSize; i++)
        {
            if (pool[i].active)
            {
                pool[i].lifeTimer -= Time.deltaTime;
                
                // Calculate fade alpha
                float alpha = Mathf.Clamp01(pool[i].lifeTimer / lifeTime);
                Color c = trailColor;
                c.a = trailColor.a * alpha;
                pool[i].sr.color = c;
                
                // Return to pool when fully faded
                if (pool[i].lifeTimer <= 0f)
                {
                    pool[i].active = false;
                    pool[i].obj.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Call this continuously while the player is sprinting.
    /// It manages its own spawn rate timer.
    /// </summary>
    public void RequestClone()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnClone();
            spawnTimer = spawnRate;
        }
    }
    
    /// <summary>
    /// Reset the timer so the first clone drops instantly upon starting a sprint.
    /// </summary>
    public void ResetTimer()
    {
        spawnTimer = 0f;
    }

    private void SpawnClone()
    {
        // Find the first available inactive clone in the pool
        for (int i = 0; i < poolSize; i++)
        {
            if (!pool[i].active)
            {
                CloneData clone = pool[i];
                
                // Snap to current position and scale
                clone.obj.transform.position = transform.position;
                clone.obj.transform.rotation = transform.rotation;
                clone.obj.transform.localScale = transform.localScale;
                
                // Copy current sprite frame and flip state
                clone.sr.sprite = playerRenderer.sprite;
                clone.sr.flipX = playerRenderer.flipX;
                clone.sr.flipY = playerRenderer.flipY;
                clone.sr.color = trailColor;
                
                // Activate
                clone.lifeTimer = lifeTime;
                clone.active = true;
                clone.obj.SetActive(true);
                return; 
            }
        }
    }
}
