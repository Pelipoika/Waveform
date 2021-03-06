﻿using System;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UI.Slider;

public class PlayerController : MonoBehaviour
{
	private Rigidbody2D      m_body;
	private SpriteRenderer   m_playerRenderer;
	private Animator         m_animator;
	private Collider2D       m_collider;
	private LineRenderer     m_laserLine;
	private CameraController m_cameraController;
	private HealthManager    m_myHealthManager;


	public GameObject UI;
	public Item ItemBaseItem;

	public EnemySpawnManager    SpawnManager;
	public CoinCollisionHandler CoinHandler;

	[Tooltip("Kamera joka renderöi tällähetkellä pelaajaa")]
	public Camera Camera;

	public Slider HealthSlider;
	public Text   EnemyCountText;
	public Image  DamageImage;
	public Image  ItemElement;

	public float FlashSpeed;
	public Color FlashColor = new Color(1f, 0f, 0f, 0.3f);

	private bool m_damaged;

	public float Acceleration;
	public float Jump;

	public  int DamagePerShot;
	private int m_shootableMask;

	private bool m_bJumping;
	private bool m_bOnLadder;

	public List<Pickup> items;
	
	[HideInInspector] public bool       TouchingChest;
	[HideInInspector] public bool       TouchingLadder;
	[HideInInspector] public bool       TouchingItem;
	[HideInInspector] public GameObject TouchedChest;
	[HideInInspector] public GameObject TouchedLadder;
	[HideInInspector] public GameObject TouchedItem;


	// Use this for initialization
	private void Start()
	{
		m_body            = GetComponent<Rigidbody2D>();
		m_playerRenderer  = GetComponent<SpriteRenderer>();
		m_animator        = GetComponent<Animator>();
		m_collider        = GetComponent<Collider2D>();
		m_laserLine       = GetComponent<LineRenderer>();
		m_myHealthManager = GetComponent<HealthManager>();

		items = new List<Pickup>();

		if (Camera == null)
			Camera = Camera.main;

		m_cameraController = Camera.GetComponent<CameraController>();

		m_laserLine.material = new Material(Shader.Find("Sprites/Default"));

		// A simple 2 color gradient with a fixed alpha of 1.0f.
		float    alpha    = 1.0f;
		Gradient gradient = new Gradient();
		gradient.SetKeys(
			colorKeys: new[]
			{
				new GradientColorKey(new Color(1f, 0.58f, 0f), 0.0f),
				new GradientColorKey(new Color(1f, 0.85f, 0f), 1.0f)
			},
			alphaKeys: new[]
			{
				new GradientAlphaKey(alpha, 0.0f),
				new GradientAlphaKey(alpha, 1.0f)
			}
		);
		m_laserLine.colorGradient = gradient;

		m_shootableMask = LayerMask.GetMask("Shootable");

		HealthSlider.value = m_myHealthManager.MaxHealth;
	}

	const float DragX = 1.0f;
	const float DragY = 0.01f;

	private bool m_bossActive;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			Debug.Log("Reload");

