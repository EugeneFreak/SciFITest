using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	[Header("UI References")]
	public Canvas mainCanvas;
	public GameObject controlPanel;

	void Start()
	{
		SetupUI();
	}

	void SetupUI()
	{
		CanvasScaler scaler = mainCanvas.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 0.5f;
	}
}