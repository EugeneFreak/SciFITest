using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TeamType { Blue, Red }
public enum DroneState { SearchingResource, MovingToResource, CollectingResource, ReturningToBase, UnloadingAtBase }

public class DroneController : MonoBehaviour
{
	[Header("Drone Settings")]
	public float speed = 5f;
	public float collectionTime = 2f;
	public float avoidanceRadius = 1f;

	private TeamType team;
	private Transform baseTransform;
	private GameManager gameManager;

	private DroneState currentState = DroneState.SearchingResource;
	private GameObject targetResource;
	private bool hasResource = false;

	private LineRenderer pathRenderer;
	private List<Vector3> currentPath = new List<Vector3>();

	private Color originalColor;
	private SpriteRenderer spriteRenderer;
	private bool isPathVisible = false;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		EnsurePathRenderer(); 
	}

	void EnsurePathRenderer()
	{
		if (pathRenderer == null)
		{
			pathRenderer = gameObject.AddComponent<LineRenderer>();

			if (pathRenderer == null)
			{
				Debug.LogError("Failed to create LineRenderer component!");
				return;
			}

			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null)
			{
				pathRenderer.material = new Material(shader);
			}
			else
			{
				Debug.LogError("Shader not found! Using default material");
				pathRenderer.material = new Material(Shader.Find("UI/Default"));
			}

			pathRenderer.startWidth = 0.2f;
			pathRenderer.endWidth = 0.2f;
			pathRenderer.enabled = false;
			pathRenderer.useWorldSpace = true;
			pathRenderer.sortingOrder = 100;
			pathRenderer.receiveShadows = false;
			pathRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		}
	}

	public void Initialize(TeamType teamType, Transform basePos, GameManager manager)
	{
		team = teamType;
		baseTransform = basePos;
		gameManager = manager;

		if (spriteRenderer == null)
			spriteRenderer = GetComponent<SpriteRenderer>();

		originalColor = team == TeamType.Blue ? Color.cyan : Color.red;
		if (spriteRenderer != null)
		{
			spriteRenderer.color = originalColor;
		}

		EnsurePathRenderer(); 

		if (pathRenderer != null && pathRenderer.material != null)
		{
			pathRenderer.material.color = originalColor;
		}
		else
		{
			Debug.LogWarning("Path renderer material not available");
		}

		Debug.Log($"{team} drone initialized at {transform.position}");
	}

	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
	}

	public void ShowPath(bool show)
	{
		isPathVisible = show;

		if (pathRenderer != null)
		{
			pathRenderer.enabled = show;
			Debug.Log($"{team} drone path visibility set to: {show}");
		}
		else
		{
			Debug.LogWarning("ShowPath called but pathRenderer is null");
		}
	}

	void Update()
	{
		switch (currentState)
		{
			case DroneState.SearchingResource:
				SearchForResource();
				break;
			case DroneState.MovingToResource:
				MoveToResource();
				break;
			case DroneState.CollectingResource:
				break;
			case DroneState.ReturningToBase:
				ReturnToBase();
				break;
			case DroneState.UnloadingAtBase:
				break;
		}

		UpdatePathVisualization();
	}

	void SearchForResource()
	{
		if (hasResource) return;

		targetResource = gameManager.FindNearestResource(transform.position);
		if (targetResource != null)
		{
			currentState = DroneState.MovingToResource;
			CalculatePath(targetResource.transform.position);
			Debug.Log($"{team} drone found resource at {targetResource.transform.position}");
		}
	}

	void MoveToResource()
	{
		if (targetResource == null)
		{
			currentState = DroneState.SearchingResource;
			if (currentPath.Count > 1) currentPath.RemoveAt(1);
			return;
		}

		Vector3 targetPos = targetResource.transform.position;
		Vector3 avoidedDirection = GetAvoidanceDirection(targetPos);

		transform.position = Vector3.MoveTowards(transform.position, transform.position + avoidedDirection, speed * Time.deltaTime);

		CalculatePath(targetPos);

		if (Vector3.Distance(transform.position, targetPos) < 0.3f)
		{
			StartCoroutine(CollectResource());
		}
	}

	Vector3 GetAvoidanceDirection(Vector3 targetDirection)
	{
		Vector3 direction = (targetDirection - transform.position).normalized;
		Vector3 avoidanceForce = Vector3.zero;

		if (gameManager != null)
		{
			List<DroneController> allDrones = gameManager.GetAllDrones();
			foreach (var otherDrone in allDrones)
			{
				if (otherDrone == this || otherDrone == null) continue;

				float distance = Vector3.Distance(transform.position, otherDrone.transform.position);
				if (distance < avoidanceRadius && distance > 0)
				{
					Vector3 avoidDirection = (transform.position - otherDrone.transform.position).normalized;
					avoidanceForce += avoidDirection * (avoidanceRadius - distance) / avoidanceRadius;
				}
			}
		}

		return (direction + avoidanceForce * 2).normalized;
	}

	IEnumerator CollectResource()
	{
		currentState = DroneState.CollectingResource;

		yield return new WaitForSeconds(collectionTime);

		if (targetResource != null && gameManager != null)
		{
			gameManager.CollectResource(targetResource, team);
			hasResource = true;
			currentState = DroneState.ReturningToBase;
			CalculatePath(baseTransform.position);
		}
		else
		{
			currentState = DroneState.SearchingResource;
		}
	}

	void ReturnToBase()
	{
		Vector3 targetPos = baseTransform.position;
		Vector3 avoidedDirection = GetAvoidanceDirection(targetPos);

		transform.position = Vector3.MoveTowards(transform.position, transform.position + avoidedDirection, speed * Time.deltaTime);

		CalculatePath(targetPos);

		if (Vector3.Distance(transform.position, targetPos) < 1f)
		{
			StartCoroutine(UnloadAtBase());
		}
	}

	IEnumerator UnloadAtBase()
	{
		currentState = DroneState.UnloadingAtBase;

		yield return new WaitForSeconds(1f);

		hasResource = false;
		currentState = DroneState.SearchingResource;
		targetResource = null;
	}

	void CalculatePath(Vector3 target)
	{
		if (currentPath.Count == 0)
		{
			currentPath.Add(transform.position);
			currentPath.Add(target);
		}
		else
		{
			if (currentPath.Count > 1)
			{
				currentPath[1] = target;
			}
			else
			{
				currentPath.Add(target);
			}
		}
	}

	void UpdatePathVisualization()
	{
		if (pathRenderer == null)
		{
			EnsurePathRenderer();

			if (pathRenderer == null)
			{
				Debug.LogWarning("PathRenderer still null after EnsurePathRenderer");
				return;
			}
		}

		if (!isPathVisible)
		{
			pathRenderer.positionCount = 0;																																																																													//made by EF
			return;
		}

		if (currentPath.Count > 0)
		{
			currentPath[0] = transform.position;
		}

		if (currentPath.Count > 1)
		{
			pathRenderer.positionCount = currentPath.Count;
			pathRenderer.SetPositions(currentPath.ToArray());

			if (pathRenderer.material != null)
			{
				pathRenderer.material.color = hasResource ?
					Color.green :
					(team == TeamType.Blue ? Color.cyan : Color.red);
			}
		}
		else
		{
			pathRenderer.positionCount = 0;
		}
	}
}