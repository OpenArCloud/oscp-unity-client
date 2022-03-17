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


    public void SetValues(JSONNode item)
    {
        Debug.Log(item);
        id.text = item["id"];
        type.text = item["type"];
        URL.text = item["url"];
        title.text = item["title"];
        description.text = item["description"];
    }
}
