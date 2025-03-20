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
    public float hungerThreshold = 80f; // Yem aramaya ba�lamak i�in a�l�k e�i�i
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
    public GameObject hungerBarPrefab;  // Slider prefab referans�
    private Slider hungerBar;  // Dinamik olarak olu�turulan slider
    public float maxHunger = 100f;
    public Aquarium_gameHandler gameHandler;
    private int previousFishLvl = 1;
    public int spawnDay;
    public enum FishSize { Small, Medium, MediumLarge, Large } // Bal�k boyutlar�
    public FishSize currentSize = FishSize.Small; // Ba�lang�� boyutu
    public float[] sizeScales; // Boyutlara g�re Scale de�erleri
    public int currentDay; // G�n bilgisi
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
       

        if (spot == null)  // E�er spot �nceden olu�turulmad�ysa sadece o zaman olu�tur
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


        // Hunger de�eri 0'dan k���kse 0'a ayarla
        if (hunger <= 0f)
        {
            hunger = 0f;
            starvationTimer += Time.deltaTime; // Zamanlay�c�y� art�r

            if (starvationTimer >= 5f) // 5 saniye ge�tiyse
            {
                TakeDamage(1); // 1 hasar ver
                starvationTimer = 0f; // Zamanlay�c�y� s�f�rla
            }
        }
        else
        {
            starvationTimer = 0f; // Hunger s�f�r�n �zerinde oldu�unda zamanlay�c�y� s�f�rla
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
        int age = currentDay - spawnDay; // Bal���n ya��n� hesapla
        CheckSizeChangeWithChance(age); // Boyut de�i�im kontrol�

    
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
            // Yem oyun alan�n�n d���na ��kt� m� kontrol et
            if (IsFoodOutOfBounds(targetFood))
            {
                targetFood.GetComponent<Food>().isBeingTargeted = false;
                targetFood = null;
                return;
            }

            transform.position = Vector2.MoveTowards(transform.position, targetFood.transform.position, speed * Time.deltaTime);

            // Bal���n y�n�n� yeme do�ru d�nd�r
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
            // Yem oyun alan�n�n d���na ��kt� m� kontrol et
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

    // Yemlerin oyun alan� d���na ��k�p ��kmad���n� kontrol eden fonksiyon
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

            // Bar�n pozisyonunu bal���n �st�nde sabitle
            hungerBar.transform.position = transform.position + new Vector3(0, 1, 0);  // Bar bal���n hemen �st�nde olur
        }
    }

    private void OnDestroy()
    {
        // Bal�k yok oldu�unda a�l�k bar�n� da yok et
        if (hungerBar != null)
        {
            Destroy(hungerBar.gameObject);
        }
    }

    public void SetSpawnDay(int day)
    {
        spawnDay = day;
        Debug.Log($"Bu bal�k {spawnDay}. g�nde spawn edildi!");
    }

    private void CheckSizeChangeWithChance(int age)
    {
        if (currentSize == FishSize.Small && age >= 15 && age <= 20 && !isGrowing)
        {
            TryTransitionTo(FishSize.Medium, age, 20); // 15-20 ya� aras� �ansa ba�l� b�y�me
        }
        else if (currentSize == FishSize.Medium && age >= 35 && age <= 40 && !isGrowing)
        {
            TryTransitionTo(FishSize.MediumLarge, age, 40); // 35-40 ya� aras� �ansa ba�l� b�y�me
        }
        else if (currentSize == FishSize.MediumLarge && age >= 60 && age <= 63 && !isGrowing)
        {
            TryTransitionTo(FishSize.Large, age, 63); // 60-63 ya� aras� �ansa ba�l� b�y�me
        }
    }

    private void TryTransitionTo(FishSize targetSize, int age, int guaranteedAge)
    {
        if (currentSize != targetSize)
        {
            if (age == guaranteedAge) // Son ya�ta kesin ge�i�
            {
                StartCoroutine(SmoothSizeTransition(targetSize));
            }
            else
            {
                float chance = 0.2f; // Ge�i� �ans� (%20)
                if (Random.value <= chance)
                {
                    StartCoroutine(SmoothSizeTransition(targetSize));
                }
            }
        }
    }

    private IEnumerator SmoothSizeTransition(FishSize targetSize)
    {
        isGrowing = true; // B�y�me i�lemi ba�lat�ld�
        currentSize = targetSize; // Yeni boyuta ge�i� yap�lacak

        float duration = 1.5f; // B�y�me s�resi (�rne�in, 5 saniye)
        float elapsed = 0f;

        Vector3 initialScale = fishTransform.localScale; // Mevcut boyut
        Vector3 targetScale = Vector3.one * sizeScales[(int)targetSize]; // Hedef boyut

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration); // Oran� 0 ile 1 aras�nda s�n�rla
            fishTransform.localScale = Vector3.Lerp(initialScale, targetScale, t); // Ge�i� yap
            yield return null; // Bir sonraki frame'i bekle
        }

        fishTransform.localScale = targetScale; // Son boyutu kesinle�tir
        isGrowing = false; // B�y�me i�lemi tamamland�
        sizeCategory = targetSize.ToString();
        Debug.Log($"Bal�k yava��a {currentSize} boyutuna ge�ti!");
    }
    public void UpdateSizeInstant()
    {
        // Boyut de�erlerini hemen Scale olarak uygula
        fishTransform.localScale = Vector3.one * sizeScales[(int)currentSize];
    }

    private void UpdateCurrentHungerDecreaseRate()
    {
        // Dinamik olarak hesaplanan baz d���� oran�n� al
        float baseRate = CalculateHungerDecreaseAmount();

        // Temizlik ve oksijen seviyelerine ba�l� eklemeler
        float cleanlinessEffect = (conditionHandler.cleanlinessLevel < 50f) ? 0.5f : 0f;
        float oxygenEffect = (conditionHandler.oxygenLevel < 50f) ? 0.5f : 0f;

        // Ge�erli d���� oran�n� hesapla (baz + temizlik + oksijen)
        hungerDecreaseRate = baseRate + cleanlinessEffect + oxygenEffect;

        // Minimum ve maksimum de�erleri s�n�rla
        hungerDecreaseRate = Mathf.Clamp(hungerDecreaseRate, 0.1f, 1.5f); // �rnek: min 0.1, max 1.5
    }
}
