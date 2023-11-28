using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace ImageSortApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var imageFiles = Directory.GetFiles(files[0], "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif")).ToArray();
            if (imageFiles.Length > 0)
            {
                var imageForm = new ImageForm(imageFiles, files[0]);
                imageForm.Show();
            }
            else
            {
                MessageBox.Show("画像ファイルがありません");
            }
        }
    }

    public class ImageForm : Form
    {
        private string[] imageFiles;
        private string sourceFolder;
        private int currentImageIndex = 0;
        private PictureBox pictureBox;
        private Button btnDelete;
        private Button btnSkip;
        private int skipFileIndex = 1;

        public ImageForm(string[] imageFiles, string sourceFolder)
        {
            this.imageFiles = imageFiles;
            this.sourceFolder = sourceFolder;
            this.pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            this.Controls.Add(pictureBox);
            this.btnDelete = new Button { Text = "Delete", Dock = DockStyle.Bottom, Height = 50 };
            this.btnDelete.Click += btnDelete_Click;
            this.Controls.Add(btnDelete);
            this.btnSkip = new Button { Text = "Skip", Dock = DockStyle.Bottom, Height = 50 };
            this.btnSkip.Click += btnSkip_Click;
            this.Controls.Add(btnSkip);
            var destinationFolder = Path.Combine(sourceFolder, "SkippedImages");
            if (Directory.Exists(destinationFolder))
            {
                var existingFiles = Directory.GetFiles(destinationFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.StartsWith(Path.GetFileName(sourceFolder)));
                if (existingFiles.Any())
                {
                    var lastFileName = existingFiles.OrderBy(f => f).Last();
                    var lastFileNumber = int.Parse(Path.GetFileNameWithoutExtension(lastFileName).Substring(Path.GetFileName(sourceFolder).Length, 4));
                    skipFileIndex = lastFileNumber + 1;
                }
            }
            ShowCurrentImage();
        }

        private void ShowCurrentImage()
        {
            if (currentImageIndex < imageFiles.Length)
            {
                using (var img = new Bitmap(imageFiles[currentImageIndex]))
                {
                    pictureBox.Image = new Bitmap(img);
                }
            }
            else
            {
                MessageBox.Show("画像ファイルがありません");
                this.Close();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            File.Delete(imageFiles[currentImageIndex]);
            currentImageIndex++;
            ShowCurrentImage();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            var destinationFolder = Path.Combine(sourceFolder, "SkippedImages");
            Directory.CreateDirectory(destinationFolder);
            var fileName = Path.GetFileName(imageFiles[currentImageIndex]);
            var newFileName = Path.GetFileName(sourceFolder) + skipFileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
            var destinationPath = Path.Combine(destinationFolder, newFileName);
            while (File.Exists(destinationPath))
            {
                skipFileIndex++;
                newFileName = Path.GetFileName(sourceFolder) + skipFileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
                destinationPath = Path.Combine(destinationFolder, newFileName);
            }
            File.Move(imageFiles[currentImageIndex], destinationPath);
            currentImageIndex++;
            skipFileIndex++;
            ShowCurrentImage();
        }
    }
}
