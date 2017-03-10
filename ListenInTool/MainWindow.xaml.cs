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
using System.Collections;

using ListenInTool.Classes;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Web;
using System.Data.SqlClient;
using System.IO;
using System.Globalization;
using Microsoft.Win32;

namespace ListenInTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool m_bRealtime = true;
        string m_strDb = "listenin_rct";

        CDataset m_dataset = new CDataset();
        CUser m_user = new CUser();

        List<CLexicalItem> m_lsLexicalItem = new List<CLexicalItem>();
        List<CChallengeItem> m_lsChallengeItem = new List<CChallengeItem>();
        string m_strFile_LexicalItem_Xml = "setA_stimuli_lexicalitems_all.xml";
        string m_strFile_ChallengeItem_Xml = "setA_stimuli_challengeitems_all.xml";

        int m_intCurStimulusIdx = 0;

        List<Label> m_lsLbImage = new List<Label>();
        List<Image> m_lsImage = new List<Image>();
        List<Button> m_lsBtnAudio = new List<Button>();

        List<CComment> m_lsComment = new List<CComment>();
        
        //----------------------------------------------------------------------------------------------------
        // MainWindow
        //----------------------------------------------------------------------------------------------------
        public MainWindow()
        {
            InitializeComponent();

            loadComment();

            m_lsLbImage.Add(lbImg1);
            m_lsLbImage.Add(lbImg2);
            m_lsLbImage.Add(lbImg3);
            m_lsLbImage.Add(lbImg4);
            m_lsLbImage.Add(lbImg5);
            m_lsLbImage.Add(lbImg6);
            m_lsLbImage.Add(lbImg7);
            m_lsLbImage.Add(lbImg8);

            m_lsImage.Add(img1);
            m_lsImage.Add(img2);
            m_lsImage.Add(img3);
            m_lsImage.Add(img4);
            m_lsImage.Add(img5);
            m_lsImage.Add(img6);
            m_lsImage.Add(img7);
            m_lsImage.Add(img8);

            m_lsBtnAudio.Add(btnAudio1);
            m_lsBtnAudio.Add(btnAudio2);
            m_lsBtnAudio.Add(btnAudio3);
            m_lsBtnAudio.Add(btnAudio4);
            m_lsBtnAudio.Add(btnAudio5);

            // load user list from database and populate list box      
            if (m_bRealtime)
            {
                string strPatientList = databaseGetPatientList();
                string[] line_r = strPatientList.Split(',');
                int intNumCols = line_r.Length;
                for (int j = 0; j < intNumCols - 1; j++)
                {
                    //cbUser.Items.Add(line_r[j]);
                    lbUser.Items.Add(line_r[j]);
                    //Console.WriteLine(line_r[j]);                
                }

                // load users' detail on tab 6
                loadPatientsDetail();
            }
            else
            {
                // for offline demo
                lbUser.Items.Add("101");
                lbUser.Items.Add("201");
                lbUser.Items.Add("1");
            }

            cbDataset.SelectedIndex = 0;

            // load dataset
            //m_dataset.loadDataset(0);

            lbUser.SelectedIndex = 0;
        }

        //----------------------------------------------------------------------------------------------------
        // loadLexicalItems (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadLexicalItems()
        {          
            m_lsLexicalItem.Clear();

            // check if file exists
            string strXmlFile = m_strFile_LexicalItem_Xml;
            if (!System.IO.File.Exists(strXmlFile))
                return;

            XElement root = XElement.Load(strXmlFile);
            int intIdx = 0;
            m_lsLexicalItem = (
                from el in root.Elements("item")
                select new CLexicalItem
                {
                    m_intId = intIdx++,  /*(int)el.Attribute("idx"),*/
                    m_strName = (string)el.Element("name"),
                    m_lsChallengeItemIdx = (
                        from el2 in el.Elements("ciIdx")
                        select ((int)el2)
                    ).ToList(),
                }
            ).ToList();

        }

        //----------------------------------------------------------------------------------------------------
        // loadChallengeItems (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadChallengeItems()
        {            
            m_lsChallengeItem.Clear();

            // check if file exists
            string strXmlFile = m_strFile_ChallengeItem_Xml; // "stimuli_noun_1syllables_challengeitems.xml";
            if (!System.IO.File.Exists(strXmlFile))
                return;

            XElement root = XElement.Load(strXmlFile);
            int intIdx = 0;
            m_lsChallengeItem = (
                from el in root.Elements("item")
                select new CChallengeItem
                {
                    m_intId = intIdx++,  /*(int)el.Attribute("idx"),*/
                    m_strName = (string)el.Element("name"),
                    m_intFrequency = (int)el.Element("freq"),
                    m_intConcreteness = (int)el.Element("conc"),
                    m_intSyllableNum = (int)el.Element("syll"),
                    m_intLexicalItemIdx = (int)el.Element("li"),
                    m_intLinguisticType = (int)el.Element("lt"),
                    m_strLinguisticCategoryName = (string)el.Element("lcn"),
                    /*m_strAudioFile = (string)el.Element("audioFile"),*/
                    m_lsAudioFile = (
                        from el2 in el.Elements("aud")
                        select ((string)el2)
                    ).ToList(),
                    m_intTargetIdx = (int)el.Element("tgt"),
                    m_lsPictureChoice = (
                        from el2 in el.Elements("picC")
                        select new CPictureChoice
                        {
                            m_strName = (string)el2.Attribute("n"),
                            m_strImageFile = (string)el2.Attribute("img"),
                            m_strType = (string)el2.Attribute("t"),
                        }
                    ).ToList(),
                    m_lsChallengeItemFeaturesIdx = (
                        from el2 in el.Elements("cifIdx")
                        select ((int)el2)
                    ).ToList(),
                    m_intForcedItem = (int)el.Element("forced"),
                }
            ).ToList();

        }        

        //----------------------------------------------------------------------------------------------------
        // showStimulus
        //----------------------------------------------------------------------------------------------------
        private void showStimulus(int intIdx)
        {
            CChallengeItem ci = m_lsChallengeItem[m_intCurStimulusIdx];
            int intLexicalItemIdx = ci.m_intLexicalItemIdx;

            lb.Content = m_intCurStimulusIdx + "  -  freq = " + ci.m_intFrequency + ", concr = " + ci.m_intConcreteness;

            // show images
            for (int i = 0; i < m_lsImage.Count; i++)
            {
                m_lsLbImage[i].Content = "";
                m_lsImage[i].Source = null;
                m_lsImage[i].Visibility = Visibility.Hidden;
            }
            for (int i = 0; i < ci.m_lsPictureChoice.Count; i++)
            {
                m_lsLbImage[i].Content = ci.m_lsPictureChoice[i].m_strName;
                string strImgfile = AppDomain.CurrentDomain.BaseDirectory + "\\images\\" + ci.m_lsPictureChoice[i].m_strImageFile + ".jpg";
                if (System.IO.File.Exists(strImgfile))
                    m_lsImage[i].Source = new BitmapImage(new Uri(strImgfile, UriKind.Absolute));
                m_lsImage[i].Visibility = Visibility.Visible;
            }
            //img1.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\images\\Lighthouse.jpg", UriKind.Absolute));            

            // show audio
            for (int i = 0; i < m_lsBtnAudio.Count; i++)
                m_lsBtnAudio[i].Visibility = Visibility.Hidden;
            for (int i = 0; i < ci.m_lsAudioFile.Count; i++)
            {
                m_lsBtnAudio[i].Content = ci.m_lsAudioFile[i];
                m_lsBtnAudio[i].Visibility = Visibility.Visible;
                string strAudiofile = AppDomain.CurrentDomain.BaseDirectory + "\\audio\\" + ci.m_lsAudioFile[i] + ".wav";
                MediaPlayer mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(strAudiofile, UriKind.Absolute));
                mediaPlayer.Play();
            }
            /*MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\audio\\F1_Adjective_phrase_1.wav", UriKind.Absolute));
            mediaPlayer.Play();*/

            /*BitmapImage bi1 = new BitmapImage();
            bi1.BeginInit();
            bi1.UriSource = new Uri(@"images/Lighthouse.jpg", UriKind.RelativeOrAbsolute);
            // To save significant application memory, set the DecodePixelWidth or   
            // DecodePixelHeight of the BitmapImage value of the image source to the desired  
            // height or width of the rendered image. If you don't do this, the application will  
            // cache the image as though it were rendered as its normal size rather then just  
            // the size that is displayed. 
            // Note: In order to preserve aspect ratio, set DecodePixelWidth or DecodePixelHeight but not both.
            //bi1.DecodePixelHeight = 770;
            bi1.DecodePixelWidth = 830;
            bi1.EndInit();
            image1.Source = bi1;
            image1.Stretch = Stretch.None;*/
        }

        //----------------------------------------------------------------------------------------------------
        // btnNext_Click
        //----------------------------------------------------------------------------------------------------
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            m_intCurStimulusIdx++;
            if (m_intCurStimulusIdx < m_lsChallengeItem.Count)
                showStimulus(m_intCurStimulusIdx);
        }

        //----------------------------------------------------------------------------------------------------
        // btnAudio1_Click
        //----------------------------------------------------------------------------------------------------
        private void btnAudio1_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Name.Equals("btnAudio1"))
                playAudio(0);
            else if (btn.Name.Equals("btnAudio2"))
                playAudio(1);
            else if (btn.Name.Equals("btnAudio3"))
                playAudio(2);
            else if (btn.Name.Equals("btnAudio4"))
                playAudio(3);
            else if (btn.Name.Equals("btnAudio5"))
                playAudio(4);
        }

        //----------------------------------------------------------------------------------------------------
        // playAudio
        //----------------------------------------------------------------------------------------------------
        private void playAudio(int intIdx)
        {
            if (intIdx >= m_lsChallengeItem[m_intCurStimulusIdx].m_lsAudioFile.Count) return;

            string strAudiofile = AppDomain.CurrentDomain.BaseDirectory + "\\audio\\" + m_lsChallengeItem[m_intCurStimulusIdx].m_lsAudioFile[intIdx] + ".wav";
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(strAudiofile, UriKind.Absolute));
            mediaPlayer.Play();
        }

        //----------------------------------------------------------------------------------------------------
        // lbStimulus_SelectionChanged
        //----------------------------------------------------------------------------------------------------
        private void lbStimulus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            m_intCurStimulusIdx = lb.SelectedIndex;            
            showStimulus(m_intCurStimulusIdx);
        }

        //----------------------------------------------------------------------------------------------------
        // cbDataset_SelectionChanged
        //----------------------------------------------------------------------------------------------------
        private void cbDataset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = ((sender as ComboBox).SelectedItem as ComboBoxItem);
            if (cbi.Content != null)
            {
                if (cbi.Content.ToString().Equals("set A"))
                {
                    m_strFile_LexicalItem_Xml = "2016-11-setA_stimuli_lexicalitems_all.xml";
                    m_strFile_ChallengeItem_Xml = "2016-11-setA_stimuli_challengeitems_all.xml";
                    lbForcedItemSetA.Items.Clear();
                }
                else if (cbi.Content.ToString().Equals("set B"))
                {
                    m_strFile_LexicalItem_Xml = "2016-11-setB_stimuli_lexicalitems_all.xml";
                    m_strFile_ChallengeItem_Xml = "2016-11-setB_stimuli_challengeitems_all.xml";
                    lbForcedItemSetB.Items.Clear();
                }
            }

            loadLexicalItems();
            loadChallengeItems();

            // show forced items          
            int intCtrA = 0;
            int intCtrB = 0;
            lbStimulus.Items.Clear();
            //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("setB_forceditems.txt"))
            {
                for (int i = 0; i < m_lsChallengeItem.Count; i++)
                {
                    lbStimulus.Items.Add(i + " - " + m_lsChallengeItem[i].m_strLinguisticCategoryName + " - " + m_lsChallengeItem[i].m_strName);
                    if ((m_lsChallengeItem[i].m_intForcedItem == 1) && (cbi.Content.ToString().Equals("set A")))
                    {
                        intCtrA++;
                        lbForcedItemSetA.Items.Add(intCtrA + " - " + i + " - " + m_lsChallengeItem[i].m_strLinguisticCategoryName + " - " + m_lsChallengeItem[i].m_strName);                        
                    }
                    if ((m_lsChallengeItem[i].m_intForcedItem == 1) && (cbi.Content.ToString().Equals("set B")))
                    {
                        intCtrB++;
                        lbForcedItemSetB.Items.Add(intCtrB + " - " + i + " - " + m_lsChallengeItem[i].m_strLinguisticCategoryName + " - " + m_lsChallengeItem[i].m_strName);

                        //sw.WriteLine(intCtrB + " - " + i + " - " + m_lsChallengeItem[i].m_strLinguisticCategoryName + " - " + m_lsChallengeItem[i].m_strName);                        
                    }
                }
            }
            m_intCurStimulusIdx = 0;            
            showStimulus(0);
        }

        //----------------------------------------------------------------------------------------------------
        // loadComment
        //----------------------------------------------------------------------------------------------------
        public void loadComment()
        {
            // if xml has already been loaded, then return
            //if (m_lsChallengeItemFeatures.Count > 0) return;

            m_lsComment.Clear();

            if (!System.IO.File.Exists("comment.csv"))
                return;

            string strWholeFile = System.IO.File.ReadAllText("comment.csv"); // @"stimuli_noun_1syllables_neighbours.csv");

            // split into lines
            strWholeFile = strWholeFile.Replace('\n', '\r');
            string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // see how many rows & columns there are
            int intNumRows = lines.Length;
            //int intNumCols = lines[0].Split(',').Length;

            int i = 1; // skip first line
            while (i < intNumRows)
            {              
                CComment comment = new CComment();

                string[] line_r = lines[i].Split(',');
                int intNumCols = line_r.Length;

                // first col = idx
                for (int j = 0; j < intNumCols; j++)
                {
                    if (j == 0) comment.m_strDataset = line_r[j];
                    if (j == 1) comment.m_strCiLinguisticCategory = line_r[j];
                    if (j == 2) comment.m_strCiName = line_r[j];
                    if (j == 3) comment.m_intIncorrectImg = Convert.ToInt32(line_r[j]);
                    if (j == 4) comment.m_intIncorrectAudio = Convert.ToInt32(line_r[j]);
                }
                m_lsComment.Add(comment);

                i++; // next line
            }    // end while
        }

        //----------------------------------------------------------------------------------------------------
        // saveCommentToCsv
        //----------------------------------------------------------------------------------------------------
        public void saveCommentToCsv()
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("comment.csv"))
            {
                sw.WriteLine("dataset,linguisticCategory,name,incorrectImg,incorrectAudio");
                for (var i = 0; i < m_lsComment.Count; i++)
                {
                    CComment comment = m_lsComment[i];
                    string strRow = "";
                    strRow = strRow + comment.m_strDataset + "," + comment.m_strCiLinguisticCategory + "," +
                                comment.m_strCiName + "," + comment.m_intIncorrectImg + "," + comment.m_intIncorrectAudio + ",";

                    // write to file
                    sw.WriteLine(strRow);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        // btnIncorrectImg_Click
        //----------------------------------------------------------------------------------------------------
        private void btnIncorrectImg_Click(object sender, RoutedEventArgs e)
        {
            CComment comment = new CComment();
            ComboBoxItem cbi = (cbDataset.SelectedItem as ComboBoxItem);
            comment.m_strDataset = cbi.Content.ToString();
            comment.m_strCiLinguisticCategory = m_lsChallengeItem[m_intCurStimulusIdx].m_strLinguisticCategoryName;
            comment.m_strCiName = m_lsChallengeItem[m_intCurStimulusIdx].m_strName;
            comment.m_intIncorrectImg = 1;
            m_lsComment.Add(comment);
        }

        //----------------------------------------------------------------------------------------------------
        // btnIncorrectAudio_Click
        //----------------------------------------------------------------------------------------------------
        private void btnIncorrectAudio_Click(object sender, RoutedEventArgs e)
        {
            CComment comment = new CComment();
            ComboBoxItem cbi = (cbDataset.SelectedItem as ComboBoxItem);
            comment.m_strDataset = cbi.Content.ToString();
            comment.m_strCiLinguisticCategory = m_lsChallengeItem[m_intCurStimulusIdx].m_strLinguisticCategoryName;
            comment.m_strCiName = m_lsChallengeItem[m_intCurStimulusIdx].m_strName;
            comment.m_intIncorrectAudio = 1;
            m_lsComment.Add(comment);
        }

        //----------------------------------------------------------------------------------------------------
        // btnSaveComment_Click
        //----------------------------------------------------------------------------------------------------
        private void btnSaveComment_Click(object sender, RoutedEventArgs e)
        {
            saveCommentToCsv();
        }

        //----------------------------------------------------------------------------------------------------
        // getUserDatasetId (xml)
        //----------------------------------------------------------------------------------------------------
        private int getUserDatasetId(string strUserId)
        {
            // check if file exists
            string strXmlFile = "data/" + "user_" + strUserId + "_profile.xml"; 
            if (!System.IO.File.Exists(strXmlFile))
                return 0;

            XElement root = XElement.Load(strXmlFile);
            int intDatasetId = (int)root.Element("datasetId");
            return intDatasetId;
        }

        //----------------------------------------------------------------------------------------------------
        // lbUser_SelectionChanged
        //----------------------------------------------------------------------------------------------------
        private void lbUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strUserId = "";            
            ListBox lb = (ListBox)sender;
            strUserId = (string)lb.Items[lb.SelectedIndex];

            // load dataset
            //if (strUserId.Equals("3"))
            //    m_dataset.loadDataset(1);
            //else 
            //    m_dataset.loadDataset(getUserDatasetId(strUserId));

            m_user.clearBindList();

            // load from online db
            List<string> lsGameDate = new List<string>();
            List<double> lsGameTimeMin = new List<double>();
            if (m_bRealtime)
            {
                string strUserProfile = databaseGetPatientData("http://italk.ucl.ac.uk/" + m_strDb + "/patient_userprofile_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_profile.xml"))
                {
                    sw.WriteLine(strUserProfile);
                }
                string strTherapyBlocks = databaseGetPatientData("http://italk.ucl.ac.uk/" + m_strDb + "/patient_therapyblocks_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_therapyblocks.xml"))
                {
                    sw.WriteLine(strTherapyBlocks);
                }
                string strTherapyBlocks_csv = databaseGetPatientData("http://italk.ucl.ac.uk/" + m_strDb + "/patient_therapyblocks_csv_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_therapyblocks.csv"))
                {
                    sw.WriteLine(strTherapyBlocks_csv);
                }
                string strCifeaturesHistory = databaseGetPatientData("http://italk.ucl.ac.uk/" + m_strDb + "/patient_cifeatures_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_challengeitemfeatures_history.xml"))
                {
                    sw.WriteLine(strCifeaturesHistory);
                }

                string strXml = databaseGetPatientData("http://italk.ucl.ac.uk/" + m_strDb + "/patient_therapytime_select.php", strUserId);
                XElement root = XElement.Parse(strXml);
                Console.WriteLine("xml = " + strXml);

                // load dataset
                m_dataset.loadDataset(strUserId, getUserDatasetId(strUserId));

                // therapy time
                string strTherapyTimeMin = (string)root.Element("therapyTime");
                //Console.WriteLine("therapy_time = " + strTherapyTime);

                // game time                
                lsGameDate = (
                       from el in root.Elements("game_time").Elements("gt")
                       select ((string)el.Attribute("date"))
                   ).ToList();
                lsGameTimeMin = (
                       from el in root.Elements("game_time").Elements("gt")
                       select ((double)el.Attribute("min"))
                   ).ToList();

                // load user
                m_user.loadProfile("data/", strUserId, m_dataset.getLexicalItemList(), m_dataset.getChallengeItemList(), m_dataset.getChallengeItemFeaturesList());
            }
            else
            {
                // load dataset
                m_dataset.loadDataset(strUserId, getUserDatasetId(strUserId));
                // load offline files
                // load user
                m_user.loadProfile("data/", strUserId, m_dataset.getLexicalItemList(), m_dataset.getChallengeItemList(), m_dataset.getChallengeItemFeaturesList());               
            }

            if (dgBlock != null)
            {
                dgBlock.ItemsSource = m_user.getBindBlockList();
                dgBlock.DataContext = m_user.getBindBlockList();
                dgBlock.Items.Refresh();

                dgLexicalItem.ItemsSource = m_user.getBindLexicalItemList();
                dgLexicalItem.DataContext = m_user.getBindLexicalItemList();
                dgLexicalItem.Items.Refresh();
                double dPercentage = Math.Round((double)(m_user.getLexicalItemCoverage() / (double)m_dataset.getLexicalItemList().Count * 100), 2);
                lbLexicalItemCoverage.Content = "Lexical items: coverage = " + m_user.getLexicalItemCoverage() + " items " + " (" + dPercentage + "%), accuracy = " + m_user.getLexicalItemAccuracy();

                dgChallengeItem.ItemsSource = m_user.getBindChallengeItemList();
                dgChallengeItem.DataContext = m_user.getBindChallengeItemList();
                dgChallengeItem.Items.Refresh();
                dPercentage = Math.Round((double)(m_user.getChallengeItemCoverage() / (double)m_dataset.getChallengeItemList().Count * 100), 2);
                lbChallengeItemCoverage.Content = "Challenge items: coverage = " + m_user.getChallengeItemCoverage() + " items " + " (" + dPercentage + "%), accuracy = " + m_user.getChallengeItemAccuracy();

                dgChallengeItemFeatures.ItemsSource = m_user.getBindChallengeItemFeaturesList();
                dgChallengeItemFeatures.DataContext = m_user.getBindChallengeItemFeaturesList();
                dgChallengeItemFeatures.Items.Refresh();
                dPercentage = Math.Round((double)(m_user.getChallengeItemFeaturesCoverage() / (double)m_dataset.getChallengeItemFeaturesList().Count * 100), 2);
                lbChallengeItemFeaturesCoverage.Content = "Challenge item features: coverage = " + m_user.getChallengeItemFeaturesCoverage() + " items " + " (" + dPercentage + "%), accuracy = " + m_user.getChallengeItemFeaturesAccuracy();

                dgForcedItem.ItemsSource = m_user.getBindForcedItemList();
                dgForcedItem.DataContext = m_user.getBindForcedItemList();
                dgForcedItem.Items.Refresh();

                /*dgTab2_Cif.ItemsSource = m_user.getBindChallengeItemFeaturesList();
                dgTab2_Cif.DataContext = m_user.getBindChallengeItemFeaturesList();
                dgTab2_Cif.Items.Refresh();*/

                bindDailyTherapyTime(strUserId, lsGameDate, lsGameTimeMin);
            }
        }

        //----------------------------------------------------------------------------------------------------
        // cbUser_SelectionChanged
        //----------------------------------------------------------------------------------------------------
        /*private void cbUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string strUserId = "";
            //ComboBoxItem cbi = (cbUser.SelectedItem as ComboBoxItem);
            ComboBoxItem cbi = ((sender as ComboBox).SelectedItem as ComboBoxItem);
            if (cbi.Content != null)
            {
                //if (cbi.Content.ToString().Equals("2"))
                strUserId = cbi.Content.ToString();
            }

            m_user.clearBindList();

            // load from online db
            {
                string strUserProfile = databaseGetPatientData("http://italk.ucl.ac.uk/listenin_rct/patient_userprofile_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_profile.xml"))
                {                    
                    sw.WriteLine(strUserProfile);                   
                }
                string strTherapyBlocks = databaseGetPatientData("http://italk.ucl.ac.uk/listenin_rct/patient_therapyblocks_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_therapyblocks.xml"))
                {
                    sw.WriteLine(strTherapyBlocks);
                }
                string strCifeaturesHistory = databaseGetPatientData("http://italk.ucl.ac.uk/listenin_rct/patient_cifeatures_select.php", strUserId);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter("data/user_" + strUserId + "_challengeitemfeatures_history.xml"))
                {
                    sw.WriteLine(strCifeaturesHistory);
                }

                string strXml = databaseGetPatientData("http://italk.ucl.ac.uk/listenin_rct/patient_therapytime_select.php", strUserId);
                XElement root = XElement.Parse(strXml);
                Console.WriteLine("xml = " + strXml);

                // therapy time
                string strTherapyTimeMin = (string)root.Element("therapyTime");
                //Console.WriteLine("therapy_time = " + strTherapyTime);

                // game time
                List<System.DateTime> lsDate = new List<System.DateTime>();
                List<double> lsGameTimeMin = new List<double>();
                lsDate = (
                       from el in root.Elements("game_time").Elements("gt")
                       select ((System.DateTime)el.Attribute("date"))
                   ).ToList();
                lsGameTimeMin = (
                       from el in root.Elements("game_time").Elements("gt")
                       select ((double)el.Attribute("min"))
                   ).ToList();

                // load user
                m_user.loadProfile("data/", strUserId, m_dataset.getLexicalItemList(), m_dataset.getChallengeItemList(), m_dataset.getChallengeItemFeaturesList());
            }            

            if (dgBlock != null)
            {
                dgBlock.ItemsSource = m_user.getBindBlockList();
                dgBlock.DataContext = m_user.getBindBlockList();
                dgBlock.Items.Refresh();

                dgLexicalItem.ItemsSource = m_user.getBindLexicalItemList();
                dgLexicalItem.DataContext = m_user.getBindLexicalItemList();
                dgLexicalItem.Items.Refresh();
                double dPercentage = Math.Round((double)(m_user.getLexicalItemCoverage() / (double)m_dataset.getLexicalItemList().Count * 100), 2);
                lbLexicalItemCoverage.Content = "Lexical items: coverage = " + m_user.getLexicalItemCoverage() + " items " + " (" + dPercentage + "%), accuracy = " + m_user.getLexicalItemAccuracy();

                dgChallengeItem.ItemsSource = m_user.getBindChallengeItemList();
                dgChallengeItem.DataContext = m_user.getBindChallengeItemList();
                dgChallengeItem.Items.Refresh();
                dPercentage = Math.Round((double)(m_user.getChallengeItemCoverage() / (double)m_dataset.getChallengeItemList().Count * 100), 2);
                lbChallengeItemCoverage.Content = "Challenge items: coverage = " + m_user.getChallengeItemCoverage() + " items " + " (" + dPercentage + "%), accuracy = " + m_user.getChallengeItemAccuracy();

                dgChallengeItemFeatures.ItemsSource = m_user.getBindChallengeItemFeaturesList();
                dgChallengeItemFeatures.DataContext = m_user.getBindChallengeItemFeaturesList();
                dgChallengeItemFeatures.Items.Refresh();
                dPercentage = Math.Round((double)(m_user.getChallengeItemFeaturesCoverage() / (double)m_dataset.getChallengeItemFeaturesList().Count * 100), 2);
                lbChallengeItemFeaturesCoverage.Content = "Challenge item features: coverage = " + m_user.getChallengeItemFeaturesCoverage() + " items " + " (" + dPercentage + "%), accuracy = " + m_user.getChallengeItemFeaturesAccuracy();
                
            }
        }*/

        //----------------------------------------------------------------------------------------------------
        // databaseGetPatientList
        //----------------------------------------------------------------------------------------------------
        private string databaseGetPatientList()
        {
            Uri address = new Uri("http://italk.ucl.ac.uk/" + m_strDb + "/get_patients.php");
            
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            StringBuilder data = new StringBuilder();
            //data.Append("patient=" + strUserId);
            //data.Append("appid=" + HttpUtility.UrlEncode(appId));
            //data.Append("&context=" + HttpUtility.UrlEncode(context));

            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // Get response  
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                //Console.WriteLine("Patient list = " + reader.ReadToEnd());

                return reader.ReadToEnd();
            }
        }

        //----------------------------------------------------------------------------------------------------
        // databaseGetPatientData
        //----------------------------------------------------------------------------------------------------
        private string databaseGetPatientData(string strUri, string strUserId)
        {
            //Uri address = new Uri("http://italk.ucl.ac.uk/" + m_strDb + "/patient_data_select.php");
            Uri address = new Uri(strUri);

            // Create the web request  
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";            

            StringBuilder data = new StringBuilder();
            data.Append("patient=" + strUserId);
            //data.Append("appid=" + HttpUtility.UrlEncode(appId));
            //data.Append("&context=" + HttpUtility.UrlEncode(context));
            
            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // Get response  
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                //Console.WriteLine("\n\n\n\n\n" + reader.ReadToEnd());

                return reader.ReadToEnd();
            }            
        }

        //----------------------------------------------------------------------------------------------------
        // bindDailyTherapyTime
        //----------------------------------------------------------------------------------------------------
        private void bindDailyTherapyTime(string strUserId, List<string> lsGameDate, List<double> lsGameTimeMin)
        {
            List<CUser_TherapyBlock> lsTherapyBlock = m_user.getTherapyBlockList();

            double dTotalTimeMin = 0;
            List<CBindDGDailyTherapyTime> lsDailyTherapyTime = new List<CBindDGDailyTherapyTime>();
            for (int i = 0; i < lsTherapyBlock.Count; i++)
            {
                DateTime dtStartTime;
                DateTime dtEndTime;

                if ( (strUserId.Equals("101")) || (strUserId.Equals("201")) /*|| (strUserId.Equals("1")) || (strUserId.Equals("2"))*/)
                { 
                    string pattern = "MM/dd/yyyy HH:mm:ss";
                    DateTime.TryParseExact(lsTherapyBlock[i].m_strStartTime, pattern, null, DateTimeStyles.None, out dtStartTime);
                    DateTime.TryParseExact(lsTherapyBlock[i].m_strEndTime, pattern, null, DateTimeStyles.None, out dtEndTime);
                }  
                else
                {
                    dtStartTime = Convert.ToDateTime(lsTherapyBlock[i].m_dtStartTime);
                    dtEndTime = Convert.ToDateTime(lsTherapyBlock[i].m_dtEndTime);
                }            

                //TimeSpan span = Convert.ToDateTime(lsTherapyBlock[i].m_strEndTime) - Convert.ToDateTime(lsTherapyBlock[i].m_strStartTime);
                TimeSpan span = dtEndTime - dtStartTime;
                double dTimeMin = Math.Round(span.TotalMinutes, 4);
                if (dTimeMin < 60)  // to filter blocks which take more than 60 min to complete
                {
                    dTotalTimeMin += dTimeMin;
                    //DateTime dtStart = Convert.ToDateTime(lsTherapyBlock[i].m_strStartTime);
                    string strDate = dtStartTime.ToString("yyyy-MM-dd");
                    //string strDate = dtStart.GetDateTimeFormats()[5];
                    //Console.WriteLine("therapy: day = " + dtStartTime.Day + " - month = " + dtStartTime.Month);

                    int intIdx = lsDailyTherapyTime.FindIndex(a => a.m_strDate.Equals(strDate));
                    if (intIdx > -1)
                    {
                        // already exist
                        lsDailyTherapyTime[intIdx].m_dTherapyTimeMin += dTimeMin;
                    }
                    else
                    {
                        CBindDGDailyTherapyTime dailyTime = new CBindDGDailyTherapyTime(lsDailyTherapyTime.Count, "", 0, 0);
                        dailyTime.m_strDate = strDate;
                        dailyTime.m_dTherapyTimeMin = dTimeMin;
                        lsDailyTherapyTime.Add(dailyTime);
                    }
                    //Console.WriteLine("date = " + strDate + " - block = " + i + " - dTimeMin = " + dTimeMin);
                }
                //Console.WriteLine("block = " + i  + " - " + dtStartTime.ToString() + " - " + dtEndTime.ToString() + " - dTimeMin = " + dTimeMin);                
            }
            Console.WriteLine("dTotalTimeMin (calculated from blocks) = " + dTotalTimeMin);

            // get total therapy time from user profile
            double dTotalTimeMin2 = m_user.getTotalTherapyTimeMin();
            Console.WriteLine("dTotalTimeMin (from user profile) = " + dTotalTimeMin2);

            if ((strUserId.Equals("101")) || (strUserId.Equals("201")) || (strUserId.Equals("401")) || (strUserId.Equals("301")) ||
                   (strUserId.Equals("302")) || (strUserId.Equals("102")) || (strUserId.Equals("104")) || (strUserId.Equals("105")))
            {
            }
            else
                dTotalTimeMin = dTotalTimeMin2;


                // bind game time
                double dTotalGameTimeMin = 0;
            for (int i = 0; i < lsGameDate.Count; i++)
            {
                DateTime dtGameDate = Convert.ToDateTime(lsGameDate[i]);
                string strDate1 = dtGameDate.ToString("yyyy-MM-dd");
                //string strDate1 = dtGameDate.GetDateTimeFormats()[5];
                //Console.WriteLine("game date = " + lsGameDate[i] + " - game_date = " + strDate1);
                //Console.WriteLine("game: day = " + dtGameDate.Day + " - month = " + dtGameDate.Month);

                int intIdx1 = lsDailyTherapyTime.FindIndex(a => a.m_strDate.Equals(strDate1));
                if (intIdx1 > -1)
                {
                    lsDailyTherapyTime[intIdx1].m_dGameTimeMin = lsGameTimeMin[i];
                    dTotalGameTimeMin += lsGameTimeMin[i];
                    //Console.WriteLine("game_date = " + lsGameDate[i] + " - game_time = " + lsGameTimeMin[i]);
                }                
            }

            dgDailyTherapyTime.ItemsSource = lsDailyTherapyTime;
            dgDailyTherapyTime.DataContext = lsDailyTherapyTime;
            dgDailyTherapyTime.Items.Refresh();
            lbTotalTherapyTime.Content = "Total therapy time : " + Math.Round(dTotalTimeMin, 4) + " mins " + " = " + Math.Round(dTotalTimeMin/60, 4) + " hours ";
            lbTotalGameTime.Content = "Total game time : " + Math.Round(dTotalGameTimeMin, 4) + " mins " + " = " + Math.Round(dTotalGameTimeMin / 60, 4) + " hours ";

        }

        //----------------------------------------------------------------------------------------------------
        // btnExportBlockTime_Click
        //----------------------------------------------------------------------------------------------------
        private void btnExportBlockTime_Click(object sender, RoutedEventArgs e)
        {  
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "unknown.csv";
            //saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.FileName))
                {
                    string strRow = "";
                    strRow += "block idx" + "," + "start time" + "," + "end time" + "," + "total min" + ",";
                    sw.WriteLine(strRow);

                    string strUserId = "";                    
                    strUserId = (string)lbUser.Items[lbUser.SelectedIndex];

                    List<CUser_TherapyBlock> lsTherapyBlock = m_user.getTherapyBlockList();

                    double dTotalTimeMin = 0;
                    for (int i = 0; i < lsTherapyBlock.Count; i++)
                    {                 
                        DateTime dtStartTime;
                        DateTime dtEndTime;

                        if ((strUserId.Equals("101")) || (strUserId.Equals("201")) /*|| (strUserId.Equals("1")) || (strUserId.Equals("2"))*/)
                        {
                            string pattern = "MM/dd/yyyy HH:mm:ss";
                            DateTime.TryParseExact(lsTherapyBlock[i].m_strStartTime, pattern, null, DateTimeStyles.None, out dtStartTime);
                            DateTime.TryParseExact(lsTherapyBlock[i].m_strEndTime, pattern, null, DateTimeStyles.None, out dtEndTime);
                        }
                        else
                        {
                            dtStartTime = Convert.ToDateTime(lsTherapyBlock[i].m_dtStartTime);
                            dtEndTime = Convert.ToDateTime(lsTherapyBlock[i].m_dtEndTime);
                        }

                        //TimeSpan span = Convert.ToDateTime(lsTherapyBlock[i].m_strEndTime) - Convert.ToDateTime(lsTherapyBlock[i].m_strStartTime);
                        TimeSpan span = dtEndTime - dtStartTime;
                        double dTimeMin = Math.Round(span.TotalMinutes, 4);
                        dTotalTimeMin += dTimeMin;
                        //Console.WriteLine("block = " + i  + " - " + dtStartTime.ToString() + " - " + dtEndTime.ToString() + " - dTimeMin = " + dTimeMin); 

                        strRow = "";
                        strRow += i + "," + dtStartTime.ToString() + "," + dtEndTime.ToString() + "," + dTimeMin;
                        sw.WriteLine(strRow);
                    }
                    //Console.WriteLine("dTotalTimeMin (calculated from blocks) = " + dTotalTimeMin);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        // btnCalculateGameTime_Click
        //----------------------------------------------------------------------------------------------------
        class CQuery
        {
            public string m_strUrl = "";
            public List<string> m_lsVarName = new List<string>();
            public List<string> m_lsVarValue = new List<string>();            
        }
        private void btnCalculateGameTime_Click(object sender, RoutedEventArgs e)
        {
            List<CQuery> lsQuery = new List<CQuery>();

            List<string> lsXmlFile = new List<string>();
            /*lsXmlFile.Add("C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.11.29-13.58.10.xml");
            lsXmlFile.Add("C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.11.29-13.58.42.xml");
            lsXmlFile.Add("C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.11.29-14.11.08.xml");
            lsXmlFile.Add("C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.11.29-14.20.09.xml");
            lsXmlFile.Add("C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.12.01-14.23.10.xml");
            lsXmlFile.Add("C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.12.01-14.39.00.xml");

            /*string strXmlFile = "C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\2016.11.29-13.58.10.xml";
            if (!System.IO.File.Exists(strXmlFile))
            {
                Console.WriteLine("btnCalculateGameTime_Click - file not exist");
                return;
            }*/

            //string strDirName = "C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-10-24 - pt401\\pt401-4\\ListenIn\\Database\\backup\\";
            string strDirName = "C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-12-14 - pt105\\com.UCL_SoftV.ListenIn_rct_2016_12_13\\files\\ListenIn\\Database\\backup\\";
            DirectoryInfo dir = new DirectoryInfo(strDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + strDirName);
            }          

            // loop through all the files
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if ( (file.Name.Contains("2017.02.05")) || (file.Name.Contains("2017.02.06")) || (file.Name.Contains("2017.02.07")) || (file.Name.Contains("2017.02.08")) || (file.Name.Contains("2017.02.09")) || (file.Name.Contains("2017.02.10")))
                {
                    string temppath = System.IO.Path.Combine(strDirName, file.Name);
                    lsXmlFile.Add(temppath);
                }
            }
                        
            for (int i = 0; i < lsXmlFile.Count; i++)
            {
                XElement root = XElement.Load(lsXmlFile[i]);
                List<CQuery> lsQuery2 = new List<CQuery>();
                lsQuery2 = (
                   from el2 in root.Elements("queries").Elements("query")
                   select new CQuery
                   {
                       m_strUrl = (string)el2.Attribute("url"),
                       m_lsVarName = (
                           from el3 in el2.Elements("variable")
                           select ((string)el3.Attribute("name"))
                       ).ToList(),
                       m_lsVarValue = (
                           from el3 in el2.Elements("variable")
                           select ((string)el3.Attribute("value"))
                       ).ToList(),
                   }
                ).ToList();

                lsQuery = lsQuery.Concat(lsQuery2).ToList();
            }

            lbCalculateGameTime.Content = "";
            double dTotal = 0;
            for (int i = 0; i < lsQuery.Count; i++)
            {
                if (lsQuery[i].m_strUrl.Equals("http://italk.ucl.ac.uk/listenin_rct/game_time_insert.php"))
                {
                    for (int j = 0; j < lsQuery[i].m_lsVarName.Count; j++)
                    {
                        if ( (lsQuery[i].m_lsVarName[j].Equals("date")) && (lsQuery[i].m_lsVarValue[j].Contains("2017-02-09")))
                        {
                            for (int k = 0; k < lsQuery[i].m_lsVarName.Count; k++)
                            {
                                if (lsQuery[i].m_lsVarName[k].Equals("totaltime"))
                                {
                                    dTotal += Convert.ToDouble(lsQuery[i].m_lsVarValue[k]);
                                    lbCalculateGameTime.Content += "+" + lsQuery[i].m_lsVarValue[k];                                    
                                }
                            }
                        }
                    }
                }
            }
            lbCalculateGameTime.Content += " = " + dTotal.ToString();
        }

        

        //----------------------------------------------------------------------------------------------------
        // btnAddNewPatient_Click
        //----------------------------------------------------------------------------------------------------        
        private void btnAddNewPatient_Click(object sender, RoutedEventArgs e)
        {        
            
            string strNewPatientId = tbNewPatientId.Text;
            
            string strNewPatientDataset = "";
            ComboBoxItem cbi = cbNewPatientDataset.SelectedItem as ComboBoxItem;
            if (cbi != null)
                strNewPatientDataset = cbi.Content.ToString();

            string strNewPatientDate = "";
            DateTime? dt = dpNewPatientDate.SelectedDate;
            if (dt != null)
                strNewPatientDate = dt.Value.ToString("yyyy-MM-dd HH:mm:ss");

            Console.WriteLine(strNewPatientId + " " + strNewPatientDataset + " " + strNewPatientDate);

            //check if patient id already exist
            if ((strNewPatientId.Equals("")) || (strNewPatientDataset.Equals("")) || (strNewPatientDate.Equals("")))
                return;
            //foreach (System.Data.DataRowView dr in dgUserDetail.ItemsSource)
            foreach (CBindDGPatientDetail dr in dgUserDetail.ItemsSource)
            {
                //MessageBox.Show(dr[1].ToString());
                //MessageBox.Show(dr.m_strPatientId);
                if (dr.m_strPatientId.Equals(strNewPatientId))
                {
                    MessageBox.Show("Patient id '" + strNewPatientId + "' already exists!");
                    return;
                }
            }            

            // send to database
            Uri address = new Uri("http://italk.ucl.ac.uk/" + m_strDb + "/patient_detail_insert.php");
            //Uri address = new Uri("http://italk.ucl.ac.uk/listenin_dev/patient_detail_insert.php");
            
            // Create the web request  
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            StringBuilder data = new StringBuilder();
            data.Append("patientid=" + strNewPatientId);
            data.Append("&dataset=" + strNewPatientDataset);
            data.Append("&date=" + strNewPatientDate);            

            //detail_insert.Add("date", dtStartTime.ToString("yyyy-MM-dd HH:mm:ss"));

            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // Get response  
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());
                                
                //return reader.ReadToEnd();
            }

            // refresh datagrid
            loadPatientsDetail();
        }

        //----------------------------------------------------------------------------------------------------
        // databaseGetPatientDetail
        //----------------------------------------------------------------------------------------------------
        private string databaseGetPatientDetail()
        {            
            Uri address = new Uri("http://italk.ucl.ac.uk/" + m_strDb + "/get_patients_detail.php");
            //Uri address = new Uri("http://italk.ucl.ac.uk/listenin_dev/get_patients_detail.php");

            // Create the web request  
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            // Set type to POST  
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            StringBuilder data = new StringBuilder();
            //data.Append("patient=" + strUserId);
            //data.Append("appid=" + HttpUtility.UrlEncode(appId));
            //data.Append("&context=" + HttpUtility.UrlEncode(context));

            // Create a byte array of the data we want to send  
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            // Write data  
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            // Get response  
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                // Get the response stream  
                StreamReader reader = new StreamReader(response.GetResponseStream());

                // Console application output  
                //Console.WriteLine("Patient list = " + reader.ReadToEnd());

                return reader.ReadToEnd();
            }
        }

        //----------------------------------------------------------------------------------------------------
        // loadPatientsDetail
        //----------------------------------------------------------------------------------------------------
        private void loadPatientsDetail()
        {
            string strPatientDetail = databaseGetPatientDetail();
            string[] line_r = strPatientDetail.Split(',');
            int intNumCols = line_r.Length;

            List<CBindDGPatientDetail> lsPatientDetail = new List<CBindDGPatientDetail>();

            for (int j = 0; j < intNumCols - 1; j++)
            {
                //Console.WriteLine("Patient detail = " + strPatientDetail);

                CBindDGPatientDetail patientDetail = new CBindDGPatientDetail(lsPatientDetail.Count, "", "", "");
                patientDetail.m_strPatientId = line_r[j];
                j++;
                patientDetail.m_strDataset = line_r[j];
                j++;
                patientDetail.m_strStartDate = line_r[j];
                lsPatientDetail.Add(patientDetail);                
            }            

            dgUserDetail.ItemsSource = lsPatientDetail;
            dgUserDetail.DataContext = lsPatientDetail;
            dgUserDetail.Items.Refresh();
        }

        //----------------------------------------------------------------------------------------------------
        // btnBrowseLogPath_Click
        //----------------------------------------------------------------------------------------------------        
        private void btnBrowseLogPath_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //----------------------------------------------------------------------------------------------------
        // btnCheckCrashPoint_Click
        //----------------------------------------------------------------------------------------------------        
        private void btnCheckCrashPoint_Click(object sender, RoutedEventArgs e)
        {
            lbCrashPoint.Items.Clear();

            //string strDirName = "C:\\Users\\Listen-In-user1\\Documents\\my_projects\\listen-in\\data\\2016-08 - rct\\2016-11-25 - pt104\\com.UCL_SoftV.ListenIn_rct_2016_11_23\\files\\Logs\\";
            string strDirName = txtLogPath.Text;
            DirectoryInfo dir = new DirectoryInfo(strDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + strDirName);
            }

            int intCrashNum = 0;

            // loop through all the files
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {                
                string strTxtFile = System.IO.Path.Combine(strDirName, file.Name);                   

                string strWholeFile = System.IO.File.ReadAllText(strTxtFile);

                //lbCrashPoint.Items.Add(file.Name);

                // split into lines
                strWholeFile = strWholeFile.Replace('\n', '\r');
                string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

                // see how many rows & columns there are
                int intNumRows = lines.Length;
                //int intNumCols = lines[0].Split(',').Length;
                                
                int i = 0;
                while (i < intNumRows)
                {
                    string strLine = lines[i];
                    if (strLine.Contains("***** FIX CORRUPTED THERAPY FILES *****"))
                    {
                        intCrashNum++;
                        lbCrashPoint.Items.Add(intCrashNum + " - " + file.Name + " - " + strLine);
                    }                    
                    i++; // next line
                }
            }            
        }

    }
}
