using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DayHandler : MonoBehaviour
{
    public int CurrentDay { get { return currentDay; } }
    public TMP_Text dayText;
    private int currentDay = 1; // Başlangıç günü
    private float timer = 0f; // ZamanlayıcıLoadGame
    private SaveManager saveManager;
    private Aquarium_gameHandler gameHandler;
    private ConditionHandler conditionHandler;
    public float dayDuration = 60f; // Bir günün süresi (saniye cinsinden)
    private string Language;


    private void Awake()
    {

        saveManager = FindObjectOfType<SaveManager>();
        gameHandler = FindObjectOfType<Aquarium_gameHandler>();
        conditionHandler = FindObjectOfType<ConditionHandler>();

        saveManager.LoadGame(gameHandler, conditionHandler);

    }
    private void Start()
    {
        Language = PlayerPrefs.GetString("Language", "English");
        UpdateDayText(); // İlk gün metnini göster
    }

    private void Update()
    {
       
        timer += Time.deltaTime;

        // Gün değişim süresine ulaşıldığında
        if (timer >= dayDuration)
        {
            timer = 0f; // Zamanlayıcıyı sıfırla
            currentDay++; // Günü artır
            UpdateDayText(); // Gün sayısını güncelle

            if (currentDay % 2 == 0)
            {
                saveManager.SaveGame(gameHandler, conditionHandler); // SaveManager üzerinden kaydetme işlemi
                Debug.Log("Game saved automatically on day: " + currentDay);
            }
        }
    }

    private void UpdateDayText()
    {
        // Gün sayısını UI üzerinde güncelle
        if (dayText != null)
        {
            switch (Language)
            {
                case "English":
                    dayText.text = $"DAY: {currentDay}";
                    break;
                case "Turkish":
                    dayText.text = $"GÜN: {currentDay}";
                    break;
                case "French":
                    dayText.text = $"JOUR: {currentDay}";
                    break;
                case "Italian":
                    dayText.text = $"GIORNO: {currentDay}";
                    break;
                case "German":
                    dayText.text = $"TAG: {currentDay}";
                    break;
                case "Spanish":
                    dayText.text = $"DÍA: {currentDay}";
                    break;


            }
            
        }
    }

    public void SaveGame()
    {
        saveManager.SaveGame(gameHandler, conditionHandler);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetCurrentDay(int day)
    {
        currentDay = day;
        UpdateDayText(); // UI'yi güncelle
    }
}
