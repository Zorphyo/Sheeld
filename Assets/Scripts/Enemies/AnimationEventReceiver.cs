using UnityEngine;

// Silences animation event warnings from imported clips
// that have baked-in foot/sound events
public class AnimationEventReceiver : MonoBehaviour
{
    // Foot step events — common in Mixamo and asset store animations
    void FootR() {}
    void FootL() {}
    void Hit()   {}
    void Land()  {}
    void Grunt() {}
}