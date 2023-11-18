using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace ImageSortApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data != null)
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var file in files)
                    {
                        ProcessDirectory(file);
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

        private void ProcessDirectory(string sourceFolder)
        {
            var imageFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif"));

            int folderIndex = 1;
            int fileIndex = 0;

            foreach (var imageFile in imageFiles)
            {
                var destinationFolder = Path.Combine(sourceFolder, folderIndex.ToString());
                Directory.CreateDirectory(destinationFolder);

                var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(imageFile));
                File.Move(imageFile, destinationPath);

                fileIndex++;
                folderIndex++;

                if (fileIndex % 18 == 0)
                {
                    folderIndex = 1;
                }
            }
        }
    }
}