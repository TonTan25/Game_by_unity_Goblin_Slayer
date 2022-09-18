using UnityEngine;
[CreateAssetMenu(fileName = "Weapon Lists", menuName = "New Weapon List")]
public class WeaponInfo : ScriptableObject {
    public Weapons[] weapons;
    [System.Serializable] public class Weapons{
        public string WPName;
        public Sprite WeaponImg;
        public int MinDamage, MaxDamage;
        [Range(0, 100)] public int CritChance;
        [Range(0,3)] public float AtkSpeed, CritDamage;
    }
}