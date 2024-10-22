using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPool : MonoBehaviour
{
    public static CharacterPool Instance { get; private set;}

    public GameObject characterPrefab;
    public int poolSize;
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
        if(pool.Count > 0)
        {
            GameObject character = pool.Dequeue();
            if(character != null)
            {
                character.SetActive(true);
                return character;
            }
            else
            {
                Debug.LogWarning("El objeto de personaje ha sido destruido, intentando obtener otro.");
                return GetCharacter();
            }
        }
        else
        {
            Debug.LogWarning("No hay personajes disponibles en el pool.");
            return null;
        }
        
   }

    public void ResetCharacter(GameObject character)
    {
        character.SetActive(false);
        character.transform.SetParent(transform);
        pool.Enqueue(character);
    }
}