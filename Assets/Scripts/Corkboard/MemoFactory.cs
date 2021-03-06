using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoFactory : MonoBehaviour
{
    private static MemoFactory _instance;
    public static MemoFactory instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<MemoFactory>();
            }
            return _instance;
        }
    }

    [SerializeField]
    private Transform memoHolder;

    [SerializeField]
    private Memo memoPrefab;

    [SerializeField]
    private RectTransform defaultRectTransform;

    [SerializeField]
    private RectTransform defaultSpawnRange;

    [SerializeField]
    private int baseNewId = 200;
    
    public delegate void MemoCreatedDelegate(Memo newMemo);
    public event MemoCreatedDelegate OnMemoCreated;

    void Start()
    {
        if (!_instance)
        {
            _instance = this;
        }
    }

    public Memo CreateNew(int memoId, string subtitle, string message, List<Emote> emotes, Vector2 position, Vector2 size, bool highlighted = true, bool editable = false)
    {        
        MemoData memoData = new MemoData();
        memoData.title = CreateTitle();
        memoData.subtitle = subtitle;
        memoData.position = position;
        memoData.size = size;
        memoData.highlighted = highlighted;
        memoData.editable = editable;
        memoData.memoId = memoId;
        memoData.message = message;
        memoData.emotes = emotes;        

        return CreateFromData(memoData);
    }

    public Memo CreateNew(string subtitle, string message, Vector2 position, bool highlighted = true, bool editable = false)
    {
        Vector2 defaultSize = defaultRectTransform ? defaultRectTransform.sizeDelta : new Vector2(300, 256);
        return CreateNew(++baseNewId, subtitle, message, null, position, defaultSize, highlighted, editable);
    }

    public Memo CreateNew(string subtitle, List<Emote> emotes, Vector2 position, bool highlighted = true)
    {
        Vector2 defaultSize = defaultRectTransform ? defaultRectTransform.sizeDelta : new Vector2(300, 256);
        return CreateNew(++baseNewId, subtitle, "", emotes, position, defaultSize, highlighted, false);
    }

    public Memo CreateNew(Vector2 position, bool highlighted = true)
    {        
        return CreateNew("", "", position, highlighted, true);
    }

    public Memo CreateNew(string subtitle, string message, bool highlighted = true, bool editable = false)
    {
        Vector2 spawnPosition = GetNewMemoPosition();
        return CreateNew(subtitle, message, spawnPosition, highlighted, editable);
    }

    public Memo CreateNew(string subtitle, List<Emote> emotes, bool highlighted = true)
    {
        Vector2 spawnPosition = GetNewMemoPosition();
        return CreateNew(subtitle, emotes, spawnPosition, highlighted);
    }

    public Memo CreateNew(bool highlighted = true)
    {
        return CreateNew("", "", highlighted, true);
    }

    public Memo CreateFromData(MemoData memoData, Vector2 position)
    {
        Memo newMemo = Instantiate(memoPrefab, memoHolder);
        memoData.position = position;
        newMemo.Data = memoData;

        OnMemoCreated?.Invoke(newMemo);

        return newMemo;
    }

    public Memo CreateFromData(MemoData memoData)
    {
        return CreateFromData(memoData, memoData.position);
    }

    private string CreateTitle()
    {
        if (Service.Game)
        {
            return Service.Game.CurrentTimeOfDay.ToString() + " " + Service.Game.CurrentDay;
        }

        return "";
    }

    private Vector2 GetNewMemoPosition()
    {
        if (defaultSpawnRange)
        {
            Vector2 spawnOffset = new Vector2(
                Random.Range(defaultSpawnRange.offsetMin.x, defaultSpawnRange.offsetMax.x),
                Random.Range(defaultSpawnRange.offsetMin.y, defaultSpawnRange.offsetMax.y)
                );

            return spawnOffset;
        }
        else if (defaultRectTransform)
        {
            return defaultRectTransform.position;
        }
        else
        {
            return Vector2.zero;
        }
    }
}
