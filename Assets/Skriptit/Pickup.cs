using JetBrains.Annotations;

public class Pickup
{
	public Pickup(int id)
	{
		Id = id;

		IncreaseCount();
	}

	private void IncreaseCount()
	{
		Count++;
	}

	public int Id { [UsedImplicitly] get; set; }
	private int Count { get; set; }
	
	public string GetItemSprite(int id)
	{
		switch (id)
		{
			case 1: return "possu";
		}

		return "";
	}
}