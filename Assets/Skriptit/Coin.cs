using UnityEngine;

public class Coin : MonoBehaviour
{
	public float PullStartTime;

	public float Speed;
	public float Acceleration;
	public float MaxSpeed;

	public Transform Puller;

	private Rigidbody2D m_body;

	private bool m_pulled;

	// Use this for initialization
	private void Start()
	{
		PullStartTime = Time.time + PullStartTime;

		m_body = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	private void Update()
	{
		if (PullStartTime > Time.time && !m_pulled)
			return;

		//OnPullStart
		if (PullStartTime < Time.time && !m_pulled)
			gameObject.layer = LayerMask.NameToLayer("CoinPulled");

		m_pulled            = true;
		m_body.gravityScale = 0.0f;

		Mathf.Clamp(Speed += Acceleration * Time.deltaTime, 0, MaxSpeed);

		transform.position = Vector2.MoveTowards(transform.position, Puller.transform.position, Speed * Time.deltaTime);
	}
}