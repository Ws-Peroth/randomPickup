﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.ComponentModel;
using UnityEngine.SceneManagement;

public class PickUp : MonoBehaviour
{
    public Text count;
    public Text havingCharactorText;

    int minRarity = 1;
    int maxRarity = 5;

    int commonRarity = 1;
    int rareRarity = 2;
    int epicRarity = 3;
    int legendRarity = 4;
    int eventLegendRarity = 5;

    int topNumber = 90;
    int rotationNumber = 10;
    public bool isPickUpTurn = false;
    bool isPiking = false;

    public int pickupCount = 0;
    public int pickupEventCount = 0;

    CharactorPool charactorPool;
    string path;

    float pickupPercent;
    int pickupCharactorNumber;
    public int picktryEvent;
    public int picktry;

    float[] rarity = new float[] { 50.0f, 35.0f, 10.0f, 0.3f };
    // 0 : common
    // 1 : rare
    // 2 : epic
    // 3 : legend
    // 4 : legend_event

    void LoadData()
    {
        // json에서 값 불러오기
        if (!File.Exists(path))
        {
            return;
        }
        else
        {
            var loadJson = File.ReadAllText(path);
            charactorPool = JsonConvert.DeserializeObject<CharactorPool>(loadJson);
        }
    }

    void UpdateData()
    {
        // 데이터 json에 저장
        string initJson = JsonConvert.SerializeObject(charactorPool, Formatting.Indented);
        File.WriteAllText(path, initJson);
    }

