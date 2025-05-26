using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomSpawner : MonoBehaviour
{
    public GameObject[] customerPrefabs;        // 일반손님 프리팹
    public GameObject[] badCustomerPrefabs;     // 나쁜손님 프리팹
    public Transform[] spawnPoints;             // 스폰 포인트
    public float badCustomerChance = 0.1f;     // 나쁜손님 등장 확률

    private int clearedCustomerCount = 0;       // 청소된 손님 수
    private List<Transform> availableSpawnPoints = new List<Transform>(); // 사용 가능한 스폰 포인트
    private HashSet<Transform> occupiedSpawnPoints = new HashSet<Transform>(); // 점유된 스폰 포인트
    private Dictionary<GameObject, Transform> customerSpawnPoints = new Dictionary<GameObject, Transform>(); // 손님과 스폰 포인트 매핑
    private Dictionary<Transform, float> spawnPointCooldowns = new Dictionary<Transform, float>(); // 스폰 포인트 쿨다운 관리

    private void Start()
    {
        availableSpawnPoints.AddRange(spawnPoints);
        StartCoroutine(SpawnLoop());
        StartCoroutine(UpdateCooldowns()); // 쿨다운 업데이트 시작
    }

    IEnumerator UpdateCooldowns()
    {
        while (true)
        {
            List<Transform> cooledDownPoints = new List<Transform>();
            
            // 쿨다운이 끝난 포인트 찾기
            foreach (var kvp in spawnPointCooldowns)
            {
                if (Time.time >= kvp.Value)
                {
                    cooledDownPoints.Add(kvp.Key);
                }
            }

            // 쿨다운이 끝난 포인트 제거
            foreach (var point in cooledDownPoints)
            {
                spawnPointCooldowns.Remove(point);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            int activeCustomers = GameObject.FindGameObjectsWithTag("Custom").Length;
            int maxCustomers = Mathf.Clamp(clearedCustomerCount < 3 ? 6 : 12, 1, 12);

            if (activeCustomers < maxCustomers)
            {
                SpawnRandomCustomer();
            }

            yield return new WaitForSeconds(10f);
        }
    }

    public void SpawnRandomCustomer()
    {
        // 선택 가능한 위치가 없으면 리턴
        List<Transform> validSpawnPoints = new List<Transform>();

        foreach (Transform point in spawnPoints)
        {
            // 쿨다운 중이 아니고 점유되지 않은 포인트만 선택
            if (!occupiedSpawnPoints.Contains(point) && !spawnPointCooldowns.ContainsKey(point))
                validSpawnPoints.Add(point);
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning("모든 스폰 포인트가 사용 중이거나 쿨다운 중입니다.");
            return;
        }

        // 랜덤 선택
        Transform selectedSpawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];

        bool spawnBad = !GameManager.instance.hasBadCustomer && Random.value < badCustomerChance;

        GameObject prefabToSpawn = spawnBad ?
            badCustomerPrefabs[Random.Range(0, badCustomerPrefabs.Length)] :
            customerPrefabs[Random.Range(0, customerPrefabs.Length)];

        GameObject newCustomer = Instantiate(prefabToSpawn, selectedSpawnPoint.position, Quaternion.identity);
        Custom custom = newCustomer.GetComponent<Custom>();

        if (custom != null)
        {
            custom.spawner = this;
            custom.isBadCustomer = spawnBad;
            custom.spawnPoint = selectedSpawnPoint;

            if (spawnBad)
            {
                GameManager.instance.hasBadCustomer = true;
                GameManager.instance.badCustomer = custom;
            }

            // 스폰 포인트 등록
            occupiedSpawnPoints.Add(selectedSpawnPoint);
            customerSpawnPoints[newCustomer] = selectedSpawnPoint;
        }
    }

    public void RespawnCustomer(GameObject oldCustomer)
    {
        if (customerSpawnPoints.ContainsKey(oldCustomer))
        {
            Transform spawnPoint = customerSpawnPoints[oldCustomer];
            occupiedSpawnPoints.Remove(spawnPoint);
            customerSpawnPoints.Remove(oldCustomer);
            
            // 쿨다운 설정 (3초)
            spawnPointCooldowns[spawnPoint] = Time.time + 3f;
        }

        StartCoroutine(RespawnRoutine(oldCustomer));
    }

    IEnumerator RespawnRoutine(GameObject oldCustomer)
    {
        Destroy(oldCustomer);
        yield return new WaitForSeconds(3f);
        SpawnRandomCustomer();
    }

    public void OnCustomerCleared()
    {
        clearedCustomerCount++;
        GameManager.instance.IncreaseCustomerCount();
    }

    // 손님이 제거될 때 호출되는 메서드
    public void OnCustomerDestroyed(GameObject customer)
    {
        if (customerSpawnPoints.ContainsKey(customer))
        {
            Transform spawnPoint = customerSpawnPoints[customer];
            occupiedSpawnPoints.Remove(spawnPoint);
            customerSpawnPoints.Remove(customer);
            
            // 쿨다운 설정 (3초)
            spawnPointCooldowns[spawnPoint] = Time.time + 3f;
        }
    }
}
