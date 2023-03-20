using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Vchasno
{
    public static class RequestData
    {
        public static string SendGet(string url, string token, out string error)
        {
            var request = GetHttpWebRequest(url, token);
            request.Method = "GET";
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            var result = ParseResponse(response, out error);
            response.Close();
            return result;
        }

        public static string FormDataRequest(string url, string token, Dictionary<string, string> postBody, FileInfo fileToUpload, string fileMimeType, string fileFormKey, out string error)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", token);
            request.KeepAlive = true;
            string boundary = CreateFormDataBoundary();
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            var requestStream = request.GetRequestStream();
            requestStream.AddKeys(postBody, boundary);
            requestStream.AddFile(fileToUpload, boundary, fileMimeType, fileFormKey);
            byte[] endBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
            requestStream.Write(endBytes, 0, endBytes.Length);
            requestStream.Close();

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
            var result = ParseResponse(response, out error);
            response.Close();
            return result;
        }

        private static HttpWebRequest GetHttpWebRequest(string url, string token)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", token);
            request.ContentType = "application/json";
            return request;
        }

        private static string CreateFormDataBoundary()
        {
            //return "----" + DateTime.Now.Ticks.ToString("x");
            return DateTime.Now.Ticks.ToString("x");
            //return "BOUNDARY";
        }

        private static void AddKeys(this Stream stream, Dictionary<string, string> dictionary, string mimeBoundary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return;
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (mimeBoundary == null || mimeBoundary.Length == 0)
            {
                throw new ArgumentException("MIME boundary may not be empty.", "mimeBoundary");
            }
            foreach (string key in dictionary.Keys)
            {
                string item = $"--{mimeBoundary}\r\nContent-Disposition: form-data; name=\"{key}\"\r\n\r\n{dictionary[key]}\r\n";
                byte[] itemBytes = System.Text.Encoding.UTF8.GetBytes(item);
                stream.Write(itemBytes, 0, itemBytes.Length);
            }
        }

        private static void AddFile(this Stream stream, FileInfo file, string mimeBoundary, string mimeType, string formKey)
        {
            if (file == null)
            {
                return;
            }
            if (!file.Exists)
            {
                throw new FileNotFoundException("Unable to find file to write to stream.", file.FullName);
            }
            if (mimeBoundary == null || mimeBoundary.Length == 0)
            {
                throw new ArgumentException("MIME boundary may not be empty.", "mimeBoundary");
            }
            if (mimeType == null || mimeType.Length == 0)
            {
                throw new ArgumentException("MIME type may not be empty.", "mimeType");
            }
            if (formKey == null || formKey.Length == 0)
            {
                throw new ArgumentException("Form key may not be empty.", "formKey");
            }
            string header = $"--{mimeBoundary}\r\nContent-Disposition: form-data; name=\"{formKey}\"; filename=\"{file.Name}\"\r\nContent-Type: {mimeType}\r\n\r\n";
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            stream.Write(headerbytes, 0, headerbytes.Length);
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }
            byte[] newlineBytes = Encoding.UTF8.GetBytes("\r\n");
            stream.Write(newlineBytes, 0, newlineBytes.Length);
        }

        private static string ParseResponse(HttpWebResponse response, out string error)
        {
            error = "";
            string responseBody = null;
            if (response.StatusCode != HttpStatusCode.OK || response.StatusCode != HttpStatusCode.Created || response.StatusCode != HttpStatusCode.Accepted)
            {
                error += response.StatusCode.ToString();
            }

            try
            {
                using (Stream inputStream = response.GetResponseStream())
                {
                    if (inputStream != null)
                    {
                        responseBody = new StreamReader(inputStream, Encoding.UTF8).ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                error += " " + ex.Message;
                return responseBody;
            }
            return responseBody;
        }
    }
}
