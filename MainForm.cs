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

        // �h���b�O�A���h�h���b�v���̃G���^�[�C�x���g�n���h��
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // �h���b�O���ꂽ�f�[�^���t�@�C���̏ꍇ�A�R�s�[�����L���ɂ���
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        // �h���b�O�A���h�h���b�v���̃h���b�v�C�x���g�n���h��
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                // �h���b�v���ꂽ�t�@�C�����擾
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // �Ή�����摜�t�@�C���݂̂��t�B���^�����O
                var imageFiles = Directory.EnumerateFiles(files[0], "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif"))
                    .ToArray();

                // �摜�t�@�C�������݂���ꍇ�A�摜�\���t�H�[�����J��
                if (imageFiles.Any())
                {
                    var imageForm = new ImageForm(imageFiles, files[0]);
                    imageForm.Show();
                }
                else
                {
                    MessageBox.Show("�摜�t�@�C��������܂���");
                }
            }
            else
            {
                MessageBox.Show("�t�H���_���m�F���Ă��������B");
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

            // PictureBox�̐ݒ�
            this.pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            this.Controls.Add(pictureBox);

            // OK�{�^���̐ݒ�
            this.btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 50 };
            this.btnOk.Click += btnOk_Click;
            this.Controls.Add(btnOk);

            // �X�L�b�v�{�^���̐ݒ�
            this.btnSkip = new Button { Text = "Skip", Dock = DockStyle.Bottom, Height = 50 };
            this.btnSkip.Click += btnSkip_Click;
            this.Controls.Add(btnSkip);

            this.Size = new Size(1240, 1240);

            // �ŏ��̉摜��\��
            ShowCurrentImage();
        }

        // ���݂̉摜��\�����郁�\�b�h
        private void ShowCurrentImage()
        {
            if (currentImageIndex < imageFiles.Length)
            {
                pictureBox.Image?.Dispose(); // ���݂̉摜��j��

                // �摜�t�@�C�����������ɓǂݍ���ł���PictureBox�ɐݒ�
                using (var fs = new FileStream(imageFiles[currentImageIndex], FileMode.Open, FileAccess.Read))
                {
                    var img = Image.FromStream(fs);
                    pictureBox.Image = img;
                }
            }
            else
            {
                MessageBox.Show("�摜�t�@�C��������܂���");
                this.Close();
            }
        }


        // OK�{�^���̃N���b�N�C�x���g�n���h��
        private void btnOk_Click(object sender, EventArgs e)
        {
            // �I�����ꂽ�摜���uProcessedImages�v�t�H���_�Ɉړ�
            MoveImage("ProcessedImages", ref okFileIndex);
        }

        // �X�L�b�v�{�^���̃N���b�N�C�x���g�n���h��
        private void btnSkip_Click(object sender, EventArgs e)
        {
            // �I�����ꂽ�摜���uSkippedImages�v�t�H���_�Ɉړ�
            MoveImage("SkippedImages", ref skipFileIndex);
        }

        // �摜���w�肳�ꂽ�t�H���_�Ɉړ����郁�\�b�h
        private void MoveImage(string folderName, ref int fileIndex)
        {
            try
            {
                // �ړ���̃t�H���_�p�X�𐶐����A���݂��Ȃ��ꍇ�͍쐬
                var destinationFolder = Path.Combine(sourceFolder, folderName);
                Directory.CreateDirectory(destinationFolder);

                // �V�����t�@�C�����𐶐�
                var fileName = $"{Path.GetFileNameWithoutExtension(sourceFolder)}_{fileIndex:D4}{Path.GetExtension(imageFiles[currentImageIndex])}";
                var destinationPath = Path.Combine(destinationFolder, fileName);

                // �����̃t�@�C�������݂���ꍇ�́A�t�@�C������ύX
                while (File.Exists(destinationPath))
                {
                    fileIndex++;
                    fileName = $"{Path.GetFileNameWithoutExtension(sourceFolder)}_{fileIndex:D4}{Path.GetExtension(imageFiles[currentImageIndex])}";
                    destinationPath = Path.Combine(destinationFolder, fileName);
                }

                // �t�@�C�����ړ����A���̉摜��\��
                File.Move(imageFiles[currentImageIndex], destinationPath);
                currentImageIndex++;
                fileIndex++;
                ShowCurrentImage();
            }
            catch (IOException ex)
            {
                // �t�@�C�����쒆�̃G���[���L���b�`���A���b�Z�[�W��\��
                MessageBox.Show($"�t�@�C�����쒆�ɃG���[���������܂���: {ex.Message}");
            }
        }
    }
}
