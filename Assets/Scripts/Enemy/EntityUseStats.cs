using UnityEngine;
[CreateAssetMenu(fileName = "Enemy UseStats", menuName = "EUse Name")]
public class EntityUseStats : ScriptableObject { // chỉ số sử dụng
    public EntityBaseStats baseStats; // chỉ số nền
    public int EntityIndext; // số thứ tự
    public float UseHealthMin, UseHealthMax, UseMinDmg, UseMaxDmg, UseSpeed; // chỉ số
    public void StatAdjust(float ScaleAmount){ // áp dụng chỉ số (chỉ số mới = chỉ số hiện tại * tỉ lệ)
        UseHealthMax *= ScaleAmount;
        UseHealthMin *= ScaleAmount;
        UseMaxDmg *= ScaleAmount;
        UseMinDmg *= ScaleAmount;
    }
    public void RefreshStats(){ // đặt lại từ đầu
        UseHealthMax = baseStats.entityStats[EntityIndext].BaseHealthMax;
        UseHealthMin = baseStats.entityStats[EntityIndext].BaseHealthMin;
        UseMaxDmg = baseStats.entityStats[EntityIndext].BaseMaxDmg;
        UseMinDmg = baseStats.entityStats[EntityIndext].BaseMinDmg;
        UseSpeed = baseStats.entityStats[EntityIndext].BaseSpeed;
    }
}