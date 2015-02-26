using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace ShortenerApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private static ManualResetEvent allDone = new ManualResetEvent(false);

        private string _json;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = new URLData();
            if (Mnemonic.Text != "")
                data.Key = Mnemonic.Text;
            else
                data.Key = DateTime.Now.ToString("yyMMdd") + new Random().Next(1, 99);
            data.Url = Url.Text;
            
            var serializer = new DataContractJsonSerializer(typeof(URLData));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms)) { _json = sr.ReadToEnd(); }
            }

            HttpWebRequest request = (HttpWebRequest)
             WebRequest.Create("https://api.mongolab.com/api/1/databases/test/collections/urls?apiKey=5062cef2e4b088b309ccc936");

            request.ContentType = "text/json";
            request.Method = "POST";

            request.BeginGetRequestStream(GetRequestStreamCallback, request);

            allDone.WaitOne();
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            Stream postStream = request.EndGetRequestStream(asynchronousResult);



            // Convert the string into a byte array. 
            byte[] byteArray = Encoding.UTF8.GetBytes(_json);

            // Write to the request stream.
            postStream.Write(byteArray, 0, _json.Length);
            postStream.Dispose();

            // Start the asynchronous operation to get the response
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();
            // Close the stream object
            streamResponse.Dispose();
            streamRead.Dispose();

            response.Dispose();
            allDone.Set();
        }
    }

    [DataContract]
    public class URLData
    {
        [DataMember]
        public String Key { get; set; }

        [DataMember]
        public String Url { get; set; }
    }

}
