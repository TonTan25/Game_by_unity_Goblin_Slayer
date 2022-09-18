using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TestFieldControl : MonoBehaviour {
    // chỉ thử nghiệm
    // set name to Test field Control
    [SerializeField] TextChange[] StartText, EndP1Text, LiveText, DieText, Die7Text, Die9Text, EndP2Text;
    [SerializeField] Text DisplayText, DisplayRoll;
    [SerializeField] Animator SceneAnimated;
    [SerializeField] GameObject GrimDrake;
    [SerializeField] RandomSpawnSet1 FieldSpawn;
    [SerializeField] AchiveIGTrack achiveIGTrack;
    [SerializeField] bool StageCompletted;
    [SerializeField] Color Normal, Live, Die, Col7, Col9;
    [SerializeField] Transform PlayerPosition;
    GameControl gameControl;
    SoundManager sounds;
    int HeadCount = 0;
    void Start(){
        FieldSpawn.CallSpawn();
        sounds = GetComponent<SoundManager>();
        PlayerPosition = FindObjectOfType<PlayerHealthSystem>().transform;
        gameControl = GetComponent<GameControl>();
        // if (PlayerPrefs.GetInt("CurMode") == 2){
        //     StageCompletted = false;
        //     SceneAnimated.SetTrigger("Blank");
        //     SceneAnimated.SetTrigger("FadeOutBlank");
        //     DisplayRoll.enabled = false;
        //     if(PlayerPrefs.GetInt("CL") == 0) DisplayText.text = "nhảy xuống để bắt đầu";
        //     else DisplayText.text = "Jump down to begin";
        //     StartCoroutine(GameProgress());
        // }
    }
    void Update(){
        if (Input.GetKeyDown(KeyCode.V)){
            StartCoroutine(SpawnGrim(1, 1f));
        }
    }
    IEnumerator SpawnGrim(int Amount, float Waitspawn){
        HeadCount = Amount;
        for (int i = 0; i < HeadCount; i++){
            GameObject Clone = Instantiate(GrimDrake); // tạo
            // vị trí
            Vector3 NewRot = new Vector3(0f, Random.Range(0, 360), 0f); // hướng xoay
            // áp dụng
            Clone.transform.position = PlayerPosition.position;
            Clone.transform.eulerAngles = NewRot;
            yield return new WaitForSeconds(Waitspawn);
        }
    }
    public void GrimDie(){
        HeadCount--;
    }
    IEnumerator GameProgress(){
        while (PlayerPosition.position.y > 25f) yield return null;
        DisplayText.text = "";
        yield return new WaitForSeconds(4f); // waiting
        int CurLine = 0; // set line
        while (CurLine < StartText.Length){
            if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = StartText[CurLine].VLine;
            else DisplayText.text = StartText[CurLine].ELine;
            sounds.Play("Text");
            yield return new WaitForSeconds(StartText[CurLine].WaitTime);
        }
        yield return new WaitForSeconds(4f);
        StartCoroutine(SpawnGrim(1, 2f));
        StageCompletted = false;
        while (!StageCompletted) yield return null;
        yield return new WaitForSeconds(4f);
        if (!achiveIGTrack.AchiveCompletedState[18]){ // check achivement
            achiveIGTrack.AchivePorgress(18, false, 1);
            yield return new WaitForSeconds(6f);
        }
        CurLine = 0; // set line
        while (CurLine < EndP1Text.Length){
            if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = EndP1Text[CurLine].VLine;
            else DisplayText.text = EndP1Text[CurLine].ELine;
            sounds.Play("Text");
            yield return new WaitForSeconds(EndP1Text[CurLine].WaitTime);
        }
        float Elaps = 0f;
        int RollNumber = 0;
        DisplayRoll.enabled = true;
        while(Elaps < 5f){
            Elaps += Time.deltaTime;
            RollNumber = Random.Range(0,101);
            DisplayRoll.text = RollNumber.ToString();
            yield return new WaitForSeconds(0.02f);
        }
// live - die chance
        if (RollNumber > 49){
            DisplayRoll.color = Live;
            DisplayRoll.text = "Game Set";
            yield return new WaitForSeconds(3f);
            DisplayRoll.enabled = false;
            CurLine = 0; // set line
            while (CurLine < LiveText.Length){
                if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = LiveText[CurLine].VLine;
                else DisplayText.text = LiveText[CurLine].ELine;
                sounds.Play("Text");
                yield return new WaitForSeconds(LiveText[CurLine].WaitTime);
            }
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(0);
        } else {
            StageCompletted = false;
            DisplayRoll.color = Die;
            DisplayRoll.text = "Hydra Game";
            yield return new WaitForSeconds(4f);
            CurLine = 0; // set line
            while (CurLine < DieText.Length){
                if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = DieText[CurLine].VLine;
                else DisplayText.text = DieText[CurLine].ELine;
                sounds.Play("Text");
                yield return new WaitForSeconds(DieText[CurLine].WaitTime);
            }
            // head count
            DisplayRoll.color = Normal;
            Elaps = 0f;
            while(Elaps < 5f){
                Elaps += Time.deltaTime;
                RollNumber = Random.Range(0,11);
                DisplayRoll.text = RollNumber.ToString();
                yield return new WaitForSeconds(0.02f);
            }
            if (RollNumber > 5){ // 7 head
                DisplayRoll.color = Col7;
                CurLine = 0; // set line
                while (CurLine < Die7Text.Length){
                    if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = Die7Text[CurLine].VLine;
                    else DisplayText.text = Die7Text[CurLine].ELine;
                    sounds.Play("Text");
                    yield return new WaitForSeconds(Die7Text[CurLine].WaitTime);
                }
                StartCoroutine(SpawnGrim(7, 3f));
            } else { // 9 head
                DisplayRoll.color = Col9;
                CurLine = 0; // set line
                while (CurLine < Die9Text.Length){
                    if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = Die9Text[CurLine].VLine;
                    else DisplayText.text = Die9Text[CurLine].ELine;
                    sounds.Play("Text");
                    yield return new WaitForSeconds(Die9Text[CurLine].WaitTime);
                }
                StartCoroutine(SpawnGrim(9, 5f));
            }
            DisplayRoll.enabled = false;
            while (!StageCompletted) yield return null;
            CurLine = 0; // set line
            while (CurLine < EndP2Text.Length){
                if (PlayerPrefs.GetInt("CL") == 0) DisplayText.text = EndP2Text[CurLine].VLine;
                else DisplayText.text = EndP2Text[CurLine].ELine;
                sounds.Play("Text");
                yield return new WaitForSeconds(EndP2Text[CurLine].WaitTime);
            }
            if (!achiveIGTrack.AchiveCompletedState[19]){ // check achivement
                achiveIGTrack.AchivePorgress(19, false, 1);
                yield return new WaitForSeconds(6f);
            }
            gameControl.PlayerControling(false);
            SceneAnimated.SetTrigger("FadeInBlank");
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene(0);
        }
    }
    [System.Serializable] public class TextChange{
        public string VLine, ELine;
        public float WaitTime;
    }
}