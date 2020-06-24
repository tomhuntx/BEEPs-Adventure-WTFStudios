using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class DataSaver : MonoBehaviour
{
	/// <summary>
	/// Creates a new binary file
	/// </summary>
	public static Data NewData()
	{
		BinaryFormatter format = new BinaryFormatter();

		// Saves in ../AppData/LocalLow/DefaultCompany/BoxGame/player.data
		string path = Application.persistentDataPath + "/player.data";
		FileStream stream = new FileStream(path, FileMode.CreateNew);

		Data data = new Data();

		format.Serialize(stream, data);
		stream.Close();

		return data;
	}

	/// <summary>
	/// Saves current progress data into a binary file
	/// </summary>
	/// <param name="gm">The GameManager script in the open scene</param>
	public static void SaveProgress(GameManager gm)
	{
		BinaryFormatter format = new BinaryFormatter();

		string path = Application.persistentDataPath + "/player.data";
		FileStream stream = new FileStream(path, FileMode.Create);

		Data data = new Data(gm);

		format.Serialize(stream, data);
		stream.Close();
	}

	/// <summary>
	/// Load the current save data
	/// </summary>
	/// <returns> Readable data </returns>
	public static Data LoadData()
	{
		string path = Application.persistentDataPath + "/player.data";
		if (File.Exists(path))
		{
			BinaryFormatter format = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			Data data = format.Deserialize(stream) as Data;
			stream.Close();

			return data;
		}
		else
		{
			Debug.LogWarning("Save file not found in directory " + path);
			return null;
		}
	}

	/// <summary>
	/// Creates a new data file in place of an existing one
	/// Resets progress to the tutorial level
	/// </summary>
	public static void ResetProgress()
	{
		string path = Application.persistentDataPath + "/player.data";
		if (File.Exists(path))
		{
			BinaryFormatter format = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

			Data data = format.Deserialize(stream) as Data;
			data.SetLevel(0);
			//...

			format.Serialize(stream, data);
			stream.Close();
		}
		else
		{
			Debug.LogError("Save file not found in directory " + path);
		}
	}
}
