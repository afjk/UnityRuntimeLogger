using System.Collections;
using System.Collections.Generic;
using com.afjk.RuntimeLogger;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace com.afjk.RuntimeLogger.Sample
{
    public class LogView
    {
        private Transform parent;
        private LogItem item;
        public int maxLogCount;

        private Queue<string> itemList = new();
        private Queue<LogItem> itemUiList = new();
        private Stack<LogItem> objectPool = new();

        public LogView(Transform parent, LogItem item, int maxLogCount)
        {
            this.parent = parent;
            this.item = item;
            this.maxLogCount = maxLogCount;
        }

        public void Add(string log, LogType type)
        {
            itemList.Enqueue(log);
            var uiItem = GetOrCreateLogItem();
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
                var item = itemUiList.Dequeue();
                ReturnToPool(item);
            }
        }

        private LogItem GetOrCreateLogItem()
        {
            LogItem logItem;
            if (objectPool.Count > 0)
            {
                logItem = objectPool.Pop();
            }
            else
            {
                logItem = GameObject.Instantiate(item);
            }

            return logItem;
        }

        private void ReturnToPool(LogItem logItem)
        {
            logItem.gameObject.SetActive(false);
            logItem.transform.parent = null;
            objectPool.Push(logItem);
        }

        public void Clear()
        {
            // プール内のオブジェクトを破棄
            while (objectPool.Count > 0)
            {
                var item = objectPool.Pop();
                GameObject.Destroy(item.gameObject);
            }

            // 表示中のオブジェクトを破棄
            while (itemUiList.Count > 0)
            {
                var item = itemUiList.Dequeue();
                GameObject.Destroy(item.gameObject);
            }

            itemList.Clear();
            itemUiList.Clear();
            objectPool.Clear();
        }
    }

    public class RuntimeLoggerUI : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private LogItem logItem;
        [SerializeField] public int maxLogCount = 100;


        [SerializeField] private ScrollRect scrollRect;

        public delegate bool LogFilter(string logString, string stackTrace, LogType type);

        LogFilter defaultFilter = (logString, stackTrace, type) => !logString.StartsWith("[Remote]");

        private bool CombinedFilter(string logString, string stackTrace, LogType type)
        {
            return defaultFilter(logString, stackTrace, type) &&
                   (userFilter == null || userFilter(logString, stackTrace, type));
        }

        private bool isLogging;
        private LogView logView;
        private bool showNewest = true;

        void OnEnable()
        {
            logView = new LogView(content, logItem, maxLogCount);
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;

            logView.Clear();
        }

        public void HandleLog(string logString, string stackTrace, LogType type)
        {

            if (isLogging && CombinedFilter(logString, stackTrace, type))
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
            StartCoroutine(ScrollToBottomCoroutine());
        }

        private IEnumerator ScrollToBottomCoroutine()
        {
            yield return null; // 次のフレームまで待機
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public void OnValueChanged(Vector2 val)
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

        private LogFilter userFilter = null;

        public void SetUserFilter(LogFilter filter)
        {
            userFilter = filter;
        }

        public void Clear()
        {
            logView.Clear();
        }
    }
}