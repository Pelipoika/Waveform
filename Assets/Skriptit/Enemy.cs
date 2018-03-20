using JetBrains.Annotations;
using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	private Rigidbody2D    m_body;
	private SpriteRenderer m_renderer;
	private Animator       m_animator;
	private BoxCollider2D  m_collider;

	private int m_playerMask;
	private int m_groundMask;

	public Transform TargetPlayer;

	public float VisionDistance;
	public float AttackDistance;
	public int   DamagePerSwing;
	public float Acceleration;
	public bool  Fleeing;


	//Kuinka kaukaa chekata kielekkeet putoamisen varalta
	public float LedgeDistance = 0.95f;

	public float FloorCheckDistanceX = 0.95f;
	public float FloorCheckDistanceY = 2.19f;



	// Use this for initialization
	private void Start()
	{
		m_body     = GetComponent<Rigidbody2D>();
		m_renderer = GetComponent<SpriteRenderer>();
		m_animator = GetComponent<Animator>();
		m_collider = GetComponent<BoxCollider2D>();

		TargetPlayer = GameObject.FindGameObjectWithTag("Player").transform;

		m_playerMask = LayerMask.GetMask("Player");
		m_groundMask = LayerMask.GetMask("Ground");
	}


	private void OnDrawGizmos()
	{
		//Piirrä lattia anturit
		var bottomRight = new Vector3(m_collider.bounds.extents.x + FloorCheckDistanceX, -m_collider.bounds.extents.y) + transform.position;
		var bottomLeft = new Vector3(-m_collider.bounds.extents.x - FloorCheckDistanceX, -m_collider.bounds.extents.y) + transform.position;

		Gizmos.DrawLine(bottomRight, new Vector3(bottomRight.x, bottomRight.y - FloorCheckDistanceY));
		Gizmos.DrawLine(bottomLeft,  new Vector3(bottomLeft.x,   bottomLeft.y - FloorCheckDistanceY));


		//Piirrä kielleke anturit
		var topRight = new Vector3(m_collider.bounds.size.x * 2,  m_collider.bounds.extents.y) + transform.position;
		var topLeft =  new Vector3(-m_collider.bounds.size.x * 2, m_collider.bounds.extents.y) + transform.position;

		Gizmos.DrawLine(topRight, new Vector3(topRight.x, topRight.y - LedgeDistance));
		Gizmos.DrawLine(topLeft,  new Vector3(topLeft.x,  topLeft.y  - LedgeDistance));
	}

	private const float DragY = 0.01f; // drag factor
	private const float DragX = 0.5f;  // drag factor

	// Update is called once per frame
	private void FixedUpdate()
	{
		//Custom drag koska rigidbodyn drag on kakka
		Vector2 vel = m_body.velocity;
		vel.y           *= 1.0f - DragY;
		vel.x           *= 1.0f - DragX;
		m_body.velocity =  vel;

		//Jos olen kuollut tai kohteeni on kuollut, lopeta
		if (m_animator.GetBool("dead") || TargetPlayer.GetComponent<HealthManager>().IsDead)
		{
			m_animator.SetBool("attack", false);
			m_animator.SetBool("run", false);

			return;
		}

		var vecAdditionVelocity = Vector2.zero;

		float flDistanceToTarget = Vector2.Distance(transform.position, TargetPlayer.position);
		if (flDistanceToTarget <= VisionDistance)
		{
			//Tarpeeks lähellä
			vecAdditionVelocity.x = TargetPlayer.position.x > transform.position.x ? Acceleration : -Acceleration;

			if (Fleeing)
				vecAdditionVelocity.x = -vecAdditionVelocity.x;

			m_renderer.flipX = vecAdditionVelocity.x < 0f;

			//Hyökkää
			if (flDistanceToTarget <= AttackDistance)
			{
				vecAdditionVelocity.x = 0f;

				m_animator.SetBool("attack", true);
			}
			else
			{
				m_animator.SetBool("attack", false);
			}

			//	m_animator.SetBool("attack", flDistanceToTarget <= AttackDistance);
		}

		//Jos liikutaan niin nuuskitaan.
		if (vecAdditionVelocity != Vector2.zero)
		{
			Vector2 testOrigin = new Vector2();

			//Testaan voidaanko hypätä.
			if (vecAdditionVelocity.x > 0) //Vasemmalle
				testOrigin = new Vector3(m_collider.bounds.size.x * 2, m_collider.bounds.extents.y) + transform.position;
			else if (vecAdditionVelocity.x < 0) //Oikealle
				testOrigin = new Vector3(-m_collider.bounds.size.x * 2, m_collider.bounds.extents.y) +transform.position;

			RaycastHit2D hit = Physics2D.Raycast(testOrigin, Vector2.down, LedgeDistance, m_groundMask);

			//osui maahan && ei tarvi hyppiä jos hypitään.
			if (hit.collider != null && Math.Abs(m_body.velocity.y) < 0.1)
			{
				m_animator.SetTrigger("jump");
				m_body.AddForce(new Vector2(0, 40f), ForceMode2D.Impulse);
			}
			else
			{
				//Hypitään tai tiputaan niin ei tarkisteta tiputaanko.
				if (Math.Abs(m_body.velocity.y) < 0.1)
				{
					//Testataan tippuisiko vihollinen kielekkeeltä.
					if (vecAdditionVelocity.x > 0)
						testOrigin = new Vector3(m_collider.bounds.extents.x + FloorCheckDistanceX, -m_collider.bounds.extents.y) + transform.position;
					else if (vecAdditionVelocity.x < 0)
						testOrigin = new Vector3(-m_collider.bounds.extents.x - FloorCheckDistanceX, -m_collider.bounds.extents.y) + transform.position;

					hit = Physics2D.Raycast(testOrigin, Vector2.down, FloorCheckDistanceY, m_groundMask);
					if (hit.collider == null)
					{
						//Jos menisimme eteenpäin, tippuisimme johonkin.
						vecAdditionVelocity.x = 0f;
					}
				}
			}
		}

		m_animator.SetBool("run", Math.Abs(vecAdditionVelocity.x) > 0.0);
		m_body.AddForce(vecAdditionVelocity);

		//Tippui ulos levelistä
		if (transform.position.y < -5f)
		{
			Destroy(gameObject);
		}
	}

	[UsedImplicitly]
	private void OnAttack()
	{
		Vector2 rayOrigin = transform.position;

		rayOrigin.y += 0.25f;
		//	rayOrigin.x += m_renderer.flipX ? -0.5f : 0.5f;

		Vector2 dirVector2 = m_renderer.flipX ? Vector2.left : Vector2.right;

		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dirVector2, AttackDistance, m_playerMask);
		if (hit.collider == null) return;

		HealthManager healthManager = hit.collider.GetComponent<HealthManager>();
		if (healthManager == null) return;
		healthManager.TakeDamage(gameObject, DamagePerSwing, hit.point);

		PlayerController controller = hit.collider.GetComponent<PlayerController>();
		if (controller == null) return;
		controller.Damaged(dirVector2);
	}
}