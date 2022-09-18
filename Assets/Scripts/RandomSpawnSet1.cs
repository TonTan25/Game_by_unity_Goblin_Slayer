using UnityEngine;
using System.Collections;
public class RandomSpawnSet1 : MonoBehaviour {
    // sinh ra vật ở một nơi ngẫu nhiên trong phạm vi ngẫu nhiên hoặc theo thời gian
    // có xoay ngẫu nhiên
    [SerializeField] Transform SpawnPoint; // chọn điểm sinh ra
    enum SpawnMode{InstantSpawn, SpawnOverTime};
    [SerializeField] SpawnMode spawnMode;
    [SerializeField] bool StaticSpawnAmount, RandomRotate, RandomScale, DrawSpawnRadious;
    // điều kiện để:      số lượng cố định |   xoay vật  |  kéo vật   | vẽ phạm vi tạo
    [SerializeField][Range(0f, 5f)] float MinSize, MaxSize;
    //                                  thông số kích cỡ
    [SerializeField] int SpawnAmount, MinSpawnAmount, MaxSpawnAmount, CurAmount;
    // số lượng tạo:      cố định   |           ngẫu nhiên          | số lượng hiện tại
    [SerializeField][Range(0f, 10f)] public float RotateAngle; // độ nghiêng của vật
    public float SpawnRaious, SpawnOverTimeWait; //
    //          phạm vi đặt | đợi để tạo tiếp, đề xuất 2,5f
    public GameObject[] ObjectToSpawn; // vật sẽ sinh ra
    public void CallSpawn() {
        if (!StaticSpawnAmount) SpawnAmount = Random.Range(MinSpawnAmount, MaxSpawnAmount); // kiểm tra điều kiện
        
        switch (spawnMode){
            default:
            case SpawnMode.InstantSpawn:
                InstantSpawn();
            break;
            case SpawnMode.SpawnOverTime:
                StartCoroutine(SpawnWithTime());
            break;
        }
    }
    void InstantSpawn(){
        Vector3 NewPosition, NewRotation;
        for (int i = 0; i < SpawnAmount; i++){
        // đặt vật
            GameObject Clone = Instantiate(ObjectToSpawn[Random.Range(0, ObjectToSpawn.Length)]);
        // lấy vị trí cho vật
            NewPosition = new Vector3(Random.Range(-SpawnRaious, SpawnRaious), 0, Random.Range(-SpawnRaious, SpawnRaious));
            NewRotation = new Vector3(Random.Range(-RotateAngle, RotateAngle), 0, Random.Range(-RotateAngle, RotateAngle));
        // kiểm tra điều kiện
            if (RandomRotate) NewRotation += new Vector3(0, Random.Range(0f, 360f), 0);
            if (RandomScale) Clone.transform.localScale = Vector3.one * Random.Range(MinSize, MaxSize);
        // đặt vật đến vị trí mới với số liệu mới
            Clone.transform.SetParent(SpawnPoint);
            Clone.transform.position = SpawnPoint.position + NewPosition; // đặt vật đến vị trí
            Clone.transform.eulerAngles = NewRotation; // độ xoay vật
        }
    }
    IEnumerator SpawnWithTime(){
        Vector3 NewPosition, NewRotation;
        CurAmount = 0;
        while (CurAmount > 0){
            // đặt vật
            GameObject Clone = Instantiate(ObjectToSpawn[Random.Range(0, ObjectToSpawn.Length)]);
        // lấy vị trí cho vật
            NewPosition = new Vector3(Random.Range(-SpawnRaious, SpawnRaious), 0, Random.Range(-SpawnRaious, SpawnRaious));
            NewRotation = new Vector3(Random.Range(-RotateAngle, RotateAngle), 0, Random.Range(-RotateAngle, RotateAngle));
        // kiểm tra điều kiện
            if (RandomRotate) NewRotation += new Vector3(0, Random.Range(0f, 360f), 0);
            if (RandomScale) Clone.transform.localScale = Vector3.one * Random.Range(MinSize, MaxSize);
        // đặt vật đến vị trí mới với số liệu mới
            Clone.transform.SetParent(SpawnPoint);
            Clone.transform.position = SpawnPoint.position + NewPosition; // đặt vật đến vị trí
            Clone.transform.eulerAngles = NewRotation; // độ xoay vật
            CurAmount--;
            yield return new WaitForSeconds(SpawnOverTimeWait);
        }
    }
    void OnDrawGizmos() { // vẽ phạm vi sinh ra
        if (DrawSpawnRadious){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(SpawnPoint.position, SpawnRaious);
        }
    }
}