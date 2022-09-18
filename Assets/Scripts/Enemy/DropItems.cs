using UnityEngine;
public class DropItems : MonoBehaviour {
    // tính năng rơi đồ
    [SerializeField] EntityBaseStats EntityBaseStats;
    public void DropSomething(int EntityIndext){ // rơi đồ
        if (PlayerPrefs.GetInt("CurMode") != 2){
            for (int i = 0; i < EntityBaseStats.entityStats[EntityIndext].itemDrop.Length; i++){
                if (Random.Range(0, 100) < EntityBaseStats.entityStats[EntityIndext].itemDrop[i].Rate){ // lấy tỉ lệ rơi
                    // lấy số lượng sẽ rơi ra
                    int ItemsAmount = Random.Range(EntityBaseStats.entityStats[EntityIndext].itemDrop[i].MinAmount, EntityBaseStats.entityStats[EntityIndext].itemDrop[i].MaxAmount + 1);
                    int DropAmount = Random.Range(1,11); // lấy số lượng ngẫu nhiên rơi ra
                    if(DropAmount > ItemsAmount) DropAmount = ItemsAmount; // tránh lỗi nhặt đồ ảo
                    for (int I = 0; I < DropAmount; I++){ // tạo số vật tương ứng với số lượng rơi
                        int SetAmount = Random.Range(1, ItemsAmount); // số lượng nén lại cho mỗi vật
                        ItemsAmount -= SetAmount; // trừ đi để cho lần tiếp
                        if (ItemsAmount > 0){
                            // lấy vị trí ngẫu nhiên
                            Vector3 RanPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 2.5f), Random.Range(-0.5f, 0.5f));
                            // tạo vật
                            GameObject Clone = Instantiate(EntityBaseStats.entityStats[EntityIndext].itemDrop[i].Item, gameObject.transform.position + RanPosition, Quaternion.identity);
                            // xoay ngẫu nhiên
                            Clone.transform.rotation = Random.rotation;
                            // đặt số lượng và tên
                            ItemsInteract itemsInteract = Clone.GetComponent<ItemsInteract>();
                            if (PlayerPrefs.GetInt("CL") == 0)
                                itemsInteract.SetItemInfo(EntityBaseStats.entityStats[EntityIndext].itemDrop[i].VItemName, SetAmount);
                        else itemsInteract.SetItemInfo(EntityBaseStats.entityStats[EntityIndext].itemDrop[i].EItemName, SetAmount);
                        }
                        if(ItemsAmount <= 0) ItemsAmount = 1; // tránh lỗi âm
                    }
                }
            }
        }
    }
}
