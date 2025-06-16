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
    private float badCustomerEnableTime = 20f; // 20초 후부터 나쁜손님 등장
    private float normalCustomerEnableTime = 10f; // 10초 후부터 일반손님 등장
    private float gameStartTime;
    private float minSpawnInterval = 5f;  // 최소 스폰 간격
    private float maxSpawnInterval = 15f;  // 최대 스폰 간격
    private bool isSpawning = false;  // 현재 스폰 중인지 여부

    private void Start()
    {
        availableSpawnPoints.AddRange(spawnPoints);
        gameStartTime = Time.time;
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
            if (!isSpawning)
            {
                int activeCustomers = GameObject.FindGameObjectsWithTag("Custom").Length;
                int maxCustomers = Mathf.Clamp(clearedCustomerCount < 3 ? 6 : 4, 1, 9);

                if (activeCustomers < maxCustomers)
                {
                    isSpawning = true;
                    SpawnRandomCustomer();
                    isSpawning = false;
                }
            }

            // 스폰 간격을 동적으로 조절
            float currentInterval = Mathf.Lerp(minSpawnInterval, maxSpawnInterval, 
                (float)clearedCustomerCount / 10f);  // 손님 수에 따라 간격 조절
            yield return new WaitForSeconds(currentInterval);
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
            {
                // 해당 위치에 쓰레기가 있는지 확인 (반경 증가)
                Collider[] colliders = Physics.OverlapSphere(point.position, 1f);
                bool hasTrash = false;
                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Trash"))
                    {
                        hasTrash = true;
                        Debug.Log($"스폰 포인트 {point.name}에 쓰레기가 있어 스킵합니다.");
                        break;
                    }
                }

                // 쓰레기가 없는 위치만 추가
                if (!hasTrash)
                {
                    validSpawnPoints.Add(point);
                }
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning("모든 스폰 포인트가 사용 중이거나 쿨다운 중이거나 쓰레기가 있습니다.");
            return;
        }

        // 랜덤 선택
        Transform selectedSpawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        Debug.Log($"선택된 스폰 포인트: {selectedSpawnPoint.name}");

        bool canSpawnBad = (Time.time - gameStartTime) >= badCustomerEnableTime;
        bool canSpawnNormal = (Time.time - gameStartTime) >= normalCustomerEnableTime;

        bool spawnBad = false;
        if (canSpawnBad && !GameManager.instance.hasBadCustomer && Random.value < badCustomerChance)
        {
            spawnBad = true;
        }
        else if (!canSpawnNormal)
        {
            // 아직 일반손님 등장 시간 전이면, 손님을 스폰하지 않음
            return;
        }

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
                Debug.Log("나쁜 손님이 스폰되었습니다.");
            }
            else
            {
                Debug.Log("일반 손님이 스폰되었습니다.");
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

        // 즉시 새로운 손님 생성하지 않고 SpawnLoop에서 처리하도록 함
        Destroy(oldCustomer);
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

    public void OnTrashCleaned(Vector3 position)
    {
        // 해당 위치의 스폰 포인트 찾기 (반경 증가)
        Transform spawnPoint = null;
        float minDistance = float.MaxValue;
        float checkRadius = 1f;  // 쓰레기 제거 시 체크 반경

        foreach (Transform point in spawnPoints)
        {
            float distance = Vector3.Distance(point.position, position);
            if (distance < checkRadius && distance < minDistance)
            {
                minDistance = distance;
                spawnPoint = point;
            }
        }

        if (spawnPoint != null)
        {
            Debug.Log($"쓰레기가 제거되어 스폰 포인트 {spawnPoint.name}가 다시 사용 가능해졌습니다.");
            // 스폰 포인트를 다시 사용 가능하게 설정
            occupiedSpawnPoints.Remove(spawnPoint);
            spawnPointCooldowns.Remove(spawnPoint);
        }
    }
}
