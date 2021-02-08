using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class tb_NetworkManager : MonoBehaviour
{

	[Serializable]
	public struct Scores
	{
		public Score[] datas;
	}

	[Serializable]
	public struct Score
	{
		public int id;
		public string pseudo;
		public int score;
		public string uuid;
	}

	const string URL_REST_API = "https://api.r-c.es/m2dt-unity/asteroids";

	public Text leaderboard;
	public InputField pseudoField;
	public Text pseudoLabel;

	string playerUUID;
	string playerPseudo;

	void Start()
	{
		print("Network : " + Application.internetReachability);
		print("Mobile : " + Application.isMobilePlatform);
		print("Platform: " + Application.platform);

		playerUUID = PlayerPrefs.GetString("player-UUID");
		if (playerUUID == "")
		{
			// Universally unique identifier
			// f9af32f7-9b4a-4876-aee1-4170f62e0d2e
			// 2dd02aad-d1a0-4f67-b128-bbff60a9a800
			Guid guid = Guid.NewGuid();
			playerUUID = guid.ToString();
			PlayerPrefs.SetString("player-UUID", playerUUID);
		}
		print(playerUUID);


		playerPseudo = PlayerPrefs.GetString("player-Pseudo");
		if (playerPseudo == "")
		{
			playerPseudo = "Anonymous";
			PlayerPrefs.SetString("player-Pseudo", playerPseudo);
		}

		tb_LoadScores();

		// tb_SendScore(48000);

		pseudoField.onValueChanged.AddListener(delegate { tb_PseudoFieldOnChange(); });
	}

	public void tb_PseudoFieldOnChange()
	{
		playerPseudo = pseudoField.text;
        pseudoLabel.text = pseudoField.text;

		PlayerPrefs.SetString("player-Pseudo", playerPseudo);
	}




	public void tb_SendScore(int score)
	{
		StartCoroutine(tb_SendScoresToNetwork(score));
	}

	IEnumerator tb_SendScoresToNetwork(int score)
	{
		// préparer les données à envoyer
		WWWForm form = new WWWForm();
		form.AddField("pseudo", playerPseudo);
		form.AddField("uuid", playerUUID);
		form.AddField("score", score);

		// envoyer
		UnityWebRequest request = UnityWebRequest.Post(URL_REST_API, form);
		// attendre
		yield return request.SendWebRequest();
		if (request.isNetworkError || request.isHttpError)
		{
			Debug.Log("Error: " + request.error);
			leaderboard.text = "Network Error - no Leaderboard";
		}
		else
		{
			tb_LoadScores();
		}
	}

	public void tb_LoadScores()
	{
		StartCoroutine(tb_LoadScoresFromNetwork());
	}

	IEnumerator tb_LoadScoresFromNetwork()
	{
		// demande de flux réseau
		UnityWebRequest request = UnityWebRequest.Get(URL_REST_API);

		// attendre le chargement
		yield return request.SendWebRequest();

		// si erreur 
		if (request.isNetworkError || request.isHttpError)
		{
			Debug.Log("Error: " + request.error);
			leaderboard.text = "Network Error - no Leaderboard";
		}
		else
		{
			string json = request.downloadHandler.text;
			print(json);
			//  leaderboard.text = json;
			Scores root = JsonUtility.FromJson<Scores>(json);
			leaderboard.text = "";
			for (int i = 0; i < root.datas.Length; i++)
			{
				Score score = root.datas[i];
				print(score.pseudo + " " + score.score);

				if (score.uuid == playerUUID)
				{
					score.pseudo = "<b>" + score.pseudo + "</b>";
				}

				leaderboard.text += score.score.ToString("000 000") + "  " + score.pseudo + "\n";
			}
		}

	}

}
