using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestsspeechAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string apiKey = "sk_ae9569fda3c96c1b84566e05e19e8aff3325ea81fe6a4df3";
        private static readonly string voiceId = "EXAVITQu4vr4xnSDxMaL"; // Default voice (Rachel)
        private readonly string filePath = "output.mp3"; // central place

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            string text = txtInput.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please enter some text first.");
                return;
            }

            await ConvertTextToSpeech(text, filePath);

            mediaPlayer.Source = new Uri(System.IO.Path.GetFullPath(filePath));
            mediaPlayer.Play();
          
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("No audio file found to delete.");
                return;
            }

            // Confirmation popup
            var result = MessageBox.Show("Are you sure you want to delete the audio file?",
                                         "Confirm Delete",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    mediaPlayer.Stop(); // stop playing if active
                    File.Delete(filePath);
                    MessageBox.Show("Audio file deleted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting file: " + ex.Message);
                }
            }
        }


        private async Task ConvertTextToSpeech(string text, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                // ✅ Only header name, then value
                client.DefaultRequestHeaders.Add("xi-api-key", apiKey.Trim());

                var requestBody = new
                {
                    text = text,
                    model_id = "eleven_monolingual_v1",
                    voice_settings = new
                    {
                        stability = 0.5,
                        similarity_boost = 0.75
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(
                    $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}", content);

                if (response.IsSuccessStatusCode)
                {
                    var audioBytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(filePath, audioBytes);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Error: " + error);
                }
            }
        }

    }
}