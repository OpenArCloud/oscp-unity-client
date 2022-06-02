using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;


//SSR = spatial service record
public class SSRItem : MonoBehaviour
{

    [SerializeField] private Text id;
    [SerializeField] private Text type;

    [SerializeField] private Text URL;
    [SerializeField] private Text title;
    [SerializeField] private Text description;

    [SerializeField] private Button button;
    [SerializeField] private Image selectedImage;

    public bool IsSelected = false;

    public void SetValues(JSONNode item)
    {
        Debug.Log(item);
        id.text = item["id"];
        type.text = item["type"];
        URL.text = item["url"];
        title.text = item["title"];
        description.text = item["description"];
    }

    public string GetURL()
    {
        return URL.text;
    }

    public void HandleClick()
    {
        IsSelected = !IsSelected;
        selectedImage.enabled = IsSelected;
    }

}
