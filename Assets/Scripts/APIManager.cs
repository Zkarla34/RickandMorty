using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class APIManager : MonoBehaviour
{
    [Header("API Settings")]
    private string apiUrl = "https://rickandmortyapi.com/api/character";
    private int currentPage = 1;
    [SerializeField] private int charactersPerPage;

    [Header("UI Elements")]
    public GameObject characterPanel;
    public GameObject characterPrefab;
    public TextMeshProUGUI pageNumberText;
    public Slider pageSlider;
    public TextMeshProUGUI errorMesage;

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
        public string species;
        public string image;
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
                    UpdateUI(list.info.pages); 
                    UpdateCharacterUI(list.results);
                    
                }
                else
                {
                    ShowError("La respuesta no contiene datos o esta vacia");
                }
            }
        }  
    }

    private void UpdateUI(int totalPages)
    {
        pageSlider.maxValue = totalPages;
        pageNumberText.text = $"Page:{currentPage}";
    }

    private void UpdateCharacterUI(List<CharacterAPI> characters)
    {
        characterInfo.Clear();
        characterInfo.AddRange(characters);

        foreach (CharacterAPI character in characterInfo)
        {
            GameObject newCharacterItem = characterPool.GetCharacter();
            newCharacterItem.transform.SetParent(characterPanel.transform, false);
            newCharacterItem.transform.Find("NameCharacter").GetComponent<TextMeshProUGUI>().text = character.name;
            Debug.Log("personaje: " + character.name);

            //Image characterImage = newCharacterItem.transform.Find("CharacterImage").GetComponent<Image>();
           // LoadImageAsync(character.image, characterImage);
        }
    }

    /*
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
    */
    private void ShowError(string message)
    {
        Debug.LogError(message);
        errorMesage.text = message;
        errorMesage.gameObject.SetActive(true);
    }

    public async void OnSliderValueChanged()
    {
        int pageNumber = Mathf.RoundToInt(pageSlider.value);
        pageNumberText.text = $"Page: {pageNumber}";
        currentPage = pageNumber;
        await GetCharactersAsync(currentPage);
    }
    public async void NextPage()
    {
        if(currentPage < pageSlider.maxValue)
        {
            currentPage++;
            pageSlider.value = currentPage;
            await GetCharactersAsync(currentPage);
        }
    }

    public async void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            pageSlider.value = currentPage;
            await GetCharactersAsync(currentPage);
        }
    }
}