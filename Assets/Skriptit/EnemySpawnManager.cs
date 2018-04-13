using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
	public GameObject EnemyGameObject;

	public GameObject[] Spawns;

	public int MinEnemiesPerBlock;
	public int MaxEnemiesPerBlock;

	private void Start()
	{
		Spawns = GameObject.FindGameObjectsWithTag("PotentialSpawn");

		foreach (GameObject spawn in Spawns)
		{
			BoxCollider2D spawnCollider = spawn.GetComponent<BoxCollider2D>();
			if (spawnCollider == null)
				continue;

			Vector2 origin = spawn.transform.position;
			origin.y += spawnCollider.size.y / 4;

			int count = Random.Range(MinEnemiesPerBlock, MaxEnemiesPerBlock);
			for (int i = 0; i < count; i++)
			{
			//	Viholliset levittäytyvät itsekseen
			//	origin.x += Random.Range(0, (spawnCollider.size.x / 4));
				
				SpawnEnemy(new Vector3(origin.x, origin.y));
			}
		}
	}

	public GameObject SpawnEnemy(Vector3 position)
	{
		return Instantiate(EnemyGameObject, position, Quaternion.identity);
	}
}