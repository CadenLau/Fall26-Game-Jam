using UnityEngine;

public class BubbleScript : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float despawnY = 7f;

    private IngredientScript ingredient;
    private Animator animator;
    private bool popped;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
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

        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

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
