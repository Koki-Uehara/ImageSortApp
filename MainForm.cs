using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace ImageSortApp
{
    // ���C���t�H�[��
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        // �h���b�O�A���h�h���b�v�̃G���^�[�C�x���g
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // �h���b�O�A���h�h���b�v�̃f�[�^���t�@�C���ł��邩�m�F
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        // �h���b�O�A���h�h���b�v�̃h���b�v�C�x���g
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            // �h���b�O�A���h�h���b�v���ꂽ�t�@�C�����擾
            if (e.Data != null)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // �摜�t�@�C�����擾
                var imageFiles = Directory.GetFiles(files[0], "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif")).ToArray();


                // �摜�t�@�C�������݂���ꍇ
                if (imageFiles.Length > 0)
                {
                    // �摜�t�H�[�����쐬���A�\��
                    var imageForm = new ImageForm(imageFiles, files[0]);
                    imageForm.Show();
                }
                else
                {
                    // �摜�t�@�C�������݂��Ȃ��ꍇ�A���b�Z�[�W��\��
                    MessageBox.Show("�摜�t�@�C��������܂���");
                }
            }
            else
            {
                MessageBox.Show("�t�H���_���m�F�ɂ��Ă��������B");
            }
        }
    }

    // �摜�t�H�[��
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
            // ������
            this.imageFiles = imageFiles;
            this.sourceFolder = sourceFolder;

            // PictureBox���쐬
            this.pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            this.Controls.Add(pictureBox);

            // �폜�{�^�����쐬
            this.btnDelete = new Button { Text = "Delete", Dock = DockStyle.Bottom, Height = 50 };
            this.btnDelete.Click += btnDelete_Click;
            this.Controls.Add(btnDelete);

            // �X�L�b�v�{�^�����쐬
            this.btnSkip = new Button { Text = "Skip", Dock = DockStyle.Bottom, Height = 50 };
            this.btnSkip.Click += btnSkip_Click;
            this.Controls.Add(btnSkip);

            // �ړ���̃t�H���_���쐬
            var destinationFolder = Path.Combine(sourceFolder, "SkippedImages");
            if (Directory.Exists(destinationFolder))
            {
                // �ړ���̃t�H���_���̃t�@�C�����擾
                var existingFiles = Directory.GetFiles(destinationFolder, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.StartsWith(Path.GetFileName(sourceFolder)));

                // �ړ���̃t�H���_���Ƀt�@�C�������݂���ꍇ
                if (existingFiles.Any())
                {
                    // �Ō�̃t�@�C�������擾
                    var lastFileName = existingFiles.OrderBy(f => f).Last();

                    // �Ō�̃t�@�C��������A�Ԃ��擾
                    var lastFileNumber = int.Parse(Path.GetFileNameWithoutExtension(lastFileName).Substring(Path.GetFileName(sourceFolder).Length, 4));

                    // �A�Ԃ��X�V
                    skipFileIndex = lastFileNumber + 1;
                }
            }

            // ���݂̉摜��\��
            ShowCurrentImage();
        }

        // ���݂̉摜��\��
        private void ShowCurrentImage()
        {
            // �摜�t�@�C�������݂���ꍇ
            if (currentImageIndex < imageFiles.Length)
            {
                // �摜��ǂݍ��݁A�\��
                using (var img = new Bitmap(imageFiles[currentImageIndex]))
                {
                    pictureBox.Image = new Bitmap(img);
                }
            }
            else
            {
                // �摜�t�@�C�������݂��Ȃ��ꍇ�A���b�Z�[�W��\�����A�t�H�[�������
                MessageBox.Show("�摜�t�@�C��������܂���");
                this.Close();
            }
        }

        // �폜�{�^���̃N���b�N�C�x���g
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // ���݂̉摜�t�@�C�����폜
            File.Delete(imageFiles[currentImageIndex]);

            // ���̉摜��
            currentImageIndex++;

            // �摜��\��
            ShowCurrentImage();
        }

        // �X�L�b�v�{�^���̃N���b�N�C�x���g
        private void btnSkip_Click(object sender, EventArgs e)
        {
            // �ړ���̃t�H���_���쐬
            var destinationFolder = Path.Combine(sourceFolder, "SkippedImages");
            Directory.CreateDirectory(destinationFolder);

            // �V�����t�@�C�������쐬
            var fileName = Path.GetFileName(imageFiles[currentImageIndex]);
            var newFileName = Path.GetFileName(sourceFolder) + skipFileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
            var destinationPath = Path.Combine(destinationFolder, newFileName);

            // �������O�̃t�@�C�������݂���ꍇ�́A�A�Ԃ��X�V
            while (File.Exists(destinationPath))
            {
                skipFileIndex++;
                newFileName = Path.GetFileName(sourceFolder) + skipFileIndex.ToString("D4") + fileName.Substring(fileName.IndexOf("_"));
                destinationPath = Path.Combine(destinationFolder, newFileName);
            }

            // �t�@�C�����ړ�
            File.Move(imageFiles[currentImageIndex], destinationPath);

            // ���̉摜��
            currentImageIndex++;

            // �A�Ԃ��X�V
            skipFileIndex++;

            // �摜��\��
            ShowCurrentImage();
        }
    }
}
