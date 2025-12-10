using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TriHelper
{
    public partial class Form1 : Form
    {
        List<Track> tracks = new List<Track>();
        List<string> tracksNames;
        List<string> renderrableTracks;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> f = Directory.GetFiles(Environment.GetEnvironmentVariable("TRICACHE")).ToList();
            List<string> f2 = Directory.GetFiles(Environment.GetEnvironmentVariable("TRICACHE_PREFETCH")).Where(x => x.Contains(".json")).ToList();
            foreach (var item in f2)
            {
                tracks.Add(JsonSerializer.Deserialize<Track>(File.ReadAllText(item)));
            }
            tracksNames = tracks.Select(x => $"{string.Join(", ", x.artists.Select(y => y.name))} - {x.title} - {x.album} - {x.id}").ToList();
            RenderT(tracksNames);
        }
        private void RenderT(List<string> trackNames)
        {
            trackNames.Sort();
            listBox1.Items.Clear();
            trackNames.ForEach(x => listBox1.Items.Add(x));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 0)
            {
                RenderT(tracksNames);
                return;
            }
            RenderT(tracksNames.Where(x => x.ToLower().Contains(textBox1.Text.ToLower())).ToList());
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(textBox2.Text.Trim().Length == 0)
            {
                RenderT(tracksNames);
                return;
            }
            //https://open.spotify.com/track/3olEPV0LkmEnXgO84IKQh8?si=42bf29d6c6e4425b
            string id = textBox2.Text.Split('/').Last().Split('?').First().ToLower();
            RenderT(tracksNames.Where(x => x.ToLower().Contains(id)).ToList());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || noItem(listBox1.SelectedItem)) return;
            List<string> f2 = Directory.GetFiles(Path.Combine(Environment.GetEnvironmentVariable("TRICACHE"), ((string)listBox1.SelectedItem).Split('-').Last().Trim(), "spotify")).ToList();
            f2.Sort();
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", f2.First()));
        }

        bool noItem(object id)
        {
            return !Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("TRICACHE"), ((string)id).Split('-').Last().Trim(), "spotify"));
        }

        string getBest(string id)
        {
            List<string> f2 = Directory.GetFiles(Path.Combine(Environment.GetEnvironmentVariable("TRICACHE"), id, "spotify")).ToList();
            f2.Sort();
            return f2.First();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || noItem(listBox1.SelectedItem)) return;
            string id = ((string)listBox1.SelectedItem).Split('-').Last().Trim();
            string album = tracks.Where(x => x.id == id).First().album.Trim();
            if (album.Length != 0)
            {
                string d = Path.GetFullPath($"{album.Replace(":", "").Replace("\\", "").Replace("/", "").Replace("*", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "")} - {DateTime.Now.ToFileTime()}");

                Directory.CreateDirectory(d);
                tracks.Where(x => x.album == album).Select(x => x.id).Distinct().ToList().ForEach(x => File.Copy(getBest(x), Path.Combine(d, x+".ogg")));
                Track t = tracks.Where(x => x.id == id).First();
                string cover = Path.Combine(d, "album.jpg");
    
                File.WriteAllText(Path.Combine(d, "list.m3u8"), $"#EXTM3U\n#PLAYLIST:{string.Join(",", t.artists.Select(x => x.name))}\n#EXTALBUMARTURL:{cover}\n{string.Join("\n", Directory.GetFiles(d).ToList().Select(x=>Path.GetFileName(x)) )}");
                try
                {
                    File.Copy(Path.Combine(Environment.GetEnvironmentVariable("TRICACHE_PREFETCH"), t.id + ".jpg"), cover);
                }
                catch (Exception)
                {
                    MessageBox.Show("Не удалось сохранить обложку");
                }

                System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", Directory.GetFiles(d).First()));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || noItem(listBox1.SelectedItem)) return;
            List<string> f2 = Directory.GetFiles(Path.Combine(Environment.GetEnvironmentVariable("TRICACHE"), ((string)listBox1.SelectedItem).Split('-').Last().Trim(), "spotify")).ToList();
            f2.Sort();
            System.Diagnostics.Process.Start(f2.First());
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }
    }
    public class Artist
    {
        public string name { get; set; }
        public string uri { get; set; }
    }

    public class Track
    {
        private string album1;

        public string type { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public List<Artist> artists { get; set; }
        public object releaseDate { get; set; }
        public int duration { get; set; }
        public bool isPlayable { get; set; }
        public bool isExplicit { get; set; }
        public object audioPreview { get; set; }
        public bool hasVideo { get; set; }
        public object relatedEntityUri { get; set; }
        public object visualIdentity { get; set; }
        public string album { get; set; }
    }

}
