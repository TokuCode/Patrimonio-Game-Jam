using Movement3D.Gameplay;
using UnityEngine;

//This script requires you to have setup your animator with 3 parameters, "InputMagnitude", "InputX", "InputZ"
//With a blend tree to control the inputmagnitude and allow blending between animations.
public class MovementInput : MonoBehaviour {

	public Animator anim;
	public float Speed;
	public float allowPlayerRotation = 0.1f;

    [Header("Animation Smoothing")]
    [Range(0,1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;
	
	void Update () {
		InputMagnitude ();
    }

	void InputMagnitude() {
		Speed = InputReader.Instance.MoveDirection.magnitude;

		if (Speed > allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StartAnimTime, Time.deltaTime);
		} else if (Speed < allowPlayerRotation) {
			anim.SetFloat ("Blend", Speed, StopAnimTime, Time.deltaTime);
		}
	}
}
