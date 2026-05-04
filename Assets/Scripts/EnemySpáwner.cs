using UnityEngine;
using System.Collections.Generic;

public class EnemySpáwner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxEnemies = 10;
    public float spawnRange = 20f;
    public float spawnInterval = 3f;

    private float timer;
    private List<GameObject> activeEnemies = new List<GameObject>();


    void Start()
    {
        
    }


    void Update()
    {
        timer += Time.deltaTime;

        activeEnemies.RemoveAll(item => item == null);

        if (timer >= spawnInterval && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            timer = 0;
        }
    }

    public void SpawnEnemy()
    {
        
    }


}
