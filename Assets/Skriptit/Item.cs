using UnityEngine;

public class Item : MonoBehaviour
{
	//adjust this to change speed
	public float Speed = .2f;

	//adjust this to change how high it goes
	public float Height = 25f;

	public int Id;

	public Vector2 OriginalPos;
	
	public Item Spawn(Vector2 position, int id)
	{
		Id = id;

		Sprite sprite = Resources.Load<Sprite>(GetItemSprite(id));

		Debug.Log(sprite);

		if (sprite == null)
			return null;

		Item item = Instantiate(this, position, Quaternion.identity);
		item.OriginalPos = position;
		item.GetComponent<SpriteRenderer>().sprite = sprite;

		return item;
	}


	public void Update()
	{
		//get the objects current position and put it in a variable so we can access it later with less code
		Vector2 pos = this.OriginalPos;
		
		//calculate what the new Y position will be
		pos.y = Mathf.Cos(Time.time * Speed) * (Height / 2) + (Height / 2);
		
		//set the object's Y to the new calculated Y
		transform.position = pos;
	}

	public string GetItemSprite(int id)
	{
		switch (id)
		{
			case 1: return "possu";
		}

		return "";
	}
}