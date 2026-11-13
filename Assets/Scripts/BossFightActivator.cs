using System.Collections.Generic;
using UnityEngine;

public class BossFightActivator : MonoBehaviour
{
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Animator exitGateAnimator;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject[] waves = new GameObject[0];

    private List<GameObject> spawnedWaves = new List<GameObject>();
    private EnemyController[] currentWaveEnemies;
    private int currentWaveIndex = -1;
    private bool isLit;
    private bool fightComplete;

    public int RemainingEnemies { get; private set; }

    private void Awake()
    {
        if (fireEffect != null)
            fireEffect.SetActive(false);

        foreach (GameObject wave in waves)
        {
            if (wave != null)
                wave.SetActive(false);
        }
    }

    public void Ignite()
    {
        if (isLit)
            return;

        if (fireEffect == null || gateAnimator == null ||
            exitGateAnimator == null || player == null || waves.Length == 0)
        {
            Debug.LogError("Assign the fire, gates, player, and waves.", this);
            return;
        }

        foreach (GameObject wave in waves)
        {
            if (wave == null)
            {
                Debug.LogError("Assign every entry in Waves.", this);
                return;
            }
        }

        isLit = true;
        fireEffect.SetActive(true);
        gateAnimator.Play("Base Layer.Gate closing motion", 0, 0f);
        StartNextWave();
    }

    private void Update()
    {
        if (!isLit || fightComplete)
            return;

        if (player.IsDead)
        {
            ResetFight();
            return;
        }

        RemainingEnemies = 0;

        foreach (EnemyController enemy in currentWaveEnemies)
        {
            if (enemy != null && !enemy.IsDead)
                RemainingEnemies++;
        }

        if (RemainingEnemies == 0)
            StartNextWave();
    }

    private void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            fightComplete = true;
            gateAnimator.Play("Base Layer.Gate motion", 0, 0f);
            exitGateAnimator.Play("Base Layer.Exit Gate motion", 0, 0f);
            Debug.Log("All boss waves defeated.", this);
            return;
        }

        GameObject template = waves[currentWaveIndex];
        GameObject wave = Instantiate(template, template.transform.parent);
        spawnedWaves.Add(wave);
        wave.SetActive(true);

        currentWaveEnemies = wave.GetComponentsInChildren<EnemyController>();
        RemainingEnemies = currentWaveEnemies.Length;

        Debug.Log(
            $"Wave {currentWaveIndex + 1} started with {RemainingEnemies} enemies.",
            this);
    }

    private void ResetFight()
    {
        isLit = false;
        fightComplete = false;
        currentWaveIndex = -1;
        RemainingEnemies = 0;
        currentWaveEnemies = null;
        fireEffect.SetActive(false);

        foreach (GameObject wave in spawnedWaves)
        {
            if (wave != null)
                Destroy(wave);
        }

        spawnedWaves.Clear();
        gateAnimator.Play("Base Layer.Gate motion", 0, 0f);
    }
}