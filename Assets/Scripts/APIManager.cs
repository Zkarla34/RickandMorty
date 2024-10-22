using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;

public class APIManager : MonoBehaviour
{
    [Header("API Settings")]
    private string apiUrl = "https://rickandmortyapi.com/api/character";
    private int currentPage = 1;
    private int totalPageCount;

    [Header("UI Elements")]
    public GameObject characterPanel;
    public Button characterPrefab;
    public TextMeshProUGUI pageNumberText;
    public TMP_Dropdown pageDropdown;
    public TextMeshProUGUI errorMesage;

    [Header("UI Characters Details")]
    public GameObject characterDetailPanel;
    public Image characterDetailImage;
    public TextMeshProUGUI characterDetailName;
    public TextMeshProUGUI characterDetailStatus;
    public TextMeshProUGUI characterDetailLocation;
    public TextMeshProUGUI characterDetailFirstSeen;

    [Header("Dependencies")]
    public CharacterPool characterPool;
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
    public List<CharacterAPI> characterInfo = new List<CharacterAPI>();


    //Data Structures
    [System.Serializable]
    public class CharacterAPI
    {
        public int id;
        public string name;
        public string status;
        public string image;
        public Location location;
        public List<string> episode;
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

    [System.Serializable]
    public class EpisodeAPI
    {
        public string name;
    }

    [System.Serializable]
    public class Info
    {
        public int count;
        public int pages;
        public string next;
        public string prev;
    }

    private async void Start()
    {
        characterPool = GameObject.Find("CharacterPoolManager").GetComponent<CharacterPool>();
        if(characterPool == null)
        {
            Debug.LogError("No se encontro");
            return;
        }
        pageDropdown.onValueChanged.AddListener(OnDropdownPageChanged);
        await GetCharactersAsync(currentPage);
    }

    private async Task GetCharactersAsync(int page)
    {
        string url = $"{apiUrl}?page={page}";
        using UnityWebRequest request = UnityWebRequest.Get(url);
        {
            var operation = request.SendWebRequest();
            while(!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError)
            {
               ShowError("Error de la solicitud: " + request.error);
            }
            else
            {
                string responseData = request.downloadHandler.text;
                CharacterList list = JsonConvert.DeserializeObject<CharacterList>(responseData);

                if (list != null && list.results != null)
                {
                    totalPageCount = list.info.pages;
                    UpdateDropdown(totalPageCount); 
                    UpdateCharacterUI(list.results);
                }
                else
                {
                    ShowError("La respuesta no contiene datos o esta vacia");
                }
            }
        }  
    }

    private async Task<string> GetFirstEpisodeNameAsync(string episodeUrl)
    {
        try
        {
            using UnityWebRequest request = UnityWebRequest.Get(episodeUrl);
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.ConnectionError
                       || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    ShowError("Error al cargar los episodios: " + request.error);
                    characterDetailFirstSeen.text = "First Seen In: Error al cargar el episodio";
                    return "Error al cargar el episodio";
                }
                else
                {
                    string responseData = request.downloadHandler.text;
                    EpisodeAPI episode = JsonConvert.DeserializeObject<EpisodeAPI>(responseData);
                    if(episode != null)
                    {
                        characterDetailFirstSeen.text = "First Seen In: " + episode.name;
                        return episode.name;
                    }
                    else
                    {
                        ShowError("Error: La respuesta no contiene datos validos");
                        characterDetailFirstSeen.text = "First Seen In: Datos no disponibles";
                        return "Datos no disponibles";
                    }
                }
            }
        }
        catch(Exception e)
        { 
            ShowError($"Error al cargar la primera vez visto: {e.Message}");
            characterDetailFirstSeen.text = "First Seen In: Error inesperado";
            return "Datos no disponibles";
        }
    }
    


    private void UpdateDropdown(int totalPages)
    {
        pageDropdown.ClearOptions();
        List<string> pageOptions = new List<string>();
        for(int i = 1; i <= totalPages; i++)
        {
            pageOptions.Add(i.ToString());
        }
        pageDropdown.AddOptions(pageOptions);
        pageDropdown.value = currentPage - 1;
        pageDropdown.RefreshShownValue();
    }

    public async void OnDropdownPageChanged(int selectedPageIndex)
    {
        int selectedPage = selectedPageIndex + 1;
        currentPage = selectedPage;
        await GetCharactersAsync(selectedPage);
    }

    private void UpdateCharacterUI(List<CharacterAPI> characters)
    {
        characterInfo.Clear();
        characterInfo.AddRange(characters);

        foreach (CharacterAPI character in characterInfo)
        {
            GameObject newCharacterItem = characterPool.GetCharacter();
            if(newCharacterItem != null)
            {
                newCharacterItem.transform.SetParent(characterPanel.transform, false);
                newCharacterItem.transform.Find("NameCharacter").GetComponent<TextMeshProUGUI>().text = character.name;
                Debug.Log("personaje: " + character.name);
                newCharacterItem.GetComponent<Button>().onClick.AddListener(() => ShowCharacterDetails(character));
            }
            
        }
    }

    
    private async void LoadImageAsync(string imageUrl, Image targetImage)
    {
        if(imageCache.TryGetValue(imageUrl, out Sprite cachedSprite))
        {
            targetImage.sprite = cachedSprite;
        }
        else
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            {
                var operation = request.SendWebRequest();
                while(!operation.isDone)
                {
                    await Task.Yield();
                }
                if (request.result == UnityWebRequest.Result.ConnectionError
                    || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    ShowError("Error al cargar la imagen: " + request.error);
                }
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    targetImage.sprite = sprite;
                    imageCache[imageUrl] = sprite;
                }
            }
        }
    }
    

    public async void ShowCharacterDetails(CharacterAPI character)
    {
        characterPanel.SetActive(false);
        
        characterDetailPanel.SetActive(true);
        characterDetailName.text = character.name;
        characterDetailStatus.text = $"Status: {character.status}";
        characterDetailLocation.text = $"Location: {character.location.name}";

        if(character.episode != null && character.episode.Count > 0)
        {
            string firstEpisodeUrl = character.episode[0];
            string firstEpisodeName = await GetFirstEpisodeNameAsync(firstEpisodeUrl);
            characterDetailFirstSeen.text = $"First Seen In: { firstEpisodeName}";
        }
        else
        {
            characterDetailFirstSeen.text = "First Seen In: Unknown";
        }

        LoadImageAsync(character.image, characterDetailImage);
    }

    public void CloseCharacterDetails()
    {
        characterDetailPanel.SetActive(false);
        characterPanel.SetActive(true);
    }
    private void ShowError(string message)
    {
        Debug.LogError(message);
        errorMesage.text = message;
        errorMesage.gameObject.SetActive(true);
    }

    public async void NextPage()
    {
        if(currentPage < totalPageCount)
        {
            currentPage++;
            pageDropdown.value = currentPage;
            await GetCharactersAsync(currentPage);
        }
    }

    public async void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            pageDropdown.value = currentPage;
            await GetCharactersAsync(currentPage);
        }
    }
}