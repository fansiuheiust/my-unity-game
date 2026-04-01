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
    public static Player Player { get; private set; }
    public static PlayerLevel PlayerLevel { get; private set; }
    public static PlayerPerk PlayerPerk { get; private set; }
    [SerializeField]
    bool loadFromSave = false;
    [SerializeField, Tooltip("This leveling data will be used for the entire game")]
    LevelingData levelingData;
    [SerializeField, Tooltip("This perk data will be used for the entire game")]
    PerkData perkData;
    [SerializeField, Tooltip("This dungeon data will be used for the entire game")]
    DungeonData dungeonData;
    public static LevelingData LevelingData => instance.levelingData;
    public static PerkData PerkData => instance.perkData;
    public static DungeonData DungeonData => instance.dungeonData;

    static string saveFilePath;
    public static StageController instance;
    void Awake() {
        saveFilePath = $"{Application.persistentDataPath}/PlayerData.bin";
        LoadPlayerData();
        instance = this;
    }

    void LoadPlayerData() {
        Player = FindFirstObjectByType<Player>();
        if (loadFromSave && File.Exists(saveFilePath)) {
            BinaryFormatter formatter = new();
            FileStream stream = new(saveFilePath, FileMode.Open);
            try {
                SaveData data = formatter.Deserialize(stream) as SaveData;

                PlayerLevel = new(data.level, data.point);
                PlayerPerk = new(data.coins);

            } catch {
                Debug.Log("Player data file load failed.");
                PlayerLevel = new();
                PlayerPerk = new();
            } finally {
                stream.Close();
            }
        } else {
            PlayerLevel = new();
            PlayerPerk = new();
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
