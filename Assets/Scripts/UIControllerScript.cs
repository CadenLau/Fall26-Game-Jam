using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIControllerScript : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private PoolScript pool;
    [SerializeField] private TimerScript timerScript;

    [Header("Ingredient UI")]
    [SerializeField] private Image[] ingredientsUI = System.Array.Empty<Image>();
    [SerializeField] private Sprite hiddenIngredientSprite;
    [SerializeField] private Sprite correctIngredientSprite;
    [SerializeField] private Image badIngredientUI;

    [Header("Round and Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI roundText;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource victorySoundSource;
    [SerializeField] private AudioSource lossSoundSource;
    [SerializeField, Range(0f, 1f)] private float musicDuckMultiplier = 0.2f;

    [Header("Recipe Visibility")]
    [SerializeField] private float recipeVisibleStart = 6f;
    [SerializeField] private float recipeVisibleMinimum = 2f;

    [Header("Recipe Peek")]
    [SerializeField] private float recipePeekDuration = 1f;
    [SerializeField] private float recipePeekTimeCost = 2f;

    [Header("Round Timer")]
    [SerializeField] private float timerStartDuration = 20f;
    [SerializeField] private float timerHardestDuration = 10f;
    [SerializeField] private float correctIngredientTimeBonus = 1f;

    [Header("Difficulty Progression")]
    [SerializeField, Min(1)] private int difficultyRampRounds = 12;

    [Header("Ingredient Penalties")]
    [SerializeField, Range(0f, 1f)] private float wrongIngredientPenalty = 0.25f;
    [SerializeField, Range(0f, 1f)] private float badIngredientPenalty = 0.5f;
    [SerializeField, Range(0f, 1f)] private float darkIngredientPenalty = 0.5f;
    [SerializeField] private float minPenalty = 1f;

    private IngredientScript[] currentIngredients;
    private IngredientScript currentBadIngredient;
    private int currentIngredientIndex;
    private int recipeLength;
    private int round = 1;
    private float recipeVisibleSeconds;
    private float recipeElapsed;
    private float roundDuration;
    private float normalMusicVolume;
    private float musicDuckTimeRemaining;

    public bool IsPlaying { get; private set; }
    public IngredientScript RequiredIngredient => currentIngredients[currentIngredientIndex];

    private void Start()
    {
        normalMusicVolume = musicSource.volume;
        gameOverPanel.SetActive(false);
        badIngredientUI.transform.parent.gameObject.SetActive(true);
        StartRound();
    }

    private void Update()
    {
        if (musicDuckTimeRemaining > 0f)
        {
            musicDuckTimeRemaining -= Time.deltaTime;
            if (musicDuckTimeRemaining <= 0f)
                musicSource.volume = normalMusicVolume;
        }

        if (!IsPlaying)
            return;

        recipeElapsed += Time.deltaTime;
        UpdateRecipeOpacity();
    }

    private void StartRound()
    {
        // Rounds 2, 5, 8, and 11 add an ingredient instead of reducing durations.
        int ingredientIncreases = Mathf.Min(4, (round + 1) / 3);
        recipeLength = 3 + ingredientIncreases;
        int difficultySteps = round - 1 - ingredientIncreases;
        float progress = Mathf.Clamp01(difficultySteps / (float)difficultyRampRounds);

        recipeVisibleSeconds = Mathf.Lerp(recipeVisibleStart, recipeVisibleMinimum, progress);
        roundDuration = Mathf.Lerp(timerStartDuration, timerHardestDuration, progress);
        recipeElapsed = 0f;
        currentIngredientIndex = 0;
        currentIngredients = new IngredientScript[recipeLength];
        IngredientScript[] badIngredients = round == 1 ? pool.IngredientPrefabs
            : System.Array.FindAll(pool.IngredientPrefabs,
                ingredient => ingredient.Type != currentBadIngredient.Type);
        currentBadIngredient = badIngredients[Random.Range(0, badIngredients.Length)];
        badIngredientUI.sprite = currentBadIngredient.Icon;

        IngredientScript[] recipeIngredients = System.Array.FindAll(
            pool.IngredientPrefabs, ingredient => ingredient.Type != currentBadIngredient.Type);

        for (int i = 0; i < currentIngredients.Length; i++)
        {
            currentIngredients[i] = recipeIngredients[Random.Range(0, recipeIngredients.Length)];
        }

        RenderRecipe();
        IsPlaying = true;
        pool.BeginRound();
        timerScript.StartTimer(roundDuration);
    }

    private void RenderRecipe()
    {
        for (int i = 0; i < ingredientsUI.Length; i++)
        {
            ingredientsUI[i].transform.parent.gameObject.SetActive(true);
            ingredientsUI[i].gameObject.SetActive(i >= ingredientsUI.Length - recipeLength);
        }

        UpdateRecipeOpacity();
    }

    private void UpdateRecipeOpacity()
    {
        float fade = Mathf.Clamp01((recipeElapsed - recipeVisibleSeconds) / 0.4f);
        bool hidden = fade >= 0.5f;
        float visibility = hidden ? (fade - 0.5f) * 2f : 1f - fade * 2f;

        for (int i = 0; i < currentIngredients.Length; i++)
        {
            Image ingredientUI = ingredientsUI[ingredientsUI.Length - recipeLength + i];
            bool completed = i < currentIngredientIndex;
            ingredientUI.sprite = completed ? correctIngredientSprite
                : hidden ? hiddenIngredientSprite : currentIngredients[i].Icon;
            Color color = ingredientUI.color;
            color.a = completed ? 1f : visibility * (i == currentIngredientIndex ? 1f : 0.6f);
            ingredientUI.color = color;
        }
    }

    public void PeekRecipe()
    {
        if (!IsPlaying || recipeElapsed < recipeVisibleSeconds + 0.4f)
            return;

        timerScript.RemoveTime(recipePeekTimeCost);
        if (!IsPlaying)
            return;

        recipeElapsed = recipeVisibleSeconds - recipePeekDuration;
        UpdateRecipeOpacity();
    }

    public bool IsCorrectIngredient(IngredientType type)
    {
        return type == IngredientType.Candy || type == RequiredIngredient.Type;
    }

    public void CheckIngredient(IngredientType type)
    {
        if (!IsPlaying)
            return;

        if (type == IngredientType.Skull)
        {
            ApplyPenalty(darkIngredientPenalty);
        }
        else if (type == currentBadIngredient.Type)
        {
            ApplyPenalty(badIngredientPenalty);
        }
        else if (IsCorrectIngredient(type))
        {
            timerScript.AddTime(correctIngredientTimeBonus);
            currentIngredientIndex++;

            if (currentIngredientIndex == currentIngredients.Length)
            {
                PlayRoundSound(victorySoundSource);
                round++;
                roundText.text = "ROUND " + round;
                StartRound();
            }
            else
            {
                RenderRecipe();
            }
        }
        else
        {
            ApplyPenalty(wrongIngredientPenalty);
        }
    }

    private void PlayRoundSound(AudioSource soundSource)
    {
        musicSource.volume = normalMusicVolume * musicDuckMultiplier;
        soundSource.Play();
        musicDuckTimeRemaining = 2f;
    }

    private void ApplyPenalty(float percent)
    {
        pool.FlashWrongIngredient();
        timerScript.RemoveTime(Mathf.Max(minPenalty, timerScript.TimeRemaining * percent));
    }

    public void OnTimerEnd()
    {
        IsPlaying = false;
        victorySoundSource.Stop();
        // PlayRoundSound(lossSoundSource);
        timerScript.StopTimer();
        pool.ClearSpawnedObjects();
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
