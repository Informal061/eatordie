using Mirror.Logging;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Aquarium_gameHandler : MonoBehaviour
{
    public GameObject shopWindow, belongingsWindow, panel;
    public Animator shopAnimator, belongingsAnimator, panelAnimator;
    public int day;
    public int totalAquariumFish;
    public Text TBPText;
    public int AqTBP;
    public GameObject PNGWorm, PNGGreenFlake, PNGOrangeFlake, PNGLightPellet, PNGDarkPellet;
    private int currentIndex = 0;
    public Image activeFishFoodImage;
    private GameObject[] fishFoods, RealFishFoods;
    private Sprite[] fishFoodSprites;
    public GameObject worm, greenFlake, orangeFlake, lightPellet, darkPellet;
    public float dropSpeed;
    public int wormCount, greenFlakeCount, orangeFlakeCount, lightPelletCount, darkPelletCount;
    public GameObject floatingTextPrefab, floatingTextDown;
    public Transform floatingTextParent, floatingDownTextParent, MoneyText;
    public TMP_Text floatingMoneyText;
    public GameObject NemoFish, BlueyFish, BandyFish, PurpyFish, ZebbyFish;
    public float spawnControlRadius = 5f;
    public int foodLvl = 1;
    public int foodEffLvl = 1;
    public int fishLvl = 1;
    public int DefensesLvl = 1;
    public Text foodEffLvlText, foodLvlText, FishLvlText, DefensesLvlText;
    public DayHandler dayHandler;
    public DefenseSelection defenseSelection;
    public GameObject pauseMenuUI; // Pause menüsü GameObject'i
    private bool isPaused = false; // Oyun duraklatýldý mý?
    public Text wormCountText, greenFlakeCountText, orangeFlakeCountText, lightPelletCountText, darkPelletCountText;
    private Dictionary<string, int> fishCounts; // Balýk kategorileri ve sayýlarý
    public Text smallFishCountText, mediumFishCountText, mediumLargeFishCountText, largeFishCountText; // UI Text elemanlarý
    public int smallFishSellPrice = 10;
    public int mediumFishSellPrice = 20;
    public int mediumLargeFishSellPrice = 30;
    public int largeFishSellPrice = 50;
    public ConditionHandler conditionHandler;

    void Awake()
    {
        
        AqTBP = PlayerPrefs.GetInt("TBP");
        TBPText.text = AqTBP.ToString();

    }

    void Start()
    {
        

        fishFoods = new GameObject[] { PNGWorm, PNGGreenFlake, PNGOrangeFlake, PNGLightPellet, PNGDarkPellet };
        RealFishFoods = new GameObject[] { worm, greenFlake, orangeFlake, lightPellet, darkPellet };

        fishFoodSprites = new Sprite[]
        {
            PNGWorm.GetComponent<SpriteRenderer>().sprite,
            PNGGreenFlake.GetComponent<SpriteRenderer>().sprite,
            PNGOrangeFlake.GetComponent<SpriteRenderer>().sprite,
            PNGLightPellet.GetComponent<SpriteRenderer>().sprite,
            PNGDarkPellet.GetComponent<SpriteRenderer>().sprite
        };

        UpdateActiveFishFood();

        fishCounts = new Dictionary<string, int>
    {
        { "Small", 0 },
        { "Medium", 0 },
        { "MediumLarge", 0 },
        { "Large", 0 }
    };

        UpdateFishCounts();
    }


    void Update()
    {

        SelectFishFood();
        GetAquariumFishCount();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // Oyunu devam ettir
            }
            else
            {
                PauseGame(); // Oyunu duraklat
            }
        }

        wormCountText.text = "x" + wormCount.ToString();
        greenFlakeCountText.text = "x" + greenFlakeCount.ToString();
        orangeFlakeCountText.text = "x" + orangeFlakeCount.ToString();
        lightPelletCountText.text = "x" + lightPelletCount.ToString();
        darkPelletCountText.text = "x" + darkPelletCount.ToString();

        CountFishBySize();

    }

    private void UpdateFishCounts()
    {
        smallFishCountText.text = $"x{fishCounts["Small"]}";
        mediumFishCountText.text = $"x{fishCounts["Medium"]}";
        mediumLargeFishCountText.text = $"x{fishCounts["MediumLarge"]}";
        largeFishCountText.text = $"x{fishCounts["Large"]}";
    }

    private void CountFishBySize()
    {
        // Tüm balýklarý bul ve kategorilere göre say
        fishCounts["Small"] = 0;
        fishCounts["Medium"] = 0;
        fishCounts["MediumLarge"] = 0;
        fishCounts["Large"] = 0;

        GameObject[] allFish = GameObject.FindGameObjectsWithTag("EnemyFish");
        foreach (GameObject fish in allFish)
        {
            FishAI_Aquarium fishAI = fish.GetComponent<FishAI_Aquarium>();
            if (fishAI != null)
            {
                fishCounts[fishAI.sizeCategory]++;
            }
        }

        UpdateFishCounts();
    }


    private void SelectFishFood()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex = (currentIndex > 0) ? currentIndex - 1 : fishFoods.Length - 1; //en baþtakine dönmediyse sola gitmeye devam et, gittiyse en sondakine dön.
            UpdateActiveFishFood();
        }


        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex = (currentIndex < fishFoods.Length - 1) ? currentIndex + 1 : 0; // en sondakine gitmediyse saða gitmeye devam et, gittiyse baþa dön.
            UpdateActiveFishFood();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SpawnFishFood();

        }
    }

    void UpdateActiveFishFood()
    {
        for (int i = 0; i < fishFoods.Length; i++)
        {
            fishFoods[i].SetActive(i == currentIndex);
        }
    }

    public void ActivateShop()

    {

        if (!belongingsWindow.activeInHierarchy)
        {


            if (shopWindow.activeInHierarchy)
            {
                shopAnimator.SetBool("ShowShop", false);
                panelAnimator.SetBool("PanelAnim", true);
                shopWindow.transform.position = new Vector3(-996f, 1811f, -6431.8f);
                shopWindow.SetActive(false);
                panel.SetActive(false);


            }
            else
            {
                shopWindow.SetActive(true);
                panel.SetActive(true);
                shopAnimator.SetBool("ShowShop", true);
            }

        }


    }


    public void ActivateBelongings()

    {
        if (!shopWindow.activeInHierarchy)
        {

            if (belongingsWindow.activeInHierarchy)
            {

                belongingsAnimator.SetBool("ShowBelongings", false);
                panelAnimator.SetBool("PanelAnim", true);
                belongingsWindow.transform.position = new Vector3(1290f, 1811f, -6431.8f);
                belongingsWindow.SetActive(false);
                panel.SetActive(false);


            }
            else
            {
                belongingsWindow.SetActive(true);
                panel.SetActive(true);
                belongingsAnimator.SetBool("ShowBelongings", true);
            }

        }
    }

    public void GetAquariumFishCount()
    {

        GameObject[] fishObjects = GameObject.FindGameObjectsWithTag("EnemyFish"); // EnemyFish tag'lý olan tüm nesneleri bul.
        totalAquariumFish = fishObjects.Length; // toplam balýk sayýsýný al






    }

    private void SpawnFishFood()
    {
        if (Time.timeScale == 0f) return;

        if (CanSpawnCurrentFood())
        {
            Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // mouse ile spawn olacaðýný belirterek mouse'ýn o anda týklandýðýndaki lokasyonunu al.
            spawnPosition.z = 0;

            GameObject selectedFishFood = Instantiate(RealFishFoods[currentIndex].gameObject, spawnPosition, Quaternion.identity); // spawn edilecek seçili yemi belirtelim.
            selectedFishFood.SetActive(true);

            Rigidbody2D rb = selectedFishFood.AddComponent<Rigidbody2D>(); // düþme hýzý.
            rb.gravityScale = dropSpeed;
            DecreaseFoodCount(currentIndex);
            AquariumSoundsHandler.Instance.PlayFoodDeploySound();

        }
        else
        {

            Debug.Log("Selected fish food is out of stock!"); // burada yemden olmadýðýný belirten bir animasyon ya da yazý koyulacak.

        }
    }

    void DecreaseFoodCount(int index)
    {
        switch (index)
        {
            case 0:
                wormCount--;
                break;
            case 1:
                greenFlakeCount--;
                break;
            case 2:
                orangeFlakeCount--;
                break;
            case 3:
                lightPelletCount--;
                break;
            case 4:
                darkPelletCount--;
                break;
        }
    }

    // Yem atýlýp atýlamayacaðýný kontrol eden fonksiyon
    bool CanSpawnCurrentFood()
    {
        switch (currentIndex)
        {
            case 0:
                return wormCount > 0;
            case 1:
                return greenFlakeCount > 0;
            case 2:
                return orangeFlakeCount > 0;
            case 3:
                return lightPelletCount > 0;
            case 4:
                return darkPelletCount > 0;
            default:
                return false;
        }
    }

    public void BuyWorm(Transform buttonTransform)
    {
        if(AqTBP >= 2)
        {
            switch (foodLvl)
            {
                case 1:
                    floatingMoneyText.text = "-2";
                    wormCount++;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 2;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);  
                    break;
                case 2:
                    floatingMoneyText.text = "-2";
                    wormCount = wormCount + 2;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 2;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 3:
                    floatingMoneyText.text = "-2";
                    wormCount = wormCount + 3;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 2;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
            }

                 
        } else
        {

            // BU VE ALTTAKÝLER DAHÝL PARASININ OLMADIÐINDA DAÝR BÝR SES ÇALABÝLÝRÝZ YA DA BÝR POP UP ÇIKARABÝLÝRÝZ TABÝ ÝKÝSÝ BÝR DE OLABÝLÝR.
        }


    }
    public void BuyGreenFlake(Transform buttonTransform)
    {

        if (AqTBP >= 4)
        {
            switch(foodLvl)
            {
                case 1:
                    floatingMoneyText.text = "-4";
                    greenFlakeCount++;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 4;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 2:
                    floatingMoneyText.text = "-4";
                    greenFlakeCount = greenFlakeCount + 2;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 4;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 3:
                    floatingMoneyText.text = "-4";
                    greenFlakeCount = greenFlakeCount + 3;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 4;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
            }


            
        } else
        {


        }



    }
    public void BuyOrangeFlake(Transform buttonTransform)
    {
        if (AqTBP >= 7)
        {

            switch (foodLvl)
            {
                case 1:
                    floatingMoneyText.text = "-7";
                    orangeFlakeCount++;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 7;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 2:
                    floatingMoneyText.text = "-7";
                    orangeFlakeCount = orangeFlakeCount + 2;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 7;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 3:
                    floatingMoneyText.text = "-7";
                    orangeFlakeCount = orangeFlakeCount + 3;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 7;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
            }

            
        }else
        {


        }

    }
    public void BuyLightPellet(Transform buttonTransform)
    {
        if (AqTBP >= 10)
        {

            switch (foodLvl)
            {
                case 1:
                    floatingMoneyText.text = "-10";
                    lightPelletCount++;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 10;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 2:
                    floatingMoneyText.text = "-10";
                    lightPelletCount = lightPelletCount + 2;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 10;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 3:
                    floatingMoneyText.text = "-10";
                    lightPelletCount = lightPelletCount + 3;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 10;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
            }

           
        } else
        {

        }

    }
    public void BuyDarkPellet(Transform buttonTransform)
    {
        if (AqTBP >= 15)
        {

            switch (foodLvl)
            {
                case 1:
                    floatingMoneyText.text = "-15";
                    darkPelletCount++;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 15;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 2:
                    floatingMoneyText.text = "-15";
                    darkPelletCount = darkPelletCount + 2;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 15;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
                case 3:
                    floatingMoneyText.text = "-15";
                    darkPelletCount = darkPelletCount + 3;
                    ShowFloatingText(buttonTransform);
                    ShowFloatingTextDown();
                    AqTBP = AqTBP - 15;
                    TBPText.text = AqTBP.ToString();
                    PlayerPrefs.SetInt("TBP", AqTBP);
                    break;
            }


            
        } else
        {

        }

    }

    private void ShowFloatingText(Transform buttonTransform)
    {
        if (floatingTextPrefab != null)
        {
            // Butonun sað tarafýnda oluþtur
            Vector3 spawnPosition = buttonTransform.position + new Vector3(3f, 0f, 0f); // Sað tarafa ekle
            GameObject floatingText = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity, floatingTextParent);
            floatingText.SetActive(true);
          
            if (floatingTextParent != null)
            {
                floatingText.transform.SetParent(floatingTextParent, false);
            }
        }
    }

    private void ShowFloatingTextDown()
    {
        if (floatingTextPrefab != null)
        {
            // Butonun sað tarafýnda oluþtur
            Vector3 spawnPosition = MoneyText.position + new Vector3(0f, -3f, 0f); // Alt tarafa ekle
            GameObject floatingText = Instantiate(floatingTextDown, spawnPosition, Quaternion.identity, floatingDownTextParent);
            floatingText.SetActive(true);
            
            if (floatingDownTextParent != null)
            {
                floatingText.transform.SetParent(floatingDownTextParent, false);
            }
        }
    }

    public void BuyNemoFish(Transform buttonTransform)
    {
        if(AqTBP >= 10)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            if (spawnPosition != Vector3.zero)
            {
                GameObject SpawnedFish = Instantiate(NemoFish, spawnPosition, Quaternion.identity);
                SpawnedFish.SetActive(true);
                FishAI_Aquarium fishScript = SpawnedFish.GetComponent<FishAI_Aquarium>();
                if(fishScript != null && dayHandler != null)
                {
                    fishScript.SetSpawnDay(dayHandler.CurrentDay);
                }
                floatingMoneyText.text = "-10";
                ShowFloatingText(buttonTransform);
                ShowFloatingTextDown();
                AqTBP = AqTBP - 10;
                TBPText.text = AqTBP.ToString();
                PlayerPrefs.SetInt("TBP", AqTBP);

            }
        }


    }

    public void BuyBlueyFish(Transform buttonTransform)
    {
        if (AqTBP >= 20)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            if(spawnPosition != Vector3.zero)
            {
                GameObject SpawnedFish = Instantiate(BlueyFish, spawnPosition, Quaternion.identity);
                SpawnedFish.SetActive(true);
                FishAI_Aquarium fishScript = SpawnedFish.GetComponent<FishAI_Aquarium>();
                if (fishScript != null && dayHandler != null)
                {
                    fishScript.SetSpawnDay(dayHandler.CurrentDay);
                }
                floatingMoneyText.text = "-20";
                ShowFloatingText(buttonTransform);
                ShowFloatingTextDown();
                AqTBP = AqTBP - 20;
                TBPText.text = AqTBP.ToString();
                PlayerPrefs.SetInt("TBP", AqTBP);
            }



        }


    }

    public void BuyBandyFish(Transform buttonTransform)
    {

        if (AqTBP >= 25)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            if (spawnPosition != Vector3.zero)
            {
                GameObject SpawnedFish = Instantiate(BandyFish, spawnPosition, Quaternion.identity);
                SpawnedFish.SetActive(true);
                FishAI_Aquarium fishScript = SpawnedFish.GetComponent<FishAI_Aquarium>();
                if (fishScript != null && dayHandler != null)
                {
                    fishScript.SetSpawnDay(dayHandler.CurrentDay);
                }
                floatingMoneyText.text = "-25";
                ShowFloatingText(buttonTransform);
                ShowFloatingTextDown();
                AqTBP = AqTBP - 25;
                TBPText.text = AqTBP.ToString();
                PlayerPrefs.SetInt("TBP", AqTBP);

            }
        }

    }

    public void BuyPurpyFish(Transform buttonTransform)
    {

        if (AqTBP >= 40)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            if (spawnPosition != Vector3.zero)
            {
                GameObject SpawnedFish = Instantiate(PurpyFish, spawnPosition, Quaternion.identity);
                SpawnedFish.SetActive(true);
                FishAI_Aquarium fishScript = SpawnedFish.GetComponent<FishAI_Aquarium>();
                if (fishScript != null && dayHandler != null)
                {
                    fishScript.SetSpawnDay(dayHandler.CurrentDay);
                }
                floatingMoneyText.text = "-40";
                ShowFloatingText(buttonTransform);
                ShowFloatingTextDown();
                AqTBP = AqTBP - 40;
                TBPText.text = AqTBP.ToString();
                PlayerPrefs.SetInt("TBP", AqTBP);

            }
        }

    }

    public void BuyZebbyFish(Transform buttonTransform)
    {
        if (AqTBP >= 60)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            if (spawnPosition != Vector3.zero)
            {
                GameObject SpawnedFish = Instantiate(ZebbyFish, spawnPosition, Quaternion.identity);
                SpawnedFish.SetActive(true);
                FishAI_Aquarium fishScript = SpawnedFish.GetComponent<FishAI_Aquarium>();
                if (fishScript != null && dayHandler != null)
                {
                    fishScript.SetSpawnDay(dayHandler.CurrentDay);
                }
                floatingMoneyText.text = "-60";
                ShowFloatingText(buttonTransform);
                ShowFloatingTextDown();
                AqTBP = AqTBP - 60;
                TBPText.text = AqTBP.ToString();
                PlayerPrefs.SetInt("TBP", AqTBP);

            }
        }


    }

    private Vector3 GetValidSpawnPosition()
    {
        Vector3 lastCheckedPosition = Vector3.zero;

        for (int i = 0; i < 100; i++)
        {
            Vector3 spawnPosition = GetRandomPositionInCameraBounds();

            lastCheckedPosition = spawnPosition;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, spawnControlRadius);
            bool isValid = true;

            foreach (var collider in colliders)
            {
                if (collider.CompareTag("targetFish"))
                {
                    isValid = false;
                    break; // Bir çakýþma bulunduðunda daha fazla kontrol gerekmez
                }
            }

            if (isValid)
            {
                return spawnPosition; // Uygun bir nokta bulundu
            }
        }

        return lastCheckedPosition;
    }

    private Vector3 GetRandomPositionInCameraBounds()
    {
        Camera camera = Camera.main; // Ana kamera
        float height = 2f * camera.orthographicSize; // Kamera yüksekliði
        float width = height * Screen.width / Screen.height; // Kamera geniþliði

        // Kamera merkezine göre sýnýrlarý hesapla
        float xMin = camera.transform.position.x - width / 2;
        float xMax = camera.transform.position.x + width / 2;
        float yMin = camera.transform.position.y - height / 2;
        float yMax = camera.transform.position.y + height / 2;

        // Rastgele pozisyon oluþtur
        float x = Random.Range(xMin, xMax);
        float y = Random.Range(yMin, yMax);
        return new Vector3(x, y, 0f); // 2D olduðu için Z ekseni sýfýr
    }

    public void UpgFishFood(Transform buttonTransform)
    {
        if (foodEffLvl >= 3)
        {

            foodEffLvl = 3;
            return;
        }

        if (AqTBP >= 15 && foodEffLvl <= 3)
        {
            foodEffLvl++;
            foodEffLvlText.text = "Lv." + foodEffLvl.ToString();
            AqTBP = AqTBP - 15;
            TBPText.text = AqTBP.ToString();
            floatingMoneyText.text = "-15";
            ShowFloatingText(buttonTransform);
            ShowFloatingTextDown();
            PlayerPrefs.SetInt("TBP", AqTBP);


        }
    }

    public void UpgFishFoodBuyAmount(Transform buttonTransform)
    {
        if(foodLvl >= 3)
        {

            foodLvl = 3;
            return;
        }

        if (AqTBP >= 20 && foodLvl <= 3) 
        {
            foodLvl++;
            foodLvlText.text = "Lv." + foodLvl.ToString();
            AqTBP = AqTBP - 20;
            TBPText.text = AqTBP.ToString();
            floatingMoneyText.text = "-20";
            ShowFloatingText(buttonTransform);
            ShowFloatingTextDown();
            PlayerPrefs.SetInt("TBP", AqTBP);


        }
        


    }
    
    public void UpgFish(Transform buttonTransform)
    {
        if (fishLvl >= 3)
        {

            fishLvl = 3;
            return;
        }

        if (AqTBP >= 25 && fishLvl <= 3)
        {
            fishLvl++;
            FishLvlText.text = "Lv." + fishLvl.ToString();
            AqTBP = AqTBP - 25;
            TBPText.text = AqTBP.ToString();
            floatingMoneyText.text = "-25";
            ShowFloatingText(buttonTransform);
            ShowFloatingTextDown();
            PlayerPrefs.SetInt("TBP", AqTBP);


        }
    }
    public void UpgDefenses(Transform buttonTransform)
    {
        if (DefensesLvl >= 3)
        {

            DefensesLvl = 3;
            return;
        }

        if (AqTBP >= 25 && DefensesLvl <= 3)
        {
            DefensesLvl++;
            DefensesLvlText.text = "Lv." + DefensesLvl.ToString();
            AqTBP = AqTBP - 25;
            TBPText.text = AqTBP.ToString();
            floatingMoneyText.text = "-25";
            ShowFloatingText(buttonTransform);
            ShowFloatingTextDown();
            PlayerPrefs.SetInt("TBP", AqTBP);


        }
    }

    public void BuyPiranha(Transform transformButton)
    {
        if (AqTBP >= 5)
        {
           floatingMoneyText.text = "-5";
           defenseSelection.defenseFishCount++;
           ShowFloatingText(transformButton);
           ShowFloatingTextDown();
           AqTBP = AqTBP - 5;
           TBPText.text = AqTBP.ToString();
           PlayerPrefs.SetInt("TBP", AqTBP);
        }
        else
        {

            // BU VE ALTTAKÝLER DAHÝL PARASININ OLMADIÐINDA DAÝR BÝR SES ÇALABÝLÝRÝZ YA DA BÝR POP UP ÇIKARABÝLÝRÝZ TABÝ ÝKÝSÝ BÝR DE OLABÝLÝR.
        }
    }

    public void BuyBomb(Transform transformButton)
    {
        if (AqTBP >= 15)
        {
            floatingMoneyText.text = "-15";
            defenseSelection.bombCount++;
            ShowFloatingText(transformButton);
            ShowFloatingTextDown();
            AqTBP = AqTBP - 15;
            TBPText.text = AqTBP.ToString();
            PlayerPrefs.SetInt("TBP", AqTBP);
        }
        else
        {

            // BU VE ALTTAKÝLER DAHÝL PARASININ OLMADIÐINDA DAÝR BÝR SES ÇALABÝLÝRÝZ YA DA BÝR POP UP ÇIKARABÝLÝRÝZ TABÝ ÝKÝSÝ BÝR DE OLABÝLÝR.
        }




    }

    public void SellFish(string sizeCategory)
    {
        GameObject fishToSell = FindFishByCategory(sizeCategory);

        if (fishToSell != null)
        {
            // Balýk yok edilip oyuncuya para eklenir
            Destroy(fishToSell);
            AddMoneyByFishSize(sizeCategory);
            Debug.Log($"{sizeCategory} sold!");
        }
        else
        {
            Debug.Log($"No {sizeCategory} available to sell.");
        }

        CountFishBySize(); // Balýk sayýsýný güncelle
    }

    private GameObject FindFishByCategory(string sizeCategory)
    {
        GameObject[] allFish = GameObject.FindGameObjectsWithTag("EnemyFish");
        foreach (GameObject fish in allFish)
        {
            FishAI_Aquarium fishAI = fish.GetComponent<FishAI_Aquarium>();
            if (fishAI != null && fishAI.sizeCategory == sizeCategory)
            {
                return fish; // Ýlk bulunan uygun balýk döndürülür
            }
        }

        return null; // Uygun balýk yoksa null döndür
    }

    private void AddMoneyByFishSize(string sizeCategory)
    {
        switch (sizeCategory)
        {
            case "Small":
                AqTBP += smallFishSellPrice;
                break;
            case "Medium":
                AqTBP += mediumFishSellPrice;
                break;
            case "MediumLarge":
                AqTBP += mediumLargeFishSellPrice;
                break;
            case "Large":
                AqTBP += largeFishSellPrice;
                break;
        }

        TBPText.text = AqTBP.ToString(); // Para miktarýný UI'da güncelle
        PlayerPrefs.SetInt("TBP", AqTBP);
    }


    public void ResumeGame()
    {
        // Pause menüsünü kapat
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Zamaný normal akýþýna döndür
        isPaused = false; // Duraklama durumunu kapat
    }

    public void PauseGame()
    {
        // Pause menüsünü aç
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Zamaný durdur
        isPaused = true; // Duraklama durumunu aç
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýþ yapýldý!");
        Application.Quit(); // Oyundan çýkýþ yapar (Build'de çalýþýr)
    }

    public void RefillOxygen(Transform buttonTransform)
    {
        int oxygenRefillCost = 15; // Oksijen doldurma maliyeti

        if (AqTBP >= oxygenRefillCost)
        {
            conditionHandler.oxygenLevel = 100f; // Oksijen seviyesini %100'e çýkar
            AqTBP -= oxygenRefillCost; // Paradan düþ
            UpdateTBPUI(); // Parayý güncelle
            AquariumSoundsHandler.Instance.PlayRefillOxygenSound();
            floatingMoneyText.text = "-15";
            ShowFloatingText(buttonTransform);
            ShowFloatingTextDown();// Hareketli yazý göster
            PlayerPrefs.SetInt("TBP", AqTBP);
            Debug.Log("Oksijen dolduruldu!");
        }
        else
        {
            Debug.Log("Yetersiz para!"); 
        }
    }

    public void CleanAquarium(Transform buttonTransform)
    {
        int cleaningCost = 5; // Akvaryum temizleme maliyeti

        if (AqTBP >= cleaningCost)
        {
            conditionHandler.cleanlinessLevel = 100f; // Temizlik seviyesini %100'e çýkar
            AqTBP -= cleaningCost; // Paradan düþ
            UpdateTBPUI(); // Parayý güncelle
            AquariumSoundsHandler.Instance.PlayRefillCleanlinessSound();
            floatingMoneyText.text = "-5";
            ShowFloatingText(buttonTransform);
            ShowFloatingTextDown();// Hareketli yazý göster
            PlayerPrefs.SetInt("TBP", AqTBP);
            Debug.Log("Akvaryum temizlendi!");
        }
        else
        {
            Debug.Log("Yetersiz para!");
        }
    }

    private void UpdateTBPUI()
    {
        TBPText.text = AqTBP.ToString();
    }
    public void UpdateLevelUI()
    {
        foodEffLvlText.text = "Lv." + foodEffLvl.ToString();
        foodLvlText.text = "Lv." + foodLvl.ToString();
        FishLvlText.text = "Lv." + fishLvl.ToString();
        DefensesLvlText.text = "Lv." + DefensesLvl.ToString();
    }

}
