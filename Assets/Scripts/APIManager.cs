using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;

public class APIManager : MonoBehaviour
{
    [Header("API Settings")]
    private string apiUrl = "https://rickandmortyapi.com/api/character";
    private int currentPage = 1;
    private int totalPageCount;

    [Header("UI Elements")]
    public GameObject characterPanel;
    public TMP_Dropdown pageDropdown;
    public Button buttonNext;
    public Button buttonPrevious;

    [Header("UI Characters Details")]
    public GameObject characterDetailPanel, loadingImage;
    public Image characterDetailImage;
    public TextMeshProUGUI characterDetailName;
    public TextMeshProUGUI characterDetailStatus;
    public TextMeshProUGUI characterDetailLocation;
    public TextMeshProUGUI characterDetailFirstSeen;

    [Header("Error UI Elements")]
    public GameObject errorPanel; 
    public TextMeshProUGUI errorPanelMessage; 
    public Button closeErrorButton;

    [Header("Dependencies")]
    public CharacterPool characterPool;
    private Dictionary<int, List<CharacterAPI>> cachedCharacters = new Dictionary<int, List<CharacterAPI>>();
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
    private Dictionary<string, string> episodeCache = new Dictionary<string, string>();
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
        public int pages;
    }

    private void Start()
    {
        
        characterPool = GameObject.Find("CharacterPoolManager").GetComponent<CharacterPool>();
        if(characterPool == null)
        {
            Debug.LogError("No se encontro pool");
            return;
        }
        buttonNext.onClick.AddListener(NextPage);
        buttonPrevious.onClick.AddListener(PreviousPage);
        pageDropdown.onValueChanged.AddListener(OnDropdownPageChanged);
        closeErrorButton.onClick.AddListener(CloseErrorPanel);
        StartCoroutine(GetCharactersCoroutine(currentPage));
    }

    private IEnumerator GetCharactersCoroutine(int page)
    {
        if (cachedCharacters.ContainsKey(page))
        {
            UpdateCharacterUI(cachedCharacters[page]);
            yield break;
        }

        string url = $"{apiUrl}?page={page}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success )
            {
                ShowError("Error en la solicitud: " + request.error);
                yield break;
            }

            try
            {
                string responseData = request.downloadHandler.text;
                CharacterList list = JsonConvert.DeserializeObject<CharacterList>(responseData);

                if (list != null && list.results != null)
                {
                    totalPageCount = list.info.pages;
                    UpdateDropdown(totalPageCount);
                    cachedCharacters[page] = list.results;
                    UpdateCharacterUI(list.results);
                    UpdateButtonState();
                }
                else
                {
                    ShowError("Datos no validos recibidos de la API");
                }
            }
            catch(System.Exception e)
            {
                ShowError("Error al procesar los datos " + e.Message);
            }
        }
    }
    private void ChangePage(int newPage)
    {
        if(currentPage != newPage)
        {
            currentPage = newPage;
            if(cachedCharacters.ContainsKey(currentPage))
            {
                UpdateCharacterUI(cachedCharacters[currentPage]);
            }
            else
            {
                StartCoroutine(GetCharactersCoroutine(currentPage));
            }
            UpdateButtonState();
            UpdateDropdownSelection();
        }
    }

     public void ShowCharacterDetails(CharacterAPI character)
    {
        characterPanel.SetActive(false);
        characterDetailPanel.SetActive(true);

        characterDetailName.text = character.name;
        characterDetailStatus.text = $"Status: {character.status}";
        characterDetailLocation.text = $"Location: {character.location.name}";

        loadingImage.SetActive(true);

        characterDetailImage.sprite = Resources.Load<Sprite>("UI/Charging");
        characterDetailImage.gameObject.SetActive(true);

        if (!imageCache.ContainsKey(character.image))
        {
            StartCoroutine(LoadImageCoroutine(character.image));
        }
        else
        {
            characterDetailImage.sprite = imageCache[character.image];
            characterDetailImage.gameObject.SetActive(true);
            loadingImage.SetActive(false);
        }
        characterDetailFirstSeen.text = "Cargando...";

        if (character.episode != null && character.episode.Count > 0)
        {
            string firstEpisodeUrl = character.episode[0];
            if(episodeCache.ContainsKey(firstEpisodeUrl))
            {
                characterDetailFirstSeen.text = $"First Seen In: {episodeCache[firstEpisodeUrl]}";
            }
            else
            {
                StartCoroutine(GetFirstEpisodeNameCoroutine(firstEpisodeUrl));
            }
        }
    }


    public void OnDropdownPageChanged(int selectedPageIndex)
    {
        int selectedPage = selectedPageIndex + 1;
        if(selectedPage != currentPage)
        {
            ChangePage(selectedPage);
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
    private void UpdateDropdownSelection()
    {
        pageDropdown.value = currentPage - 1;
        pageDropdown.RefreshShownValue();
    }

    private void UpdateCharacterUI(List<CharacterAPI> characters)
    {
        foreach (Transform child in characterPanel.transform)
        {
            child.gameObject.SetActive(false);
        }

        for(int i=0; i< characters.Count; i++)
        {
            GameObject newCharacterItem = characterPool.GetCharacter();
            if(newCharacterItem != null)
            {
                newCharacterItem.transform.SetParent(characterPanel.transform, false);
                newCharacterItem.transform.Find("NameCharacter").GetComponent<TextMeshProUGUI>().text = characters[i].name;

                Button button = newCharacterItem.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                int index = i;
                button.onClick.AddListener(() => ShowCharacterDetails(characters[index]));
                newCharacterItem.SetActive(true);
            }
        }
    }
    
    private IEnumerator LoadImageCoroutine(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            if(request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imageCache[imageUrl] = sprite;
                characterDetailImage.sprite = sprite;
                characterDetailImage.gameObject.SetActive(true);
                loadingImage.SetActive(false);
            }
            else
            {
                ShowError("Error al cargar la imagen " + request.error);
                loadingImage.SetActive(false);
            }
        }
    }

    
    private IEnumerator GetFirstEpisodeNameCoroutine(string episodeUrl)
    {
         using (UnityWebRequest request = UnityWebRequest.Get(episodeUrl))
         {
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                ShowError("Error al cargar el episodio: " + request.error);
                characterDetailFirstSeen.text = "First Seen In: Unknown";
            }
            else
            {
                try
                {
                    string responseData = request.downloadHandler.text;
                    EpisodeAPI episodeAPI = JsonConvert.DeserializeObject<EpisodeAPI>(responseData);
                    if (episodeAPI != null)
                    {
                        string episodeName = episodeAPI.name;
                        if(!episodeCache.ContainsKey(episodeUrl))
                        {
                            episodeCache[episodeUrl] = episodeName;
                        }
                        characterDetailFirstSeen.text = $"First Seen In: {episodeName}";
                    }
                    else
                    {
                        characterDetailFirstSeen.text = "First Seen In: Unknown";
                    }
                }
                catch(System.Exception e)
                {
                    ShowError("Error al cargar la primera vista " + e.Message);
                    characterDetailFirstSeen.text = "First Seen In: Unknown";
                }
            }
        }
    }
  
    public void CloseCharacterDetails()
    {
        characterDetailPanel.SetActive(false);
        characterPanel.SetActive(true);
    }
    private void ShowError(string message)
    {
        Debug.LogError(message);
        errorPanelMessage.text = message;
        errorPanel.SetActive(true);
    }
    
    private void CloseErrorPanel()
    {
        errorPanel.SetActive(false);
    }

    private void UpdateButtonState()
    {
        buttonNext.interactable = currentPage < totalPageCount;
        buttonPrevious.interactable = currentPage > 1;
    }

    public void NextPage()
    {
        if(currentPage < totalPageCount)
        {
            ChangePage(currentPage + 1);
            UpdateDropdownSelection();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 1)
        {
            ChangePage(currentPage - 1);
            UpdateDropdownSelection();
        }
    }
}