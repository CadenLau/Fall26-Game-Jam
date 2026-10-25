using UnityEngine;

public class SplashScript : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(Color color, SpriteRenderer poolRenderer)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        spriteRenderer.sortingLayerID = poolRenderer.sortingLayerID;
        spriteRenderer.sortingOrder = poolRenderer.sortingOrder + 1;
    }

    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            Destroy(gameObject);
    }
}
