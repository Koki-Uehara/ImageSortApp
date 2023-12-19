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
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
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
            else
            {
                MessageBox.Show("フォルダを確認してください。");
            }
        }
    }

    public class ImageForm : Form
    {
        private string[] imageFiles;
        private string sourceFolder;
        private int currentImageIndex = 0;
        private PictureBox pictureBox;
        private Button btnOk;
        private Button btnSkip;
        private int okFileIndex = 1;
        private int skipFileIndex = 1;

        public ImageForm(string[] imageFiles, string sourceFolder)
        {
            this.imageFiles = imageFiles;
            this.sourceFolder = sourceFolder;

            this.pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            this.Controls.Add(pictureBox);

            this.btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 50 };
            this.btnOk.Click += btnOk_Click;
            this.Controls.Add(btnOk);

            this.btnSkip = new Button { Text = "Skip", Dock = DockStyle.Bottom, Height = 50 };
            this.btnSkip.Click += btnSkip_Click;
            this.Controls.Add(btnSkip);

            this.Size = new Size(1240, 1240);

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

        private void btnOk_Click(object sender, EventArgs e)
        {
            MoveImage("ProcessedImages", ref okFileIndex);
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            MoveImage("SkippedImages", ref skipFileIndex);
        }

        private void MoveImage(string folderName, ref int fileIndex)
        {
            var destinationFolder = Path.Combine(sourceFolder, folderName);
            Directory.CreateDirectory(destinationFolder);

            var fileName = Path.GetFileName(imageFiles[currentImageIndex]);
            var newFileName = Path.GetFileName(sourceFolder) + fileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
            var destinationPath = Path.Combine(destinationFolder, newFileName);

            while (File.Exists(destinationPath))
            {
                fileIndex++;
                newFileName = Path.GetFileName(sourceFolder) + fileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
                destinationPath = Path.Combine(destinationFolder, newFileName);
            }

            File.Move(imageFiles[currentImageIndex], destinationPath);
            currentImageIndex++;
            fileIndex++;
            ShowCurrentImage();
        }
    }
}
