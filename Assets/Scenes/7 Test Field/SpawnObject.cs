
using UnityEngine;
public class SpawnObject : MonoBehaviour {
            // sử dụng để tạo vật cố định bất kỳ
    enum SpawnType{RandomSpawnWithTransform, SpawnWithPosition};
    [SerializeField] SpawnType spawnType;
    [SerializeField] ObjectToSpawn Object;
    void Awake(){
        switch (spawnType){
            default:
            case SpawnType.RandomSpawnWithTransform:

            break;

            case SpawnType.SpawnWithPosition:

            break;

        }
    }
    void SpawnWithTransform(){
        // Ray hit;

    }
    [System.Serializable] public class ObjectToSpawn{
        public GameObject[] Objects;
    }
}