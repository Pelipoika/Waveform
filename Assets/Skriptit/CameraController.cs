using UnityEngine;
using UnityEngine.Assertions;

//Pixel perfect koodi varastettu
//https://github.com/cmilr/DeadSimple-Pixel-Perfect-Camera/blob/master/Assets/DSPixelPerfectCamera.cs
//Vähän vaan siivosin sitä

public class CameraController : MonoBehaviour
{
	// ReSharper disable once InconsistentNaming
	[SerializeField] private int pixelsPerUnit = 16;

	// ReSharper disable once InconsistentNaming
	[SerializeField] private int verticalUnitsOnScreen = 4;

	private float  m_finalUnitSize;
	private Camera m_camera;

	private void Awake()
	{
		m_camera = gameObject.GetComponent<Camera>();
		Assert.IsNotNull(m_camera);

		SetOrthographicSize();
	}

	private void SetOrthographicSize()
	{
		ValidateUserInput();

		// get device's screen height and divide by the number of units 
		// that we want to fit on the screen vertically. this gets us
		// the basic size of a unit on the the current device's screen.
		var tempUnitSize = Screen.height / verticalUnitsOnScreen;

		// with a basic rough unit size in-hand, we now round it to the
		// nearest power of pixelsPerUnit (ex; 16px.) this will guarantee
		// our sprites are pixel perfect, as they can now be evenly divided
		// into our final device's screen height.
		m_finalUnitSize = GetNearestMultiple(tempUnitSize, pixelsPerUnit);

		// ultimately, we are using the standard pixel art formula for 
		// orthographic cameras, but approaching it from the view of:
		// how many standard Unity units do we want to fit on the screen?
		// formula: cameraSize = ScreenHeight / (DesiredSizeOfUnit * 2)
		m_camera.orthographicSize = Screen.height / (m_finalUnitSize * 2.0f);
	}

	private static int GetNearestMultiple(int value, int multiple)
	{
		int rem    = value % multiple;
		int result = value - rem;
		if (rem > (multiple / 2))
			result += multiple;

		return result;
	}

	private void ValidateUserInput()
	{
		if (pixelsPerUnit == 0)
		{
			pixelsPerUnit = 1;
			Debug.Log("Warning: Pixels-per-unit must be greater than zero. " +
			          "Resetting to minimum allowed.");
		}
		else if (verticalUnitsOnScreen == 0)
		{
			verticalUnitsOnScreen = 1;
			Debug.Log("Warning: Units-on-screen must be greater than zero." +
			          "Resetting to minimum allowed.");
		}
	}

	//Mun koodit alhaalla
	public Vector2 CameraOffset = new Vector2(0.0f, 2.0f);

	public Vector3   CameraPosition = Vector3.zero;
	public Transform Target;
	public float     SmootAmount = 0.3f;

	//Kameraan vaikuttamassa olevat ulkoiset/extra voimat
	private float   m_force;
	private Vector3 m_forceDirection;
	public  float   CameraKickSmoothAmount = 1.0f;

	private void FixedUpdate()
	{
		CameraPosition = new Vector3(
			Mathf.SmoothStep(transform.position.x, Target.transform.position.x + CameraOffset.x, SmootAmount),
			Mathf.SmoothStep(transform.position.y, Target.transform.position.y + CameraOffset.y, SmootAmount)
		);

		m_forceDirection.x = Mathf.MoveTowards(m_forceDirection.x, 0f, CameraKickSmoothAmount);
		m_forceDirection.y = Mathf.MoveTowards(m_forceDirection.y, 0f, CameraKickSmoothAmount);

		CameraPosition += m_forceDirection * m_force;
	}

	public void AddForce(Vector3 direction, float amount)
	{
		m_forceDirection = direction;
		m_force          = amount;
	}

	private void LateUpdate()
	{
		transform.position = CameraPosition + Vector3.forward * -10;
	}
}