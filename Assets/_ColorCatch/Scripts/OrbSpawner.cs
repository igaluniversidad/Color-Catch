using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class OrbSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Orb orbPrefab;
    [SerializeField] private float spawnYPosition = 6f;

    [Header("Dificultad")]
    [SerializeField] private float minOrbSpeed = 4.5f;
    [SerializeField] private float maxOrbSpeed = 11f;
    [SerializeField] private float minSpawnInterval = 0.55f;
    [SerializeField] private float maxSpawnInterval = 1.4f;
    [SerializeField] private int scoreForMaxDifficulty = 60;

    private IObjectPool<Orb> orbPool;
    private readonly List<Orb> activeOrbs = new List<Orb>();
    private float timer = 0f;
    private bool isSpawning = false;
    private float currentSpeed;
    private float currentInterval;

    void Awake()
    {
        orbPool = new ObjectPool<Orb>(
            createFunc: CreateOrb,
            actionOnGet: OnGetOrb,
            actionOnRelease: OnReleaseOrb,
            actionOnDestroy: OnDestroyOrb,
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 20
        );
    }

    private Orb CreateOrb()
    {
        Orb orb = Instantiate(orbPrefab);
        orb.SetPool(orbPool);
        return orb;
    }

    private void OnGetOrb(Orb orb)
    {
        if (orb == null) return;
        orb.gameObject.SetActive(true);
        if (!activeOrbs.Contains(orb))
        {
            activeOrbs.Add(orb);
        }
    }

    private void OnReleaseOrb(Orb orb)
    {
        if (orb == null) return;
        orb.gameObject.SetActive(false);
        activeOrbs.Remove(orb);
    }

    private void OnDestroyOrb(Orb orb)
    {
        if (orb != null && orb.gameObject != null)
        {
            Destroy(orb.gameObject);
        }
    }

    void OnEnable() => GameManager.OnScoreUpdated += UpdateDifficulty;
    void OnDisable() => GameManager.OnScoreUpdated -= UpdateDifficulty;

    void Start() => UpdateDifficulty(0);

    void Update()
    {
        if (!isSpawning) return;

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            SpawnOrb();
            timer = 0f;
        }
    }

    private void SpawnOrb()
    {
        Orb orb = orbPool.Get();
        if (orb != null)
        {
            orb.transform.position = new Vector3(0f, spawnYPosition, 0f);
            orb.SetColor((GameColor)Random.Range(0, 4));
            orb.SetSpeed(currentSpeed);
        }
    }

    public void ClearAllActiveOrbs()
    {
        timer = 0f;

        // Devolvemos todos los orbes activos al Pool de manera segura sin destruir sus instancias
        for (int i = activeOrbs.Count - 1; i >= 0; i--)
        {
            if (activeOrbs[i] != null)
            {
                activeOrbs[i].ReturnToPool();
            }
        }

        activeOrbs.Clear();
    }

    private void UpdateDifficulty(int score)
    {
        float t = Mathf.Clamp01((float)score / scoreForMaxDifficulty);
        float curveProgress = Mathf.Sin(t * Mathf.PI * 0.5f);

        currentSpeed = Mathf.Lerp(minOrbSpeed, maxOrbSpeed, curveProgress);
        currentInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, curveProgress);
    }

    public void SetSpawning(bool active)
    {
        isSpawning = active;
        if (!active)
        {
            timer = 0f;
        }
    }
}