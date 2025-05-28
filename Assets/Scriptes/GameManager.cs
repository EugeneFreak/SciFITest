using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
	[Header("Game Settings")]
	public GameObject dronePrefab;
	public GameObject resourcePrefab;
	public GameObject blueBasePrefab;
	public GameObject redBasePrefab;

	[Header("UI Elements")]
	public Slider droneCountSlider;
	public Slider droneSpeedSlider;
	public InputField resourceSpawnRateInput;
	public Toggle showPathToggle;
	public Text blueScoreText;
	public Text redScoreText;
	public Text droneCountText;
	public Text droneSpeedText;

	[Header("Game Parameters")]
	public float mapWidth = 20f;
	public float mapHeight = 12f;
	public float resourceSpawnRate = 3f;
	public int maxDronesPerTeam = 5;
	public float droneSpeed = 5f;

	private List<DroneController> blueDrones = new List<DroneController>();
	private List<DroneController> redDrones = new List<DroneController>();
	private List<GameObject> resources = new List<GameObject>();

	private GameObject blueBase;
	private GameObject redBase;

	private int blueScore = 0;
	private int redScore = 0;

	private Camera mainCamera;

	void Start()
	{
		mainCamera = Camera.main;
		SetupCamera();
		CreateBases();
		SetupUI(); 
		CreateInitialDrones();

		for (int i = 0; i < 3; i++)
		{
			SpawnResource();
		}

		StartCoroutine(SpawnResources());

		Debug.Log("Game started successfully");
	}

	void SetupCamera()
	{
		mainCamera.transform.position = new Vector3(0, 0, -10);
		mainCamera.orthographic = true;
		mainCamera.orthographicSize = mapHeight / 2f + 2f;
	}

	void CreateBases()
	{
		blueBase = Instantiate(blueBasePrefab, new Vector3(-8f, 0, 0), Quaternion.identity);
		blueBase.GetComponent<SpriteRenderer>().color = Color.blue;
		blueBase.name = "BlueBase";

		redBase = Instantiate(redBasePrefab, new Vector3(8f, 0, 0), Quaternion.identity);
		redBase.GetComponent<SpriteRenderer>().color = Color.red;
		redBase.name = "RedBase";
	}

	void CreateInitialDrones()
	{
		int dronesPerTeam = Mathf.RoundToInt(droneCountSlider.value);
		CreateDrones(dronesPerTeam);
	}

	void CreateDrones(int count)
	{
		count = Mathf.Clamp(count, 1, 5);

		foreach (var drone in blueDrones)
			if (drone != null) DestroyImmediate(drone.gameObject);
		foreach (var drone in redDrones)
			if (drone != null) DestroyImmediate(drone.gameObject);

		blueDrones.Clear();
		redDrones.Clear();

		for (int i = 0; i < count; i++)
		{
			Vector3 spawnPos = new Vector3(-8f + Random.Range(-1f, 1f), Random.Range(-2f, 2f), 0);
			GameObject drone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
			DroneController controller = drone.GetComponent<DroneController>();
			controller.Initialize(TeamType.Blue, blueBase.transform, this);
			controller.SetSpeed(droneSpeed);
			controller.ShowPath(showPathToggle.isOn);
			drone.GetComponent<SpriteRenderer>().color = Color.cyan;
			drone.name = "BlueDrone_" + i;
			blueDrones.Add(controller);
		}

		for (int i = 0; i < count; i++)
		{
			Vector3 spawnPos = new Vector3(8f + Random.Range(-1f, 1f), Random.Range(-2f, 2f), 0);
			GameObject drone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
			DroneController controller = drone.GetComponent<DroneController>();
			controller.Initialize(TeamType.Red, redBase.transform, this);
			controller.SetSpeed(droneSpeed);
			controller.ShowPath(showPathToggle.isOn);
			drone.GetComponent<SpriteRenderer>().color = Color.red;
			drone.name = "RedDrone_" + i;
			redDrones.Add(controller);
		}

		Debug.Log($"Created {count} blue drones and {count} red drones");
	}

	void SetupUI()
	{
		droneCountSlider.maxValue = 5;
		droneCountSlider.minValue = 1;
		droneCountSlider.value = 3;

		droneSpeedSlider.minValue = 1f;
		droneSpeedSlider.maxValue = 10f;
		droneSpeedSlider.value = droneSpeed; 

		if (resourceSpawnRateInput != null)
		{
			resourceSpawnRateInput.text = resourceSpawnRate.ToString();
		}

		droneCountSlider.onValueChanged.AddListener(OnDroneCountChanged);
		droneSpeedSlider.onValueChanged.AddListener(OnDroneSpeedChanged);
		if (resourceSpawnRateInput != null)
		{
			resourceSpawnRateInput.onEndEdit.AddListener(OnResourceSpawnRateChanged);
		}
		showPathToggle.onValueChanged.AddListener(OnShowPathToggled);

		UpdateUI();
	}

	void OnDroneCountChanged(float value)
	{
		CreateDrones(Mathf.RoundToInt(value));
		UpdateUI();
	}

	void OnDroneSpeedChanged(float value)
	{
		droneSpeed = value;
		foreach (var drone in blueDrones)
			if (drone != null) drone.SetSpeed(droneSpeed);
		foreach (var drone in redDrones)
			if (drone != null) drone.SetSpeed(droneSpeed);
		UpdateUI();
	}

	void OnResourceSpawnRateChanged(string value)
	{
		if (float.TryParse(value, out float newRate))
		{
			resourceSpawnRate = Mathf.Clamp(newRate, 0.5f, 10f);

			if (resourceSpawnRateInput != null)
			{
				resourceSpawnRateInput.text = resourceSpawnRate.ToString();
			}
			Debug.Log($"Resource spawn rate changed to: {resourceSpawnRate}");
		}
		else
		{
			if (resourceSpawnRateInput != null)
			{
				resourceSpawnRateInput.text = resourceSpawnRate.ToString();
			}
		}
	}

	void OnShowPathToggled(bool show)
	{
		foreach (var drone in blueDrones)
			if (drone != null) drone.ShowPath(show);
		foreach (var drone in redDrones)
			if (drone != null) drone.ShowPath(show);
	}

	void UpdateUI()
	{
		droneCountText.text = $"Дронов: {Mathf.RoundToInt(droneCountSlider.value)}";
		droneSpeedText.text = $"Скорость: {droneSpeed:F1}";
		blueScoreText.text = $"Синие: {blueScore}";
		redScoreText.text = $"Красные: {redScore}";
	}

	void Update()
	{
		if (Time.time % 2f < Time.deltaTime)
		{
			Debug.Log($"Resources on map: {resources.Count}, Blue drones: {blueDrones.Count}, Red drones: {redDrones.Count}");
		}
	}

	IEnumerator SpawnResources()
	{
		while (true)
		{
			yield return new WaitForSeconds(resourceSpawnRate);

			if (resources.Count < 15)
			{
				SpawnResource();
			}
		}
	}

	void SpawnResource()
	{
		Vector3 spawnPos;
		int attempts = 0;

		do
		{
			spawnPos = new Vector3(
				Random.Range(-6f, 6f),
				Random.Range(-4f, 4f),
				0
			);
			attempts++;
		} while (attempts < 10 && (Vector3.Distance(spawnPos, blueBase.transform.position) < 2f ||
								   Vector3.Distance(spawnPos, redBase.transform.position) < 2f));

		GameObject resource = Instantiate(resourcePrefab, spawnPos, Quaternion.identity);
		if (resource.GetComponent<SpriteRenderer>() != null)
		{
			resource.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
		resource.name = "Resource_" + resources.Count;
		resources.Add(resource);

		Debug.Log($"Spawned resource at {spawnPos}, total resources: {resources.Count}");
	}

	public GameObject FindNearestResource(Vector3 position, List<GameObject> excludeList = null)
	{
		GameObject nearest = null;
		float nearestDistance = float.MaxValue;

		resources.RemoveAll(r => r == null);

		foreach (var resource in resources)
		{
			if (resource == null) continue;
			if (excludeList != null && excludeList.Contains(resource)) continue;

			float distance = Vector3.Distance(position, resource.transform.position);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = resource;
			}
		}

		Debug.Log($"Found nearest resource: {(nearest != null ? nearest.name : "null")}, available resources: {resources.Count}");
		return nearest;
	}

	public void CollectResource(GameObject resource, TeamType team)
	{
		if (resources.Contains(resource))
		{
			resources.Remove(resource);
			Destroy(resource);

			if (team == TeamType.Blue)
				blueScore++;
			else
				redScore++;

			UpdateUI();
		}
	}

	public List<DroneController> GetAllDrones()
	{
		List<DroneController> allDrones = new List<DroneController>();
		allDrones.AddRange(blueDrones);
		allDrones.AddRange(redDrones);
		return allDrones;
	}
}