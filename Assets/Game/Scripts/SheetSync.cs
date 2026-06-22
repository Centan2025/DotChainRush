using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SheetSync : MonoBehaviour
{
    private const string url = "https://script.google.com/macros/s/AKfycbz_example/exec";

    public static void SendTelemetry()
    {
        if (GameBrain.Instance == null) return;
        
        // Build JSON payload
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.AppendFormat("\"failRate\": {0},", TelemetrySystem.failRate);
        sb.AppendFormat("\"runsPlayed\": {0},", TelemetrySystem.runsPlayed);
        sb.AppendFormat("\"levelsPlayed\": {0},", TelemetrySystem.levelsPlayed);
        sb.Append("\"usage\": {");
        int count = 0;
        foreach (var pair in TelemetrySystem.usage)
        {
            sb.AppendFormat("\"{0}\": {1}", pair.Key, pair.Value);
            if (++count < TelemetrySystem.usage.Count) sb.Append(",");
        }
        sb.Append("}");
        sb.Append("}");

        if (GameBrain.Instance != null && GameBrain.Instance.gameObject.activeInHierarchy)
        {
            GameBrain.Instance.StartCoroutine(PostRequest(sb.ToString()));
        }
    }

    private static IEnumerator PostRequest(string json)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SheetSync] Telemetry successfully synchronized with dashboard!");
            }
            else
            {
                Debug.LogWarning("[SheetSync] Dashboard synchronization failed: " + req.error);
            }
        }
    }
}
