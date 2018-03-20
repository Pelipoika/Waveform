using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawnManager : MonoBehaviour
{
	public GameObject EneyGameObject;

	public GameObject[] Spawns;

	public int MinEnemiesPerBlock;
	public int MaxEnemiesPerBlock;

	void Start()
	{
		Spawns = GameObject.FindGameObjectsWithTag("PotentialSpawn");

		foreach (GameObject spawn in Spawns)
		{
			BoxCollider2D spawnCollider = spawn.GetComponent<BoxCollider2D>();
			if (spawnCollider == null) throw new ArgumentNullException();

			Vector2 origin = spawn.transform.position;
			origin.y += spawnCollider.bounds.extents.y;

			int count = Random.Range(MinEnemiesPerBlock, MaxEnemiesPerBlock);
			for (int i = 0; i < count; i++)
			{
				origin.x += Random.Range(-spawnCollider.bounds.extents.x, spawnCollider.bounds.extents.x);
				
				Instantiate(EneyGameObject, new Vector3(origin.x, origin.y), Quaternion.identity);
			}
		}
	}
}