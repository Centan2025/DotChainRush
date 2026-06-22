using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DashboardSync : MonoBehaviour
{
    private const string url = "https://script.google.com/macros/s/AKfycbz_dashboard/exec";
    private static List<string> batchQueue = new List<string>();
    private static float lastSendTime = 0f;
    private const float SendCooldown = 15f; // Throttle: wait at least 15s between sends

    public static void QueueReport(string reportJson)
    {
        batchQueue.Add(reportJson);
        TriggerBatchSend();
    }

    private static void TriggerBatchSend()
    {
        if (Time.time - lastSendTime < SendCooldown) return;
        if (batchQueue.Count == 0) return;

        // Build batch JSON payload
        StringBuilder sb = new StringBuilder();
        sb.Append("{\"batch\": [");
        for (int i = 0; i < batchQueue.Count; i++)
        {
            sb.Append(batchQueue[i]);
            if (i < batchQueue.Count - 1) sb.Append(",");
        }
        sb.Append("]}");

        batchQueue.Clear();
        lastSendTime = Time.time;

        if (GameBrain.Instance != null && GameBrain.Instance.gameObject.activeInHierarchy)
        {
            GameBrain.Instance.StartCoroutine(PostBatchPayload(sb.ToString()));
        }
    }

    private static IEnumerator PostBatchPayload(string payload)
    {
        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(payload);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[DashboardSync] Batch telemetry upload complete.");
            }
        }
    }
}
