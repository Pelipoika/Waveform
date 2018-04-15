using UnityEngine;
using UnityEngine.UI;

public class CoinCollisionHandler : MonoBehaviour
{
	public GameObject PlayerGameObject;

	public Text CoinCountText;
	public int  CollectedCoins;

	private PlayerController m_playerController;

	private void Start()
	{
		m_playerController = PlayerGameObject.GetComponent<PlayerController>();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.transform.CompareTag("Chest"))
		{
			if (!PlayerGameObject)
				return;

			m_playerController.TouchingChest = true;
			m_playerController.TouchedChest  = collision.gameObject;
		}

		if (collision.transform.CompareTag("Coin"))
		{
			Destroy(collision.gameObject);

			CollectedCoins++;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!other.transform.CompareTag("Chest"))
			return;

		if (!PlayerGameObject)
			return;

		m_playerController.TouchingChest = false;
		m_playerController.TouchedChest  = null;
	}

	private void Update()
	{
		if (CoinCountText == null)
			return;

		CoinCountText.text = CollectedCoins.ToString();
	}
}