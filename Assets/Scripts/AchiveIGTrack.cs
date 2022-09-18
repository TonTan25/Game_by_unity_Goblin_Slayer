using UnityEngine;
using System;
public class AchiveIGTrack : MonoBehaviour {
// dùng trong khi chạy game để theo dõi tiến trình
    [SerializeField] AchivementBaseInfo AchiveBaseData;
    [SerializeField] AchiveIngameGet achiveIngameGet;
    [SerializeField] int AchiveAdd;
    [SerializeField] int[] AchiveIndextProgress; // tiến trình thành tựu
    public bool[] AchiveCompletedState; // điều kiện xác nhận
    SoundManager sounds;
    void Awake(){
        Array.Resize(ref AchiveIndextProgress, AchiveBaseData.achivements.Length);
        Array.Resize(ref AchiveCompletedState, AchiveBaseData.achivements.Length);
        // kiểm tra thành tựu
        for (int i = 0; i < AchiveBaseData.achivements.Length; i++){
            if (PlayerPrefs.GetInt("Achivement " + i) >= AchiveBaseData.achivements[i].AchiveTotalProgress)
                AchiveCompletedState[i] = true; // xác nhận đã hoàn thành thành tựu
            else AchiveCompletedState[i] = false;
        }
        sounds = GetComponent<SoundManager>();
    }
    public void AchivePorgress(int AchiveIndext, bool IsIncrement, int AchiveSetAmount){ // phát triển thành tựu nếu chưa hoàn thành
        if(!AchiveCompletedState[AchiveIndext]){
            if (IsIncrement) AchiveIndextProgress[AchiveIndext] += AchiveSetAmount;
            else if(!IsIncrement && AchiveSetAmount > AchiveIndextProgress[AchiveIndext]) AchiveIndextProgress[AchiveIndext] = AchiveSetAmount;
            PlayerPrefs.SetInt("Achivement " + AchiveIndext, AchiveIndextProgress[AchiveIndext]);
            PlayerPrefs.Save();
            if (AchiveIndextProgress[AchiveIndext] >= AchiveBaseData.achivements[AchiveIndext].AchiveTotalProgress){
                if (PlayerPrefs.GetInt("CL") == 0){
                    achiveIngameGet.AchiveDisplayActive(AchiveBaseData.achivements[AchiveIndext].AchiveIcon, AchiveBaseData.achivements[AchiveIndext].AchiveNameV, AchiveBaseData.achivements[AchiveIndext].AchiveInfoV);
                } else {
                    achiveIngameGet.AchiveDisplayActive(AchiveBaseData.achivements[AchiveIndext].AchiveIcon, AchiveBaseData.achivements[AchiveIndext].AchiveNameE, AchiveBaseData.achivements[AchiveIndext].AchiveInfoE);
                }
                PlayerPrefs.SetInt("Achivement " + AchiveIndext, AchiveBaseData.achivements[AchiveIndext].AchiveTotalProgress);
                AchiveCompletedState[AchiveIndext] = true;
                sounds.Play("AchiveGet");
            }
        }
    }
}