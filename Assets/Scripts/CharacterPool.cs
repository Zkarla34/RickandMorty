using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPool : MonoBehaviour
{
    public GameObject characterPrefab;
    public int poolSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < poolSize; i++)
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
}
