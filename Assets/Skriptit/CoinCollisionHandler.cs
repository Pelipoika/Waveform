using UnityEngine;

public class CoinCollisionHandler : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.transform.CompareTag("Coin"))
		{
			Destroy(collision.gameObject);

			Debug.Log("Kolikko napattu");
		}
	}
}