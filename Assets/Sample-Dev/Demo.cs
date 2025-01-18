using System.Collections;
using UnityEngine;

namespace com.afjk.RuntimeLogger.Sample
{
    public class Demo : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(LogMessages());
        }

        IEnumerator LogMessages()
        {
            while (true)
            {
                Debug.Log("Sample Log");
                Debug.LogWarning("Sample Warning");
                Debug.LogError("Sample Error");

                yield return new WaitForSeconds(1f);
            }
        }
    }
}
