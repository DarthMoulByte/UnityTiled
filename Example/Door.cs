using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Sprite _closedSprite;

    [SerializeField] private Sprite _openSprite;

    [SerializeField] private bool _defaultOpen;

    void Start()
    {
        SetCorrectSprite();
    }

    private void SetCorrectSprite()
    {
        if (_defaultOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    void Open()
    {
        GetComponent<SpriteRenderer>().sprite = _openSprite;
    }

    void Close()
    {
        GetComponent<SpriteRenderer>().sprite = _closedSprite;
    }

    void OnTmxMapImported()
    {
        SetCorrectSprite();
    }
}