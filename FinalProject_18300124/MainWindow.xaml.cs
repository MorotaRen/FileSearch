using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace FinalProject_18300124 {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>

    public partial class MainWindow : Window {

        //Stringの拡張データ
        public ObservableCollection<string> ExtensionData { get; } = new ObservableCollection<string>();

        //データテーブル(初回読み込み保存用)
        DataTable m_FirstTable = new DataTable();

        //データテーブル(クローン保存用)
        DataTable m_CloneTable = new DataTable();

        //データテーブル(実際表示用)
        DataTable m_DisplayTable = new DataTable();

        //検索リミット
        int       m_Limit;

        //チェックボックス
        DataColumn m_CheckBox = new DataColumn("自作",typeof(bool));

        //フォルダ階層:
        int m_FileNum,

        //書き込み横の数字
            m_FileColsNum;

        /// -----------------------------------<summary>
        /// MainWindow生成時呼び出し
        /// </summary>----------------------------------
        public MainWindow(){
            Initialize();
        }


        /// -----------------------------------<summary>
        /// 初期化
        /// </summary>----------------------------------
        private void Initialize()
        {
            //チェックボックスにNULL不可能に
            m_CheckBox.AllowDBNull = false;
            //デフォルトで入ってるなんか四角いやつを消す
            m_CheckBox.DefaultValue = false;
            //コンポーネントの初期化
            InitializeComponent();
            //初回なので階層は１固定
            m_FileNum = 1;
            //初期書き込み行は１固定
            m_FileColsNum = 1;
        }

        /// -----------------------------------<summary>
        /// ファイルロードボタン関数
        /// </summary>----------------------------------
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            //クローンテーブル
            m_CloneTable = new DataTable();
            //実際表示されるテーブル
            m_DisplayTable = new DataTable();
            //更新前テーブル
            m_FirstTable = new DataTable();
            //開始前の初期化
            Initialize();

            //チェックボックスの生成
            m_CheckBox = new DataColumn("自作", typeof(bool));
            m_CheckBox.AllowDBNull = false;
            m_CheckBox.DefaultValue = false;
            //初回階層なので１
            m_FileNum = 1;

            //フォルダオープンダイアログ
            FolderBrowserDialog openfolderDialog = new FolderBrowserDialog();

            //Colmnsの追加
            m_FirstTable.Columns.Add("フォルダ");
            m_FirstTable.Columns.Add("ファイル");
            m_FirstTable.Columns.Add("拡張子");

            //正しく開けたらフォルダサーチ開始
            if (openfolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderSearch(openfolderDialog.SelectedPath,0);
            }
            else
            {
                System.Windows.MessageBox.Show("オープンフォルダダイアログを正しく表示できませんでした", "警告！", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            //チェックボックスリストに拡張子を追加
            this.DataCheckBoxList.DataContext = ExtensionData;

            //初回テーブルにチェックボックスを追加
            m_FirstTable.Columns.Add(m_CheckBox);

            //テーブルを表示
            SetDisplayTable(m_FirstTable);
        }


        /// -----------------------------------<summary>
        /// 書き出しボタンの関数
        /// テキスト形式での書き出し
        /// </summary>----------------------------------
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            //row数
            int rows = m_DisplayTable.Rows.Count;

            //cols数
            int cols = m_DisplayTable.Columns.Count;

            //まずセーブするところのパス
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            //初期ファイル名
            saveFileDialog.FileName = "ExportFileName";

            //初期パス
            saveFileDialog.InitialDirectory = "@C:\\";

            //ファイルタイトル
            saveFileDialog.Title = "ファイルの名前を入力してください";

            //フィルタ
            saveFileDialog.Filter = "テキストファイル(.txt) | *.txt";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //制作
                using (System.IO.StreamWriter sm = new System.IO.StreamWriter(saveFileDialog.FileName))
                {
                    //Rows分回す
                    for (int i = 0; i < rows;i++)
                    {
                        //Cols分回す
                        for (int j = 0; j < cols; j++)
                        {
                            //カラム名を格納して確認する
                            var ColumsName = m_DisplayTable.Columns[j].ColumnName;
                            //フォルダ検知
                            if (ColumsName.Contains("フォルダ"))
                            {
                                //空白が続く場合空白を入力
                                if (m_DisplayTable.Rows[i][j].ToString() == "" && m_DisplayTable.Rows[i][j + 1].ToString() == "")
                                {
                                    sm.Write("");
                                }
                                //空白の場合 L を入力
                                if (m_DisplayTable.Rows[i][j].ToString() == "")
                                {
                                    sm.Write(" L ");
                                    sm.Write(m_DisplayTable.Rows[i][j].ToString());
                                }
                                else
                                {
                                    //空白じゃなかったらそのまま
                                    sm.Write(m_DisplayTable.Rows[i][j].ToString());
                                }
                            }
                            //ファイル検知
                            if (ColumsName.Contains("ファイル"))
                            {
                                //空白じゃない場合そのまんま入力
                                if (m_DisplayTable.Rows[i][j].ToString() != "")
                                {
                                    sm.Write(m_DisplayTable.Rows[i][j].ToString());
                                }
                            }
                            //拡張子検知
                            if (ColumsName.Contains("拡張子"))
                            {
                                //→かFolderだったら空白
                                if (m_DisplayTable.Rows[i][j].ToString() == "→" || m_DisplayTable.Rows[i][j].ToString() == "Folder")
                                {
                                    sm.Write("");
                                }
                                else
                                {
                                    //じゃなかったらそのまま入力
                                    sm.Write(m_DisplayTable.Rows[i][j].ToString());
                                }
                            }
                            //自作チェックボックス検知
                            if (ColumsName.Contains("自作"))
                            {
                                //Trueなら◎
                                if (m_DisplayTable.Rows[i][j].ToString() == "True")
                                {
                                    sm.WriteLine("◎");
                                }
                                else {
                                    sm.WriteLine();
                                }
                            }
                        }
                    }
                    //すべて終わったら閉じる
                    sm.Close();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("フォルダセーブダイアログを正しく表示できませんでした", "警告！", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        /// -------------------------------------------<summary>
        /// フォルダサーチ
        /// </summary>------------------------------------------
        /// <param name="path">     選択パス  　　    　</param>
        /// <param name="columsNum">横何番目に記入するか</param>
        void FolderSearch(string path, int columsNum)
        {
            //まずフォルダ一覧取得
            var Folders = GetFolders(path);

            //フォルダ分回す
            for (int i = 0; i < Folders.Count(); i++)
            {
                //初回テーブルにRowsを作成
                var Folderrow = m_FirstTable.NewRow();
                //フォルダ名を入力
                Folderrow[columsNum] = System.IO.Path.GetFileName(Folders[i]);
                //Rowsの追加
                m_FirstTable.Rows.Add(Folderrow);
                //拡張子欄に"Folder"を追加
                Folderrow[columsNum + 2] = "Folder";
                //ファイルサーチの開始
                FileSearch(Folders[i],m_FileColsNum);
            }
            //0でもサーチをします
            if (Folders.Count() == 0)
            {
                FileSearch(path, m_FileColsNum);
            }
        }


        /// -------------------------------------------<summary>
        /// ファイルサーチ
        /// </summary>------------------------------------------
        /// <param name="path">     中身を見るフォルダパス</param>
        /// <param name="columsNum">横何番目に記入するか</param>
        void FileSearch(string folderpath,int columsNum)
        {
            //ファイルのパスの取得
            var Files = GetFiles(folderpath);
            //フォルダパスの取得
            var Folders = GetFolders(folderpath);
            //取得したのパスの連携
            var SearchTarget = Files.Concat(Folders).ToArray();

            for (int j = 0; j < SearchTarget.Count(); j++)
            {
                if (Directory.Exists(SearchTarget[j]))
                {
                    m_FileNum++;
                    //第二階層までのリミット
                    if (m_FileNum > m_Limit) {
                        break;
                    }
                    //タイトルの追加
                    m_FirstTable.Columns.Add("第" + m_FileNum + "階層ファイル");
                    m_FirstTable.Columns.Add("第" + m_FileNum + "拡張子");
                    //ファイル名記入用の作成
                    var Filerow = m_FirstTable.NewRow();
                    //ファイルネームを書きこみ
                    Filerow[columsNum] = System.IO.Path.GetFileNameWithoutExtension(SearchTarget[j]);
                    //ファイル名用に作成したものを実際に追加
                    m_FirstTable.Rows.Add(Filerow);
                    //拡張子のところに「→」を追加
                    Filerow[columsNum + 1] = "→";
                    //拡張子を含めた数ずらす
                    m_FileColsNum += 2;
                    //ファイルサーチを続ける
                    FileSearch(SearchTarget[j], m_FileColsNum);
                } else {
                    //ファイル名記入用
                    var Filerow = m_FirstTable.NewRow();
                    //拡張子名の取得
                    string ExtensionName = System.IO.Path.GetExtension(Files[j]);
                    //拡張子がソート用になかったら追加する
                    if (!ExtensionData.Contains(ExtensionName))
                    {
                        ExtensionData.Add(ExtensionName);
                    }
                    //拡張子のみの取得
                    Filerow[columsNum] = System.IO.Path.GetFileNameWithoutExtension(Files[j]);
                    //拡張子名を入れる
                    Filerow[columsNum+1] = ExtensionName;
                    //実際の追加
                    m_FirstTable.Rows.Add(Filerow);
                }
            }
        }


        /// -------------------------------------------<summary>
        /// 拡張子ソート
        /// </summary>------------------------------------------
        ///<param name="extension">ソートする拡張子名   </param>
        void SelectSort(List<String> extension)
        {
            //初回テーブルのクローンの追加
            DataTable CloneFirstTabe = m_FirstTable.Clone();
            //ソートする拡張子が選ばれてなかったら初回テーブル表示
            if (extension == null) {
                this.dataGrid.DataContext = m_FirstTable;
            }else{
                //ソートする拡張子を検索してクローンテーブルを編集
                foreach (var fex in extension) {
                    string FileExtension = "拡張子 = " + "'" + fex + "'";
                    DataRow[] drs = m_FirstTable.Select(FileExtension);
                    foreach (var dr in drs) {
                        var dAddRow = CloneFirstTabe.NewRow();
                        dAddRow.ItemArray = dr.ItemArray;
                        CloneFirstTabe.Rows.Add(dAddRow);
                    }
                }
                //編集したクローンテーブルを表示
                SetDisplayTable(CloneFirstTabe);
            }
        }


        /// -------------------------------------------<summary>
        /// ディレクトリ内のフォルダを検索
        /// </summary>------------------------------------------
        /// <param name="path">パス</param>
        string[] GetFolders(string path)
        {
            var folders = Directory.GetDirectories(path,"*");
            return folders;
        }


        /// -------------------------------------------<summary>
        /// ディレクトリ内ファイルを検索
        /// </summary>------------------------------------------
        /// <param name="path">パス</param>
        string[] GetFiles(string path)
        {
            var files = Directory.GetFiles(path,"*");
            return files;
        }



        /// -------------------------------------------<summary>
        /// ソート更新ボタン押された時の関数
        /// </summary>------------------------------------------
        private void ChangeList_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.IList lbi = (DataCheckBoxList as System.Windows.Controls.ListBox).SelectedItems;
            int IsSelect = lbi.Count;
            List<string> SelectedNames = new List<string>();
            if (IsSelect > 0) {
                if (lbi != null){
                    foreach (String selectedname in lbi){
                        SelectedNames.Add(selectedname);
                    }
                    SelectSort(SelectedNames);
                }
            }else{
                SelectSort(null);
            }
        }


        /// -------------------------------------------<summary>
        /// 上限値の設定
        /// </summary>------------------------------------------
        private void UpperLimitValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(UpperLimitValue.Text,out m_Limit))
            {
                UpperLimitValue.Text = "0";
                m_Limit = 0;
            }
            if (int.Parse(UpperLimitValue.Text) > 60)
            {
                System.Windows.MessageBox.Show(" 60 以上はかなり重いのでおすすめしません！","警告！",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }

        /// -------------------------------------------<summary>
        /// 表示テーブルのセット
        /// </summary>------------------------------------------
        /// <param name="setTable">表示するテーブル</param>
        private void SetDisplayTable(DataTable setTable)
        {
            m_DisplayTable = setTable;
            this.dataGrid.DataContext = m_DisplayTable;
        }
    }
}
