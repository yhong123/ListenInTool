﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Collections.ObjectModel;

namespace ListenInTool.Classes
{
    class CUser
    {
        string m_strAppPath = "";
        public string m_strUserId = "1";

        public double m_dTotalTherapyTimeMin = 0;

        public int m_intCurNoiseLevel = 0;
        List<int> m_lsNoiseLevelHistory = new List<int>();
        List<int> m_lsNoiseBlockIdx = new List<int>();

        // dataset
        int m_intDatasetId = 0;
        List<CLexicalItem> m_lsLexicalItem = new List<CLexicalItem>();
        List<CChallengeItem> m_lsChallengeItem = new List<CChallengeItem>();
        List<CChallengeItemFeatures> m_lsChallengeItemFeatures = new List<CChallengeItemFeatures>();
        double m_dCorpusTotalComplexity = 0;

        // user profiles
        public List<CUser_TherapyBlock> m_lsTherapyBlock = new List<CUser_TherapyBlock>();
        public List<CUser_ChallengeItemFeatures_History> m_lsChallengeItemFeatures_History = new List<CUser_ChallengeItemFeatures_History>();
        public List<CUser_ChallengeItemFeatures_HistoryComplexity> m_lsChallengeItemFeatures_HistoryComplexity = new List<CUser_ChallengeItemFeatures_HistoryComplexity>();
        public List<CUser_ChallengeItem_HistoryComplexity> m_lsChallengeItem_HistoryComplexity = new List<CUser_ChallengeItem_HistoryComplexity>();
        public List<CUser_LexicalItem_HistoryComplexity> m_lsLexicalItem_HistoryComplexity = new List<CUser_LexicalItem_HistoryComplexity>();

        public List<CUser_LexicalItem_HistoryExposure> m_lsLexicalItem_HistoryExposure = new List<CUser_LexicalItem_HistoryExposure>();

        public int m_intCurBlockType = 0;  // 0 = normal block, 1 = forced block
        public List<CUser_ForcedBlockHistory_Weekly> m_lsForcedBlockHistory_Weekly = new List<CUser_ForcedBlockHistory_Weekly>();

        // for DG binding
        private ObservableCollection<CBindDataGridBlock> m_lsBindBlock = new ObservableCollection<CBindDataGridBlock>();
        private ObservableCollection<CBindDGLexicalItem> m_lsBindLexicalItem = new ObservableCollection<CBindDGLexicalItem>();
        private ObservableCollection<CBindDGChallengeItem> m_lsBindChallengeItem = new ObservableCollection<CBindDGChallengeItem>();
        private ObservableCollection<CBindDGChallengeItemFeatures> m_lsBindChallengeItemFeatures = new ObservableCollection<CBindDGChallengeItemFeatures>();
        private ObservableCollection<CBindDGForcedItem> m_lsBindForcedItem = new ObservableCollection<CBindDGForcedItem>();

        int m_intLexicalItemCoverage = 0;
        int m_intChallengeItemCoverage = 0;
        int m_intChallengeItemFeaturesCoverage = 0;
        double m_dLexicalItemAccuracy = 0;
        double m_dChallengeItemAccuracy = 0;
        double m_dChallengeItemFeaturesAccuracy = 0;

        //----------------------------------------------------------------------------------------------------
        // CUser
        //----------------------------------------------------------------------------------------------------
        public CUser()
        {
            //loadTherapyBlocks();
            //loadChallengeItemFeatures_History();
        }

        //----------------------------------------------------------------------------------------------------
        // loadProfile
        //----------------------------------------------------------------------------------------------------
        public void loadProfile(string strAppPath, string strUserId, List<CLexicalItem> lsLexicalItem, List<CChallengeItem> lsChallengeItem, List<CChallengeItemFeatures> lsChallengeItemFeatures)
        {
            /*m_strAppPath = strAppPath;
            m_strUserId = strUserId;
            loadTherapyBlocks();
            loadChallengeItem_History();*/

            m_lsLexicalItem = lsLexicalItem;
            m_lsChallengeItem = lsChallengeItem;
            m_lsChallengeItemFeatures = lsChallengeItemFeatures;

            for (int i = 0; i < m_lsChallengeItemFeatures.Count; i++)
                m_dCorpusTotalComplexity += m_lsChallengeItemFeatures[i].m_dComplexity_Overall;
            
            m_strAppPath = strAppPath;
            m_strUserId = strUserId;
            loadUserProfile();
            Console.WriteLine("m_strUserId = " + m_strUserId + " noise level = " + m_intCurNoiseLevel + " corpus complexity = " + m_dCorpusTotalComplexity);

            if ((strUserId.Equals("101")) || (strUserId.Equals("201")) || (strUserId.Equals("401")) || (strUserId.Equals("301")) ||
                   (strUserId.Equals("302")) || (strUserId.Equals("102")) || (strUserId.Equals("104")) )
                loadTherapyBlocks();
            else
                loadTherapyBlocks_Csv();
            loadChallengeItemFeatures_History();
            //loadChallengeItemFeatures_HistoryComplexity();
            //loadChallengeItem_HistoryComplexity();
            //loadLexicalItem_HistoryComplexity();
            loadLexicalItem_HistoryExposure();            
        }

        //----------------------------------------------------------------------------------------------------
        // getDatasetId
        //----------------------------------------------------------------------------------------------------
        public int getDatasetId()
        {
            return m_intDatasetId;
        }

        //----------------------------------------------------------------------------------------------------
        // getCurNoiseLevel
        //----------------------------------------------------------------------------------------------------
        public int getCurNoiseLevel()
        {
            return m_intCurNoiseLevel;
        }

        //----------------------------------------------------------------------------------------------------
        // getBlockType
        //----------------------------------------------------------------------------------------------------
        public int getBlockType()
        {
            return m_intCurBlockType;
        }

        //----------------------------------------------------------------------------------------------------
        // getTotalTherapyTime
        //----------------------------------------------------------------------------------------------------
        public double getTotalTherapyTimeMin()
        {
            return m_dTotalTherapyTimeMin;
        }

        //----------------------------------------------------------------------------------------------------
        // getTherapyBlockList
        //----------------------------------------------------------------------------------------------------
        public List<CUser_TherapyBlock> getTherapyBlockList()
        {
            return m_lsTherapyBlock;
        }

        //----------------------------------------------------------------------------------------------------
        // getLastTherapyBlock
        //----------------------------------------------------------------------------------------------------
        public CUser_TherapyBlock getLastTherapyBlock()
        {
            if (m_lsTherapyBlock.Count == 0)
                return null;
            return m_lsTherapyBlock[m_lsTherapyBlock.Count-1];
        }

        //----------------------------------------------------------------------------------------------------
        // getLastTherapyBlock_UserAbility
        //----------------------------------------------------------------------------------------------------
        public double getLastTherapyBlock_UserAbility()
        {
            double dLastUserAbility = 0.01;
            if (m_lsTherapyBlock.Count == 0)
                return 0.01;
            else
            {
                for (int i = m_lsTherapyBlock.Count - 1; i >= 0; i--)
                {
                    if ((m_lsTherapyBlock[i].m_intNoiseLevel == 0) && (m_lsTherapyBlock[i].m_intBlockType == 0))
                    {
                        dLastUserAbility = m_lsTherapyBlock[i].m_dUserAbility_Accumulated;
                        break;
                    }
                }
            }
            return dLastUserAbility;

            /*if (m_lsTherapyBlock.Count == 0)
                return 0.1;
            return m_lsTherapyBlock[m_lsTherapyBlock.Count - 1].m_dUserAbility;*/
        }


