using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPool : MonoBehaviour
{
    public static CharacterPool Instance { get; private set;}

    public GameObject characterPrefab;
    public int poolSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

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
        for (int i = 0; i < poolSize; i++)
        {
            GameObject character = Instantiate(characterPrefab);
            character.SetActive(false);
            pool.Enqueue(character);
        }
    }
   public GameObject GetCharacter()
   {
        GameObject character = pool.Dequeue();
        character.SetActive(true);
        pool.Enqueue(character);
        return character;
   }

    public void ResetCharacter(GameObject character)
    {
        character.SetActive(false);
        character.transform.SetParent(transform);
        pool.Enqueue(character);
    }
}