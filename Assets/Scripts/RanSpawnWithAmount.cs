using UnityEngine;
using System; // cần có để có thể điều chỉnh lại danh sách, nếu có random thì phải dùng UnityEngine.Random()
public class RanSpawnWithAmount : MonoBehaviour {
    // gắn trong nhà, có ít nhất 1 vật sẽ spawn
    public GameObject[] SpawnObject; // vật sẽ spawn
    public GameObject[] SpawnPosition; // nơi sẽ spawn
    [Range(1, 100)]
    public int SecondSpawnRate = 0; // tỉ lệ có thêm 1 rương
    void Start(){
        // spawn chắc chắn một vật
        GameObject ClonerMain = GetRandomGameObject();
        // xoay vật
        Quaternion RanRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
        // lấy số đại diện cho điêm spawn
        int a = UnityEngine.Random.Range(0, SpawnPosition.Length); 
        GameObject PositionMain = SpawnPosition[a];
        // đặt      vật spawn       nơi sẽ spawn                độ xoay
        Instantiate(ClonerMain, PositionMain.transform.position, RanRotation);
        // quay thêm lần nữa
        int b = UnityEngine.Random.Range(0, SpawnPosition.Length);
        // tỉ lệ spawn ra vật tiếp theo
        if (CanSpawn() && b != a)
        {
            GameObject ClonerSide = GetRandomGameObject(); // lấy
            Quaternion RanRotation1 = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0); // xoay
            // đặt      vật spawn       nơi sẽ spawn                độ xoay
            Instantiate(ClonerSide, SpawnPosition[b].transform.position, RanRotation1); // đặt
        }
        gameObject.SetActive(false);
    }
    public bool CanSpawn(){ // quay số ngẫu nhiên, số nhỏ hơn spawn rate sẽ có
        if (UnityEngine.Random.Range(0, 100) < SecondSpawnRate)
        return true;
        else
        return false;
    }
    public GameObject GetRandomGameObject(){ // lấy một vật ngẫu nhiên có trong danh sách
        return SpawnObject[UnityEngine.Random.Range(0, SpawnObject.Length)];
    }
    /*void RemoveIndex<T>(ref T[] arr, int Index){ //sửa lại danh sách []
        for (int i = Index; i < arr.Length; i++)
        {
            arr[i] = arr[i + 1];
        }
        Array.Resize(ref arr, arr.Length - 1);
    }*/
}