    void InitData()
    {
        path = Application.dataPath + "/charactorInformation.json";

        string[] charactorList_Common = new string[] { "Com_A", "Com_B", "Com_C", "Com_D", "Com_E" };
        string[] charactorList_Rare = new string[] { "Rar_F", "Rar_G", "Rar_H", "Rar_I", "Rar_J" };
        string[] charactorList_Epic = new string[] { "Epi_K", "Epi_L", "Epi_M", "Epi_N", "Epi_O" };

        string[] charactorList_Legendary = new string[] { "LEG_P", "LEG_Q", "LEG_R", "LEG_S", "LEG_T" };
        string[] charactorList_PickupLegendary = new string[] { "EV_PickUp_A", "EV_PickUp_B", "EV_LEG_P", "EV_LEG_Q", "EV_LEG_R", "EV_LEG_S", "EV_LEG_T" };

        pickupPercent = 50.0f;
        pickupCharactorNumber = 2;
        picktry = 0;
        picktryEvent = 0;

        bool isExists = File.Exists(path);

        if (!isExists)
        {
            // Charactors 형태의 빈 파일 생성
            string initJson = JsonConvert.SerializeObject(new CharactorPool() { charactorListWithRarity = new List<CharactorList>() }, Formatting.Indented);
            File.WriteAllText(path, initJson);

            // 생성된 파일을 읽어옴, 그 후 Charactors 형으로 변형
            var loadJson = File.ReadAllText(path);
            CharactorPool initCharactor = JsonConvert.DeserializeObject<CharactorPool>(loadJson);

            AddInitData(initCharactor, charactorList_Common, commonRarity);
            AddInitData(initCharactor, charactorList_Rare, rareRarity);
            AddInitData(initCharactor, charactorList_Epic, epicRarity);
            AddInitData(initCharactor, charactorList_Legendary, legendRarity);
            AddInitData(initCharactor, charactorList_PickupLegendary, eventLegendRarity);

            string json = JsonConvert.SerializeObject(initCharactor, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        LoadData();
    }

    void AddInitData(CharactorPool initCharactor, string[] charactorList, int rarity)
    {
        initCharactor.charactorListWithRarity.Add(new CharactorList());

        foreach (string charactor in charactorList)
        {
            

            initCharactor.charactorListWithRarity[rarity - 1].charactor.Add(
                new Charactor()
                {
                    Name = charactor,
                    HavingCount = 0
                }
            );
        }
    }

    bool IsPercentPass(float passPoint) 
    {
        return Random.Range(0.0f, 100.0f) <= passPoint;
    }

    string ReturnGetCharactor(int rarity, bool IsPickup) 
    {
        rarity = IsPickup && (rarity == legendRarity) ? eventLegendRarity : rarity;

        int listLength = charactorPool.charactorListWithRarity[rarity - 1].charactor.Count;

        string pickCharactor;
        int pickedIndex;

        if (rarity >= legendRarity)
        {
            if (IsPickup)
            {
                if (IsPercentPass(pickupPercent) || isPickUpTurn)
                {
                    pickedIndex = Random.Range(0, pickupCharactorNumber);
                    picktryEvent = 0;
                    isPickUpTurn = false;
                }
                else
                {
                    pickedIndex = Random.Range(pickupCharactorNumber - 1, listLength);
                    picktryEvent = 0;
                    isPickUpTurn = true;
                }

            }
            else
            {
                pickedIndex = Random.Range(0, listLength);
                picktry = 0;
            }
        }
        else
        {
            pickedIndex = Random.Range(0, listLength);               
        }

        charactorPool.charactorListWithRarity[rarity - 1].charactor[pickedIndex].HavingCount++;
        pickCharactor = charactorPool.charactorListWithRarity[rarity - 1].charactor[pickedIndex].Name;

        Debug.Log("pickup charactor = " + pickCharactor);
        return pickCharactor;
    }

    int SetRarity() 
    {
        for(int i = rarity.Length -1; i >= minRarity -1; i--)
        {
            if (i == 0) return minRarity;
            else if (IsPercentPass(rarity[i])) return i + 1;
        }
        return 0;
    }

    public void EventCharactorPickUp()
    {
        Debug.Log("이벤트 가챠");

        if (!isPiking)
        {
            isPiking = true;
            for (int i = 0; i < 10; i++)
            {
                pickupEventCount++;
                picktryEvent++;

                if (picktryEvent == topNumber && isPickUpTurn) // 천장
                {
                    int pickedIndex = Random.Range(0, pickupCharactorNumber);
                    charactorPool.charactorListWithRarity[eventLegendRarity - 1].charactor[pickedIndex].HavingCount++;
                    Debug.Log("pickup charactor = " + charactorPool.charactorListWithRarity[eventLegendRarity - 1].charactor[pickedIndex].Name);
                    isPickUpTurn = false;
                    picktryEvent = 0;
                }
                else if (picktryEvent == topNumber) // 반천장
                {
                    ReturnGetCharactor(eventLegendRarity, true);
                }
                else if (picktryEvent % rotationNumber == 0) // 10연차에 Epic 확정
                    ReturnGetCharactor(epicRarity, true);

                else
                    ReturnGetCharactor(SetRarity(), true);
                UpdateData();
            }
        }
        isPiking = false;
    }

    public void CharactorPickUp() 
    {
        Debug.Log("상시 가챠");

        if (!isPiking) {
            isPiking = true;
            for(int i = 0; i < 10; i++)
            {
                pickupCount++;
                picktry++;

                if (picktry == topNumber)  // 상시 천장
                {
                    ReturnGetCharactor(legendRarity, false);
                }

                else if (picktry % rotationNumber == 0) // 10연차에 Epic 확정
                    ReturnGetCharactor(epicRarity, false);

                else
                    ReturnGetCharactor(SetRarity(), false);

                UpdateData();
            }
        }

        isPiking = false;

    }

    public void ResetPickup()
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }catch(IOException e)
            {
                print(e.Message);
                return;
            }
        }

        InitData();

        pickupCount = 0;
        pickupEventCount = 0;
        picktryEvent = 0;
        picktry = 0;

    }

    public void ShowHavingCharactorList()
    {
        string[] rarityName = new string[] { "Common","Rare","Epic","Legend","Event Legend" };

        var loadJson = File.ReadAllText(path);
        CharactorPool havingCharactor = JsonConvert.DeserializeObject<CharactorPool>(loadJson);

        for(int i = 0; i < maxRarity; i++)
        {

        }

    }

    void Start()
    {
        InitData();
    }

    private void Update()
    {
        count.text = "상시 뽑기 : " + pickupCount + "\n이벤트 뽑기 : " + picktryEvent;
    }
}

//하나의 캐릭터의 정보를 담고 있는 클래스
public class Charactor
{
    public string Name { get; set; }
    public int HavingCount { get; set; }
}
//특정 레어도를 가진 캐릭터의 리스트
public class CharactorList
{
    public List<Charactor> charactor = new List<Charactor>();
}
//캐릭터 클래스
public class CharactorPool
{
    public List<CharactorList> charactorListWithRarity = new List<CharactorList>();
}