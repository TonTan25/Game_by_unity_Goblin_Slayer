using UnityEngine;

public class ObjectSpawnPos : MonoBehaviour {
    //scrips dùng để spawn vật tại những nơi nhất định
    public bool Scaled;
    [Range(1f, 10f)] public float MaxScale, MinScale;
    public GameObject[] ObjectToSpawn; // vật spawn
    public GameObject[] SpawnPosition; // nơi spawn
    void Awake(){
        if (MinScale > MaxScale) MinScale = MaxScale - 0.001f;
        for (int i = 0; i < SpawnPosition.Length; i++)
        {
            // lấy vị trí của vật nơi spawn
            Vector3 NewClonePosition = SpawnPosition[i].transform.position;
            // chọn vật sẽ spawn với con số đại diện ngẫu nhiên
            GameObject NewCloneObject = ObjectToSpawn[Random.Range(0, ObjectToSpawn.Length)];
            // lấy vị trí xoay ngẫu nhiên
            Vector3 NewCloneRotation = new Vector3 (0, Random.Range(0f, 360f), 0);
            // kéo tỉ lệ ngẫu nhiên của vật nếu được bật
            Instantiate(NewCloneObject, NewClonePosition, NewCloneObject.transform.rotation);
            NewCloneObject.transform.eulerAngles = NewCloneObject.transform.eulerAngles + NewCloneRotation;
            if (Scaled)
            {
                Vector3 Scaling = Vector3.one * Random.Range(MinScale, MaxScale);
                NewCloneObject.transform.localScale = Scaling;
            }
            // spawn vật
            Destroy(SpawnPosition[i]); //xóa vị trí spawn để giảm trọng tải
        }
    }
}