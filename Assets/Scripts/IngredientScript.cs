using UnityEngine;

public enum IngredientType
{
    Mushroom,
    Eye,
    Frog,
    Tentacle,
    Apple,
    Candy,
    Skull
}

public class IngredientScript : MonoBehaviour
{
    [SerializeField] private IngredientType type;
    [SerializeField] private float despawnY = -7f;

    private Collider2D ingredientCollider;
    private Rigidbody2D rb;

    public IngredientType Type => type;
    public Sprite Icon => GetComponent<SpriteRenderer>().sprite;

    private void Awake()
    {
        ingredientCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        ingredientCollider.enabled = false;
        rb.simulated = false;
    }

    private void Update()
    {
        if (transform.position.y < despawnY)
            Destroy(gameObject);
    }

    public void OnPop()
    {
        rb.simulated = true;
        ingredientCollider.enabled = true;
    }
}
