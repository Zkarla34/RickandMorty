using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterPool : MonoBehaviour
{
    public static CharacterPool Instance { get; private set;}

    public GameObject characterPrefab;
    public int poolSize;
    public Transform parentTransform;
    private List<GameObject> pool;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject character = Instantiate(characterPrefab, parentTransform);
            RectTransform rect = character.GetComponent<RectTransform>();
            if(rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            character.SetActive(false);
            pool.Add(character);
        }
    }
   public GameObject GetCharacter()
   {
        foreach(var obj in pool)
        {
            if(!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        GameObject character = Instantiate(characterPrefab, parentTransform);
        RectTransform rect = character.GetComponent<RectTransform>();
        if(rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }
        character.SetActive(true);
        pool.Add(character);
        return character;
   }
 }  