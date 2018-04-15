using System;
using System.IO;
using UnityEngine;
using UnityRest;

[Serializable]
public class ItemData
{
	// ReSharper disable once InconsistentNaming
	public int    id;
	// ReSharper disable once InconsistentNaming
	public string name;
	// ReSharper disable once UnassignedField.Global
	// ReSharper disable once InconsistentNaming
	public string image;
}

public class ItemManager : MonoBehaviour
{
	private void Awake()
	{
		string gameDataFileName = "items.json";

		// Path.Combine combines strings into a file path
		// Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
		string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);

		Debug.Log(filePath);

		if (File.Exists(filePath))
		{
			// Read the json from the file into a string
			string dataAsJson = File.ReadAllText(filePath);

			// Pass the json to JsonUtility, and tell it to create a GameData object from it
			ItemData[] objects = JsonHelper.FromJsonWrapped<ItemData>(dataAsJson);

			foreach (ItemData item in objects)
			{
				Debug.Log(item.image);
			}
		}
		else
		{
			Debug.LogError("Cannot load game data!");
		}
	}
}

namespace UnityRest
{
	public static class JsonHelper
	{
		public static T[] FromJson<T>(string jsonArray)
		{
			jsonArray = WrapArray(jsonArray);
			return FromJsonWrapped<T>(jsonArray);
		}

		public static T[] FromJsonWrapped<T>(string jsonObject)
		{
			Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(jsonObject);
			return wrapper.items;
		}

		private static string WrapArray(string jsonArray)
		{
			return "{ \"items\": " + jsonArray + "}";
		}

		public static string ToJson<T>(T[] array)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.items = array;
			return JsonUtility.ToJson(wrapper);
		}

		public static string ToJson<T>(T[] array, bool prettyPrint)
		{
			Wrapper<T> wrapper = new Wrapper<T>();
			wrapper.items = array;
			return JsonUtility.ToJson(wrapper, prettyPrint);
		}

		[Serializable]
		private class Wrapper<T>
		{
			// ReSharper disable once InconsistentNaming
			public T[] items;
		}
	}
}