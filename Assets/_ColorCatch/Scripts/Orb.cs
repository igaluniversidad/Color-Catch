using UnityEngine;
using UnityEngine.Pool;

public class Orb : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject hitParticlePrefab;

    public GameColor ColorType { get; private set; }

    private IObjectPool<Orb> pool;
    private bool isProcessed = false;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        isProcessed = false;
    }

    public void SetPool(IObjectPool<Orb> orbPool)
    {
        pool = orbPool;
    }

    void Update()
    {
        // Solo se mueve si el juego está en estado activo de partida
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (transform.position.y < -7f)
        {
            ReturnToPool();
        }
    }

    public void SetColor(GameColor color)
    {
        ColorType = color;
        if (spriteRenderer != null)
            spriteRenderer.color = ColorPalette.GetColor(color);
    }

    public void SetSpeed(float newSpeed) => fallSpeed = newSpeed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isProcessed) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        RingController ring = other.GetComponent<RingController>() ?? other.GetComponentInParent<RingController>();

        if (ring != null)
        {
            isProcessed = true;

            if (ring.GetTopColor() == ColorType)
            {
                CameraShake.Instance?.Shake(0.08f, 0.1f);
                SpawnParticles();
                GameManager.Instance?.AddScore(1);
            }
            else
            {
                CameraShake.Instance?.Shake(0.25f, 0.35f);
                GameManager.Instance?.GameOver();
            }

            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        if (isProcessed && !gameObject.activeSelf) return;

        isProcessed = true;

        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SpawnParticles()
    {
        if (hitParticlePrefab == null) return;

        GameObject pObj = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
        if (pObj.TryGetComponent<ParticleSystem>(out var ps))
        {
            var main = ps.main;
            main.startColor = ColorPalette.GetColor(ColorType);
        }
    }
}