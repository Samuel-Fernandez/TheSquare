using System.Collections;
using UnityEngine;

public enum MyliaArmSide { Left, Right, Both }

// Controle les deux bras de Mylia phase 2 independamment (rotation cible + vitesse angulaire).
// L'epaule (point d'ancrage au corps) reste toujours au meme endroit : chaque bras tourne sur son
// propre pivot local (transform.localRotation), sa position locale ne change jamais. Pour que ca
// tienne visuellement, le sprite de chaque bras doit avoir son Pivot (import settings du sprite,
// mode Custom) place exactement sur le point d'attache a l'epaule, et non au centre du sprite.
public class MyliaPhase2Arms : MonoBehaviour
{
    [Header("Arm References")]
    public Transform leftArm;
    public Transform rightArm;

    [Header("Test Sequence (Start)")]
    public bool playTestSequenceOnStart = true;
    public float testRotationSpeed = 90f;

    float leftDefaultAngle;
    float rightDefaultAngle;
    float leftCurrentAngle;
    float rightCurrentAngle;

    Coroutine leftRoutine;
    Coroutine rightRoutine;

    private void Start()
    {
        if (leftArm != null)
        {
            leftDefaultAngle = leftArm.localEulerAngles.z;
            leftCurrentAngle = leftDefaultAngle;
        }

        if (rightArm != null)
        {
            rightDefaultAngle = rightArm.localEulerAngles.z;
            rightCurrentAngle = rightDefaultAngle;
        }

        if (playTestSequenceOnStart)
        {
            StartCoroutine(TestSequenceRoutine());
        }
    }

    // Sequence de demo : quelques rotations a differentes vitesses puis retour a la position de
    // depart via Reset, pour valider visuellement le pivot des bras dans l'editeur.
    IEnumerator TestSequenceRoutine()
    {
        yield return new WaitForSeconds(1f);

        SetArmRotation(MyliaArmSide.Left, -45f, testRotationSpeed);
        SetArmRotation(MyliaArmSide.Right, 45f, testRotationSpeed);
        yield return new WaitForSeconds(2f);

        SetArmRotation(MyliaArmSide.Left, 90f, testRotationSpeed * 2f);
        yield return new WaitForSeconds(1f);

        SetArmRotation(MyliaArmSide.Right, -90f, testRotationSpeed * 0.5f);
        yield return new WaitForSeconds(2f);

        SetArmRotation(MyliaArmSide.Both, 180f, testRotationSpeed);
        yield return new WaitForSeconds(2f);

        ResetArms(testRotationSpeed);
    }

    // Fait tourner le(s) bras vers l'angle donne (degres, sens trigonometrique, 0 = rotation d'origine).
    // speedDegPerSec <= 0 applique la rotation instantanement ; sinon anime a cette vitesse angulaire.
    public void SetArmRotation(MyliaArmSide side, float angleDeg, float speedDegPerSec = 0f)
    {
        if (side == MyliaArmSide.Both)
        {
            SetArmRotation(MyliaArmSide.Left, angleDeg, speedDegPerSec);
            SetArmRotation(MyliaArmSide.Right, angleDeg, speedDegPerSec);
            return;
        }

        if (side == MyliaArmSide.Left)
        {
            ApplyRotation(leftArm, angleDeg, speedDegPerSec, ref leftRoutine, ref leftCurrentAngle);
        }
        else
        {
            ApplyRotation(rightArm, angleDeg, speedDegPerSec, ref rightRoutine, ref rightCurrentAngle);
        }
    }

    // Remet le(s) bras a leur rotation d'origine (celle lue au Start, sur le transform en scene).
    public void ResetArms(float speedDegPerSec = 0f)
    {
        SetArmRotation(MyliaArmSide.Left, leftDefaultAngle, speedDegPerSec);
        SetArmRotation(MyliaArmSide.Right, rightDefaultAngle, speedDegPerSec);
    }

    void ApplyRotation(Transform arm, float targetAngle, float speedDegPerSec, ref Coroutine routine, ref float currentAngle)
    {
        if (arm == null) return;

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (speedDegPerSec <= 0f)
        {
            currentAngle = targetAngle;
            SetLocalZ(arm, currentAngle);
            return;
        }

        routine = StartCoroutine(RotateRoutine(arm, targetAngle, speedDegPerSec, currentAngle, side: arm == leftArm));
    }

    // "side" ne sert qu'a savoir dans quel champ (leftCurrentAngle/rightCurrentAngle) ecrire l'angle
    // courant a chaque frame, pour que ResetArms/une nouvelle rotation reparte de la bonne valeur.
    IEnumerator RotateRoutine(Transform arm, float targetAngle, float speedDegPerSec, float startAngle, bool side)
    {
        float current = startAngle;

        while (!Mathf.Approximately(current, targetAngle))
        {
            current = Mathf.MoveTowardsAngle(current, targetAngle, speedDegPerSec * Time.deltaTime);
            SetLocalZ(arm, current);

            if (side) leftCurrentAngle = current; else rightCurrentAngle = current;

            yield return null;
        }

        SetLocalZ(arm, targetAngle);
        if (side) leftCurrentAngle = targetAngle; else rightCurrentAngle = targetAngle;

        if (side) leftRoutine = null; else rightRoutine = null;
    }

    void SetLocalZ(Transform arm, float angleDeg)
    {
        Vector3 euler = arm.localEulerAngles;
        euler.z = angleDeg;
        arm.localEulerAngles = euler;
    }
}
