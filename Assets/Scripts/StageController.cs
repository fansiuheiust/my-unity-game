using Combat;
using Progression;
using Progression.Balance;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class StageController : MonoBehaviour
{
    // Player stats
    public Player Player { get; private set; }
    public PlayerLevel PlayerLevel { get; private set; }
    public PlayerPerk PlayerPerk { get; private set; }
    [SerializeField]
    bool loadFromSave = false;
    [SerializeField, Tooltip("This leveling data will be used for the player's leveling")]
    Leveling levelingData;
    [SerializeField, Tooltip("This perk data will be used for the player's perks")]
    Perks perkData;
    static string saveFilePath;
    public static StageController Controller;
    void Awake() {
        Controller = this;
        saveFilePath = $"{Application.persistentDataPath}/PlayerData.bin";
        LoadPlayerData();
    }

    void LoadPlayerData() {
        Player = FindFirstObjectByType<Player>();
        if (loadFromSave && File.Exists(saveFilePath)) {
            BinaryFormatter formatter = new();
            FileStream stream = new(saveFilePath, FileMode.Open);
            try {
                SaveData data = formatter.Deserialize(stream) as SaveData;

                PlayerLevel = new(levelingData, data.level, data.point);
                PlayerPerk = new(perkData, data.coins);

            } catch {
                Debug.Log("Player data file load failed.");
                PlayerLevel = new(levelingData);
                PlayerPerk = new(perkData);
            } finally {
                stream.Close();
            }
        } else {
            PlayerLevel = new(levelingData);
            PlayerPerk = new(perkData);
        }
    }

    public void SaveData() {
        Dictionary<CoinType, uint[]> coins = new();
        foreach (CoinType t in System.Enum.GetValues(typeof(CoinType))) {
            coins.Add(t, new uint[Global.Rarities.Length]);
            for (uint i = 0; i < Global.Rarities.Length; i++) {
                coins[t][i] = PlayerPerk.Coin(t, i);
            }
        }
        SaveData data = new() { level = PlayerLevel.Level, point = PlayerLevel.Point, coins = coins };
        BinaryFormatter formatter = new();
        FileStream fileStream = new(saveFilePath, FileMode.Create);
        try {
            formatter.Serialize(fileStream, data);
        } catch {
            Debug.Log("Player data file save failed");
        } finally {
            fileStream.Close();
        }
    }
}
[System.Serializable]
class SaveData {
    public uint level;
    public uint point;

    public Dictionary<CoinType, uint[]> coins;
    // TODO: save unlocked perks
}
