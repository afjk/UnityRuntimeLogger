using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterPresenter : MonoBehaviour
{
    [SerializeField] private RuntimeLoggerUI runtimeLoggerUI;
    [SerializeField] private Toggle logToggle;
    [SerializeField] public Toggle warningToggle;
    [SerializeField] private Toggle errorToggle;

    [SerializeField] private TMP_InputField filterInputFieldText;
    [SerializeField] private Button setFilterButton;
    [SerializeField] private Button clearButton;

    // Start is called before the first frame update
    void Start()
    {
        logToggle.onValueChanged.AddListener((x)=>ApplyFilter());
        warningToggle.onValueChanged.AddListener((x)=>ApplyFilter());
        errorToggle.onValueChanged.AddListener((x)=>ApplyFilter());
        setFilterButton.onClick.AddListener(ApplyFilter);
        
        clearButton.onClick.AddListener( ()=>runtimeLoggerUI.Clear());
        ApplyFilter();
    }
    
    private void ApplyFilter()
    {
        string filterText = filterInputFieldText.text;

        RuntimeLoggerUI.LogFilter filter = (logString, stackTrace, type) =>
        {
            bool typeMatches = (type == LogType.Log && logToggle.isOn) ||
                               (type == LogType.Warning && warningToggle.isOn) ||
                               (type == LogType.Error && errorToggle.isOn) ||
                               (type == LogType.Assert && errorToggle.isOn);

            bool textMatches = string.IsNullOrEmpty(filterText) || logString.Contains(filterText);

            return typeMatches && textMatches;
        };

        runtimeLoggerUI.SetUserFilter(filter);
    }
}
