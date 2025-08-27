using Azure.Functions;
using RESTClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : Singleton<DataManager> {
    [SerializeField] private bool saveDataLocally = false;
    [SerializeField] private bool sendAzureData = false;
    [SerializeField] public string DefaultRITchCode = "NONE00";

    private AzureFunctionClient client;
    private Action<IRestResponse<string>> action;

    public static AppSessionData AppSessionData { get; private set; }

    protected override void Awake( ) {
        base.Awake( );

        AppSessionData = new AppSessionData( );
        client = AzureFunctionClient.Create("RitchSIRA");
    }

    /// <summary>
    /// Upload session data to Azure or save the data locally, depending on the currently enabled settings
    /// </summary>
    /// <param name="sessionData">The session data object to send</param>
    public void UploadSessionData(SessionData sessionData) {
        // Update the session data with the current information in the app session
        string identifier = GetSessionFileIdentifier(sessionData.ToString( ));
        sessionData.RITchCode = AppSessionData.RITchCode;
        sessionData.UserData = AppSessionData.UserData;
        sessionData.ID = GetSessionFileIdentifier(identifier);
        string json = JsonUtility.ToJson(sessionData);

        if (sendAzureData) {
            AzureCall(json);
            Debug.Log($"Sent data to Azure: {json}");
        }

        if (saveDataLocally) {
            string dataPath = $"{GetDataPath(identifier)}.json";
            using StreamWriter streamWriter = new StreamWriter(dataPath);
            streamWriter.Write(json);
            Debug.Log($"Saved data to {dataPath}: {json}");
        }
    }

    /// <summary>
    /// Main call function that sends data to Azure
    /// </summary>
    /// <param name="message">The message to send to the Azure server to be stored there. Most likely will be some json data</param>
	private void AzureCall(string message) {
        if (message == null) {
            return;
        }

        AzureFunction azureFunction = new AzureFunction("RecieveGameDataFunction", client, "");
        StartCoroutine(azureFunction.Post(message, action, "", null));
    }

    /// <summary>
    /// Get the full persistent data path with a specified file name
    /// </summary>
    /// <param name="fileName">The file name to get the data path of</param>
    /// <returns>A string containing the full data path</returns>
    public string GetDataPath(string fileName) {
        // Make sure the file name does not contain any characters that will lead to an error when saving the file
        fileName = fileName.Replace(":", " ").Replace(".", " ");

        if (Directory.Exists(Application.persistentDataPath)) {
            return Path.Combine(Application.persistentDataPath, fileName);
        }
        return Path.Combine(Application.streamingAssetsPath, fileName);
    }

    /// <summary>
    /// Get the file identifier for any data that needs to be saved or sent to Azure
    /// </summary>
    /// <param name="details">Additional details to add to the file identifier, like what the contents of the file contains</param>
    /// <returns>A string that contains the fill file identifier. This does not include a file type at the end and needs to be added later for local data saving</returns>
    private string GetSessionFileIdentifier(string details) {
        return $"{AppSessionData.RITchCode}-{DateTime.UtcNow:o}-{details}";
    }

    /// <summary>
    /// Check to see if a RITch code is valid or not
    /// </summary>
    /// <param name="ritchCode">The RITch code to check</param>
    /// <returns>true if the RITch code is 6 characters long, is not the default RITch code, and has only alphanumeric characters, false otherwise</returns>
    public bool CheckForValidRITchCode(string ritchCode) {
        return (ritchCode.Length == 6 && ritchCode != DefaultRITchCode && ritchCode.All(x => char.IsLetterOrDigit(x)));
    }
}
