using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class BGMPlay : MonoBehaviour{
    [SerializeField] Text MusicNames;
    [SerializeField] AudioSource MusicScource;
    public AudioClip[] BGMClip;
    enum MusicState{Standby, Repeating}
    MusicState musicState;
    void Awake(){ // kiểm tra xem chỉ có duy nhất 1 cái trong game
        musicState = MusicState.Standby;
    //  kiểm tra lặp lại | tên void | thời gian khi gọi lần đầu | lặp lại sau | (giây)
    }
    void LateUpdate(){
        switch (musicState){
            default:
            case MusicState.Standby:
                if(!MusicScource.isPlaying){
                    StartCoroutine(RepeatMusic());
                    musicState = MusicState.Repeating;
                }
            break;

            case MusicState.Repeating:
                // do nothing
            break;
        }
    }
    IEnumerator RepeatMusic(){
        yield return new WaitForSeconds(5f);
        int CurMusic = Random.Range(0, BGMClip.Length); // lấy nhạc mới 
        MusicScource.clip = BGMClip[CurMusic]; // đặt nhạc
        MusicScource.Play(); // chạy nhạc
        // đổi lại tên nhạc
        if (PlayerPrefs.GetInt("CL") == 0){
            MusicNames.text = "Nhạc nền: " + BGMClip[CurMusic].name;
        } else {
            MusicNames.text = "Music: " + BGMClip[CurMusic].name;
        }
        musicState = MusicState.Standby;
    }
}
