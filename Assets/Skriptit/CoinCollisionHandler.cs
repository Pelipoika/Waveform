using UnityEngine;
using UnityEngine.UI;

public class CoinCollisionHandler : MonoBehaviour
{
	public Text CoinCountText;

	private int m_collectedCoins;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.transform.CompareTag("Coin"))
		{
			Destroy(collision.gameObject);

			Debug.Log("Kolikko napattu");

			m_collectedCoins++;
		}
	}

	private void Update()
	{
		if (CoinCountText == null)
			return;

		CoinCountText.text = m_collectedCoins.ToString();
	}
}