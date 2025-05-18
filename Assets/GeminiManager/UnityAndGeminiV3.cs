﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using System;
using System.Text.RegularExpressions;
using System.Linq;


[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}



[System.Serializable]
public class InlineData
{
    public string mimeType;
    public string data;
}

// Text-only part
[System.Serializable]
public class TextPart
{
    public string text;
}

// Image-capable part
[System.Serializable]
public class ImagePart
{
    public string text;
    public InlineData inlineData;
}

[System.Serializable]
public class TextContent
{
    public string role;
    public TextPart[] parts;
}

[System.Serializable]
public class TextCandidate
{
    public TextContent content;
}

[System.Serializable]
public class TextResponse
{
    public TextCandidate[] candidates;
}

[System.Serializable]
public class ImageContent
{
    public string role;
    public ImagePart[] parts;
}

[System.Serializable]
public class ImageCandidate
{
    public ImageContent content;
}

[System.Serializable]
public class ImageResponse
{
    public ImageCandidate[] candidates;
}


// For text requests
[System.Serializable]
public class ChatRequest
{
    public TextContent[] contents;
}


public class UnityAndGeminiV3: MonoBehaviour
{
    [Header("JSON API Configuration")]
    public TextAsset jsonApi;

    [Header("Scheduling Integration")]
    public ProcessCar processCar;       // ← drag your ProcessCar GameObject here
    public ProcessManager processManager;


    private string apiKey = ""; 
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"; // Edit it and choose your prefer model
    private string imageEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp-image-generation:generateContent"; //End point for image generation

    [Header("ChatBot Function")]
    public TMP_InputField inputField;
    public TMP_Text uiText;
    private TextContent[] chatHistory;

    [Header("Prompt Function")]
    public string prompt = "";

    [Header("Image Prompt Function")]
    public string imagePrompt = "";
    public Material skyboxMaterial; 


    void Start()
    {
        UnityAndGeminiKey jsonApiKey = JsonUtility.FromJson<UnityAndGeminiKey>(jsonApi.text);
        apiKey = jsonApiKey.key;   
        chatHistory = new TextContent[] { };
        if (prompt != ""){StartCoroutine( SendPromptRequestToGemini(prompt));};
        if (imagePrompt != ""){StartCoroutine( SendPromptRequestToGeminiImageGenerator(imagePrompt));};
    }

    private IEnumerator SendPromptRequestToGemini(string promptText)
    {
        string url = $"{apiEndpoint}?key={apiKey}";
     
        string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"{" + promptText + "}\"}]}]}";

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Create a UnityWebRequest with the JSON data
        using (UnityWebRequest www = new UnityWebRequest(url, "POST")){
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError(www.error);
            } else {
                Debug.Log("Request complete!");
                TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                    {
                        //This is the response to your request
                        string text = response.candidates[0].content.parts[0].text;
                        Debug.Log(text);
                    }
                else
                {
                    Debug.Log("No text found.");
                }
            }
        }
    }
    public void SendChat()
    {
        string userMessage = inputField.text.Trim();

        // If it looks like a scheduling request, parse & run locally:
        if (userMessage.StartsWith("simulate", StringComparison.OrdinalIgnoreCase) &&
            userMessage.IndexOf("priority", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            ParseAndRunScheduling(userMessage);
        }
        else
        {
            // fallback to Gemini
            StartCoroutine(SendChatRequestToGemini(userMessage));
        }
    }

    void ParseAndRunScheduling(string input)
    {
        // 1) Check algorithm keyword
        if (!Regex.IsMatch(input, @"(?i)preemptive\s+priority"))
        {
            uiText.text = "Sorry, I only support preemptive priority scheduling right now.";
            return;
        }

        // 2) Extract numeric lists
        float[] arrivals = ExtractNumbersAfterKeyword(input, "arrival times?");
        float[] bursts = ExtractNumbersAfterKeyword(input, "burst times?");
        float[] priorities = ExtractNumbersAfterKeyword(input, "priorities?");

        // 3) Validate
        if (arrivals == null || bursts == null || priorities == null ||
            arrivals.Length != bursts.Length || arrivals.Length != priorities.Length)
        {
            uiText.text = "Couldn't parse your arrival/burst/priority lists. " +
                          "Make sure you write, for example:\n" +
                          "Arrival times: 0,1,2\nBurst times: 5,3,4\nPriorities: 1,2,1";
            return;
        }

        // 4) Build definitions
        var defs = new ProcessDefinitionList
        {
            processes = arrivals
                .Select((a, i) => new ProcessDefinition
                {
                    processName = "P" + (i + 1),
                    arrivalTime = a,
                    burstTime = bursts[i],
                    priority = priorities[i]
                })
                .ToArray()
        };

        // 5) Hide manual UI, spawn & start
        processManager.ProcessInputPanel.SetActive(false);
        processCar.SpawnCarsFromList(defs);
        processManager.OnStartRace();

        uiText.text = $"⏱ Launched {defs.processes.Length} cars with preemptive priority.";
    }

    /// <summary>
    /// Finds “Keyword: num1,num2,…” in the input and returns the floats.
    /// </summary>
    float[] ExtractNumbersAfterKeyword(string input, string keyword)
    {
        // (?i) = case‑insensitive, then e.g. “arrival time:” or “arrival times:”
        var match = Regex.Match(input, $@"(?i){keyword}\s*[:]\s*([\d\.,\s]+)");
        if (!match.Success) return null;

        string numbers = match.Groups[1].Value;
        // split on comma or space:
        var parts = Regex.Split(numbers, @"[\s,]+")
                         .Where(s => s.Length > 0);
        try
        {
            return parts.Select(s => float.Parse(s)).ToArray();
        }
        catch
        {
            return null;
        }
    }

    private IEnumerator SendChatRequestToGemini(string newMessage)
    {

        string url = $"{apiEndpoint}?key={apiKey}";
     
        TextContent userContent = new TextContent
        {
            role = "user",
            parts = new TextPart[]
            {
                new TextPart { text = newMessage }
            }
        };

        List<TextContent> contentsList = new List<TextContent>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray(); 

        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };

        string jsonData = JsonUtility.ToJson(chatRequest);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        // Create a UnityWebRequest with the JSON data
        using (UnityWebRequest www = new UnityWebRequest(url, "POST")){
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.LogError(www.error);
            } else {
                Debug.Log("Request complete!");
                TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                    {
                        //This is the response to your request
                        string reply = response.candidates[0].content.parts[0].text;
                        TextContent botContent = new TextContent
                        {
                            role = "model",
                            parts = new TextPart[]
                            {
                                new TextPart { text = reply }
                            }
                        };

                        Debug.Log(reply);
                        //This part shows the text in the Canvas
                        uiText.text = reply;
                        int idx = reply.IndexOf("{\"processes\":");
                        if (idx >= 0)
                        {
                            string json = reply.Substring(idx);
                            try
                            {
                                var list = JsonUtility.FromJson<ProcessDefinitionList>(json);
                                // hide manual panel
                                processManager.ProcessInputPanel.SetActive(false);
                                // spawn and immediately start the race
                                processCar.SpawnCarsFromList(list);
                                processManager.OnStartRace();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("JSON parse/spawn failed: " + e);
                            }
                        }
                    //This part adds the response to the chat history, for your next message
                    contentsList.Add(botContent);
                        chatHistory = contentsList.ToArray();
                    }
                else
                {
                    Debug.Log("No text found.");
                }
             }
        }  
    }


