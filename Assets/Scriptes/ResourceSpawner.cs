using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
	[Header("Resource Settings")]
	public GameObject resourcePrefab;
	public float spawnRadius = 0.5f;

	private ParticleSystem spawnEffect;

	void Start()
	{
		GameObject effectGO = new GameObject("SpawnEffect");
		effectGO.transform.SetParent(transform);
		spawnEffect = effectGO.AddComponent<ParticleSystem>();

		var main = spawnEffect.main;
		main.startColor = Color.yellow;
		main.startSize = 0.1f;
		main.startLifetime = 1f;
		main.maxParticles = 50;

		var emission = spawnEffect.emission;
		emission.rateOverTime = 0;
	}

	public void SpawnResource()
	{
		if (spawnEffect != null)
			spawnEffect.Play();
	}
}