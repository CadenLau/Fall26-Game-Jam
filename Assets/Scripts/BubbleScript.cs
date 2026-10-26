using UnityEngine;
[RequireComponent(typeof(Animator))]

[RequireComponent(typeof(Collider2D))]
public class BubbleScript : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float despawnY = 7f;
    [SerializeField] private float wiggleAmountBase = 0.4f;
    [SerializeField] private float wiggleAmountDeviation = 0.05f;
    [SerializeField] private float wiggleFrequencyBase = 0.3f;
    [SerializeField] private float wiggleFrequencyDeviation = 0.05f;

    private float wiggleAmount;
    private float wiggleFrequency;
    private float startX;
    private float lifetime;
    private IngredientScript ingredient;
    private Animator animator;
    private bool popped;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        startX = transform.position.x;

        wiggleAmount = wiggleAmountBase + Random.Range(-wiggleAmountDeviation, wiggleAmountDeviation);
        wiggleFrequency = wiggleFrequencyBase + Random.Range(-wiggleFrequencyDeviation, wiggleFrequencyDeviation);
    }

    public void Initialize(IngredientScript contents)
    {
        ingredient = contents;
        ingredient.transform.SetParent(transform, false);
        ingredient.transform.localPosition = Vector3.zero;

        string state = ingredient.Type == IngredientType.Candy ? "Candy"
            : ingredient.Type == IngredientType.Skull ? "Skull" : "Normal";
        animator.Play(state, 0, 0f);
        animator.Update(0f);
    }

    private void Update()
    {
        if (popped)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                Destroy(gameObject);
            return;
        }

        // transform.position += Vector3.up * (floatSpeed * Time.deltaTime);
        lifetime += Time.deltaTime;

        Vector3 position = transform.position;
        position.x = startX + Mathf.Sin(lifetime * wiggleFrequency * Mathf.PI * 2f) * wiggleAmount;
        position.y += floatSpeed * Time.deltaTime;

        transform.position = position;

        if (transform.position.y > despawnY)
            Destroy(gameObject);
    }

    public void Pop()
    {
        if (popped)
            return;

        popped = true;
        GetComponent<Collider2D>().enabled = false;
        ingredient.transform.SetParent(transform.parent, true);
        ingredient.OnPop();
        // Let the visual finish even if the round's remaining objects are cleared.
        transform.SetParent(null, true);
        animator.speed = 1f;
    }
}
