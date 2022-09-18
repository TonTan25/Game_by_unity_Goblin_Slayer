using UnityEngine;
using System;
public class AchivementInsert : MonoBehaviour {
    // gắn vào một empty có component "vertical layout group", "content size fitter"
    // load dữ liệu từ AchivementData
    // hiển thị các thành tựu trên màn
    [SerializeField] AchivementBaseInfo achivementBase; // thông tin của thành tựu
    [SerializeField] GameObject AchiveContainer, AchiveListContainer; //
    //            | chứa mục thành tựu trống   |   sẽ thả những thành tựu vào đây|
    public GameObject[] AchiveList; // lấy danh sách các thành tựu
    public void RefreshList(){ // xóa toàn bộ danh sách thành tựu
        for (int i = 0; i < AchiveList.Length; i++){
            AchiveList[i].GetComponent<AchiveChange>().RefreshAchive(i);
        }
    }
    public void CreateList(){
        Array.Resize(ref AchiveList, achivementBase.achivements.Length); // sửa danh sách thành tựu
        for (int i = 0; i < achivementBase.achivements.Length; i++){
            AchiveList[i] = Instantiate(AchiveContainer, AchiveListContainer.transform); // đặt ra (đặt làm con của gameObject)
            AchiveList[i].GetComponent<AchiveChange>().AchiveImg.sprite = achivementBase.achivements[i].AchiveIcon;
            AchiveList[i].GetComponent<AchiveChange>().RefreshAchive(i);
        }
        TestAchiveCheck();
    }
    void TestAchiveCheck(){
        int AchiveProgs = 0;
        for (int i = 0; i < achivementBase.achivements.Length; i++){
            if (PlayerPrefs.GetInt("Achivement " + i) >= achivementBase.achivements.Length) AchiveProgs++;
        }
        if (AchiveProgs >= 15){
            FindObjectOfType<MainMenuControl>().TestFieldBut.SetActive(true);
            AchiveList[18].SetActive(true);
            AchiveList[19].SetActive(true);
        } else {
            FindObjectOfType<MainMenuControl>().TestFieldBut.SetActive(false);
            AchiveList[18].SetActive(false);
            AchiveList[19].SetActive(false);
        }
    }
}