        //----------------------------------------------------------------------------------------------------
        // getChallengeItem_HistoryList
        //----------------------------------------------------------------------------------------------------
        public List<CUser_ChallengeItemFeatures_History> getChallengeItemFeatures_HistoryList()
        {
            return m_lsChallengeItemFeatures_History;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItemFeatures_HistoryComplexityList
        //----------------------------------------------------------------------------------------------------
        public List<CUser_ChallengeItemFeatures_HistoryComplexity> getChallengeItemFeatures_HistoryComplexityList()
        {
            return m_lsChallengeItemFeatures_HistoryComplexity;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItem_HistoryComplexityList
        //----------------------------------------------------------------------------------------------------
        public List<CUser_ChallengeItem_HistoryComplexity> getChallengeItem_HistoryComplexityList()
        {
            return m_lsChallengeItem_HistoryComplexity;
        }

        //----------------------------------------------------------------------------------------------------
        // getLexicalItem_HistoryComplexityList
        //----------------------------------------------------------------------------------------------------
        public List<CUser_LexicalItem_HistoryComplexity> getLexicalItem_HistoryComplexityList()
        {
            return m_lsLexicalItem_HistoryComplexity;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItemPresentedCtr
        //----------------------------------------------------------------------------------------------------
        public int getChallengeItemFeaturesPresentedCtr(int intChallengeItemFeaturesIdx)
        {
            int intCtr = 0;
            int intIdx = m_lsChallengeItemFeatures_History.FindIndex(a => a.m_intChallengeItemFeaturesIdx == intChallengeItemFeaturesIdx);
            if (intIdx > -1)
                intCtr = m_lsChallengeItemFeatures_History[intIdx].m_lsPresentHistory.Count;
            return intCtr;
        }

        //----------------------------------------------------------------------------------------------------
        // getLexicalItemExposureCtr
        //----------------------------------------------------------------------------------------------------
        public int getLexicalItemExposureCtr(int intLexicalItemIdx)
        {
            return m_lsLexicalItem_HistoryExposure[intLexicalItemIdx].m_intExposureCtr;            
        }

        //----------------------------------------------------------------------------------------------------
        // getBindBlockList
        //----------------------------------------------------------------------------------------------------
        public ObservableCollection<CBindDataGridBlock> getBindBlockList()
        {
            return m_lsBindBlock;
        }        

        //----------------------------------------------------------------------------------------------------
        // getBindLexicalItemList
        //----------------------------------------------------------------------------------------------------
        public ObservableCollection<CBindDGLexicalItem> getBindLexicalItemList()
        {
            return m_lsBindLexicalItem;
        }

        //----------------------------------------------------------------------------------------------------
        // getBindChallengeItemList
        //----------------------------------------------------------------------------------------------------
        public ObservableCollection<CBindDGChallengeItem> getBindChallengeItemList()
        {
            return m_lsBindChallengeItem;
        }

        //----------------------------------------------------------------------------------------------------
        // getBindChallengeItemFeaturesList
        //----------------------------------------------------------------------------------------------------
        public ObservableCollection<CBindDGChallengeItemFeatures> getBindChallengeItemFeaturesList()
        {
            return m_lsBindChallengeItemFeatures;
        }

        //----------------------------------------------------------------------------------------------------
        // getBindForcedItemList
        //----------------------------------------------------------------------------------------------------
        public ObservableCollection<CBindDGForcedItem> getBindForcedItemList()
        {
            return m_lsBindForcedItem;
        }

        //----------------------------------------------------------------------------------------------------
        // clearBindList
        //----------------------------------------------------------------------------------------------------
        public void clearBindList()
        {
            m_lsBindBlock.Clear();
            m_lsBindLexicalItem.Clear();
            m_lsBindChallengeItem.Clear();
            m_lsBindChallengeItemFeatures.Clear();
            m_lsBindForcedItem.Clear();
            return;
        }

        //----------------------------------------------------------------------------------------------------
        // getLexicalItemCoverage
        //----------------------------------------------------------------------------------------------------
        public int getLexicalItemCoverage()
        {
            return m_intLexicalItemCoverage;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItemCoverage
        //----------------------------------------------------------------------------------------------------
        public int getChallengeItemCoverage()
        {
            return m_intChallengeItemCoverage;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItemFeaturesCoverage
        //----------------------------------------------------------------------------------------------------
        public int getChallengeItemFeaturesCoverage()
        {
            return m_intChallengeItemFeaturesCoverage;
        }

        //----------------------------------------------------------------------------------------------------
        // getLexicalItemAccuracy
        //----------------------------------------------------------------------------------------------------
        public double getLexicalItemAccuracy()
        {
            return m_dLexicalItemAccuracy;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItemAccuracy
        //----------------------------------------------------------------------------------------------------
        public double getChallengeItemAccuracy()
        {
            return m_dChallengeItemAccuracy;
        }

        //----------------------------------------------------------------------------------------------------
        // getChallengeItemFeaturesAccuracy
        //----------------------------------------------------------------------------------------------------
        public double getChallengeItemFeaturesAccuracy()
        {
            return m_dChallengeItemFeaturesAccuracy;
        }

        //----------------------------------------------------------------------------------------------------
        // loadUserProfile (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadUserProfile()
        {
            m_intCurNoiseLevel = 0;
            m_lsNoiseLevelHistory.Clear();
            m_lsNoiseBlockIdx.Clear();
            m_dTotalTherapyTimeMin = 0;
            m_intCurBlockType = 0;
            m_lsForcedBlockHistory_Weekly.Clear();

            m_intLexicalItemCoverage = 0;
            m_intChallengeItemCoverage = 0;
            m_intChallengeItemFeaturesCoverage = 0;
            m_dLexicalItemAccuracy = 0;
            m_dChallengeItemAccuracy = 0;
            m_dChallengeItemFeaturesAccuracy = 0;

            // check if file exists
            string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_profile.xml"; // "user_profile.xml";
            if (!System.IO.File.Exists(strXmlFile))
                return;

            XElement root = XElement.Load(strXmlFile);

            m_strUserId = (string)root.Element("userid");
            m_intDatasetId = (int)root.Element("datasetId");

            if (root.Element("totalTherapyTimeMin") != null)
                m_dTotalTherapyTimeMin = (double)root.Element("totalTherapyTimeMin");

            m_intCurNoiseLevel = (int)root.Element("curNoiseLevel");
            m_lsNoiseLevelHistory = (
                from el2 in root.Elements("noiseHistory").Elements("level")
                select ((int)el2)
            ).ToList();
            m_lsNoiseBlockIdx = (
                from el2 in root.Elements("noiseBlocks").Elements("idx")
                select ((int)el2)
            ).ToList();            

            m_lsForcedBlockHistory_Weekly = (
                from el2 in root.Elements("forcedBlocks").Elements("weekly")
                select new CUser_ForcedBlockHistory_Weekly
                {
                    m_strDate = (string)el2.Element("date"),                    
                    m_lsForcedBlockHistory_Daily = (
                        from el3 in el2.Elements("daily")
                        select new CUser_ForcedBlockHistory_Daily
                        {
                            m_strDate = (string)el3.Element("date"),
                            m_lsBlockIdx = (
                                from el4 in el3.Elements("idx")
                                select ((int)el4)
                            ).ToList(),        }
                    ).ToList(),
                }
            ).ToList();
        }

        //----------------------------------------------------------------------------------------------------
        // loadTherapyBlocks (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadTherapyBlocks()
        {
            // if xml has already been loaded, then return
            //if (m_lsTherapyBlock.Count > 0) return;

            m_lsTherapyBlock.Clear();

            // check if file exists
            string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_therapyblocks.xml";
            Console.WriteLine("loadTherapyBlocks = " + strXmlFile);
            if (!System.IO.File.Exists(strXmlFile))
                return;            

            XElement root = XElement.Load(strXmlFile);

            Console.WriteLine("");

            m_lsTherapyBlock = (
                from el in root.Elements("block")
                select new CUser_TherapyBlock
                {
                    m_strStartTime = (string)el.Element("sTime"),
                    m_strEndTime = (string)el.Element("eTime"),
                    m_dtStartTime = (DateTime)el.Element("sTime"),
                    m_dtEndTime = (DateTime)el.Element("eTime"),
                    //m_dLinguisticCateogryComplexity = (double)el.Element("linguisticCateogry"),
                    m_intLinguisticType = (int)el.Element("lt"),
                    /*m_dNoiseLevelComplexity = (double)el.Element("noiseLevel"),*/
                    m_intNoiseLevel = (int)el.Element("noise"),
                    /*m_intDiversityNum = (int)el.Element("diversityNum"),*/
                    m_intBlockType = (int)el.Element("bType"),
                    m_lsChallengeItemFeaturesIdx = (
                        from el2 in el.Elements("cif").Elements("item")
                        select ((int)el2.Attribute("idx"))
                    ).ToList(),
                    m_lsIsDiversity = (
                        from el2 in el.Elements("cif").Elements("item")
                        select ((int)el2.Attribute("div"))
                    ).ToList(),
                    m_lsResponseAccuracy = (
                        from el2 in el.Elements("cif").Elements("item")
                        select ((int)el2.Attribute("res"))
                    ).ToList(),
                    m_lsChallengeItemFeatures_Complexity = (
                        from el2 in el.Elements("cif").Elements("item")
                        select ((double)el2.Attribute("cpx"))
                    ).ToList(),
                    /*m_lsResponseAccuracy = (
                        from el2 in el.Elements("response").Elements("acc")
                        select ((int)el2)
                    ).ToList(),*/
                    m_dAccuracyRate = (double)el.Element("accRate"),
                    m_dBlockComplexity = (double)el.Element("blockCpx"),
                    m_dUserAbility_Accumulated = (double)el.Element("userAb"),
                    m_dUserAbility_ThisBlockAvg = (double)el.Element("userAb_TB"),
                    m_dTrainingStep = (double)el.Element("ts"),
                    m_dNextBlock_DiversityThresholdUpper = (double)el.Element("dtu"),
                    m_dNextBlock_DiversityThresholdLower = (double)el.Element("dtl"),
                    m_dMean_Frequency = (double)el.Element("freq").Attribute("m"),
                    m_dMean_Concreteness = (double)el.Element("conc").Attribute("m"),
                    m_dMean_DistractorNum = (double)el.Element("distr").Attribute("m"),
                    m_dStdDeviation_Frequency = (double)el.Element("freq").Attribute("sd"),
                    m_dStdDeviation_Concreteness = (double)el.Element("conc").Attribute("sd"),
                    m_dStdDeviation_DistractorNum = (double)el.Element("distr").Attribute("sd"),
                }
            ).ToList();

            // bind to datagrid
            for (int i = 0; i < m_lsTherapyBlock.Count; i++)
            {
                CBindDataGridBlock block = new CBindDataGridBlock(i, m_lsTherapyBlock[i], m_lsLexicalItem, m_lsChallengeItem, m_lsChallengeItemFeatures);
                m_lsBindBlock.Add(block);
            }

            /*for (int i = 0; i < m_lsLexicalItem.Count; i++)
            {
                Console.WriteLine(m_lsLexicalItem[i].m_strName);
                string str = "";
                for (int j = 0; j < m_lsLexicalItem[i].m_lsChallengeItemIdx.Count; j++)
                    str = str + m_lsLexicalItem[i].m_lsChallengeItemIdx[j] + ", ";
                Console.WriteLine(str);
            }*/
        }

        //----------------------------------------------------------------------------------------------------
        // loadTherapyBlocks_Csv (csv)
        //----------------------------------------------------------------------------------------------------
        private void loadTherapyBlocks_Csv()
        {
            m_lsTherapyBlock.Clear();

            // check if file exists
            //string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_therapyblocks.xml";
            string strCsvFile = System.IO.Path.Combine(m_strAppPath, "user_" + m_strUserId + "_therapyblocks.csv");
            if (!System.IO.File.Exists(strCsvFile))
                return;

            string strWholeFile = System.IO.File.ReadAllText(strCsvFile);

            // split into lines
            strWholeFile = strWholeFile.Replace('\n', '\r');
            string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // see how many rows & columns there are
            int intNumRows = lines.Length;
            //int intNumCols = lines[0].Split(',').Length;

            int i = 0;
            while (i < intNumRows)
            {
                //Console.WriteLine (lines [i]);
                CUser_TherapyBlock block = new CUser_TherapyBlock();

                string[] line_r = lines[i].Split(',');
                int intNumCols = line_r.Length;

                // first col = idx
                //for (int j = 1; j < intNumCols - 1; j++)
                {
                    //Console.WriteLine(line_r[j]);

                    //strRow = strRow + m_lsTherapyBlock[i].m_dtStartTime.ToString("yyyy-MM-dd HH:mm:ss") + ",";
                    string strYear = "";
                    string strMonth = "";
                    string strDay = "";
                    string strHour = "";
                    string strMinute = "";
                    string strSecond = "";

                    if (line_r[1] != "") strYear = line_r[1];
                    if (line_r[2] != "") strMonth = line_r[2];
                    if (line_r[3] != "") strDay = line_r[3];
                    if (line_r[4] != "") strHour = line_r[4];
                    if (line_r[5] != "") strMinute = line_r[5];
                    if (line_r[6] != "") strSecond = line_r[6];
                    //Debug.Log(strYear + "-" + strMonth + "-" + strDay + " " + strHour + ":" + strMinute + ":" + strSecond);
                    block.m_dtStartTime = Convert.ToDateTime(strYear + "-" + strMonth + "-" + strDay + " " + strHour + ":" + strMinute + ":" + strSecond);

                    if (line_r[7] != "") strYear = line_r[7];
                    if (line_r[8] != "") strMonth = line_r[8];
                    if (line_r[9] != "") strDay = line_r[9];
                    if (line_r[10] != "") strHour = line_r[10];
                    if (line_r[11] != "") strMinute = line_r[11];
                    if (line_r[12] != "") strSecond = line_r[12];
                    block.m_dtEndTime = Convert.ToDateTime(strYear + "-" + strMonth + "-" + strDay + " " + strHour + ":" + strMinute + ":" + strSecond);


                    if (line_r[13] != "") block.m_intRoundIdx = Convert.ToInt32(line_r[13]);
                    if (line_r[14] != "") block.m_intLinguisticType = Convert.ToInt32(line_r[14]);
                    if (line_r[15] != "") block.m_intNoiseLevel = Convert.ToInt32(line_r[15]);
                    if (line_r[16] != "") block.m_intBlockType = Convert.ToInt32(line_r[16]);

                    int j = 17;
                    for (int k = 0; k < CConstants.g_intItemNumPerBlock; k++)
                    {
                        int intStartIdx = j + (k * 5);
                        if (line_r[intStartIdx + 0] != "") block.m_lsChallengeItemFeaturesIdx.Add(Convert.ToInt32(line_r[intStartIdx + 0]));
                        if (line_r[intStartIdx + 1] != "") block.m_lsIsDiversity.Add(Convert.ToInt32(line_r[intStartIdx + 1]));
                        if (line_r[intStartIdx + 2] != "") block.m_lsResponseAccuracy.Add(Convert.ToInt32(line_r[intStartIdx + 2]));
                        if (line_r[intStartIdx + 3] != "") block.m_lsResponseRtSec.Add(Convert.ToDouble(line_r[intStartIdx + 3]));
                        if (line_r[intStartIdx + 4] != "") block.m_lsChallengeItemFeatures_Complexity.Add(Convert.ToDouble(line_r[intStartIdx + 4]));
                    }

                    j = j + (CConstants.g_intItemNumPerBlock * 5);
                    if (line_r[j] != "") block.m_dAccuracyRate = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dBlockComplexity = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dUserAbility_Accumulated = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dUserAbility_ThisBlockAvg = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dTrainingStep = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dNextBlock_DiversityThresholdUpper = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dNextBlock_DiversityThresholdLower = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dMean_Frequency = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dStdDeviation_Frequency = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dMean_Concreteness = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dStdDeviation_Concreteness = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dMean_DistractorNum = Convert.ToDouble(line_r[j]);
                    j++;
                    if (line_r[j] != "") block.m_dStdDeviation_DistractorNum = Convert.ToDouble(line_r[j]);
                }
                m_lsTherapyBlock.Add(block);

                i++; // next line
            }    // end while

            // bind to datagrid
            for (i = 0; i < m_lsTherapyBlock.Count; i++)
            {
                CBindDataGridBlock block = new CBindDataGridBlock(i, m_lsTherapyBlock[i], m_lsLexicalItem, m_lsChallengeItem, m_lsChallengeItemFeatures);
                m_lsBindBlock.Add(block);
            }
        }

        //----------------------------------------------------------------------------------------------------
        // loadChallengeItemFeatures_History (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadChallengeItemFeatures_History()
        {
            // if xml has already been loaded, then return
            //if (m_lsChallengeItemFeatures_History.Count > 0) return;

            m_lsChallengeItemFeatures_History.Clear();

            // check if file exists
            string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_challengeitemfeatures_history.xml";
            if (!System.IO.File.Exists(strXmlFile))
                return;            

            /*
			<item idx="0">
			  	<challengeItemIdx>0</challengeItemIdx>			  			  	
			  	<presentHistory>
                    <history sessionIdx="0" blockIdx="0" acc="0" /history>
                    <history sessionIdx="0" blockIdx="0" acc="0" /history>
                    <history sessionIdx="0" blockIdx="0" acc="0" /history>
			    </presentHistory>          
            </item> 
			*/

            XElement root = XElement.Load(strXmlFile);

            m_lsChallengeItemFeatures_History = (
                from el in root.Elements("item")
                select new CUser_ChallengeItemFeatures_History
                {
                    m_intChallengeItemFeaturesIdx = (int)el.Element("cifIdx"),
                    m_lsPresentHistory = (
                        from el2 in el.Elements("hist").Elements("h")
                        select new CPresentHistory
                        {
                            m_intSessionIdx = (int)el2.Attribute("sIdx"),
                            m_intBlockIdx = (int)el2.Attribute("bIdx"),
                            m_intAccuracy = (int)el2.Attribute("acc"),
                        }
                    ).ToList(),                    
                }
            ).ToList();

            calculateCoverage();

        }
        
        //----------------------------------------------------------------------------------------------------
        // calculateCoverage
        //----------------------------------------------------------------------------------------------------
        public void calculateCoverage()
        {
            // construct lexical items and challenge items presentCtr and correctCtr vectors
            List<int> lsLexicalItem_PresentCtr = new List<int>();
            List<int> lsLexicalItem_CorrectCtr = new List<int>();
            for (int i = 0; i<m_lsLexicalItem.Count; i++)
            {
                lsLexicalItem_PresentCtr.Add(0);
                lsLexicalItem_CorrectCtr.Add(0);
            }
            List<int> lsChallengeItem_PresentCtr = new List<int>();
            List<int> lsChallengeItem_CorrectCtr = new List<int>();
            for (int i = 0; i<m_lsChallengeItem.Count; i++)
            {
                lsChallengeItem_PresentCtr.Add(0);
                lsChallengeItem_CorrectCtr.Add(0);
            }
            List<int> lsChallengeItemFeatures_PresentCtr = new List<int>();
            List<int> lsChallengeItemFeatures_CorrectCtr = new List<int>();
            for (int i = 0; i < m_lsChallengeItemFeatures.Count; i++)
            {
                lsChallengeItemFeatures_PresentCtr.Add(0);
                lsChallengeItemFeatures_CorrectCtr.Add(0);
            }

            for (int i = 0; i < m_lsChallengeItemFeatures_History.Count; i++)
            {
                int intChallengeItemFeaturesIdx = m_lsChallengeItemFeatures_History[i].m_intChallengeItemFeaturesIdx;
                int intChallengeItemIdx = m_lsChallengeItemFeatures[intChallengeItemFeaturesIdx].m_intChallengeItemIdx;
                int intLexicalItemIdx = m_lsChallengeItem[intChallengeItemIdx].m_intLexicalItemIdx;               

                for (var j = 0; j < m_lsChallengeItemFeatures_History[i].m_lsPresentHistory.Count; j++)
                {
                    lsLexicalItem_PresentCtr[intLexicalItemIdx] += 1;
                    lsChallengeItem_PresentCtr[intChallengeItemIdx] += 1;
                    lsChallengeItemFeatures_PresentCtr[intChallengeItemFeaturesIdx] += 1;

                    if (m_lsChallengeItemFeatures_History[i].m_lsPresentHistory[j].m_intAccuracy == 1)
                    {
                        lsLexicalItem_CorrectCtr[intLexicalItemIdx] += 1;
                        lsChallengeItem_CorrectCtr[intChallengeItemIdx] += 1;
                        lsChallengeItemFeatures_CorrectCtr[intChallengeItemFeaturesIdx] += 1;
                    }
                }                
            }

            // bind datagrid lexical items
            int intPresentedCtrTotal = 0;
            int intCorrectCtrTotal = 0;
            for (int i = 0; i<m_lsLexicalItem.Count; i++)
            {
                CBindDGLexicalItem item = new CBindDGLexicalItem(i, m_lsLexicalItem[i], lsLexicalItem_PresentCtr[i], lsLexicalItem_CorrectCtr[i]);
                m_lsBindLexicalItem.Add(item);
                if (lsLexicalItem_PresentCtr[i] > 0)
                    m_intLexicalItemCoverage += 1;
                intPresentedCtrTotal += lsLexicalItem_PresentCtr[i];
                intCorrectCtrTotal += lsLexicalItem_CorrectCtr[i];
            }
            if (intPresentedCtrTotal > 0)
                m_dLexicalItemAccuracy = Math.Round(((double)intCorrectCtrTotal / intPresentedCtrTotal), 3);

            // bind datagrid challenge items
            intPresentedCtrTotal = 0;
            intCorrectCtrTotal = 0;
            for (int i = 0; i<m_lsChallengeItem.Count; i++)
            {
                CBindDGChallengeItem item = new CBindDGChallengeItem(i, m_lsChallengeItem[i], lsChallengeItem_PresentCtr[i], lsChallengeItem_CorrectCtr[i], m_lsLexicalItem);
                m_lsBindChallengeItem.Add(item);
                if (lsChallengeItem_PresentCtr[i] > 0)
                    m_intChallengeItemCoverage += 1;
                intPresentedCtrTotal += lsChallengeItem_PresentCtr[i];
                intCorrectCtrTotal += lsChallengeItem_CorrectCtr[i];

                // bind datagrid forced items
                if (m_lsChallengeItem[i].m_intForcedItem == 1)
                {
                    CBindDGForcedItem forcedItem = new CBindDGForcedItem(i, m_lsChallengeItem[i], lsChallengeItem_PresentCtr[i], lsChallengeItem_CorrectCtr[i], m_lsLexicalItem);
                    m_lsBindForcedItem.Add(forcedItem);
                }
            }
            if (intPresentedCtrTotal > 0)
                m_dChallengeItemAccuracy = Math.Round(((double)intCorrectCtrTotal / intPresentedCtrTotal), 3);

            // bind datagrid challenge item features
            intPresentedCtrTotal = 0;
            intCorrectCtrTotal = 0;
            for (int i = 0; i < m_lsChallengeItemFeatures.Count; i++)
            {
                CBindDGChallengeItemFeatures item = new CBindDGChallengeItemFeatures(i, m_lsChallengeItemFeatures[i], lsChallengeItemFeatures_PresentCtr[i], lsChallengeItemFeatures_CorrectCtr[i], m_lsLexicalItem, m_lsChallengeItem);
                m_lsBindChallengeItemFeatures.Add(item);
                if (lsChallengeItemFeatures_PresentCtr[i] > 0)
                    m_intChallengeItemFeaturesCoverage += 1;
                intPresentedCtrTotal += lsChallengeItemFeatures_PresentCtr[i];
                intCorrectCtrTotal += lsChallengeItemFeatures_CorrectCtr[i];
            }
            if (intPresentedCtrTotal > 0)
                m_dChallengeItemFeaturesAccuracy = Math.Round(((double)intCorrectCtrTotal / intPresentedCtrTotal), 3);
        }

        //----------------------------------------------------------------------------------------------------
        // loadChallengeItemFeatures_HistoryComplexity (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadChallengeItemFeatures_HistoryComplexity()
        {
            // if xml has already been loaded, then return
            //if (m_lsChallengeItemFeatures_History.Count > 0) return;

            m_lsChallengeItemFeatures_HistoryComplexity.Clear();

            // check if file exists
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_challengeitemfeatures_history_complexity.csv";
            if (!System.IO.File.Exists(strCsvFile))
            {
                // set up user's historyComplexity with the original complexity
                for (int k = 0; k < m_lsChallengeItemFeatures.Count; k++)
                {
                    CUser_ChallengeItemFeatures_HistoryComplexity historyComplexity = new CUser_ChallengeItemFeatures_HistoryComplexity();
                    historyComplexity.m_lsBlockIdx.Add(0);
                    historyComplexity.m_lsComplexity_Overall.Add(m_lsChallengeItemFeatures[k].m_dComplexity_Overall);
                    m_lsChallengeItemFeatures_HistoryComplexity.Add(historyComplexity);
                }
                saveChallengeItemFeatures_HistoryComplexity_Csv();
                return;
            }

            string strWholeFile = System.IO.File.ReadAllText(strCsvFile); 

            // split into lines
            strWholeFile = strWholeFile.Replace('\n', '\r');
            string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // see how many rows & columns there are
            int intNumRows = lines.Length;
            //int intNumCols = lines[0].Split(',').Length;

            int i = 0;
            while (i < intNumRows)
            {
                //Console.WriteLine (lines [i]);

                CUser_ChallengeItemFeatures_HistoryComplexity history = new CUser_ChallengeItemFeatures_HistoryComplexity();

                string[] line_r = lines[i].Split(',');
                int intNumCols = line_r.Length;

                // first col = idx
                for (int j = 1; j < intNumCols-1; j++)
                {
                    //Console.WriteLine(line_r[j]);
                    history.m_lsBlockIdx.Add(Convert.ToInt32(line_r[j]));
                    j++;
                    history.m_lsComplexity_Overall.Add(Math.Round(Convert.ToDouble(line_r[j]), 4));                    
                }
                m_lsChallengeItemFeatures_HistoryComplexity.Add(history);                   

                i++; // next line
            }    // end while

        }

        //----------------------------------------------------------------------------------------------------
        // loadChallengeItem_HistoryComplexity (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadChallengeItem_HistoryComplexity()
        {
            // if xml has already been loaded, then return
            //if (m_lsChallengeItemFeatures_History.Count > 0) return;

            m_lsChallengeItem_HistoryComplexity.Clear();

            // check if file exists
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_challengeitem_history_complexity.csv";
            if (!System.IO.File.Exists(strCsvFile))
            {
                // set up user's historyComplexity with the original complexity
                for (int k = 0; k < m_lsChallengeItem.Count; k++)
                {
                    CUser_ChallengeItem_HistoryComplexity historyComplexity = new CUser_ChallengeItem_HistoryComplexity();
                    historyComplexity.m_lsBlockIdx.Add(0);
                    historyComplexity.m_lsComplexity.Add(1);
                    m_lsChallengeItem_HistoryComplexity.Add(historyComplexity);
                }
                saveChallengeItem_HistoryComplexity_Csv();
                return;
            }

            string strWholeFile = System.IO.File.ReadAllText(strCsvFile);

            // split into lines
            strWholeFile = strWholeFile.Replace('\n', '\r');
            string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // see how many rows & columns there are
            int intNumRows = lines.Length;
            //int intNumCols = lines[0].Split(',').Length;

            int i = 0;
            while (i < intNumRows)
            {
                //Console.WriteLine (lines [i]);
                CUser_ChallengeItem_HistoryComplexity history = new CUser_ChallengeItem_HistoryComplexity();

                string[] line_r = lines[i].Split(',');
                int intNumCols = line_r.Length;

                // first col = idx
                for (int j = 1; j < intNumCols - 1; j++)
                {
                    //Console.WriteLine(line_r[j]);
                    history.m_lsBlockIdx.Add(Convert.ToInt32(line_r[j]));
                    j++;
                    history.m_lsComplexity.Add(Math.Round(Convert.ToDouble(line_r[j]), 4));
                }
                m_lsChallengeItem_HistoryComplexity.Add(history);

                i++; // next line
            }    // end while
        }

        //----------------------------------------------------------------------------------------------------
        // loadLexicalItem_HistoryComplexity (xml)
        //----------------------------------------------------------------------------------------------------
        public void loadLexicalItem_HistoryComplexity()
        {
            // if xml has already been loaded, then return
            //if (m_lsChallengeItemFeatures_History.Count > 0) return;

            m_lsLexicalItem_HistoryComplexity.Clear();

            // check if file exists
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_lexicalitem_history_complexity.csv";
            if (!System.IO.File.Exists(strCsvFile))
            {
                // set up user's historyComplexity with the original complexity
                for (int k = 0; k < m_lsLexicalItem.Count; k++)
                {
                    CUser_LexicalItem_HistoryComplexity historyComplexity = new CUser_LexicalItem_HistoryComplexity();
                    historyComplexity.m_lsBlockIdx.Add(0);
                    historyComplexity.m_lsComplexity.Add(1);
                    m_lsLexicalItem_HistoryComplexity.Add(historyComplexity);
                }
                saveLexicalItem_HistoryComplexity_Csv();
                return;
            }

            string strWholeFile = System.IO.File.ReadAllText(strCsvFile);

            // split into lines
            strWholeFile = strWholeFile.Replace('\n', '\r');
            string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // see how many rows & columns there are
            int intNumRows = lines.Length;
            //int intNumCols = lines[0].Split(',').Length;

            int i = 0;
            while (i < intNumRows)
            {
                //Console.WriteLine (lines [i]);
                CUser_LexicalItem_HistoryComplexity history = new CUser_LexicalItem_HistoryComplexity();

                string[] line_r = lines[i].Split(',');
                int intNumCols = line_r.Length;

                // first col = idx
                for (int j = 1; j < intNumCols - 1; j++)
                {
                    //Console.WriteLine(line_r[j]);
                    history.m_lsBlockIdx.Add(Convert.ToInt32(line_r[j]));
                    j++;
                    history.m_lsComplexity.Add(Math.Round(Convert.ToDouble(line_r[j]), 4));
                }
                m_lsLexicalItem_HistoryComplexity.Add(history);

                i++; // next line
            }    // end while
        }

        //----------------------------------------------------------------------------------------------------
        // loadLexicalItem_History (csv)
        //----------------------------------------------------------------------------------------------------
        public void loadLexicalItem_HistoryExposure()
        {
            // if csv has already been loaded, then return
            if (m_lsLexicalItem_HistoryExposure.Count > 0) return;

            m_lsLexicalItem_HistoryExposure.Clear();

            // check if file exists
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_lexicalitem_history_exposure.csv";
            if (!System.IO.File.Exists(strCsvFile))
            {
                // set up user's history with the 0 exposure
                for (int k = 0; k < m_lsLexicalItem.Count; k++)
                {
                    CUser_LexicalItem_HistoryExposure history = new CUser_LexicalItem_HistoryExposure();
                    history.m_intExposureCtr = 0;
                    m_lsLexicalItem_HistoryExposure.Add(history);
                }
                saveLexicalItem_HistoryExposure_Csv();
                return;
            }

            string strWholeFile = System.IO.File.ReadAllText(strCsvFile);

            // split into lines
            strWholeFile = strWholeFile.Replace('\n', '\r');
            string[] lines = strWholeFile.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // see how many rows & columns there are
            int intNumRows = lines.Length;
            //int intNumCols = lines[0].Split(',').Length;

            int i = 0;
            while (i < intNumRows)
            {
                //Console.WriteLine (lines [i]);

                CUser_LexicalItem_HistoryExposure history = new CUser_LexicalItem_HistoryExposure();

                string[] line_r = lines[i].Split(',');
                int intNumCols = line_r.Length;

                // first col = idx
                for (int j = 1; j < intNumCols - 1; j++)
                {
                    //Console.WriteLine(line_r[j]);
                    if (j == 1) history.m_intExposureCtr = Convert.ToInt32(line_r[j]);
                }
                m_lsLexicalItem_HistoryExposure.Add(history);

                i++; // next line
            }    // end while

        }

        //----------------------------------------------------------------------------------------------------
        // resetBlock
        //----------------------------------------------------------------------------------------------------
        public void resetBlock()
        {
            m_intCurBlockType = 0;
            m_intCurNoiseLevel = 0;
        }

        //----------------------------------------------------------------------------------------------------
        // getCurBlockType
        //----------------------------------------------------------------------------------------------------
        public int getCurBlockType()
        {
            if (m_lsForcedBlockHistory_Weekly.Count == 0) return 0;

            m_intCurBlockType = 0;

            // check if it is the start of a new week
            CUser_ForcedBlockHistory_Weekly lastWeekly = m_lsForcedBlockHistory_Weekly.Last();
            DateTime dtLastWeeklyDate = Convert.ToDateTime(lastWeekly.m_strDate);
            double dTotalDays = (System.DateTime.Now - dtLastWeeklyDate).TotalDays;
            if (dTotalDays >= 7)
            {
                // set new weekly schedule
                CUser_ForcedBlockHistory_Weekly weekly = new CUser_ForcedBlockHistory_Weekly();
                weekly.m_strDate = System.DateTime.Now.ToString();
                m_lsForcedBlockHistory_Weekly.Add(weekly);
            }
            else
            {
                // continue this week schedule

                // check if this week has already more than 17 forced blocks
                int intTotal = 0;
                for (int i = 0; i < lastWeekly.m_lsForcedBlockHistory_Daily.Count; i++)
                    intTotal += lastWeekly.m_lsForcedBlockHistory_Daily[i].m_lsBlockIdx.Count;
                if (intTotal >= 17)
                    return 0;

                // check if it is the start of a new day
                if (lastWeekly.m_lsForcedBlockHistory_Daily.Count == 0)
                {
                    // set up new daily schedule
                    CUser_ForcedBlockHistory_Daily daily = new CUser_ForcedBlockHistory_Daily();
                    daily.m_strDate = System.DateTime.Now.ToString();
                    lastWeekly.m_lsForcedBlockHistory_Daily.Add(daily);
                    m_intCurBlockType = 0;
                }
                else
                {
                    CUser_ForcedBlockHistory_Daily lastDaily = lastWeekly.m_lsForcedBlockHistory_Daily.Last();
                    DateTime dtLastDailyDate = Convert.ToDateTime(lastDaily.m_strDate);
                    if ((System.DateTime.Now - dtLastDailyDate).TotalDays < 1)
                    {
                        // continue today's schedule
                        if (lastDaily.m_lsBlockIdx.Count < 4)  // each day should have a min of 4 forced blocks 
                        {
                            int intLastBlockIdx = 0;
                            if (lastDaily.m_lsBlockIdx.Count > 0)
                                intLastBlockIdx = lastDaily.m_lsBlockIdx.Last();
                            if  ( ((m_lsTherapyBlock.Count - intLastBlockIdx) % 4) == 0)
                                m_intCurBlockType = 1;
                        }
                    }
                    else
                    {
                        // set up new daily schedule
                        CUser_ForcedBlockHistory_Daily daily = new CUser_ForcedBlockHistory_Daily();
                        daily.m_strDate = System.DateTime.Now.ToString();
                        lastWeekly.m_lsForcedBlockHistory_Daily.Add(daily);
                        m_intCurBlockType = 0;
                    }
                }
            }

            // check if it is clash with the cur noise block, if yes, move the noise block to the next block
            if (m_intCurBlockType == 1)  
            { 
                if (m_lsNoiseBlockIdx.Count > 0)               
                    if (m_lsNoiseBlockIdx.Last() == m_lsTherapyBlock.Count)  // current block should be a noise block
                        m_lsNoiseBlockIdx[m_lsNoiseBlockIdx.Count - 1] = m_lsNoiseBlockIdx.Last() + 1;
            }
            return m_intCurBlockType;
        }

        //----------------------------------------------------------------------------------------------------
        // getCurBlockNoiseLevel
        //----------------------------------------------------------------------------------------------------
        public int getCurBlockNoiseLevel()
        {
            //int intNoiseLevel = 0;

            // check if it is time to set the next noise block
            if ((m_lsTherapyBlock.Count % CConstants.g_intNoiseBlockInterval) == 0)
            {
                // check if it is time to increase or regress noise level based on the last five noise blocks
                if (m_lsNoiseLevelHistory.Count > 0)
                {
                    if ((m_lsNoiseBlockIdx.Count % CConstants.g_intNoiseBlockNumPerWindow) == 0)
                    {
                        //List<CUser_TherapyBlock> lsTherapyBlockNoise = new List<CUser_TherapyBlock>();
                        //lsTherapyBlockNoise = m_lsTherapyBlock.FindAll(a => a.m_intNoiseLevel > 0);
                        double dTotal = 0;
                        int intCtr = 0;
                        for (int i = m_lsNoiseBlockIdx.Count - 1; i >= 0; i--)
                        {                            
                            {
                                double dNoiseBlockAccuracy = m_lsTherapyBlock[m_lsNoiseBlockIdx[i]].m_dAccuracyRate;
                                double dPrevBlockAccuracy = m_lsTherapyBlock[m_lsNoiseBlockIdx[i] - 1].m_dAccuracyRate;
                                if ((dNoiseBlockAccuracy == 1) && (dPrevBlockAccuracy == 1))
                                    dTotal += 0.1;
                                else
                                    dTotal += (dNoiseBlockAccuracy - dPrevBlockAccuracy);
                                intCtr++;
                                Console.WriteLine("dNoiseBlockAccuracy = " + dNoiseBlockAccuracy + " dPrevBlockAccuracy = " + dPrevBlockAccuracy + " ctr = " + intCtr);
                                if (intCtr >= CConstants.g_intNoiseBlockNumPerWindow)
                                    break;
                            }
                        }
                        int intNextNoiseLevel = m_lsNoiseLevelHistory.Last();
                        if (intCtr >= CConstants.g_intNoiseBlockNumPerWindow)
                        {
                            double dAverage = dTotal / CConstants.g_intNoiseBlockNumPerWindow;
                            Console.WriteLine("dAverage = " + dAverage);
                            if (dAverage >= 0)
                                intNextNoiseLevel = m_lsNoiseLevelHistory.Last() + 1;
                            else if (dAverage <= -0.3)
                                intNextNoiseLevel = m_lsNoiseLevelHistory.Last() - 1;
                            if (intNextNoiseLevel < (int)CConstants.g_NoiseLevel.PhoneVoice)
                                intNextNoiseLevel = (int)CConstants.g_NoiseLevel.PhoneVoice;
                            if (intNextNoiseLevel > (int)CConstants.g_NoiseLevel.Noise5)
                                intNextNoiseLevel = (int)CConstants.g_NoiseLevel.Noise5;
                        }
                        m_lsNoiseLevelHistory.Add(intNextNoiseLevel);
                        Console.WriteLine("intNextNoiseLevel = " + intNextNoiseLevel);
                    }
                }
                else
                    m_lsNoiseLevelHistory.Add(1);

                // set the next noise block index
                Random rnd = new Random();
                int intRnd = rnd.Next(0, CConstants.g_intNoiseBlockInterval);
                // no two consecutive noise blocks are allowed
                if ((intRnd == 0) && (getLastTherapyBlock().m_intNoiseLevel > 0))
                    intRnd++;
                m_lsNoiseBlockIdx.Add(m_lsTherapyBlock.Count + intRnd);
                Console.WriteLine("intNextNoiseBlockIdx = " + (m_lsTherapyBlock.Count + intRnd));
            }

            m_intCurNoiseLevel = 0;
            if ( (m_lsNoiseBlockIdx.Count > 0) && (m_lsNoiseLevelHistory.Count > 0) )
                if (m_lsNoiseBlockIdx.Last() == m_lsTherapyBlock.Count)  // current block should be a noise block
                    m_intCurNoiseLevel = m_lsNoiseLevelHistory.Last();

            return m_intCurNoiseLevel;
        }

        //----------------------------------------------------------------------------------------------------
        // updateHistory
        //----------------------------------------------------------------------------------------------------
 /*       public void updateHistory(string strStartTime, double dLinguisticCateogry, double dNoiseLevel, List<int> lsLexicalItemIdx,
                                    List<int> lsChallengeItemFeaturesIdx, List<int> lsIsDiversity, List<int> lsResponse, int intDiversityNum,
                                    double dMean_Frequency, double dMean_Concreteness, double dMean_DistractorNum,
                                    double dStdDeviation_Frequency, double dStdDeviation_Concreteness, double dStdDeviation_DistractorNum
                                    )
        {
            if ( (lsChallengeItemFeaturesIdx.Count == 0) || (lsResponse.Count == 0) )
                    return;

            if (lsChallengeItemFeaturesIdx.Count != lsResponse.Count)
                return;

            // update lexical item history
            for (int i = 0; i < lsLexicalItemIdx.Count; i++)
            {
                int intIdx = lsLexicalItemIdx[i];
                m_lsLexicalItem_HistoryExposure[intIdx].m_intExposureCtr++;
            }

            // update m_lsTherapyBlock
            CUser_TherapyBlock therapyBlock = new CUser_TherapyBlock();
            therapyBlock.m_strStartTime = strStartTime;
            therapyBlock.m_strEndTime = System.DateTime.Now.ToString();

            //therapyBlock.m_dLinguisticCateogryComplexity = dLinguisticCateogry;
            // convert linguistic category
            if (dLinguisticCateogry >= 1)
                therapyBlock.m_intLinguisticCateogry = 2;
            else if (dLinguisticCateogry >= 0.55)
                therapyBlock.m_intLinguisticCateogry = 1;
            else                
                therapyBlock.m_intLinguisticCateogry = 0;

            //therapyBlock.m_dNoiseLevelComplexity = dNoiseLevel;
            therapyBlock.m_intNoiseLevel = m_intCurNoiseLevel;
            therapyBlock.m_intDiversityNum = intDiversityNum;
            therapyBlock.m_intBlockType = m_intCurBlockType;

            int intTotalAccuracy = 0;
            double dTotalComplexity = 0;
            //double dTotalAbility = 0;
            int intCorrectResponseCtr = 0;
            int intIncorrectResponseCtr = 0;
            double dTotalComplexity_Correct = 0;
            double dTotalComplexity_Incorrect = 0;

            for (int i = 0; i < lsChallengeItemFeaturesIdx.Count; i++)
            {
                therapyBlock.m_lsChallengeItemFeaturesIdx.Add(lsChallengeItemFeaturesIdx[i]);
                therapyBlock.m_lsIsDiversity.Add(lsIsDiversity[i]);
                therapyBlock.m_lsResponseAccuracy.Add(lsResponse[i]);
                intTotalAccuracy += lsResponse[i];

                // calculate complexity
                double dItemOriginalComplexity = m_lsChallengeItemFeatures_HistoryComplexity[lsChallengeItemFeaturesIdx[i]].m_lsComplexity_Overall.First();

                double dItemCurComplexity = m_lsChallengeItemFeatures_HistoryComplexity[lsChallengeItemFeaturesIdx[i]].m_lsComplexity_Overall.Last();

                int intChallengeItemIdx = m_lsChallengeItemFeatures[lsChallengeItemFeaturesIdx[i]].m_intChallengeItemIdx;
                double dChallengeItemCurComplexity = m_lsChallengeItem_HistoryComplexity[intChallengeItemIdx].m_lsComplexity.Last();

                int intLexicalItemIdx = m_lsChallengeItem[intChallengeItemIdx].m_intLexicalItemIdx;
                double dLexicalItemCurComplexity = m_lsLexicalItem_HistoryComplexity[intLexicalItemIdx].m_lsComplexity.Last();

                double dComplexity = Math.Round(dItemCurComplexity * dChallengeItemCurComplexity * dLexicalItemCurComplexity, 4);
                therapyBlock.m_lsChallengeItemFeatures_Complexity.Add(dComplexity);
                dTotalComplexity += dComplexity;

                //dTotalAbility += (dItemCurComplexity * lsResponse[i]); // (dItemOriginalComplexity * lsResponse[i]);

                // reduce item's complexity if user has a correct response, 
                if (lsResponse[i] == 1)
                {
                    intCorrectResponseCtr++;
                    dTotalComplexity_Correct += dComplexity;
                    // don not update complexity if this is a noise block or a forced block
                    if ((therapyBlock.m_intNoiseLevel == 0) && (therapyBlock.m_intBlockType == 0))
                        updateChallengeItemFeatures_Complexity(m_lsTherapyBlock.Count, lsChallengeItemFeaturesIdx[i], true);
                }
                else
                {
                    intIncorrectResponseCtr++;
                    dTotalComplexity_Incorrect += dComplexity;
                    //if (therapyBlock.m_intNoiseLevel == 0)
                    //    updateChallengeItemFeatures_Complexity(lsChallengeItemFeaturesIdx[i], false);
                }
            }

            therapyBlock.m_dAccuracyRate = Math.Round(((double)intTotalAccuracy / (double)lsResponse.Count), 4);
            therapyBlock.m_dBlockComplexity = Math.Round((dTotalComplexity / (double)lsResponse.Count), 4);
            //therapyBlock.m_dUserAbility = Math.Round(((double)dTotalAbility / (double)lsResponse.Count), 4);
            //therapyBlock.m_dUserAbility = Math.Round(((double)dTotalAbility / intCorrectResponseCtr), 4);

            // increase user ability
            double dLastUserAbility = getLastTherapyBlock_UserAbility();
            double dAverageAbility = 0;
            double dCorerctDifference = 0;
            double dIncorerctDifference = 0;
            therapyBlock.m_dUserAbility = dLastUserAbility;
            if (intCorrectResponseCtr > 0)
            {
                dAverageAbility = Math.Round(((double)dTotalComplexity_Correct / intCorrectResponseCtr), 4);
                dCorerctDifference = dAverageAbility - dLastUserAbility;  
                //if (dCorerctDifference > 0)
                //    therapyBlock.m_dUserAbility = therapyBlock.m_dUserAbility + dCorerctDifference;
                //Console.WriteLine("save: lastUserAbility = " + dLastUserAbility + " correctAbility = " + dAverageAbility + " correctDifference = " + dDifference + " userAbility = " + therapyBlock.m_dUserAbility);
            }
            if (intIncorrectResponseCtr > 0)
            {
                dAverageAbility = Math.Round(((double)dTotalComplexity_Incorrect / intIncorrectResponseCtr), 4);
                dIncorerctDifference = dAverageAbility - dLastUserAbility;
                //if (dIncorerctDifference < 0)
                //    therapyBlock.m_dUserAbility = therapyBlock.m_dUserAbility + dIncorerctDifference;
                //Console.WriteLine("save: lastUserAbility = " + dLastUserAbility + " incorrectAbility = " + dAverageAbility + " incorrectDifference = " + dDifference + " userAbility = " + therapyBlock.m_dUserAbility);
            }

            if (therapyBlock.m_dAccuracyRate >= 0.7)
            {
                //dAverageAbility = Math.Round(((double)dTotalComplexity_Correct / intCorrectResponseCtr) + 0.01, 4);
                //therapyBlock.m_dUserAbility = dAverageAbility;
                if (dCorerctDifference > 0)
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);  // increase by dCorerctDifference
                else
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + 0.001, 4);  // increase by 0.01
            }
            else if (therapyBlock.m_dAccuracyRate <= 0.4)
            {
                if (dIncorerctDifference < 0)
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);  // regress by incorerctDifference
                else
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility - 0.01, 4);  // regress by 0.01
            }
            else
            {
                if ((dCorerctDifference > 0) && (dIncorerctDifference > 0) && (dIncorerctDifference > dCorerctDifference))
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);  // increase by dCorerctDifference
                else if ((dCorerctDifference > 0) && (dIncorerctDifference > 0))
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + ((dCorerctDifference + dIncorerctDifference) / 2), 4); // increase by average
                else if ((dCorerctDifference > 0) && (dIncorerctDifference == 0))
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);  // increase by dCorerctDifference
                else if ((dCorerctDifference > 0) && (dIncorerctDifference < 0))
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dIncorerctDifference, 4);  // regress by dIncorerctDifference
                //else if ((dCorerctDifference < 0) && (dIncorerctDifference > 0))
                //    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);  // regress by dCorerctDifference
                //else if ((dCorerctDifference < 0) && (dIncorerctDifference < 0) && (dIncorerctDifference < dCorerctDifference))
                //    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dIncorerctDifference, 4);  // regress by dIncorerctDifference
                else if ((dCorerctDifference < 0) && (dIncorerctDifference < 0))
                    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + ((dCorerctDifference + dIncorerctDifference) / 2), 4);  // regress by dCorerctDifference
                //else if ((dCorerctDifference < 0) && (dIncorerctDifference == 0))
                //    therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);  // regress by dCorerctDifference
            }

            /*if ((dCorerctDifference > 0) && (dIncorerctDifference < 0))
                therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + ((dCorerctDifference + dIncorerctDifference) / 2), 4);
            else if (dCorerctDifference > 0)
                therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dCorerctDifference, 4);
            else if (dIncorerctDifference < 0)
                therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dIncorerctDifference, 4);
*/
 /*           Console.WriteLine("save: lastUserAbility = " + dLastUserAbility + " correctDifference = " + dCorerctDifference + " incorrectDifference = " + dIncorerctDifference + " userAbility = " + therapyBlock.m_dUserAbility);

            /*
            double dImprovement = Math.Round(dTotalComplexity_Correct / m_dCorpusTotalComplexity, 4); // dAverageAbility - dLastUserAbility;         
            double dRegression = Math.Round(dTotalComplexity_Incorrect / m_dCorpusTotalComplexity, 4);
            therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility + dImprovement - dRegression, 4);
            */

            // penalise user ability
            /*double dAverageIncorrect = 0;
            double dRegression = 0;
            if (intIncorrectResponseCtr > 0)
            {
                dAverageIncorrect = Math.Round(((double)dTotalComplexity_Incorrect / intIncorrectResponseCtr), 4);
                dRegression = dAverageIncorrect - dLastUserAbility;
                //therapyBlock.m_dUserAbility -= Math.Sqrt(Math.Pow(dRegression, 2));
            }
            if (therapyBlock.m_dAccuracyRate >= CConstants.g_dUserAccuracyThresholdMax)
            {
                // because the lexical item's complexity values are reduced by 2% for every correct response, so it might happen that the dImprovement < 0 
                if (dImprovement > 0)
                    therapyBlock.m_dUserAbility = dLastUserAbility + dImprovement;
                else
                    therapyBlock.m_dUserAbility = dLastUserAbility;
            }                
            else if (therapyBlock.m_dAccuracyRate <= CConstants.g_dUserAccuracyThresholdMin)
            {
                therapyBlock.m_dUserAbility = Math.Round(dLastUserAbility - Math.Sqrt(Math.Pow(dRegression, 2)), 4);
                if (therapyBlock.m_dUserAbility < 0)
                    therapyBlock.m_dUserAbility = 0;
            }            
            else
                therapyBlock.m_dUserAbility = dLastUserAbility; */

            //Console.WriteLine("save: lastUserAbility = " + dLastUserAbility + " improvement = " + dImprovement + " regression = " + dRegression + " userAbility = " + therapyBlock.m_dUserAbility);
 /*           Console.WriteLine("save: lastUserAbility = " + dLastUserAbility + " userAbility = " + therapyBlock.m_dUserAbility);

            /*double dIncrementFactor = 0;
            double dLastUserAbility = getLastTherapyBlock_UserAbility();
            if (therapyBlock.m_dAccuracyRate > CConstants.g_dUserAccuracyThresholdMax)
                dIncrementFactor = dLastUserAbility * CConstants.g_dUserAccuracyFactor;
            else if (therapyBlock.m_dAccuracyRate < CConstants.g_dUserAccuracyThresholdMax)
                dIncrementFactor = -(dLastUserAbility * CConstants.g_dUserAccuracyFactor);
            if (dLastUserAbility == 0)
                therapyBlock.m_dUserAbility = Math.Round(((double)dTotalAbility / (double)lsResponse.Count), 4);
            else
                therapyBlock.m_dUserAbility = dLastUserAbility + Math.Round(dIncrementFactor, 4);*/

  /*          therapyBlock.m_dMean_Frequency = Math.Round(dMean_Frequency, 4);
            therapyBlock.m_dMean_Concreteness = Math.Round(dMean_Concreteness, 4);
            therapyBlock.m_dMean_DistractorNum = Math.Round(dMean_DistractorNum, 4);
            therapyBlock.m_dStdDeviation_Frequency = Math.Round(dStdDeviation_Frequency, 4);
            therapyBlock.m_dStdDeviation_Concreteness = Math.Round(dStdDeviation_Concreteness, 4);
            therapyBlock.m_dStdDeviation_DistractorNum = Math.Round(dStdDeviation_DistractorNum, 4);
            m_lsTherapyBlock.Add(therapyBlock);

            // update total therapy time
            TimeSpan span = DateTime.Now - Convert.ToDateTime(therapyBlock.m_strStartTime);
            m_dTotalTherapyTimeMin += Math.Round(span.TotalMinutes, 4);

            // update m_lsChallengeItemFeatures_History
            for (int i = 0; i < lsChallengeItemFeaturesIdx.Count; i++)
            {
                int intIdx = m_lsChallengeItemFeatures_History.FindIndex(a => a.m_intChallengeItemFeaturesIdx == lsChallengeItemFeaturesIdx[i]);
                if (intIdx > -1)
                {
                    // this challenge item has been presented before
                    CPresentHistory presentHistory = new CPresentHistory();
                    presentHistory.m_intSessionIdx = 0;
                    presentHistory.m_intBlockIdx = m_lsTherapyBlock.Count - 1;
                    presentHistory.m_intAccuracy = lsResponse[i];
                    m_lsChallengeItemFeatures_History[intIdx].m_lsPresentHistory.Add(presentHistory);
                }
                else
                {
                    // this challenge item is presented for the first time
                    CUser_ChallengeItemFeatures_History challengeItem_History = new CUser_ChallengeItemFeatures_History();
                    challengeItem_History.m_intChallengeItemFeaturesIdx = lsChallengeItemFeaturesIdx[i];
                    CPresentHistory presentHistory = new CPresentHistory();
                    presentHistory.m_intSessionIdx = 0;
                    presentHistory.m_intBlockIdx = m_lsTherapyBlock.Count - 1;
                    presentHistory.m_intAccuracy = lsResponse[i];
                    challengeItem_History.m_lsPresentHistory.Add(presentHistory);

                    m_lsChallengeItemFeatures_History.Add(challengeItem_History);
                }
            }

            // set the start date in m_lsForcedBlockHistory_Weekly
            if (m_lsTherapyBlock.Count == 1)
            {
                CUser_ForcedBlockHistory_Weekly weekly = new CUser_ForcedBlockHistory_Weekly();
                weekly.m_strDate = System.DateTime.Now.ToString();
                m_lsForcedBlockHistory_Weekly.Add(weekly);
            }
            if (m_intCurBlockType == 1)
            {
                m_lsForcedBlockHistory_Weekly.Last().m_lsForcedBlockHistory_Daily.Last().m_lsBlockIdx.Add(m_lsTherapyBlock.Count - 1);
            }

            // save to xml
            saveUserProfileToXml();
            saveTherapyBlocksToXml();
            saveChallengeItemFeaturesHistoryToXml();
            saveChallengeItemFeatures_HistoryComplexity_Csv();
            saveChallengeItem_HistoryComplexity_Csv();
            saveLexicalItem_HistoryComplexity_Csv();
            saveLexicalItem_HistoryExposure_Csv();
            
        }
*/
        //----------------------------------------------------------------------------------------------------
        // saveUserProfileToXml
        //----------------------------------------------------------------------------------------------------
        public void updateChallengeItemFeatures_Complexity(int intBlockIdx, int intChallengeItemFeaturesIdx, bool bReduce)
        {
            // reduce challengeitemfeature by 5% based on minimum exposure = 20
            double dOriginalComplexity = m_lsChallengeItemFeatures_HistoryComplexity[intChallengeItemFeaturesIdx].m_lsComplexity_Overall.First();
            double dLastComplexity = m_lsChallengeItemFeatures_HistoryComplexity[intChallengeItemFeaturesIdx].m_lsComplexity_Overall.Last();
            double dNewComplexity = dLastComplexity;
            if (bReduce)
                dNewComplexity = dLastComplexity - (dOriginalComplexity / CConstants.g_intItemComplexityDecrementFactor);
            else
                dNewComplexity = dLastComplexity + (dOriginalComplexity / CConstants.g_intItemComplexityDecrementFactor);

            m_lsChallengeItemFeatures_HistoryComplexity[intChallengeItemFeaturesIdx].m_lsComplexity_Overall.Add(Math.Round(dNewComplexity, 4));
            m_lsChallengeItemFeatures_HistoryComplexity[intChallengeItemFeaturesIdx].m_lsBlockIdx.Add(intBlockIdx);

            // reduce the complexity of the challenge item in the tree accordingly
            double dTotalOriginalComplexity = 0;
            double dTotalLastComplexity = 0;
            int intChallengeItemIdx = m_lsChallengeItemFeatures[intChallengeItemFeaturesIdx].m_intChallengeItemIdx;
            for (int i = 0; i < m_lsChallengeItem[intChallengeItemIdx].m_lsChallengeItemFeaturesIdx.Count; i++)
            {
                int intFeaturesIdx = m_lsChallengeItem[intChallengeItemIdx].m_lsChallengeItemFeaturesIdx[i];
                dTotalOriginalComplexity += m_lsChallengeItemFeatures_HistoryComplexity[intFeaturesIdx].m_lsComplexity_Overall.First();
                dTotalLastComplexity += m_lsChallengeItemFeatures_HistoryComplexity[intFeaturesIdx].m_lsComplexity_Overall.Last();
            }
            dNewComplexity = 0;
            if (dTotalOriginalComplexity > 0) dNewComplexity = Math.Round(dTotalLastComplexity / dTotalOriginalComplexity, 4);
            m_lsChallengeItem_HistoryComplexity[intChallengeItemIdx].m_lsComplexity.Add(dNewComplexity);
            m_lsChallengeItem_HistoryComplexity[intChallengeItemIdx].m_lsBlockIdx.Add(intBlockIdx);

            // reduce the complexity of the lexical item in the tree accordingly
            dTotalOriginalComplexity = 0;
            dTotalLastComplexity = 0;
            int intLexicalItemIdx = m_lsChallengeItem[intChallengeItemIdx].m_intLexicalItemIdx;
            for (int i = 0; i < m_lsLexicalItem[intLexicalItemIdx].m_lsChallengeItemIdx.Count; i++)
            {
                int intItemIdx = m_lsLexicalItem[intLexicalItemIdx].m_lsChallengeItemIdx[i];
                dTotalOriginalComplexity += m_lsChallengeItem_HistoryComplexity[intItemIdx].m_lsComplexity.First();
                dTotalLastComplexity += m_lsChallengeItem_HistoryComplexity[intItemIdx].m_lsComplexity.Last();
            }
            dNewComplexity = 0;
            if (dTotalOriginalComplexity > 0) dNewComplexity = Math.Round(dTotalLastComplexity / dTotalOriginalComplexity, 4);
            m_lsLexicalItem_HistoryComplexity[intLexicalItemIdx].m_lsComplexity.Add(dNewComplexity);
            m_lsLexicalItem_HistoryComplexity[intLexicalItemIdx].m_lsBlockIdx.Add(intBlockIdx);

        }

        //----------------------------------------------------------------------------------------------------
        // saveUserProfileToXml
        //----------------------------------------------------------------------------------------------------
        public void saveUserProfileToXml()
        {
            // save lsTrial to xml 
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?>" +
                "<root>" +
                "</root>");

            // Save the document to a file. White space is preserved (no white space).
            string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_profile.xml"; // "user_profile.xml";

            XmlElement xmlChild = doc.CreateElement("userid");
            xmlChild.InnerText = m_strUserId;
            doc.DocumentElement.AppendChild(xmlChild);

            xmlChild = doc.CreateElement("totalTherapyTimeMin");
            xmlChild.InnerText = m_dTotalTherapyTimeMin.ToString();
            doc.DocumentElement.AppendChild(xmlChild);

            xmlChild = doc.CreateElement("curNoiseLevel");
            xmlChild.InnerText = m_intCurNoiseLevel.ToString();
            doc.DocumentElement.AppendChild(xmlChild);
                        
            xmlChild = doc.CreateElement("noiseHistory");
            for (var i = 0; i < m_lsNoiseLevelHistory.Count; i++)
            {
                XmlElement xmlChild2 = doc.CreateElement("level");
                xmlChild2.InnerText = m_lsNoiseLevelHistory[i].ToString();
                xmlChild.AppendChild(xmlChild2);
            }
            doc.DocumentElement.AppendChild(xmlChild);

            xmlChild = doc.CreateElement("noiseBlocks");
            for (var i = 0; i < m_lsNoiseBlockIdx.Count; i++)
            {
                XmlElement xmlChild2 = doc.CreateElement("idx");
                xmlChild2.InnerText = m_lsNoiseBlockIdx[i].ToString();
                xmlChild.AppendChild(xmlChild2);
            }
            doc.DocumentElement.AppendChild(xmlChild);

            // save forcedBlockHistory
            xmlChild = doc.CreateElement("forcedBlocks");
            for (var i = 0; i < m_lsForcedBlockHistory_Weekly.Count; i++)
            {
                XmlElement xmlChild2 = doc.CreateElement("weekly");
                
                // add weekly date
                XmlElement xmlChild3 = doc.CreateElement("date");
                xmlChild3.InnerText = m_lsForcedBlockHistory_Weekly[i].m_strDate;
                xmlChild2.AppendChild(xmlChild3);

                // add weekly's daily 
                for (var j = 0; j < m_lsForcedBlockHistory_Weekly[i].m_lsForcedBlockHistory_Daily.Count; j++)
                {
                    xmlChild3 = doc.CreateElement("daily");
                    // daily date
                    XmlElement xmlChild4 = doc.CreateElement("date");
                    xmlChild4.InnerText = m_lsForcedBlockHistory_Weekly[i].m_lsForcedBlockHistory_Daily[j].m_strDate;
                    xmlChild3.AppendChild(xmlChild4);
                    // daily block idx list
                    for (var k = 0; k < m_lsForcedBlockHistory_Weekly[i].m_lsForcedBlockHistory_Daily[j].m_lsBlockIdx.Count; k++)
                    {
                        xmlChild4 = doc.CreateElement("idx");
                        xmlChild4.InnerText = m_lsForcedBlockHistory_Weekly[i].m_lsForcedBlockHistory_Daily[j].m_lsBlockIdx[k].ToString();
                        xmlChild3.AppendChild(xmlChild4);
                    }
                    xmlChild2.AppendChild(xmlChild3);
                }

                xmlChild.AppendChild(xmlChild2);
            }
            doc.DocumentElement.AppendChild(xmlChild);

            //doc.PreserveWhitespace = true;
            doc.Save(strXmlFile);
        }

        //----------------------------------------------------------------------------------------------------
        // saveTherapyBlocksToXml
        //----------------------------------------------------------------------------------------------------
  /*      public void saveTherapyBlocksToXml()
        {
            // save lsTrial to xml 
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?>" +
                "<root>" +
                "</root>");

            // Save the document to a file. White space is preserved (no white space).
            string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_therapyblocks.xml";

            /*
			<block idx="0">
			  	<linguisticCateogry>0</linguisticCateogry>
			  	<noiseLevel>0</noiseLevel>	
                <accuracyRate>0.o</accuracyRate>			  	
			  	<challengeItems>
                    <idx> 0 </idx>
                    <idx> 3 </idx>
                    <idx> 10 </idx>
			    </challengeItems> 
                <responses>
                    <acc> 0 </acc>
                    <acc> 1 </acc>
                    <acc> 0 </acc>
			    </responses> 
                <freq m="0.0", sd="0.0" /> 
                <conc m="0.0", sd="0.0" /> 
                <distr m="0.0", sd="0.0" />               
			</block> 
			*/

 /*           for (int i = 0; i < m_lsTherapyBlock.Count; i++)
            {
                XmlElement xmlNode = doc.CreateElement("block");
                XmlAttribute attr = doc.CreateAttribute("idx");
                attr.Value = i.ToString();
                xmlNode.SetAttributeNode(attr);

                XmlElement xmlChild2 = doc.CreateElement("sTime");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_strStartTime; //System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("eTime");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_strEndTime;
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("lc");
                //xmlChild2.InnerText = m_lsTherapyBlock[i].m_dLinguisticCateogryComplexity.ToString();
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_intLinguisticCateogry.ToString();
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("noise");
                //xmlChild2.InnerText = m_lsTherapyBlock[i].m_dNoiseLevelComplexity.ToString();
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_intNoiseLevel.ToString();
                xmlNode.AppendChild(xmlChild2);

                /*xmlChild2 = doc.CreateElement("diversityNum");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_intDiversityNum.ToString();
                xmlNode.AppendChild(xmlChild2);*/

  /*              xmlChild2 = doc.CreateElement("bType");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_intBlockType.ToString();
                xmlNode.AppendChild(xmlChild2);

                // add challenge item idx
                XmlElement xmlChallengeItem = doc.CreateElement("cif");
                for (var j = 0; j < m_lsTherapyBlock[i].m_lsChallengeItemFeaturesIdx.Count; j++)
                {
                    /*XmlElement xmlChild3 = doc.CreateElement("idx");
                    xmlChild3.InnerText = m_lsTherapyBlock[i].m_lsChallengeItemFeaturesIdx[j].ToString();
                    xmlChallengeItem.AppendChild(xmlChild3);*/

   /*                 XmlElement xmlChild3 = doc.CreateElement("item");
                    attr = doc.CreateAttribute("idx");
                    attr.Value = m_lsTherapyBlock[i].m_lsChallengeItemFeaturesIdx[j].ToString();
                    xmlChild3.SetAttributeNode(attr);

                    attr = doc.CreateAttribute("div");
                    attr.Value = m_lsTherapyBlock[i].m_lsIsDiversity[j].ToString();
                    xmlChild3.SetAttributeNode(attr);

                    attr = doc.CreateAttribute("res");
                    attr.Value = m_lsTherapyBlock[i].m_lsResponseAccuracy[j].ToString();
                    xmlChild3.SetAttributeNode(attr);

                    attr = doc.CreateAttribute("cpx");
                    attr.Value = m_lsTherapyBlock[i].m_lsChallengeItemFeatures_Complexity[j].ToString();
                    xmlChild3.SetAttributeNode(attr);

                    xmlChallengeItem.AppendChild(xmlChild3);
                }
                xmlNode.AppendChild(xmlChallengeItem);

                // add responses
                /*XmlElement xmlResponse = doc.CreateElement("response");
                for (var j = 0; j < m_lsTherapyBlock[i].m_lsResponseAccuracy.Count; j++)
                {
                    XmlElement xmlChild3 = doc.CreateElement("acc");
                    xmlChild3.InnerText = m_lsTherapyBlock[i].m_lsResponseAccuracy[j].ToString();
                    xmlResponse.AppendChild(xmlChild3);
                }
                xmlNode.AppendChild(xmlResponse);*/

  /*              xmlChild2 = doc.CreateElement("accRate");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_dAccuracyRate.ToString();
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("blockCpx");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_dBlockComplexity.ToString();
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("userAb");
                xmlChild2.InnerText = m_lsTherapyBlock[i].m_dUserAbility.ToString();
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("freq");
                attr = doc.CreateAttribute("m");
                attr.Value = m_lsTherapyBlock[i].m_dMean_Frequency.ToString();
                xmlChild2.SetAttributeNode(attr);
                attr = doc.CreateAttribute("sd");
                attr.Value = m_lsTherapyBlock[i].m_dStdDeviation_Frequency.ToString();
                xmlChild2.SetAttributeNode(attr);
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("conc");
                attr = doc.CreateAttribute("m");
                attr.Value = m_lsTherapyBlock[i].m_dMean_Concreteness.ToString();
                xmlChild2.SetAttributeNode(attr);
                attr = doc.CreateAttribute("sd");
                attr.Value = m_lsTherapyBlock[i].m_dStdDeviation_Concreteness.ToString();
                xmlChild2.SetAttributeNode(attr);
                xmlNode.AppendChild(xmlChild2);

                xmlChild2 = doc.CreateElement("distr");
                attr = doc.CreateAttribute("m");
                attr.Value = m_lsTherapyBlock[i].m_dMean_DistractorNum.ToString();
                xmlChild2.SetAttributeNode(attr);
                attr = doc.CreateAttribute("sd");
                attr.Value = m_lsTherapyBlock[i].m_dStdDeviation_DistractorNum.ToString();
                xmlChild2.SetAttributeNode(attr);
                xmlNode.AppendChild(xmlChild2);
                
                doc.DocumentElement.AppendChild(xmlNode);
            }

            //doc.PreserveWhitespace = true;
            doc.Save(strXmlFile);
        }
*/
        //----------------------------------------------------------------------------------------------------
        // saveChallengeItemFeaturesHistoryToXml
        //----------------------------------------------------------------------------------------------------
        public void saveChallengeItemFeaturesHistoryToXml()
        {
            // save lsTrial to xml 
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?>" +
                "<root>" +
                "</root>");

            // Save the document to a file. White space is preserved (no white space).
            string strXmlFile = m_strAppPath + "user_" + m_strUserId + "_challengeitemfeatures_history.xml";          
            
            /*
			<item idx="0">
			  	<challengeItemIdx>0</challengeItemIdx>			  			  	
			  	<presentHistory>
                    <history sessionIdx="0" blockIdx="0" acc="0" /history>
                    <history sessionIdx="0" blockIdx="0" acc="0" /history>
                    <history sessionIdx="0" blockIdx="0" acc="0" /history>
			    </presentHistory>          
            </item> 
			*/

            for (int i = 0; i < m_lsChallengeItemFeatures_History.Count; i++)
            {
                XmlElement xmlNode = doc.CreateElement("item");
                XmlAttribute attr = doc.CreateAttribute("idx");
                attr.Value = i.ToString();
                xmlNode.SetAttributeNode(attr);

                XmlElement xmlChild2 = doc.CreateElement("cifIdx");
                xmlChild2.InnerText = m_lsChallengeItemFeatures_History[i].m_intChallengeItemFeaturesIdx.ToString();
                xmlNode.AppendChild(xmlChild2);

                // add history
                xmlChild2 = doc.CreateElement("hist");
                for (var j = 0; j < m_lsChallengeItemFeatures_History[i].m_lsPresentHistory.Count; j++)
                {
                    XmlElement xmlChild3 = doc.CreateElement("h");
                    attr = doc.CreateAttribute("sIdx");
                    attr.Value = m_lsChallengeItemFeatures_History[i].m_lsPresentHistory[j].m_intSessionIdx.ToString();
                    xmlChild3.SetAttributeNode(attr);
                    attr = doc.CreateAttribute("bIdx");
                    attr.Value = m_lsChallengeItemFeatures_History[i].m_lsPresentHistory[j].m_intBlockIdx.ToString();
                    xmlChild3.SetAttributeNode(attr);
                    attr = doc.CreateAttribute("acc");
                    attr.Value = m_lsChallengeItemFeatures_History[i].m_lsPresentHistory[j].m_intAccuracy.ToString();
                    xmlChild3.SetAttributeNode(attr);

                    xmlChild2.AppendChild(xmlChild3);
                }
                xmlNode.AppendChild(xmlChild2);                

                doc.DocumentElement.AppendChild(xmlNode);
            }

            //doc.PreserveWhitespace = true;
            doc.Save(strXmlFile);
        }

        //----------------------------------------------------------------------------------------------------
        // saveChallengeItemFeatures_HistoryComplexity_Csv
        //----------------------------------------------------------------------------------------------------
        public void saveChallengeItemFeatures_HistoryComplexity_Csv()
        {
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_challengeitemfeatures_history_complexity.csv";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(strCsvFile))
            {
                for (var i = 0; i < m_lsChallengeItemFeatures_HistoryComplexity.Count; i++)
                {
                    CUser_ChallengeItemFeatures_HistoryComplexity history = m_lsChallengeItemFeatures_HistoryComplexity[i];

                    string strRow = "";
                    strRow = strRow + i + ",";
                    for (var j = 0; j < history.m_lsComplexity_Overall.Count; j++)
                    {
                        strRow = strRow + history.m_lsBlockIdx[j] + ",";
                        strRow = strRow + history.m_lsComplexity_Overall[j] + ",";
                    }

                    // write to file
                    sw.WriteLine(strRow);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        // saveChallengeItem_HistoryComplexity_Csv
        //----------------------------------------------------------------------------------------------------
        public void saveChallengeItem_HistoryComplexity_Csv()
        {
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_challengeitem_history_complexity.csv";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(strCsvFile))
            {
                for (var i = 0; i < m_lsChallengeItem_HistoryComplexity.Count; i++)
                {
                    CUser_ChallengeItem_HistoryComplexity history = m_lsChallengeItem_HistoryComplexity[i];

                    string strRow = "";
                    strRow = strRow + i + ",";
                    for (var j = 0; j < history.m_lsComplexity.Count; j++)
                    {
                        strRow = strRow + history.m_lsBlockIdx[j] + ",";
                        strRow = strRow + history.m_lsComplexity[j] + ",";
                    }

                    // write to file
                    sw.WriteLine(strRow);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        // saveLexicalItem_HistoryComplexity_Csv
        //----------------------------------------------------------------------------------------------------
        public void saveLexicalItem_HistoryComplexity_Csv()
        {
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_lexicalitem_history_complexity.csv";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(strCsvFile))
            {
                for (var i = 0; i < m_lsLexicalItem_HistoryComplexity.Count; i++)
                {
                    CUser_LexicalItem_HistoryComplexity history = m_lsLexicalItem_HistoryComplexity[i];

                    string strRow = "";
                    strRow = strRow + i + ",";
                    for (var j = 0; j < history.m_lsComplexity.Count; j++)
                    {
                        strRow = strRow + history.m_lsBlockIdx[j] + ",";
                        strRow = strRow + history.m_lsComplexity[j] + ",";
                    }

                    // write to file
                    sw.WriteLine(strRow);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        // saveLexicalItem_History_Csv
        //----------------------------------------------------------------------------------------------------
        public void saveLexicalItem_HistoryExposure_Csv()
        {
            string strCsvFile = m_strAppPath + "user_" + m_strUserId + "_lexicalitem_history_exposure.csv";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(strCsvFile))
            {
                for (var i = 0; i < m_lsLexicalItem_HistoryExposure.Count; i++)
                {
                    CUser_LexicalItem_HistoryExposure history = m_lsLexicalItem_HistoryExposure[i];

                    string strRow = "";
                    strRow = strRow + i + "," + history.m_intExposureCtr + ","; 
                    
                    // write to file
                    sw.WriteLine(strRow);
                }
            }
        }

    }
}
