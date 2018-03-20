using UnityEngine;
using UnityEngine.UI;

public class SelfDestruct : MonoBehaviour
{
	public float MaxEloAika;
	public float MinEloAika;

	private Text m_text;

	private void Start()
	{
		Destroy(gameObject, Random.Range(MinEloAika, MaxEloAika));

		m_text = GetComponent<Text>();
	}

	private void Update()
	{
		if (m_text == null)
			return;

		var textColor = m_text.color;
		textColor.a  = Mathf.SmoothStep(textColor.a, 0f, 7f * Time.deltaTime);
		m_text.color = textColor;
	}
}