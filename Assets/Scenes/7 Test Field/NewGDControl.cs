using System.Collections;
using UnityEngine.AI;
using UnityEngine;
public class NewGDControl : MonoBehaviour {
    [SerializeField][Range(0.5f, 1.5f)] float AGMinScale, AGMaxScale;
    [SerializeField] float RangeRad, RangeCD, AOERad, AOECD, MeleeRad, BeamRad, JumpTimes, MinDmg, MaxDmg;
    [SerializeField] int ReRangeAtkChance;
    [SerializeField] NavMeshAgent AG;
    [SerializeField] Animator AGAnimator;
    [SerializeField] EnemyHealthSystem AGHealth;
    [SerializeField] PlayerHealthSystem Target;
    enum AGStage{ StandFirm, Phase1, Phase2 } [SerializeField] AGStage AGstate;
    float Distant;
    bool MeleeAtked, RangeAtked;
    void Start(){
        transform.localScale = Vector3.one * Random.Range(AGMinScale, AGMaxScale);
    }
    void AGStation(){
        AGstate = AGStage.StandFirm;
        AG.SetDestination(transform.position);
    }
    void LateUpdate(){
        switch (AGstate){
            default:
            case AGStage.StandFirm:
            break;
            case AGStage.Phase1:
                ActivePhase1();
            break;
            case AGStage.Phase2:
                ActivePhase2();
            break;
        }
        if(!AGHealth.Alive) Die();
    }
    void Die(){
        AGstate = AGStage.StandFirm;
        Debug.Log("Play Die");
        AG.Stop();
    }
    void ActivePhase1(){
        Distant = Vector3.Distance(transform.position, Target.transform.position);
        if (Distant < MeleeRad && !MeleeAtked){
            MeleeAtked = true;
            Debug.Log("MelAtk");
            Invoke(nameof(EVReMeleeAtk), 3f);
            AGStation();
        } else if(Distant < RangeRad && !MeleeAtked && !RangeAtked){
            MeleeAtked = true;
            RangeAtked = true;
            Debug.Log("RangeAtk");
            AGStation();
        }
        if (Distant > MeleeRad){
            AG.SetDestination(Target.transform.position);
            Debug.Log("Moving Toward");
        }
    }
    void EVReMeleeAtk(){
        MeleeAtked = false;
    }
    void EVReRangeAtk(){
        if (Random.Range(0,11) < ReRangeAtkChance){
            ReRangeAtk();
        } else Invoke(nameof(ReRangeAtk), Random.Range(RangeCD * 0.8f, RangeCD));
        MeleeAtked = false;
    }
    void ReRangeAtk(){
        RangeAtked = false;
    }
    void ActivePhase2(){

    }
}