using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;

public class APIManager : MonoBehaviour
{
    private string apiUrl = "https://rickandmortyapi.com/api/character";
    private int currentPage = 1;

    //Lista para los personajes
    public List<CharacterAPI> characterInfo = new List<CharacterAPI>();

    public GameObject characterPanel;
    public GameObject characterPrefab;

    public class CharacterAPI
    {
        public int id;
        public string name;
        public string status;
        public Location location;
    }

    [System.Serializable]
    public class Location
    {
        public string name;
    }

    [System.Serializable]
    public class CharacterList
    {
        public Info info;
        public List<CharacterAPI> results;
    }

    public class Info
    {
        public int count;
        public int pages;
        public string next;
        public string prev;
    }

    private void Start()
    {
        GetCharacters(currentPage);
    }

    public void GetCharacters(int page)
    {
        StartCoroutine(GetDataCharacters(page));
    }

    // Coroutine para obtener personajes de la API
    private IEnumerator GetDataCharacters(int page)
    {
        string url = $"{apiUrl}?page={page}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError
            || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error de la solicitud: " + request.error);
        }
        else
        {
            string responseData = request.downloadHandler.text;
            CharacterList list = JsonConvert.DeserializeObject<CharacterList>(responseData);
            if (list != null && list.results != null)
            {
                characterInfo.Clear();
                characterInfo.AddRange(list.results);

                foreach (Transform child in characterPanel.transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (CharacterAPI character in characterInfo)
                {
                    GameObject newCharacterItem = Instantiate(characterPrefab, characterPanel.transform);
                    newCharacterItem.transform.Find("NameCharacter").GetComponent<TextMeshProUGUI>().text = character.name;
                    Debug.Log("personaje: " + character.name);

                }
            }
            else
            {
                Debug.LogError("La respuesta no contiene datos o esta vacia");
            }


        }
    }

    public void NextPage()
    {
        currentPage++;
        GetCharacters(currentPage);
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            GetCharacters(currentPage);
        }
    }
}


