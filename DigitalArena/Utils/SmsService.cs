using System.IO;
using System.Net;
using System.Text;
using System;

public class SmsService
{
    private readonly string apiKey = "eKqF8P128EsOHtJIt9qE";
    private readonly string senderId = "8809617619615";
    private readonly string apiUrl = "http://bulksmsbd.net/api/smsapi";

    public string SendSms(string phoneNumber, string message)
    {
        try
        {
            string encodedMessage = Uri.EscapeUriString(message);
            string url = $"{apiUrl}?api_key={apiKey}&senderid={senderId}&number={phoneNumber}&message={encodedMessage}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}