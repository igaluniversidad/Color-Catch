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
    private bool isSpawning = false; // Inicia apagado hasta pulsar Play
    private float currentSpeed;
    private float currentInterval;

    void Awake()
    {
        orbPool = new ObjectPool<Orb>(
            createFunc: CreateOrb,
            actionOnGet: OnGetOrb,
            actionOnRelease: OnReleaseOrb,
            actionOnDestroy: orb => Destroy(orb.gameObject),
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
        orb.gameObject.SetActive(true);
        activeOrbs.Add(orb);
    }

    private void OnReleaseOrb(Orb orb)
    {
        orb.gameObject.SetActive(false);
        activeOrbs.Remove(orb);
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
        orb.transform.position = new Vector3(0f, spawnYPosition, 0f);
        orb.SetColor((GameColor)Random.Range(0, 4));
        orb.SetSpeed(currentSpeed);
    }

    public void ClearAllActiveOrbs()
    {
        // Detiene cualquier invocación o corrutina pendiente de spawn
        CancelInvoke();
        StopAllCoroutines();

        // Busca y destruye todos los orbes activos en la escena
        Orb[] activeOrbs = FindObjectsByType<Orb>(FindObjectsSortMode.None);
        for (int i = 0; i < activeOrbs.Length; i++)
        {
            if (activeOrbs[i] != null)
            {
                Destroy(activeOrbs[i].gameObject);
            }
        }
    }

    private void UpdateDifficulty(int score)
    {
        float t = Mathf.Clamp01((float)score / scoreForMaxDifficulty);
        float curveProgress = Mathf.Sin(t * Mathf.PI * 0.5f);

        currentSpeed = Mathf.Lerp(minOrbSpeed, maxOrbSpeed, curveProgress);
        currentInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, curveProgress);
    }

    public void SetSpawning(bool active) => isSpawning = active;
}