using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishAI_Aquarium : MonoBehaviour
{

    public float speed;
    public float health;
    public float hunger;
    public float hungerDecreaseRate = 1f;
    public float hungerThreshold = 80f; // Yem aramaya baþlamak için açlýk eþiði
    public Transform moveSpots;
    private Transform[] transformspot;
    private GameObject[] SpottoMove;
    private int randomSpot;
    private float waitTime;
    public float startWaitTime;
    public float direction;
    private AudioSource audioSource;
    public float minX, maxX, minY, maxY;
    public GameObject movespotransform;
    private GameObject spot;
    private int olddata;
    public GameObject leftBoundary, rightBoundary, topBoundary, bottomBoundary;
    private GameObject targetFood;
    public GameObject hungerBarPrefab;  // Slider prefab referansý
    private Slider hungerBar;  // Dinamik olarak oluþturulan slider
    public float maxHunger = 100f;
    public Aquarium_gameHandler gameHandler;
    private int previousFishLvl = 1;
    public int spawnDay;
    public enum FishSize { Small, Medium, MediumLarge, Large } // Balýk boyutlarý
    public FishSize currentSize = FishSize.Small; // Baþlangýç boyutu
    public float[] sizeScales; // Boyutlara göre Scale deðerleri
    public int currentDay; // Gün bilgisi
    public Transform fishTransform;
    public DayHandler dayHandler;
    private bool isGrowing;
    public string sizeCategory;
    public ConditionHandler conditionHandler;
    private float baseDecreaseRate;
    private float starvationTimer = 0f;


    private void Awake()
    {
        baseDecreaseRate = hungerDecreaseRate;


        gameHandler = FindObjectOfType<Aquarium_gameHandler>();
        dayHandler = FindObjectOfType<DayHandler>();
        moveSpots = movespotransform.GetComponent<Transform>();
        minX = leftBoundary.transform.position.x;
        maxX = rightBoundary.transform.position.x;
        minY = bottomBoundary.transform.position.y;
        maxY = topBoundary.transform.position.y;

        GameObject hungerBarInstance = Instantiate(hungerBarPrefab, transform.position, Quaternion.identity);

        hungerBarInstance.transform.SetParent(GameObject.Find("Canvas_Windows").transform, false);
        hungerBar = hungerBarInstance.GetComponent<Slider>();


        hungerBar.maxValue = maxHunger;
        hungerBar.value = hunger;
    }

    void Start()
    {
       

        if (spot == null)  // Eðer spot önceden oluþturulmadýysa sadece o zaman oluþtur
        {
            spot = Instantiate(movespotransform);
        }

        spot.transform.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        waitTime = startWaitTime;
        fishTransform = transform;
        UpdateSizeInstant();

        sizeCategory = currentSize.ToString();

    }
    private float CalculateHungerDecreaseAmount()
    {
        float amount = baseDecreaseRate;

        for (int i = 2; i <= gameHandler.fishLvl; i++)
        {
            amount = amount - 0.02f;
        }


        return amount;
    }
    void Update()
    {
       

        UpdateCurrentHungerDecreaseRate();

            

        hunger -= hungerDecreaseRate * Time.deltaTime;


        // Hunger deðeri 0'dan küçükse 0'a ayarla
        if (hunger <= 0f)
        {
            hunger = 0f;
            starvationTimer += Time.deltaTime; // Zamanlayýcýyý artýr

            if (starvationTimer >= 5f) // 5 saniye geçtiyse
            {
                TakeDamage(1); // 1 hasar ver
                starvationTimer = 0f; // Zamanlayýcýyý sýfýrla
            }
        }
        else
        {
            starvationTimer = 0f; // Hunger sýfýrýn üzerinde olduðunda zamanlayýcýyý sýfýrla
        }


        if (hunger < hungerThreshold && targetFood == null)
        {
            FindNearestFood();
        }

        if (targetFood != null)
        {
            MoveTowardsFood();
        }
        else
        {
            Wander();
        }

        UpdateHungerBar();

        currentDay = dayHandler.CurrentDay;
        int age = currentDay - spawnDay; // Balýðýn yaþýný hesapla
        CheckSizeChangeWithChance(age); // Boyut deðiþim kontrolü

    
    }

    private void Wander()
    {
        transform.position = Vector2.MoveTowards(transform.position, spot.transform.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, spot.transform.position) < 1f)
        {
            if (waitTime <= 0)
            {
                spot.transform.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }

        if (spot.transform.position.x < transform.position.x)
        {
            Flip(180);
        }
        else if (spot.transform.position.x > transform.position.x)
        {
            Flip(0);
        }
    }

    private void MoveTowardsFood()
    {
        if (targetFood != null)
        {
            // Yem oyun alanýnýn dýþýna çýktý mý kontrol et
            if (IsFoodOutOfBounds(targetFood))
            {
                targetFood.GetComponent<Food>().isBeingTargeted = false;
                targetFood = null;
                return;
            }

            transform.position = Vector2.MoveTowards(transform.position, targetFood.transform.position, speed * Time.deltaTime);

            // Balýðýn yönünü yeme doðru döndür
            if (targetFood.transform.position.x < transform.position.x)
            {
                Flip(180);
            }
            else if (targetFood.transform.position.x > transform.position.x)
            {
                Flip(0);
            }

            if (Vector3.Distance(transform.position, targetFood.transform.position) < 0.1f)
            {
              Food foodComponent = targetFood.GetComponent<Food>();
                if (foodComponent != null)
                {
                    float hungerAmount = foodComponent.GetHungerIncreaseAmount();
                    EatFood(targetFood, hungerAmount);
                }
            }
        }
    }

    private void FindNearestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("FishFood");
        float minDistance = Mathf.Infinity;
        GameObject nearestFood = null;

        foreach (GameObject food in foods)
        {
            // Yem oyun alanýnýn dýþýna çýktý mý kontrol et
            if (IsFoodOutOfBounds(food))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, food.transform.position);
            Food foodScript = food.GetComponent<Food>();
            if (distance < minDistance && !foodScript.isBeingTargeted)
            {
                minDistance = distance;
                nearestFood = food;
            }
        }

        if (nearestFood != null)
        {
            targetFood = nearestFood;
            targetFood.GetComponent<Food>().isBeingTargeted = true;
        }
    }

    private void EatFood(GameObject food, float hungerIncreaseAmount)
    {
        hunger = Mathf.Min(hunger + hungerIncreaseAmount, 100f);
        Destroy(food);
        AquariumSoundsHandler.Instance.PlayFishEatFood();
        targetFood = null;

        if(hunger > 100f)
        {
            hunger = 100f;
        }
    }

    private void Flip(float direction)
    {
        Quaternion value = transform.localRotation;
        value.y = direction;
        transform.localRotation = value;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "EnemyFish")
        {
            spot.transform.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
    }

    // Yemlerin oyun alaný dýþýna çýkýp çýkmadýðýný kontrol eden fonksiyon
    private bool IsFoodOutOfBounds(GameObject food)
    {
        Vector3 position = food.transform.position;
        return (position.x < minX || position.x > maxX || position.y < minY || position.y > maxY);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);  // EnemyFish yok edilir
    }

    private void UpdateHungerBar()
    {
        if (hungerBar != null)
        {
            hungerBar.value = hunger;

            // Barýn pozisyonunu balýðýn üstünde sabitle
            hungerBar.transform.position = transform.position + new Vector3(0, 1, 0);  // Bar balýðýn hemen üstünde olur
        }
    }

    private void OnDestroy()
    {
        // Balýk yok olduðunda açlýk barýný da yok et
        if (hungerBar != null)
        {
            Destroy(hungerBar.gameObject);
        }
    }

    public void SetSpawnDay(int day)
    {
        spawnDay = day;
        Debug.Log($"Bu balýk {spawnDay}. günde spawn edildi!");
    }

    private void CheckSizeChangeWithChance(int age)
    {
        if (currentSize == FishSize.Small && age >= 15 && age <= 20 && !isGrowing)
        {
            TryTransitionTo(FishSize.Medium, age, 20); // 15-20 yaþ arasý þansa baðlý büyüme
        }
        else if (currentSize == FishSize.Medium && age >= 35 && age <= 40 && !isGrowing)
        {
            TryTransitionTo(FishSize.MediumLarge, age, 40); // 35-40 yaþ arasý þansa baðlý büyüme
        }
        else if (currentSize == FishSize.MediumLarge && age >= 60 && age <= 63 && !isGrowing)
        {
            TryTransitionTo(FishSize.Large, age, 63); // 60-63 yaþ arasý þansa baðlý büyüme
        }
    }

    private void TryTransitionTo(FishSize targetSize, int age, int guaranteedAge)
    {
        if (currentSize != targetSize)
        {
            if (age == guaranteedAge) // Son yaþta kesin geçiþ
            {
                StartCoroutine(SmoothSizeTransition(targetSize));
            }
            else
            {
                float chance = 0.2f; // Geçiþ þansý (%20)
                if (Random.value <= chance)
                {
                    StartCoroutine(SmoothSizeTransition(targetSize));
                }
            }
        }
    }

    private IEnumerator SmoothSizeTransition(FishSize targetSize)
    {
        isGrowing = true; // Büyüme iþlemi baþlatýldý
        currentSize = targetSize; // Yeni boyuta geçiþ yapýlacak

        float duration = 1.5f; // Büyüme süresi (örneðin, 5 saniye)
        float elapsed = 0f;

        Vector3 initialScale = fishTransform.localScale; // Mevcut boyut
        Vector3 targetScale = Vector3.one * sizeScales[(int)targetSize]; // Hedef boyut

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration); // Oraný 0 ile 1 arasýnda sýnýrla
            fishTransform.localScale = Vector3.Lerp(initialScale, targetScale, t); // Geçiþ yap
            yield return null; // Bir sonraki frame'i bekle
        }

        fishTransform.localScale = targetScale; // Son boyutu kesinleþtir
        isGrowing = false; // Büyüme iþlemi tamamlandý
        sizeCategory = targetSize.ToString();
        Debug.Log($"Balýk yavaþça {currentSize} boyutuna geçti!");
    }
    public void UpdateSizeInstant()
    {
        // Boyut deðerlerini hemen Scale olarak uygula
        fishTransform.localScale = Vector3.one * sizeScales[(int)currentSize];
    }

    private void UpdateCurrentHungerDecreaseRate()
    {
        // Dinamik olarak hesaplanan baz düþüþ oranýný al
        float baseRate = CalculateHungerDecreaseAmount();

        // Temizlik ve oksijen seviyelerine baðlý eklemeler
        float cleanlinessEffect = (conditionHandler.cleanlinessLevel < 50f) ? 0.5f : 0f;
        float oxygenEffect = (conditionHandler.oxygenLevel < 50f) ? 0.5f : 0f;

        // Geçerli düþüþ oranýný hesapla (baz + temizlik + oksijen)
        hungerDecreaseRate = baseRate + cleanlinessEffect + oxygenEffect;

        // Minimum ve maksimum deðerleri sýnýrla
        hungerDecreaseRate = Mathf.Clamp(hungerDecreaseRate, 0.1f, 1.5f); // Örnek: min 0.1, max 1.5
    }
}
