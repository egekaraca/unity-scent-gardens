using UnityEngine;

public class NPCGreeting : MonoBehaviour
{
    public Animator npcAnimator;     // NPC Animator
    public AudioSource greetingAudio; // Ses dosyas� (konu�ma sesi)
    public string talkBoolName = "isTalking"; // Animator parametresi
    private bool hasGreeted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasGreeted && other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger - Greeting start");

            // 1. Wave ba�lat
            npcAnimator.SetTrigger("greetTrigger");

            // 2. Ses �almay� planla
            Invoke(nameof(StartTalking), GetWaveClipLength());

            hasGreeted = true;
        }
    }

    void StartTalking()
    {
        if (greetingAudio != null)
        {
            // 3. Talk animasyonu ba�lat
            npcAnimator.SetBool(talkBoolName, true);

            // 4. Ses �al
            greetingAudio.Play();

            // 5. Ses bitince Idle'a d�n
            Invoke(nameof(StopTalking), greetingAudio.clip.length);
        }
    }

    void StopTalking()
    {
        npcAnimator.SetBool(talkBoolName, false);
    }

    // Animator i�indeki Wave klibinin s�resini bulma
    float GetWaveClipLength()
    {
        foreach (var clip in npcAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.ToLower().Contains("wave"))
            {
                return clip.length;
            }
        }
        return 0f;
    }
}
