using UnityEngine;

public class Item : MonoBehaviour
{
	public int            Id;
	public SpriteRenderer Renderer;

	// Use this for initialization
	private void Start()
	{
		Renderer = GetComponent<SpriteRenderer>();
	}

	public Item Spawn(Vector3 position, int id, string image)
	{
		Id = id;

		Sprite sprite = Resources.Load<Sprite>(image);
		
		Debug.Log(sprite);

		if (sprite == null)
			return null;

		Renderer.sprite = sprite;

		return Instantiate(this, position, Quaternion.identity);
	}
}