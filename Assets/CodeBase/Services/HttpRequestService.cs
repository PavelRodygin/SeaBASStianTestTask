using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeBase.Services
{
    /// <summary>
    /// Service for making HTTP requests using UnityWebRequest + UniTask
    /// </summary>
    public class HttpRequestService
    {
        /// <summary>
        /// Makes GET request to specified URL
        /// </summary>
        public async UniTask<HttpResponse> GetAsync(string url, int timeoutSeconds = 10)
        {
            if (string.IsNullOrEmpty(url))
                return new HttpResponse { IsSuccess = false, Error = "URL is null or empty" };

            using var request = UnityWebRequest.Get(url);
            request.timeout = timeoutSeconds;

            try
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return new HttpResponse
                    {
                        IsSuccess = true,
                        StatusCode = request.responseCode,
                        ResponseText = request.downloadHandler.text,
                        Headers = request.GetResponseHeaders()
                    };
                }
                else
                {
                    return new HttpResponse
                    {
                        IsSuccess = false,
                        StatusCode = request.responseCode,
                        Error = $"{request.error} (Code: {request.responseCode})",
                        ResponseText = request.downloadHandler?.text
                    };
                }
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    IsSuccess = false,
                    Error = $"Exception: {ex.Message}"
                };
            }
        }
    }

    /// <summary>
    /// HTTP response data
    /// </summary>
    public struct HttpResponse
    {
        public bool IsSuccess;
        public long StatusCode;
        public string ResponseText;
        public string Error;
        public Dictionary<string, string> Headers;
    }
}

