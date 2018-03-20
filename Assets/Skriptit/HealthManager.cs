using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
	public int MaxHealth;
	public int Health;

	public bool IsDead;

	private Animator       m_animator;
	private BoxCollider2D  m_collider;
	private Rigidbody2D    m_body;
	private SpriteRenderer m_renderer;
	private GameObject     m_worldSpaceCanvas;

	private int m_corpseLayer;

	// ReSharper disable once InconsistentNaming
	[SerializeField] private Text DamageNumero;

	// ReSharper disable once InconsistentNaming
	[SerializeField] private GameObject CoinPrefab;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
		m_collider = GetComponent<BoxCollider2D>();
		m_body     = GetComponent<Rigidbody2D>();
		m_renderer = GetComponent<SpriteRenderer>();

		m_corpseLayer = LayerMask.NameToLayer("Corpse");

		m_worldSpaceCanvas = GameObject.Find("WorldSpaceCanvas");

		Health = MaxHealth;
		IsDead = false;
	}

	public void TakeDamage(GameObject attacker, int amount, Vector3 hitPoint)
	{
		if (IsDead)
			return;

		Health -= amount;

		if (Health <= 0)
			Die(attacker);

		Vector2 dir = transform.position - attacker.transform.position;
		dir.Normalize();

		//	dir.y =  Random.Range(.1f, .2f);
		dir *= Random.Range(50f, 100f);

		var numero = Instantiate(DamageNumero, hitPoint, Quaternion.identity, m_worldSpaceCanvas.transform);
		numero.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-2, 2), Random.Range(5, 10));

		numero.text =  (amount >= 0) ? "-" : "+";
		numero.text += Mathf.Abs(amount);

		//	Vector3 screenPos = Camera.main.WorldToScreenPoint(hitPoint);

		numero.transform.position = hitPoint;

		m_body.AddForceAtPosition(dir, hitPoint, ForceMode2D.Impulse);
	}

	public void Die(GameObject attacker)
	{
		//Jep
		IsDead = true;

		//Kuolema animaatio
		m_animator.SetTrigger("dead");

		//Ei saa laaserit enää osua kuolleeseen
		m_collider.isTrigger = false;

		//Kuolleet taakse
		m_renderer.sortingOrder = 0;

		//Ei pelaaja
		if (!gameObject.CompareTag("Player"))
		{
			//Ruumiit vaihtavat kerrosta jotta niihin ei voisi osua mutta niihin vaikuttaisivat vielä voimat.
			gameObject.layer = m_corpseLayer;

			var pos = transform.position;

			var count = Random.Range(1, 8);
			for (var i = 0; i < count; i++)
			{
				var coin = Instantiate(CoinPrefab, pos, Quaternion.identity);
				coin.GetComponent<Coin>().Puller          = attacker.transform;
				coin.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-2, 2), Random.Range(5, 10));
			}
		}
	}
}