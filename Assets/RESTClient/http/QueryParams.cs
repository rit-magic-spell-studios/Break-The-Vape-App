// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RESTClient {
  public class QueryParams {
    private Dictionary<string, string> parameters;

    public QueryParams() {
      parameters = new Dictionary<string, string>();
    }

    public void AddParam(string key, string value) {
      parameters.Add(key, value);
    }

    public override string ToString() {
      if (parameters.Count == 0) {
        return "";
      }
      StringBuilder sb = new StringBuilder("?");
      foreach (KeyValuePair<string, string> param in parameters) {
        string key = UnityWebRequest.EscapeURL(param.Key);
        string value = UnityWebRequest.EscapeURL(param.Value);
        sb.Append(key + "=" + value + "&");
      }
      sb.Remove(sb.Length - 1, 1);
      return sb.ToString();
    }
  }
}
