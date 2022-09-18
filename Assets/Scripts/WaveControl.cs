using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
public class WaveControl : MonoBehaviour { // chỉ để thử nghiệm
// thử sinh ra liên tục, có thể áp dụng cho Waves và Grim
    // chung
    public Text CurWaveText, DashText, WaveDelayedText;
    //  hiển thị      | đợt hiện tại |  lượng quái   | thời gian chờ đợt mới |
    [SerializeField] bool DrawSpawnRange;
    [SerializeField] int CurEnemy; // lượng quái hiện tại
    public int CurWave, SpawnAmount, MinIncrs, MaxIncrs;
    //              đợt hiện tại| số lượng tạo|   số lượng tăng   |
    [SerializeField] float WaveDelay, SpawnDelay, SpawnRadious;
    //                  chờ đợt tiếp|chờ tạo tiếp| phạm vi tạo |
    [SerializeField][Range(1f,1.1f)] float StatsScale; // tỉ lệ sức mạnh theo thời gian
    [SerializeField] int EntityTypeMax = 1, WaveTillNewType, WaveTillNewTool;
    public Transform[] SpawnPoint; //vị trí ngẫu nhiên sẽ tạo ra
    [SerializeField] EntityType[] EntityList; // danh sách tạo ra chính
    [SerializeField] EntityUseStats[] EUseStats; // bảng thay đổi sức mạnh quái
    [SerializeField] PlayerCombatSystem playerCombatSystem; // theo dõi số lượng trang bị
    [SerializeField] EntityBaseStats BaseStats;
    AchiveIGTrack achiveIGTrack;
    public enum SpawnState{ Spawning, Checking };
    public SpawnState state;
    [SerializeField] GameObject GD;
    void Start(){
        achiveIGTrack = FindObjectOfType<AchiveIGTrack>();
        if (PlayerPrefs.GetInt("CL") == 0) CurWaveText.text = "Đợt 0";
        else CurWaveText.text = "Wave 0";
        playerCombatSystem = FindObjectOfType<PlayerCombatSystem>();
        WaveDelayedText.enabled = false;
        for (int i = 0; i < EUseStats.Length; i++) EUseStats[i].RefreshStats();
        state = SpawnState.Spawning;
        if (PlayerPrefs.GetInt("CurMode") == 1){
            StartCoroutine(DashCheck());
            StartCoroutine(GearChecking());
            StartCoroutine(EntityChecking());
        } else if (PlayerPrefs.GetInt("CurMode") == 2){
            for (int i = 0; i < EUseStats.Length; i++) EUseStats[i].StatAdjust(10f);
        }
    }
    void Update(){
        switch (state){
            default:
            case SpawnState.Checking:
                if(CurEnemy <= 0){
                    SetInstantSpawn();
                    state = SpawnState.Spawning;
                }
            break;
            case SpawnState.Spawning:
                // chờ tạo
            break;
        }
        if (Input.GetKeyDown(KeyCode.V)){
            GameObject Clone = Instantiate(GD); // tạo
            // vị trí
            Vector3 NewRot = new Vector3(0f, Random.Range(0, 360), 0f); // hướng xoay
            // áp dụng
            Clone.transform.position = SpawnPoint[Random.Range(0, SpawnPoint.Length)].position;
            Clone.transform.eulerAngles = NewRot;
        }
        if(Input.GetKeyDown(KeyCode.T)) StartSpawnEndLess();
    }
    public void SetInstantSpawn(){
        ExpandEnemy();
        StartCoroutine(StartSpawn());
    }
    public void ExpandEnemy(){
        SpawnAmount += Random.Range(MinIncrs, MaxIncrs + 1); // tăng số lượng tạo
        for (int i = 0; i < EUseStats.Length; i++) EUseStats[i].StatAdjust(StatsScale); // tăng tỉ lệ sức mạnh
    }
    public void EntityAmountReduce(){
        CurEnemy--;
        if (PlayerPrefs.GetInt("CurMode") == 2) Invoke(nameof(StartSpawnEndLess), 2f);
    }
    IEnumerator EntityChecking(){ // kiểm tra giới hạn loại quái
        if (EntityTypeMax < EntityList.Length){
            while (WaveTillNewType > 0) yield return null;
            EntityTypeMax++; // tăng giới hạn quái tạo ra
            WaveTillNewType = Random.Range(2,4); // đặt lại số đợt mới
            StartCoroutine(EntityChecking()); // chạy lại nếu chưa đủ
        }
    }
    IEnumerator DashCheck(){
        DashText.enabled = false;
        yield return new WaitWhile(() => CurWave < 10);
        FindObjectOfType<PlayerHealthSystem>().DashAble = true;
        DashText.enabled = true;
        yield return new WaitForSeconds(5f);
        Destroy(DashText);
    }
    IEnumerator GearChecking(){
        if (!playerCombatSystem.WeaponsAllowed[0] || !playerCombatSystem.WeaponsAllowed[1] || !playerCombatSystem.WeaponsAllowed[2] || !playerCombatSystem.WeaponsAllowed[3]){
            while(WaveTillNewTool > 0) yield return null;
            int A = BaseStats.entityStats.Length - 1;
            int E = Random.Range(0, BaseStats.entityStats[A].itemDrop.Length); // trang bị rơi ra
            Vector3 DropPos = playerCombatSystem.transform.position + new Vector3(0, 3f, 0);
            GameObject Clone = Instantiate(BaseStats.entityStats[A].itemDrop[E].Item);
            Clone.transform.position = DropPos;
            Clone.transform.rotation = Random.rotation;
            ItemsInteract itemsInteract = Clone.GetComponent<ItemsInteract>();
            if (PlayerPrefs.GetInt("CL") == 0)
                itemsInteract.SetItemInfo(BaseStats.entityStats[A].itemDrop[E].VItemName, 1);
            else itemsInteract.SetItemInfo(BaseStats.entityStats[A].itemDrop[E].EItemName, 1);
            WaveTillNewTool = Random.Range(2,4); // đặt lại số đợt mới
            StartCoroutine(GearChecking()); // chạy lại nếu chưa đủ
        }
    }
    // lệnh tạo quái theo thời gian
    IEnumerator StartSpawn(){
        WaveDelayedText.enabled = false;
        yield return new WaitForSeconds(Random.Range(3f,5f)); // đợi
        CurWave++; // tăng đợt hiện tại
        if (PlayerPrefs.GetInt("CL") == 0) CurWaveText.text = "Đợt " + CurWave;
        else CurWaveText.text = "Wave " + CurWave;
        // đếm thời gian ngược lại
        float Elaps = WaveDelay;
        WaveDelayedText.enabled = true;
        while(Elaps > 0){
            Elaps -= Time.deltaTime;
            WaveDelayedText.text = ((int)Elaps).ToString();
            yield return null;
        }
        WaveDelayedText.enabled = false;
        if(PlayerPrefs.GetInt("CurMode") == 1){
            achiveIGTrack.AchivePorgress(10, false, CurWave);
            achiveIGTrack.AchivePorgress(11, false, CurWave);
            achiveIGTrack.AchivePorgress(12, false, CurWave);
            achiveIGTrack.AchivePorgress(13, false, CurWave);
        }
        // bắt đầu tạo
        int SpawnLeft = 0; // lấy số lượng mới
        while (SpawnLeft < SpawnAmount){
            SpawnLeft++; // thay số ảo
            CurEnemy++; // tăng số lượng để hiển thị và thay đổi
            // tạo
            int IndextType = Random.Range(0, EntityTypeMax); // lấy dạng quái
            int IndextList = Random.Range(0, EntityList[IndextType].EntityList.Length); // lấy loại quái
            GameObject Clone = Instantiate(EntityList[IndextType].EntityList[IndextList]); // tạo
            // vị trí
            Vector3 NewPos = new Vector3(Random.Range(-SpawnRadious, SpawnRadious), 0f, Random.Range(-SpawnRadious, SpawnRadious));
            Vector3 NewRot = new Vector3(0f, Random.Range(0, 360), 0f); // hướng xoay
            // áp dụng
            Clone.GetComponent<NavMeshAgent>().Warp(SpawnPoint[Random.Range(0, SpawnPoint.Length)].position + NewPos);
            Clone.transform.eulerAngles = NewRot;
            // kiểm tra trang bị của người chơi
            yield return new WaitForSeconds(Random.Range(SpawnDelay * 0.8f, SpawnDelay * 1.2f)); // đợi
        }
        WaveTillNewTool--;
        WaveTillNewType--;
        if (SpawnDelay > 0.55f) SpawnDelay -= 0.05f;
        if (playerCombatSystem.SkillCD > 10.25f) playerCombatSystem.SkillCD -= 0.25f;
        if (playerCombatSystem.PokeCD > 1.85f) playerCombatSystem.PokeCD -= 0.05f;
        state = SpawnState.Checking; // đổi trạng thái
    }
    // chỉ áp dụng cho T Field
    public void SpawnEndless(int AddAmount){
        SpawnAmount += AddAmount;
        Invoke(nameof(StartSpawnEndLess), 2f);
    }
    void StartSpawnEndLess(){
        if (CurEnemy < SpawnAmount){
            CurEnemy++;
            // tạo
            int IndextType = Random.Range(0, EntityList.Length); // lấy dạng quái
            int IndextList = Random.Range(0, EntityList[IndextType].EntityList.Length); // lấy loại quái
            GameObject Clone = Instantiate(EntityList[IndextType].EntityList[IndextList]); // tạo
            // vị trí
            Vector3 NewPos = new Vector3(Random.Range(-SpawnRadious, SpawnRadious), 0f, Random.Range(-SpawnRadious, SpawnRadious));
            Vector3 NewRot = new Vector3(0f, Random.Range(0, 360), 0f); // hướng xoay
            // áp dụng
            Clone.GetComponent<NavMeshAgent>().Warp(SpawnPoint[Random.Range(0, SpawnPoint.Length)].position + NewPos);
            Clone.transform.eulerAngles = NewRot;
            Invoke("StartSpawnEndLess", 5f);
        }
    }
    void OnDrawGizmos() { // tầm đánh
        if (DrawSpawnRange){
            Gizmos.color = Color.green;
            for (int i = 0; i < SpawnPoint.Length; i++) Gizmos.DrawWireSphere(SpawnPoint[i].position, SpawnRadious); // hiện tầm
        }
    }
    [System.Serializable] public class EntityType { // danh sách quái
        public string Name; // tên loại quái
        public GameObject[] EntityList; // danh sách quái
    }
}