private IEnumerator SendPromptRequestToGeminiImageGenerator(string promptText)
{
    string url = $"{imageEndpoint}?key={apiKey}";
    
    // Create the proper JSON structure with model specification
    string jsonData = $@"{{
        ""contents"": [{{
            ""parts"": [{{
                ""text"": ""{promptText}""
            }}]
        }}],
        ""generationConfig"": {{
            ""responseModalities"": [""Text"", ""Image""]
        }}
    }}";

    byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

    // Create a UnityWebRequest with the JSON data
    using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
    {
        www.uploadHandler = new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) 
        {
            Debug.LogError(www.error);
        } 
        else 
        {
            Debug.Log("Request complete!");
            Debug.Log("Full response: " + www.downloadHandler.text); // Log full response for debugging
            
            // Parse the JSON response
            try 
            {
                ImageResponse response = JsonUtility.FromJson<ImageResponse>(www.downloadHandler.text);
                
                if (response.candidates != null && response.candidates.Length > 0 && 
                    response.candidates[0].content != null && 
                    response.candidates[0].content.parts != null)
                {
                    foreach (var part in response.candidates[0].content.parts)
                    {
                        if (!string.IsNullOrEmpty(part.text))
                        {
                            Debug.Log("Text response: " + part.text);
                        }
                        else if (part.inlineData != null && !string.IsNullOrEmpty(part.inlineData.data))
                        {
                            // This is the base64 encoded image data
                            byte[] imageBytes = System.Convert.FromBase64String(part.inlineData.data);
                            
                            // Create a texture from the bytes
                            Texture2D tex = new Texture2D(2, 2);
                            tex.LoadImage(imageBytes);
                            byte[] pngBytes = tex.EncodeToPNG();
                            string path = Application.persistentDataPath + "/gemini-image.png";
                            File.WriteAllBytes(path, pngBytes);
                            Debug.Log("Saved to: " + path);
                            Debug.Log("Image received successfully!");

                            // Load the saved image back as Texture2D
                            string imagePath = Path.Combine(Application.persistentDataPath, "gemini-image.png");
                            
                            Texture2D panoramaTex = new Texture2D(2, 2);
                            panoramaTex.LoadImage(File.ReadAllBytes(imagePath));

                            Texture2D properlySizedTex = ResizeTexture(panoramaTex, 1024, 512);
                            
                            // Apply to a panoramic skybox material
                            if (skyboxMaterial != null)
                            {
                                // Switch to panoramic shader
                                skyboxMaterial.shader = Shader.Find("Skybox/Panoramic");
                                skyboxMaterial.SetTexture("_MainTex", properlySizedTex);
                                DynamicGI.UpdateEnvironment();
                                Debug.Log("Skybox updated with panoramic image!");
                            }
                            else
                            {
                                Debug.LogError("Skybox material not assigned!");
                            }

                            // Another approach but might cause distorsion

                            
                            // Texture2D savedTex = new Texture2D(2, 2);
                            // savedTex.LoadImage(File.ReadAllBytes(path));

                            // // Convert to Cubemap (simplified approach - may distort)
                            // Cubemap newCubemap = new Cubemap(savedTex.width, TextureFormat.RGBA32, false);
                            // for (int i = 0; i < 6; i++)
                            // {
                            //     newCubemap.SetPixels(savedTex.GetPixels(), (CubemapFace)i);
                            // }
                            // newCubemap.Apply();

                            // // Apply to skybox
                            // if (skyboxMaterial != null)
                            // {
                            //     skyboxMaterial.SetTexture("_Tex", newCubemap);
                            //     DynamicGI.UpdateEnvironment();
                            //     Debug.Log("Skybox updated with new image!");
                            // }                            

                        }
                    }
                }
                else
                {
                    Debug.Log("No valid response parts found.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("JSON Parse Error: " + e.Message);
            }
        }
    }
    }


    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight);
        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }



}


