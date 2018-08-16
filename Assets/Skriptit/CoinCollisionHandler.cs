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
	
		if (collision.transform.CompareTag("Ladder"))
		{
			if (!PlayerGameObject)
				return;

			m_playerController.TouchingLadder = true;
			m_playerController.TouchedLadder  = collision.gameObject;
		}
		
		if (collision.transform.CompareTag("Pickup"))
		{
			if (!PlayerGameObject)
				return;

			m_playerController.TouchingItem = true;
			m_playerController.TouchedItem  = collision.gameObject;
		}
		
		if (collision.transform.CompareTag("Coin"))
		{
			Destroy(collision.gameObject);

			CollectedCoins+= 5000;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (!PlayerGameObject)
			return;

		
		if (other.transform.CompareTag("Ladder"))
		{
			if (!PlayerGameObject)
				return;

			m_playerController.TouchingLadder = false;
			m_playerController.TouchedLadder  = null;
		}
		
		if (other.transform.CompareTag("Chest"))
		{
			m_playerController.TouchingChest = false;
			m_playerController.TouchedChest  = null;
		}
	}

	private void Update()
	{
		if (CoinCountText == null)
			return;

		CoinCountText.text = CollectedCoins.ToString();
	}
}