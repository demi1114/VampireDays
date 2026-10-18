
using UnityEngine;

public class AncestralBloodSkill : MonoBehaviour
{
    private SkillManager skillManager;

    private void Awake()
    {
        skillManager = GetComponent<SkillManager>();

        if (skillManager == null)
        {
            Debug.LogError(
                "AncestralBloodSkill : “¯‚¶GameObject‚ÉSkillManager‚ª‚ ‚è‚Ü‚¹‚ñB"
            );
        }
    }

    public void Activate()
    {
        if (skillManager == null)
        {
            Debug.LogError(
                "yn‘c‚Ì‘NŒŒzSkillManager‚ªŒ©‚Â‚©‚ç‚È‚¢‚½‚ß”­“®‚Å‚«‚Ü‚¹‚ñB"
            );
            return;
        }

        Debug.Log("yn‘c‚Ì‘NŒŒzActivate() ŒÄ‚Ño‚µ");

        // Š’†‚ÌSummonEMagicƒXƒLƒ‹‚ğ‹­§”­“®‚·‚é
        skillManager.ActivateAncestralBloodEffect();
    }
}
