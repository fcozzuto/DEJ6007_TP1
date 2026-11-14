using System.Collections.Generic;
using UnityEngine;

public class BossFightActivator : MonoBehaviour
{
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Animator exitGateAnimator;
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject[] waves = new GameObject[0];
    [SerializeField] private Light exitLight;
    [SerializeField] private GameObject endGameCameraTrigger;

    private List<GameObject> spawnedWaves = new List<GameObject>();
    private EnemyController[] currentWaveEnemies;
    private int currentWaveIndex = -1;
    private bool isLit;
    private bool fightComplete;
    private TriggerCameraBehindPlayer endCameraTrigger;

    public int RemainingEnemies { get; private set; }

    // Start with the brazier unlit, the exit camera trigger locked, and all wave templates hidden.
    private void Awake()
    {
        if (fireEffect != null)
            fireEffect.SetActive(false);

        if (endGameCameraTrigger != null)
            endCameraTrigger = endGameCameraTrigger.GetComponent<TriggerCameraBehindPlayer>();

        if (endCameraTrigger != null)
            endCameraTrigger.SetUnlocked(false);

        // These groups are templates; only their spawned copies should be active.
        foreach (GameObject wave in waves)
        {
            if (wave != null)
                wave.SetActive(false);
        }
    }

    // Light the brazier, close the boss-room entrance, and start the first wave.
    public void Ignite()
    {
        if (isLit)
            return;

        if (fireEffect == null || gateAnimator == null ||
            exitGateAnimator == null || player == null || waves.Length == 0 ||
            exitLight == null || endCameraTrigger == null)
        {
            Debug.LogError("Assign the fire, gates, player, waves, exit light, and end-game camera trigger.", this);
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

    // Count the survivors and advance the fight, or reset the arena if the player dies.
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
            // Count deaths immediately instead of waiting for the ragdoll objects to be destroyed.
            if (enemy != null && !enemy.IsDead)
                RemainingEnemies++;
        }

        if (RemainingEnemies == 0)
            StartNextWave();
    }

    // Spawn the next wave, or open the gates and unlock the ending when every wave is cleared.
    private void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            fightComplete = true;
            gateAnimator.Play("Base Layer.Gate motion", 0, 0f);
            exitGateAnimator.Play("Base Layer.Exit Gate motion", 0, 0f);
            exitLight.gameObject.SetActive(true);
            endCameraTrigger.SetUnlocked(true);
            Debug.Log("All boss waves defeated.", this);
            return;
        }

        GameObject template = waves[currentWaveIndex];
        // Fresh copies let the fight restart after enemies from the previous attempt are destroyed.
        GameObject wave = Instantiate(template, template.transform.parent);
        spawnedWaves.Add(wave);
        wave.SetActive(true);

        currentWaveEnemies = wave.GetComponentsInChildren<EnemyController>();
        RemainingEnemies = currentWaveEnemies.Length;

        Debug.Log(
            $"Wave {currentWaveIndex + 1} started with {RemainingEnemies} enemies.",
            this);
    }

    // Remove the spawned waves and reset the brazier, gate, light, and camera trigger for another attempt.
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
        exitLight.gameObject.SetActive(false);
        endCameraTrigger.SetUnlocked(false);
    }
}