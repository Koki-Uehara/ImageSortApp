using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ImageSortApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data != null)
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var file in files)
                    {
                        Console.WriteLine("Processing " + file);
                        await ProcessDirectory(file);
                        Console.WriteLine("Finished processing " + file);
                    }

                    MessageBox.Show("Š®—¹‚µ‚Ü‚µ‚½");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private async Task ProcessDirectory(string sourceFolder)
        {
            var imageFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif"));

            int folderIndex = 1;
            int fileIndex = 0;
            int imageIndex = 1;

            foreach (var imageFile in imageFiles)
            {
                var destinationFolder = Path.Combine(sourceFolder, Path.GetFileName(sourceFolder) + "_" + folderIndex.ToString());

                Directory.CreateDirectory(destinationFolder);

                var fileName = Path.GetFileName(imageFile);
                var newFileName = imageIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
                var destinationPath = Path.Combine(destinationFolder, newFileName);

                Console.WriteLine("Moving " + imageFile + " to " + destinationPath);

                await Task.Run(() => File.Move(imageFile, destinationPath));

                fileIndex++;
                imageIndex++;

                folderIndex++;

                if (fileIndex % 18 == 0)
                {
                    folderIndex = 1;
                }
            }
        }
    }
}
