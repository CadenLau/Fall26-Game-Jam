using UnityEngine;

public class PoolScript : MonoBehaviour
{
    [SerializeField] private BubbleScript bubblePrefab;
    [SerializeField] private SplashScript splashPrefab;
    [SerializeField] private AudioSource splashSoundSource;
    [SerializeField] private IngredientScript[] ingredientPrefabs = System.Array.Empty<IngredientScript>();
    [SerializeField] private IngredientScript goldIngredientPrefab;
    [SerializeField] private IngredientScript darkIngredientPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private SpriteRenderer poolRenderer;
    [SerializeField] private Color wrongIngredientColor = Color.red;
    [SerializeField] private Vector2 spawnRange = new Vector2(-3f, 3f);
    [SerializeField] private float spawnHeight = 0.75f;
    [SerializeField] private float delay = 1f;
    [SerializeField] private float repeatRate = 1.5f;
    [SerializeField, Range(0f, 1f)] private float goldChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float darkChance = 0.15f;
    [SerializeField, Min(1)] private int requiredIngredientEvery = 5;

    private UIControllerScript controller;
    private Transform spawnedObjects;
    private float spawnTimer;
    private int spawnsSinceRequired;
    private Color originalColor;
    private Color flashStartColor;
    private float flashTimeRemaining;

    public IngredientScript[] IngredientPrefabs => ingredientPrefabs;

    private void Awake()
    {
        controller = canvas.GetComponent<UIControllerScript>();
        spawnedObjects = new GameObject("Round Objects").transform;
        originalColor = poolRenderer.color;
    }

    private void Update()
    {
        if (flashTimeRemaining > 0f)
        {
            flashTimeRemaining = Mathf.Max(0f, flashTimeRemaining - Time.deltaTime);
            float progress = 1f - flashTimeRemaining;
            poolRenderer.color = progress < 0.5f
                ? Color.Lerp(flashStartColor, wrongIngredientColor,
                    Mathf.SmoothStep(0f, 1f, progress * 2f))
                : Color.Lerp(wrongIngredientColor, originalColor,
                    Mathf.SmoothStep(0f, 1f, (progress - 0.5f) * 2f));
        }

        if (!controller.IsPlaying)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnBubbleWithIngredient();
            spawnTimer = repeatRate;
        }
    }

    public void FlashWrongIngredient()
    {
        flashStartColor = poolRenderer.color;
        flashTimeRemaining = 1f;
    }

    public void BeginRound()
    {
        ClearSpawnedObjects();
        spawnTimer = delay;
        spawnsSinceRequired = 0;
    }

    public void ClearSpawnedObjects()
    {
        foreach (Transform child in spawnedObjects)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void SpawnBubbleWithIngredient()
    {
        IngredientScript prefab;
        spawnsSinceRequired++;
        float roll = Random.value;

        if (spawnsSinceRequired >= requiredIngredientEvery)
            prefab = controller.RequiredIngredient;
        else if (roll < goldChance)
            prefab = goldIngredientPrefab;
        else if (roll < goldChance + darkChance)
            prefab = darkIngredientPrefab;
        else
            prefab = ingredientPrefabs[Random.Range(0, ingredientPrefabs.Length)];

        if (prefab.Type == controller.RequiredIngredient.Type || prefab.Type == IngredientType.Candy)
            spawnsSinceRequired = 0;

        Vector3 position = transform.position + new Vector3(
            Random.Range(spawnRange.x, spawnRange.y), spawnHeight, 0f);
        BubbleScript bubble = Instantiate(bubblePrefab, position, Quaternion.identity, spawnedObjects);
        IngredientScript ingredient = Instantiate(prefab, position, Quaternion.identity, spawnedObjects);
        bubble.Initialize(ingredient);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!controller.IsPlaying || !collision.gameObject.activeInHierarchy
            || !collision.CompareTag("Ingredient") || !collision.enabled)
            return;

        Vector3 position = new Vector3(collision.bounds.center.x,
            GetComponent<Collider2D>().bounds.max.y, transform.position.z);
        collision.enabled = false;
        IngredientType type = collision.GetComponent<IngredientScript>().Type;
        SplashScript splash = Instantiate(splashPrefab, position, Quaternion.identity);
        splash.Initialize(controller.IsCorrectIngredient(type) ? originalColor : wrongIngredientColor,
            poolRenderer);
        splashSoundSource.PlayOneShot(splashSoundSource.clip);
        Destroy(collision.gameObject);
        controller.CheckIngredient(type);
    }
}
