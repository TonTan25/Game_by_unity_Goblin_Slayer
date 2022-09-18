using UnityEngine;
using System.Collections;
public class ItemsInteract : MonoBehaviour
{
    // đặt trong item
    // có thể tự động thu thập sau thời gian ngẫu nhiên
    enum ItemType{ StackItem, Pickaxe, Sword, LargeSword }
    [SerializeField] ItemType itemType;
    [SerializeField] int StackType, ItemAmount = 1; // số lượng
    // 0 = đợt, 1 = vàng, 2 = máu, 3 = thể lực, 4 = trang bị
    [SerializeField] string ItemName; // tên của vật phẩm
    [SerializeField] Sprite ItemImgs; // ảnh của vật
    [SerializeField] Color TextColor; // màu chữ
    void Start(){
        StartCoroutine(ItemActive());
    }
    // đặt thông tin của vật
    public void SetItemInfo(string NewItemName, int NewItemAmount){
        ItemName = NewItemName;
        ItemAmount = NewItemAmount;
    }
    public IEnumerator ItemActive(){
        GetComponent<SphereCollider>().enabled = false; // tắt khả năng nhặt
        // tạo vụ nổ ngẫu nhiên
        GetComponent<Rigidbody>().AddExplosionForce(Random.Range(50f, 200f), gameObject.transform.position + new Vector3(0,0,1f), 2f);
        yield return new WaitForSeconds(2f); // thời gian chờ cho vật di chuyển
        GetComponent<SphereCollider>().enabled = true; // bật khả năng nhặt
        yield return new WaitForSeconds(Random.Range(5f, 8f)); // thời gian chờ trước khi xóa
        IncreaseLoot();
    }
    void OnTriggerEnter(Collider other){ // nhặt vật
        if(other.gameObject.CompareTag("Player")) IncreaseLoot();
    }
    void IncreaseLoot(){
        switch (itemType){
            default:
            case ItemType.StackItem:
                if (StackType == 1) FindObjectOfType<AchiveIGTrack>().AchivePorgress(14, true, ItemAmount);
                else FindObjectOfType<AchiveIGTrack>().AchivePorgress(15, true, ItemAmount);
            break;
            case ItemType.Pickaxe:
                FindObjectOfType<PlayerCombatSystem>().WeaponsAllowed[1] = true;
            break;
            case ItemType.Sword:
                FindObjectOfType<PlayerCombatSystem>().WeaponsAllowed[2] = true;
            break;
            case ItemType.LargeSword:
                FindObjectOfType<PlayerCombatSystem>().WeaponsAllowed[3] = true;
            break;
        }
        StopAllCoroutines(); // dừng bộ đếm phụ
        FindObjectOfType<GameControl>().ItemPickPop(StackType, ItemImgs, ItemName, ItemAmount, TextColor);
        for (int i = 0; i < ItemAmount; i++){
            if(StackType == 2) FindObjectOfType<PlayerCombatSystem>().PotsUpdate();
            else if(StackType == 3) FindObjectOfType<PlayerHealthSystem>().StaRestore(10);
        }
        GetComponent<SoundManager>().Play("Pickup" + Random.Range(0, 4)); // âm thanh
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        Destroy(gameObject, 1.5f); // xóa vật sau 1.5 giây
    }
}