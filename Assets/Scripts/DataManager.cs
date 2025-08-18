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

        ResetAppSessionData( );
        client = AzureFunctionClient.Create("RitchSRA");
    }

    /// <summary>
    /// Reset the app session data back to its original state
    /// </summary>
    public void ResetAppSessionData( ) {
        AppSessionData = new AppSessionData(DefaultRITchCode);
    }

    /// <summary>
    /// Upload session data to Azure or save the data locally, depending on the currently enabled settings
    /// </summary>
    /// <param name="sessionData">The session data object to send</param>
    public void UploadSessionData(SessionData sessionData) {
        string json = JsonUtility.ToJson(sessionData);
        string identifier = GetSessionFileIdentifier(sessionData.ToString( ));

        if (sendAzureData) {
            AzureCall(json, identifier);
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
    /// Set the current RITch code. This will automatically load the user data as well (if it exists)
    /// </summary>
    /// <param name="ritchCode">The RITch code to set</param>
    public void SetRITchCode(string ritchCode) {
        AppSessionData.RITchCode = ritchCode;
        AppSessionData.UserData = LoadUserData(ritchCode);
    }

    /// <summary>
    /// Remove a locally saved user data file
    /// </summary>
    /// <param name="ritchCode">The RITch code of the user data to remove</param>
    /// <returns>true if the file was successfully deleted, false otherwise</returns>
    public bool RemoveUserData(string ritchCode) {
        if (!CheckForValidRITchCode(ritchCode)) {
            return false;
        }

        // Make sure the file exists before deleting it
        string dataPath = $"{GetDataPath(ritchCode)}.json";
        if (File.Exists(dataPath)) {
            File.Delete(dataPath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Load user data from a specific RITch code
    /// </summary>
    /// <param name="ritchCode">The RITch code to load</param>
    /// <returns>UserData object if the file was successfully read and converted to the UserData class, null otherwise</returns>
    private UserData LoadUserData(string ritchCode) {
        if (!CheckForValidRITchCode(ritchCode)) {
            return null;
        }

        // Make sure the file exists
        string dataPath = $"{GetDataPath(ritchCode)}.json";
        if (!File.Exists(dataPath)) {
            return null;
        }

        // Read the text in the file
        using StreamReader streamReader = new StreamReader(dataPath);
        string fileText = streamReader.ReadToEnd( );

        // Convert to a user data object if there is text within the file
        if (fileText.Length > 0) {
            UserData userData = JsonUtility.FromJson<UserData>(fileText);
            Debug.Log($"Loaded data from {dataPath}: {fileText}");
            return userData;
        }

        return null;
    }

    private void SaveUserData(string ritchCode) {

    }

    /// <summary>
    /// Main call function that sends data to Azure
    /// </summary>
    /// <param name="message">The message to send to the Azure server to be stored there. Most likely will be some json data</param>
    /// <param name="fileIdentifier">An identifier for the file that will be stored on Azure</param>
	private void AzureCall(string message, string fileIdentifier) {
        if (message == null) {
            return;
        }

        // Create AzureFunction, RecieveGameDataFunction is the name of the function we are targeting on azure
        AzureFunction azureFunction = new AzureFunction("RecieveGameDataFunction", client, "");

        // Simple Azure post request to send the data
        StartCoroutine(azureFunction.Post(message, action, fileIdentifier, null));
    }

    /// <summary>
    /// Get the full persistent data path with a specified file name
    /// </summary>
    /// <param name="fileName">The file name to get the data path of</param>
    /// <returns>A string containing the full data path</returns>
    private string GetDataPath(string fileName) {
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
