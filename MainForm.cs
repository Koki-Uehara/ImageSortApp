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

        // ドラッグアンドドロップ時のエンターイベントハンドラ
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // ドラッグされたデータがファイルの場合、コピー操作を有効にする
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        // ドラッグアンドドロップ時のドロップイベントハンドラ
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                // ドロップされたファイルを取得
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // 対応する画像ファイルのみをフィルタリング
                var imageFiles = Directory.EnumerateFiles(files[0], "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".bmp") || s.EndsWith(".gif"))
                    .ToArray();

                // 画像ファイルが存在する場合、画像表示フォームを開く
                if (imageFiles.Any())
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

            // PictureBoxの設定
            this.pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
            this.Controls.Add(pictureBox);

            // OKボタンの設定
            this.btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 50 };
            this.btnOk.Click += btnOk_Click;
            this.Controls.Add(btnOk);

            // スキップボタンの設定
            this.btnSkip = new Button { Text = "Skip", Dock = DockStyle.Bottom, Height = 50 };
            this.btnSkip.Click += btnSkip_Click;
            this.Controls.Add(btnSkip);

            this.Size = new Size(1240, 1240);

            // 最初の画像を表示
            ShowCurrentImage();
        }

        // 現在の画像を表示するメソッド
        private void ShowCurrentImage()
        {
            if (currentImageIndex < imageFiles.Length)
            {
                pictureBox.Image?.Dispose(); // 現在の画像を破棄

                // 画像ファイルをメモリに読み込んでからPictureBoxに設定
                using (var fs = new FileStream(imageFiles[currentImageIndex], FileMode.Open, FileAccess.Read))
                {
                    var img = Image.FromStream(fs);
                    pictureBox.Image = img;
                }
            }
            else
            {
                MessageBox.Show("画像ファイルがありません");
                this.Close();
            }
        }


        // OKボタンのクリックイベントハンドラ
        private void btnOk_Click(object sender, EventArgs e)
        {
            // 選択された画像を「ProcessedImages」フォルダに移動
            MoveImage("ProcessedImages", ref okFileIndex);
        }

        // スキップボタンのクリックイベントハンドラ
        private void btnSkip_Click(object sender, EventArgs e)
        {
            // 選択された画像を「SkippedImages」フォルダに移動
            MoveImage("SkippedImages", ref skipFileIndex);
        }

        // 画像を指定されたフォルダに移動するメソッド
        private void MoveImage(string folderName, ref int fileIndex)
        {
            try
            {
                // 移動先のフォルダパスを生成し、存在しない場合は作成
                var destinationFolder = Path.Combine(sourceFolder, folderName);
                Directory.CreateDirectory(destinationFolder);

                // 新しいファイル名を生成
                var fileName = $"{Path.GetFileNameWithoutExtension(sourceFolder)}_{fileIndex:D4}{Path.GetExtension(imageFiles[currentImageIndex])}";
                var destinationPath = Path.Combine(destinationFolder, fileName);

                // 同名のファイルが存在する場合は、ファイル名を変更
                while (File.Exists(destinationPath))
                {
                    fileIndex++;
                    fileName = $"{Path.GetFileNameWithoutExtension(sourceFolder)}_{fileIndex:D4}{Path.GetExtension(imageFiles[currentImageIndex])}";
                    destinationPath = Path.Combine(destinationFolder, fileName);
                }

                // ファイルを移動し、次の画像を表示
                File.Move(imageFiles[currentImageIndex], destinationPath);
                currentImageIndex++;
                fileIndex++;
                ShowCurrentImage();
            }
            catch (IOException ex)
            {
                // ファイル操作中のエラーをキャッチし、メッセージを表示
                MessageBox.Show($"ファイル操作中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}
