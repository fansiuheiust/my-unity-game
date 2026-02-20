using Progression;
using Progression.Balance;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Combat {
    public class Player : Mob {

        Transform _camera;
        static string saveFilePath;
        public PlayerLevel Level { get; private set; }
        public PlayerPerk PerkManager { get; private set; }
        [SerializeField]
        bool loadFromSave = false;
        [SerializeField, Tooltip("This leveling data will be used for the player's leveling")]
        Leveling levelingData;
        [SerializeField, Tooltip("This perk data will be used for the player's perks")]
        Perks perkData;
        protected override void Awake() {
            base.Awake();
            saveFilePath = $"{Application.persistentDataPath}/PlayerData.bin";
            if (loadFromSave && File.Exists(saveFilePath)) {
                BinaryFormatter formatter = new();
                FileStream stream = new(saveFilePath, FileMode.Open);
                try {
                    SaveData data = formatter.Deserialize(stream) as SaveData;

                    Level = new(levelingData, data.level, data.point);
                    PerkManager = new(perkData, data.coins);

                } catch {
                    Level = new(levelingData);
                    PerkManager = new(perkData);
                } finally {
                    stream.Close();
                }
            } else {
                Level = new(levelingData);
                PerkManager = new(perkData);
            }

            _camera = transform.Find("Camera");
            Faction = Faction.Ally;
        }



        protected override void Die(Mob killer) {
            Debug.Log("You died, but let me restore your HP.");
            Stats.Heal(Stats.Final.MaxHp, Stats);
        }

        /// <summary>
        /// Rotates player's movement
        /// </summary>
        /// <param name="rotation">rotation</param>
        public void RotateMovement(Quaternion rotation) {
            CastMovement<PlayerMovement>().Rotate(rotation);
        }

        public void RotateToCamera() {
            _rotatable.localEulerAngles = new Vector3(0, _camera.localEulerAngles.y, 0);
        }


        public void GainCoin(CoinType type, uint tier, uint amount) {
            PerkManager.GainCoin(type, tier, amount);
            Level.AddPoint((uint)(amount * Mathf.Pow(perkData.CoinDecompositionRatio, tier) * perkData.CoinPerLevelPoint));
        }


        public void SaveData() {
            SaveData data = new() { level = Level.Level, point = Level.Point, coins = PerkManager.CoinDataForSavingOnly };
            BinaryFormatter formatter = new();
            FileStream fileStream = new(saveFilePath, FileMode.Create);
            try {
                formatter.Serialize(fileStream, data);
            } catch { } finally {
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
}