			return;
		}

		if (EnemyCountText == null)
			return;
	
		DoItemEffects();

		
		DamageImage.color = m_damaged ? FlashColor : Color.Lerp(DamageImage.color, Color.clear, FlashSpeed * Time.deltaTime);

		m_damaged = false;

		if (m_myHealthManager.IsDead)
		{
			EnemyCountText.text = "YOU LOSE!";
			return;
		}
		
		if (!m_bOnLadder && TouchingLadder 
			&& Input.GetKeyDown(KeyCode.UpArrow)   || Input.GetKeyDown(KeyCode.W)
		    || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
		{
			MountLadder();
		}

		if (TouchingChest && Input.GetKeyDown(KeyCode.F))
		{
			Chest chestScript = TouchedChest.GetComponent<Chest>();
			int   chestCost   = chestScript.Price;

			if (CoinHandler.CollectedCoins < chestCost || chestScript.Used)
			{
				Debug.Log("Ei pysty");
			}
			else
			{
				CoinHandler.CollectedCoins -= chestCost;

				chestScript.Used           = true;
				chestScript.ChestInfo.text = "";

				var item = ItemBaseItem.Spawn(transform.position, 1);

				Debug.Log("Ostit chestin ", item);
			}
		}
		
		if (TouchingItem)
		{
			AddItem(TouchedItem.GetComponent<Item>().Id);

			Destroy(TouchedItem);
			TouchingItem = false;
			TouchedItem = null;
		}
		

		int iCount = GameObject.FindGameObjectsWithTag("Enemy").Count(enemy => !enemy.GetComponent<HealthManager>().IsDead);

		if (iCount != 0)
		{
			EnemyCountText.text = m_bossActive
				? "KILL THE FINAL BOSS!"
				: (iCount != 1 ? iCount + " enemies alive" : iCount + " enemy alive");
		}
		else
		{
			EnemyCountText.text = "YOU WIN!\nPress R to Restart";
		}

		if (iCount != 0 || m_bossActive)
		{
			return;
		}

		GameObject boss = SpawnManager.SpawnEnemy(new Vector3(-35f, 0f));

		HealthManager bossHpManager = boss.GetComponent<HealthManager>();
		Enemy         bossEnemy     = boss.GetComponent<Enemy>();

		bossHpManager.Health = bossHpManager.MaxHealth *= 10;

		bossEnemy.Acceleration   =  300f;
		bossEnemy.AttackDistance =  2f;
		bossEnemy.DamagePerSwing *= 2;
		bossEnemy.VisionDistance *= 2f;

		boss.transform.localScale *= 2f;

		m_bossActive = true;
	}

	private void AddItem(int id)
	{
		Pickup item = new Pickup(id);

		AddItemToHud(item);
		
		items.Add(item);
		
		Debug.Log(items.ToString());
	}

	private void AddItemToHud(Pickup item)
	{
		Sprite sprite = Resources.Load<Sprite>(item.GetItemSprite(item.Id));

		Instantiate(ItemElement, new Vector2(0f, 40f), Quaternion.identity, UI.transform);
		
		Debug.Log(sprite);
	}
	
	private float _nextCoinTime;
	
	private void DoItemEffects()
	{
		foreach (Pickup item in items)
		{
			switch (item.Id)
			{
				case 1:
				{
					if(Time.time > _nextCoinTime)
					{
						CoinHandler.CollectedCoins++;
						_nextCoinTime = Time.time + 1.0f;
					}
					break;
				}
			}
		}
	}
	
	public void Damaged(Vector2 dirVector2)
	{
		m_damaged = true;

		HealthSlider.value = m_myHealthManager.Health;

		m_cameraController.AddForce(dirVector2, 0.1f);
	}

	// Update is called once per frame
	private void FixedUpdate()
	{
		//Custom drag koska rigidbodyn drag on kakka
		Vector2 vel = m_body.velocity;
		vel.y           *= 1.0f - DragY;
		vel.x           *= 1.0f - DragX;
		
		if(!m_bOnLadder)
		{
			m_body.velocity = vel;
		}

		//Jos olen kuollut tai kohteeni on kuollut, lopeta
		if (m_myHealthManager.IsDead)
		{
			m_animator.SetBool("aim", false);
			m_animator.SetBool("shoot", false);
			m_animator.SetBool("run", false);

			return;
		}

		if (IsOnGround())
		{
			m_bJumping = false;
		}

		Vector2 vecAdditionVelocity = Vector2.zero;
	
		if(!m_bOnLadder)
		{
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
			{
				if (!m_animator.GetBool("aim"))
					vecAdditionVelocity.x = -Acceleration;
	
				m_playerRenderer.flipX = true;
			}
			else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			{
				if (!m_animator.GetBool("aim"))
					vecAdditionVelocity.x = Acceleration;
	
				m_playerRenderer.flipX = false;
			}
		
			if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.LeftControl))
			{
				m_animator.SetBool("aim", true);
				m_animator.SetBool("shoot", true);
			}
			else
			{
				m_animator.SetBool("shoot", m_animator.GetBool("aim") && Input.GetKey(KeyCode.Mouse0));
				m_animator.SetBool("aim", Input.GetKey(KeyCode.Mouse1));
			}
		}
		
		m_animator.SetBool("run", Mathf.Abs(vecAdditionVelocity.x) > 0.0);

		if (m_bOnLadder)
		{
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				Vector2 newPos = m_body.transform.position;
				newPos.y += 10.0f * Time.fixedDeltaTime;
				
				m_body.MovePosition(newPos);
			}
			
			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				Vector2 newPos = m_body.transform.position;
				newPos.y -= 10.0f * Time.fixedDeltaTime;
				
				m_body.MovePosition(newPos);
			}
			
			if (!TouchingLadder || Input.GetKey(KeyCode.Space))
			{
				DismountLadder();
			}
		}
		
		if (!DidJustJump() && IsOnGround() && Input.GetKey(KeyCode.Space))
		{
			vecAdditionVelocity.y = m_body.velocity.y + Jump;
			m_bJumping = true;
		}

		//Tippui ulos levelistä
		if (transform.position.y < -5f)
		{
			m_myHealthManager.Die(gameObject);
		}
		
		if(!m_bOnLadder)
		{
			FinalCollisionCheck(vecAdditionVelocity);
		}
	}

	private void MountLadder()
	{
		if (TouchedLadder == null)
			return;
		
		Vector2 newPos = m_body.transform.position;
		newPos.x = TouchedLadder.transform.position.x;

		m_body.transform.position = newPos;

		gameObject.layer = LayerMask.NameToLayer("CoinPulled");
		
		m_body.velocity = Vector2.zero;
		m_bOnLadder = true;
		m_body.gravityScale = 0.0f;
	}

	private void DismountLadder()
	{
		m_bOnLadder = false;
		m_body.gravityScale = 10.0f;
		
		gameObject.layer = LayerMask.NameToLayer("Player");
	}
	
	public float ShotDistance = 50f;

	[UsedImplicitly]
	private void FireBullet()
	{
		Vector2 rayOrigin = transform.position;

		rayOrigin.y += 0.25f;
		rayOrigin.x += m_playerRenderer.flipX ? -0.5f : 0.5f;

		m_laserLine.SetPosition(0, rayOrigin);

		Vector2 dirVector2 = m_playerRenderer.flipX ? Vector2.left : Vector2.right;

		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dirVector2, ShotDistance, m_shootableMask);
		if (hit.collider != null)
		{
			m_laserLine.SetPosition(1, hit.point);

			HealthManager healthManager = hit.collider.GetComponent<HealthManager>();

			if (healthManager != null)
			{
				healthManager.TakeDamage(gameObject, DamagePerShot, hit.point);
			}
		}
		else
		{
			m_laserLine.SetPosition(1, rayOrigin + dirVector2 * ShotDistance);
		}

		dirVector2.y = Random.Range(0f, 1f);
		m_cameraController.AddForce(dirVector2, -.25f);

		StartCoroutine(ShotEffect());
	}

	private IEnumerator ShotEffect()
	{
		m_laserLine.enabled = true;

		yield return .1f;

		m_laserLine.enabled = false;
	}

	//https://forum.unity.com/threads/object-wont-fall-when-i-apply-horizontal-velocity-and-is-colliding-with-wall.143698/#post-2911222
	private void FinalCollisionCheck(Vector2 vecAdditionVelocity)
	{
		if (IsOnGround())
		{
			//Fuck it dood
			m_body.AddForce(vecAdditionVelocity);
			return;
		}

		// Get the velocity
		Vector2 moveDirection = new Vector2(m_body.velocity.x * Time.fixedDeltaTime, m_body.velocity.y * Time.fixedDeltaTime);

		// Get bounds of Collider
		var bottomRight = new Vector2(m_collider.bounds.max.x, m_collider.bounds.max.y - 0.2f);
		var topLeft     = new Vector2(m_collider.bounds.min.x, m_collider.bounds.min.y + 0.2f);

		// Move collider in direction that we are moving
		bottomRight += moveDirection;
		topLeft     += moveDirection;

		// Check if the body's current velocity will result in a collision
		if (Physics2D.OverlapArea(topLeft, bottomRight, m_shootableMask))
		{
			// If so, stop the movement
			m_body.velocity = new Vector3(0, m_body.velocity.y, 0);
		}
		else
		{
			// Else, add movement
			m_body.AddForce(vecAdditionVelocity);
		}
	}

	private bool DidJustJump()
	{
		return m_bJumping && (m_body.velocity.y > 0.0f);
	}

	private bool IsOnGround()
	{
		ContactPoint2D[] contacts = new ContactPoint2D[8];
		m_body.GetContacts(contacts);

		foreach (ContactPoint2D contact in contacts)
		{
			if (contact.normal.y > 0.6)
			{
				return true;
			}
		}

		return false;
	}
}