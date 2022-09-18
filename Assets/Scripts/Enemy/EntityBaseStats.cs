using UnityEngine;
[CreateAssetMenu(fileName = "Enemy BaseStats", menuName = "Enemy Base stats list")]
public class EntityBaseStats : ScriptableObject { //sử dụng để chứa thông tin gốc
    public EntityStats[] entityStats;
    [System.Serializable] public class EntityStats{
        public string name; // tên của mục tiêu
        public float BaseHealthMin, BaseHealthMax, BaseSpeed, BaseMinDmg, BaseMaxDmg;
        // chỉ số   |           máu            |   tốc độ   |      sát thương     |
        public ItemDrop[] itemDrop; // đồ sẽ rơi ra khi chết
    }
    [System.Serializable] public class ItemDrop{
        public string EItemName, VItemName;
        [Range(0,100)] public float Rate; // tỉ lệ rơi
        public int MinAmount, MaxAmount; // số lượng rơi
        public GameObject Item; // vật sẽ rơi
    }
}