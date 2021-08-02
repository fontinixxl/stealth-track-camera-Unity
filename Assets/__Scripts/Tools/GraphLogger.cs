using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class GraphLogger {
    const int maxDebugLogLength = 16000;

    static Dictionary<int, StringBuilder>   logDict;

    static GraphLogger() {
        logDict = new Dictionary<int, StringBuilder>();
    }


    static public void Log(params object[] objs) {
        Log(0, objs);
    }
    static public void Log(int n, params object[] objs) {
        if (!logDict.ContainsKey(n)) {
            logDict.Add(n, new StringBuilder());
        } else {
            logDict[n].Append("\n");
        }

        for (int i=0; i<objs.Length; i++) {
            if (i>0) {
                logDict[n].Append("\t");
            }

            logDict[n].Append(objs[i].ToString());
        }
            
    }

    static public void PrintLog(int n=0) {
        if (!logDict.ContainsKey(n)) {
            Debug.LogWarning("GraphLogger:PrintLog( "+n+" ) - Attempt to pring non-existent log number "+n+".");
            return;
        }
        int start = 0;
        while (start + maxDebugLogLength < logDict[n].Length) {
			Debug.Log(logDict[n].ToString(start, maxDebugLogLength));
            start += maxDebugLogLength;
        }
        Debug.Log(logDict[n].ToString(start, logDict[n].Length-start));
    }

    static public void ClearLog(int n=0) {
        if (logDict.ContainsKey(n)) {
            logDict.Remove(n);
        }
    }
}
