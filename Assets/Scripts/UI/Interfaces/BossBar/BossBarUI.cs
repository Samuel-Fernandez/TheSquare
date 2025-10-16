using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBarUI : MonoBehaviour
{
    public Scrollbar bossBar;
    public TextMeshProUGUI bossNameTxt;

    string bossName;
    int bossMaxLife;

    Vector3 originalPosition;
    Vector3 originalScale;

    private void Start()
    {
        StartCoroutine(PlayIntroAnimationRoutine());
    }

    public void InitBossBar(string idBoss, int bossMaxLife)
    {
        bossName = LocalizationManager.instance.GetText("BOSS", idBoss);
        bossNameTxt.text = bossName;

        this.bossMaxLife = bossMaxLife;

        UpdateBossLife(bossMaxLife);
    }

    public void UpdateBossLife(int life)
    {
        bossBar.size = (float)life / bossMaxLife;
    }

    private IEnumerator PlayIntroAnimationRoutine()
    {

        originalPosition = bossNameTxt.rectTransform.localPosition;
        originalScale = bossNameTxt.rectTransform.localScale;

        // Étape 1 : Préparation
        RectTransform nameRect = bossNameTxt.rectTransform;

        nameRect.localPosition = new Vector3(0f, 0f, 0f);
        nameRect.localScale = originalScale * 1.5f;

        // Active avec fade (2s)
        UIAnimator.instance.ActivateObjectWithTransition(bossNameTxt.gameObject, 2f);
        yield return new WaitForSeconds(2f);

        // Étape 2 : Retour position/échelle d'origine (0.5s)
        float t = 0f;
        Vector3 startPos = nameRect.localPosition;
        Vector3 startScale = nameRect.localScale;

        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float ratio = t / 0.5f;

            nameRect.localPosition = Vector3.Lerp(startPos, originalPosition, ratio);
            nameRect.localScale = Vector3.Lerp(startScale, originalScale, ratio);
            yield return null;
        }

        nameRect.localPosition = originalPosition;
        nameRect.localScale = originalScale;

        // Étape 3 : Apparition barre de vie (0.5s)
        UIAnimator.instance.ActivateObjectWithTransition(bossBar.gameObject, 0.5f);
    }

}
