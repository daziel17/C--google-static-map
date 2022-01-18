using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        string google_key = "";
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] waypoints = {
                            "44.8765065,-0.4444849",
                            "44.8843778,-0.1368667"
            };
            this.txtUrl.Text = this.getStaticMapURLForDirection("45.291002,-0.868131", "44.683159,-0.405704", waypoints);

            var task = Task.Run(() => this.requestsImage(this.txtUrl.Text));
            task.Wait();

            this.pcMap.Image = Image.FromStream(task.Result);

        }

        public string getStaticMapURLForDirection(string origin, string destination, string[] waypoints, string size = "500x500") {

            string[] markers = new string[waypoints.Length + 2];
            int waypoints_label_iter = 0;

            markers[waypoints_label_iter++] = this.formatMarkers("green", "_" + waypoints_label_iter, origin);
            foreach (string waypoint in waypoints)
            {
                markers[waypoints_label_iter++] = this.formatMarkers("blue", "_" + waypoints_label_iter, waypoint);
            }
            markers[waypoints_label_iter++] = this.formatMarkers("red", "_" + waypoints_label_iter, destination);

            string url = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&waypoints={2}&key={3}", 
                                        origin,
                                        destination, 
                                        string.Join("|", waypoints), this.google_key);

            var task = Task.Run(()=> this.requestsAsync(url));
            task.Wait();
            JObject googleDirection = JObject.Parse(task.Result);

            var a = googleDirection["routes"][0];

            string polyline = HttpUtility.UrlEncode(googleDirection["routes"][0]["overview_polyline"]["points"].ToString());
            return string.Format("https://maps.googleapis.com/maps/api/staticmap?size={0}&maptype=roadmap&path=enc:{1}&{2}&key={3}", size, polyline, string.Join("&", markers), this.google_key);
        }

        private string formatMarkers( string color, string label, string waypoint) {
            string formatMarker = "markers=color:{0}{1}label:{2}";
            label = HttpUtility.UrlEncode(string.Format("{0}|{1}", label, waypoint));
            return string.Format(formatMarker, color, HttpUtility.UrlEncode("|"), label);
        }

        private async Task<string> requestsAsync(string url) {

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
        }

        private async Task<System.IO.Stream> requestsImage(string url)
        {

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStreamAsync();
                return content;
            }
        }

    }
}
