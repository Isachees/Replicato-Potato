using UnityEngine;

// disables the joystick slide movement of the interaction sdk
// attach it to any empty object in the scene
public class DisableSlideLocomotion : MonoBehaviour
{
    void Start()
    {
        DisableSlide();
    }

    void DisableSlide()
    {
        // include inactive objects too, just in case
        MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

        foreach (MonoBehaviour mb in all)
        {
            if (mb == null) continue;

            string typeName = mb.GetType().Name;

            // only slide components, turn and the locomotor stay untouched
            if (typeName.Contains("Slide"))
            {
                mb.enabled = false;
                Debug.Log("slide disabled: " + typeName + " on " + mb.gameObject.name);
            }
        }
    }
}