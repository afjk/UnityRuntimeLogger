using System.Collections;
using System.Collections.Generic;
using com.afjk.RuntimeLogger;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LogView
{
    private Transform parent;
    private LogItem item;
    public int maxLogCount = 100;

    private Queue<LogItem> itemUiList = new ();
    private Queue<string> itemList = new ();
    
    public LogView(Transform parent, LogItem item)
    {
        this.parent = parent;
        this.item = item;
    }
    
    public void Add(string log, LogType type)
    {
        itemList.Enqueue(log);
        var uiItem = GameObject.Instantiate(item);
        uiItem.transform.parent = parent;
        uiItem.transform.localPosition = Vector3.zero;
        uiItem.transform.localRotation = Quaternion.identity;
        uiItem.transform.localScale = Vector3.one;
        uiItem.gameObject.SetActive(true);
        uiItem.AddText(log, type);
        itemUiList.Enqueue(uiItem);
        
        // ログが最大行数を超えた場合、古いログを削除
        if (itemList.Count > maxLogCount)
        {
            itemList.Dequeue();
            itemUiList.Dequeue();
        }
    }

    public void clear()
    {
        itemList.Clear();
        itemUiList.Clear();
    }
}

public class RuntimeLoggerUI : MonoBehaviour
{
    [SerializeField]
    private LogItem logItem;

    [SerializeField] private Transform content;

    [SerializeField] private ScrollRect scrollRect;
    RuntimeLogSender.LogFilter filter = (logString, stackTrace, type) => logString.StartsWith("[Remote]");

    private bool isLogging;
    public int maxLogCount = 100;
    private LogView logView;
    private bool showNewest = true;
    
    void OnEnable()
    {
        logView = new LogView(content, logItem);
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        
        logView.clear();
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {

        if (isLogging && (filter == null || !filter(logString, stackTrace, type)))
        {
            // ログをリストに追加
            logView.Add(logString, type);
            if (showNewest)
            {
                ScrollToBottom();
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        isLogging = true;
    }
    
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    public void OnVarueChaned(Vector2 val)
    {
        if (val.y > 0)
        {
            showNewest = false;
        }
        else
        {
            showNewest = true;
        }
    }
}
