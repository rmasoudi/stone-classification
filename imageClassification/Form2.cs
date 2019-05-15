using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.ML.Structure;
using Emgu.CV.CvEnum;
using System.Diagnostics;
namespace imageClassification
{
    public partial class Form2 : Form
    {
        Stopwatch watch;
        Matrix<float> data;
        Matrix<float> response;
        Image<Bgr, Byte> img;
        DTree dtree;
        SVM svm;
        int imageCounter = 0;
        int trainCount = 144;
        int dimen = 500;
        int featureCount = 4;
        Image<Bgr, Byte> histogram;

        double maxSM = 0;
        double maxCor = 0;
        double maxVar = 0;
        double maxIDM = 0;

        long totalTrainTime = 0;
        long totalTestTime = 0;
        
        public Form2()
        {
            InitializeComponent();
            data = new Matrix<float>(trainCount, featureCount);
            response = new Matrix<float>(trainCount, 1);
            dtree = new DTree();
            int[,] ls={{4,100,100,4}};
            Matrix<int> layerSize=new Matrix<int>(ls);
            svm = new SVM();
            histogram = new Image<Bgr, byte>(4*trainCount, 256);
        }
        
        private void GetTrainData(Image<Bgr, Byte> image)
        {

            using (Image<Gray, Byte> img = image.Convert<Gray, Byte>().Resize(dimen, dimen, INTER.CV_INTER_LINEAR))
            {
                
                double[] features = extractFeatures(img);
                for (int i = 0; i < features.Length; i++)
                {
                        data[imageCounter, i] = (float)features[i];
                }
                
            }
        }
        public double[] extractFeatures(Image<Gray, Byte> img)
        {
            int n=dimen;
            int m =256;
            double[,] horizantal=new double[n,n];
            double[,] vertical=new double[n,n];
            double[,] asli=new double[n,n];
            double[,] farei=new double[n,n];
            for (int i = 0; i < dimen; i++)
            {
                for (int j = 0; j < dimen; j++)
                {
                    double cell = CvInvoke.cvGet2D(img, i, j).v0;
                    if (j != 0)
                    {
                        horizantal[(int)cell, (int)CvInvoke.cvGet2D(img, i, j - 1).v0]++;   //  0 degree>>>left
                    }
                    if (j <dimen-1)
                        horizantal[(int)cell, (int)CvInvoke.cvGet2D(img, i, j+1).v0]++;   //  0 degree>>>right
                    if (i != 0)
                        vertical[(int)cell, (int)CvInvoke.cvGet2D(img, i-1, j).v0]++;     // 90 degree>>>up
                    if (i < dimen-1)
                        vertical[(int)cell, (int)CvInvoke.cvGet2D(img, i+1, j).v0]++;     // 90 degree>>>down
                    if (i != 0 && j<dimen-1)
                        farei[(int)cell, (int)CvInvoke.cvGet2D(img, i-1, j+1).v0]++;      // 45 degree>>>upper right
                    if (i<dimen-1 && j != 0)
                        farei[(int)cell, (int)CvInvoke.cvGet2D(img, i+1, j-1).v0]++;      // 45 degree>>>lower left
                    if (i!=0 && j != 0)
                        asli[(int)cell, (int)CvInvoke.cvGet2D(img, i-1, j-1).v0]++;       //135 degree>>>upper left
                    if (i<dimen-1 && j<dimen-1)
                        asli[(int)cell, (int)CvInvoke.cvGet2D(img, i+1, j+1).v0]++;       //135 degree>>>lower right
                }

            }
            //now we calculate fetures from 4 matrix. then calculate average features
            double ux_hor = 0;
            double uy_hor = 0;
            double Qx_hor = 0;
            double Qy_hor = 0;
            double secondMoment_hor = 0;
            double contrast_hor = 0;
            double correlation_hor = 0;
            double variance_hor = 0;
            double sum_hor=0;
            double corfactor_hor = 0;
            double avg_hor = 0;
            double idm_hor = 0;
            double entropy_hor = 0;
            double ux_ver = 0;
            double uy_ver = 0;
            double Qx_ver = 0;
            double Qy_ver = 0;
            double secondMoment_ver = 0;
            double contrast_ver = 0;
            double correlation_ver = 0;
            double variance_ver = 0;
            double sum_ver = 0;
            double corfactor_ver = 0;
            double avg_ver = 0;
            double idm_ver = 0;
            double entropy_ver = 0;
            double ux_asl = 0;
            double uy_asl = 0;
            double Qx_asl = 0;
            double Qy_asl = 0;
            double secondMoment_asl = 0;
            double contrast_asl = 0;
            double correlation_asl = 0;
            double variance_asl = 0;
            double sum_asl = 0;
            double corfactor_asl = 0;
            double avg_asl = 0;
            double idm_asl = 0;
            double entropy_asl = 0;
            double ux_far = 0;
            double uy_far = 0;
            double Qx_far = 0;
            double Qy_far = 0;
            double secondMoment_far = 0;
            double contrast_far = 0;
            double correlation_far = 0;
            double variance_far = 0;
            double sum_far = 0;
            double avg_far = 0;
            double corfactor_far = 0;
            double idm_far = 0;
            double entropy_far = 0;
            for (int i = 0; i < m; i++)
            {
                double tempSumHor=0;
                double tempSumVer = 0;
                double tempSumAsl = 0;
                double tempSumFar = 0;
                for (int j = 0; j < m; j++)
                {
                    
                    //normalization
                    horizantal[i, j] = horizantal[i, j] / (2*n * (n - 1));
                    vertical[i, j] = vertical[i, j] / (2*n * (n - 1));
                    asli[i, j] = asli[i, j] / (2*(n-1) * (n - 1));
                    farei[i, j] = farei[i, j] / (2*(n-1) * (n - 1));

                    sum_hor += horizantal[i, j];
                    sum_ver += vertical[i, j];
                    sum_asl += asli[i, j];
                    sum_far += farei[i, j];

                    tempSumHor += horizantal[i, j];
                    tempSumVer += vertical[i, j];
                    tempSumAsl += asli[i, j];
                    tempSumFar += farei[i, j];
                    //////////////////////////////////////////////////////////////
                    secondMoment_hor += (horizantal[i, j] * horizantal[i, j]);
                    secondMoment_ver += (vertical[i, j] * vertical[i, j]);
                    secondMoment_asl += (asli[i, j] * asli[i, j]);
                    secondMoment_far += (farei[i, j] * farei[i, j]);

                    corfactor_hor += (i * j * horizantal[i, j]);
                    corfactor_ver += (i * j * vertical[i, j]);
                    corfactor_asl += (i * j * asli[i, j]);
                    corfactor_far += (i * j * farei[i, j]);
                    ////////////////////////////////////////////////////////////
                    idm_hor += ((1 / (1 + (i - j) * (i - j)))*horizantal[i,j]);
                    idm_ver += ((1 / (1 + (i - j) * (i - j))) * vertical[i, j]);
                    idm_asl += ((1 / (1 + (i - j) * (i - j))) * asli[i, j]);
                    idm_far += ((1 / (1 + (i - j) * (i - j))) * farei[i, j]);
                    entropy_hor += (horizantal[i, j] * Math.Log (horizantal[i, j]));
                    entropy_ver += (vertical [i, j] * Math.Log(vertical[i, j]));
                    entropy_asl += (asli[i, j] * Math.Log(asli[i, j]));
                    entropy_far += (farei[i, j] * Math.Log(farei[i, j]));
                }
                ux_hor=tempSumHor/m;
                ux_ver=tempSumVer/m;
                ux_asl=tempSumAsl/m;
                ux_far=tempSumFar/m;

                Qx_hor += ((tempSumHor - ux_hor) * (tempSumHor - ux_hor));
                Qx_ver += ((tempSumVer - ux_ver)*(tempSumVer - ux_ver));
                Qx_asl += ((tempSumAsl - ux_asl)*(tempSumAsl - ux_asl));
                Qx_far += ((tempSumFar - ux_far) * (tempSumFar - ux_far));
            }

            avg_hor = sum_hor / (m * m);
            avg_ver = sum_ver / (m * m);
            avg_asl = sum_asl / (m * m);
            avg_far = sum_far / (m * m);
           
            Qx_hor = Math.Sqrt(Qx_hor / (m - 1));
            Qx_ver = Math.Sqrt(Qx_ver / (m - 1));
            Qx_asl = Math.Sqrt(Qx_asl / (m - 1));
            Qx_far = Math.Sqrt(Qx_far / (m - 1));

            for (int i = 0; i < m; i++)
            {
                double tempSumHor = 0;
                double tempSumVer = 0;
                double tempSumAsl = 0;
                double tempSumFar = 0;
                for (int j = 0; j < m; j++)
                {

                    tempSumHor += horizantal[j, i];
                    tempSumVer += vertical[j, i];
                    tempSumAsl += asli[j, i];
                    tempSumFar += farei[j, i];
                    //////////////////////////////////////////////////////////////////
                    variance_hor += ((i - avg_hor) * (i - avg_hor) * horizantal[i, j]);
                    variance_ver += ((i - avg_ver) * (i - avg_ver) * vertical[i, j]);
                    variance_asl += ((i - avg_asl) * (i - avg_asl) * asli[i, j]);
                    variance_far += ((i - avg_far) * (i - avg_far) * farei[i, j]);
                }
                uy_hor = tempSumHor / m;
                uy_ver = tempSumVer / m;
                uy_asl = tempSumAsl / m;
                uy_far = tempSumFar / m;

                Qy_hor += (tempSumHor - uy_hor);
                Qy_ver += (tempSumVer - uy_ver);
                Qy_asl += (tempSumAsl - uy_asl);
                Qy_far += (tempSumFar - uy_far);

            }
            Qy_hor = Math.Sqrt(Qy_hor / (m - 1));
            Qy_ver = Math.Sqrt(Qy_ver / (m - 1));
            Qy_asl = Math.Sqrt(Qy_asl / (m - 1));
            Qy_far = Math.Sqrt(Qy_far / (m - 1));
            /////////////////////////////////////////////////////////////////////
            correlation_hor = (corfactor_hor - ux_hor * uy_hor) / Qx_hor * Qy_hor;
            correlation_ver = (corfactor_ver - ux_ver * uy_ver) / Qx_ver * Qy_ver;
            correlation_asl = (corfactor_asl - ux_asl * uy_asl) / Qx_asl * Qy_asl;
            correlation_far = (corfactor_far - ux_far * uy_far) / Qx_far * Qy_far;
            /////////////////////////////////////////////////////////////////////
            entropy_hor = -entropy_hor;
            entropy_ver = -entropy_ver;
            entropy_asl = -entropy_asl;
            entropy_far = -entropy_far;
            
            double[] result=new double[featureCount];
            result[0] = (secondMoment_hor  + secondMoment_ver  + secondMoment_asl + secondMoment_far) / 4;
            result[1] = (idm_hor  + idm_ver  + idm_asl  + idm_far) / 4;
            result[2] = (variance_hor + variance_ver+ variance_asl + variance_far) / 4;
            result[3] = (correlation_hor + correlation_ver + correlation_asl  + correlation_far) / 4;
            if (result[0] > maxSM)
                maxSM = result[0];
            if (result[1] > maxIDM)
                maxIDM = result[1];
            if (result[2] > maxVar)
                maxVar= result[2];
            if (result[3] > maxCor)
                maxCor = result[3];
            //result[4] = (entropy_hor / (2 * n * (n - 1)) + entropy_ver / (2 * n * (n - 1)) + entropy_asl / (2 * (n - 1) * (n - 1)) + entropy_far / (2 * (n - 1) * (n - 1))) / 4;
            return result;
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        public void makeDataSet()
        {
            //get values for train matrix
            for (; imageCounter < trainCount/4; imageCounter++)
            {
                string path="c:\\test\\a\\" + imageCounter + ".jpg";
                img = new Image<Bgr, byte>(path);
                GetTrainData(img);
                response[imageCounter, 0] = 'a';
                progressBar1.Value = imageCounter * 100 / trainCount;
            }
            for (; imageCounter < trainCount / 2; imageCounter++)
            {
                string path = "c:\\test\\b\\" + (imageCounter - trainCount / 4) + ".jpg";
                img = new Image<Bgr, byte>(path);
                GetTrainData(img);
                response[imageCounter, 0] = 'b';
                progressBar1.Value = imageCounter * 100 / trainCount;
            }
            for (; imageCounter < 3*trainCount / 4; imageCounter++)
            {
                string path = "c:\\test\\c\\" + (imageCounter - trainCount / 2) + ".jpg";
                img = new Image<Bgr, byte>(path);
                GetTrainData(img);
                response[imageCounter, 0] = 'c';
                progressBar1.Value = imageCounter * 100 / trainCount;
            }
            for (; imageCounter < trainCount; imageCounter++)
            {
                string path = "c:\\test\\d\\" + (imageCounter - 3 * trainCount / 4) + ".jpg";
                img = new Image<Bgr, byte>(path);
                GetTrainData(img);
                response[imageCounter, 0] ='d';
                progressBar1.Value = imageCounter * 100 / trainCount;
            }
            for (int i = 0; i < trainCount; i++)
            {
                data[i, 0] /= (float)maxSM;
                data[i, 1] /= (float)maxIDM;
                data[i, 2] /= (float)maxVar;
                data[i, 3] /= (float)maxCor;
                PointF p1 = new PointF(4 * i, 256*data[i, 0]);
                histogram.Draw(new CircleF(p1, 2), new Bgr(255, 100, 100), -1);
                PointF p2 = new PointF(4 * i, 256 * data[i, 1]);
                histogram.Draw(new CircleF(p2, 2), new Bgr(100, 255, 100), -1);
                PointF p3 = new PointF(4 * i, 256 * data[i, 2]);
                histogram.Draw(new CircleF(p3, 2), new Bgr(100, 100, 255), -1);
                PointF p4 = new PointF(4 * i, 256 * data[i, 3]);
                histogram.Draw(new CircleF(p4, 2), new Bgr(36, 235, 240), -1);
            }
            histogram.Draw(new LineSegment2D(new Point(trainCount, 0), new Point(trainCount, 255)), new Bgr(255, 255, 255),3);
            histogram.Draw(new LineSegment2D(new Point(2*trainCount, 0), new Point(2*trainCount, 255)), new Bgr(255, 255, 255), 3);
            histogram.Draw(new LineSegment2D(new Point(3*trainCount, 0), new Point(3*trainCount, 255)), new Bgr(255, 255,255), 3);

            pictureBox2.Image = histogram;
            
        }
        private void button2_Click(object sender, EventArgs e)
        {

        }
        private Matrix<float> getSampleData(Image<Bgr, Byte> image)
        {
            Matrix<float> sample = new Matrix<float>(1, featureCount);
            Image<Bgr, Byte> img = new Image<Bgr, byte>(dimen,dimen);
            using (Image<Gray, Byte> sampleImg = image.Convert<Gray, Byte>().Resize(dimen, dimen, INTER.CV_INTER_LINEAR))
            {
                double[] features = extractFeatures(sampleImg);
                features[0] /= (float)maxSM;
                features[1] /= (float)maxIDM;
                features[2] /= (float)maxVar;
                features[3] /= (float)maxCor;
                Image<Bgr, byte> tempHistogram = new Image<Bgr, byte>(4 * trainCount, 256);
                tempHistogram = histogram.Clone();
                tempHistogram.Draw(new LineSegment2D(new Point(0, (int)(256 * features[0])), new Point(4 * trainCount, (int)(256 * features[0]))), new Bgr(255, 100, 100), 1);
                tempHistogram.Draw(new LineSegment2D(new Point(0, (int)(256 * features[1])), new Point(4 * trainCount, (int)(256 * features[1]))), new Bgr(100, 255, 100), 1);
                tempHistogram.Draw(new LineSegment2D(new Point(0, (int)(256 * features[2])), new Point(4 * trainCount, (int)(256 * features[2]))), new Bgr(100, 100, 255), 1);
                tempHistogram.Draw(new LineSegment2D(new Point(0, (int)(256 * features[3])), new Point(4 * trainCount, (int)(256 * features[3]))), new Bgr(36, 235, 240), 1);
                pictureBox2.Image = tempHistogram;
                
                for (int i = 0; i < featureCount; i++)
                {
                    sample[0, i] = (float)features[i];
                }
            }
            return sample;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            watch = Stopwatch.StartNew();
            makeDataSet();
            watch.Stop();
            txtFeatureTime.Text = "" + watch.ElapsedMilliseconds;
            progressBar1.Value = 100;
            int trainingSampleCount = (int)(trainCount * 0.75);
            int testSampleCount = (int)(trainCount * 0.25);
            watch = Stopwatch.StartNew();
            if (chkDtree.Checked)
            {
                Matrix<Byte> varType = new Matrix<byte>(data.Cols + 1, 1);
                varType.SetValue((byte)VAR_TYPE.NUMERICAL);
                Matrix<byte> sampleIdx = new Matrix<byte>(data.Rows, 1);
                using (Matrix<byte> sampleRows = sampleIdx.GetRows(0, trainingSampleCount, 1))
                    sampleRows.SetValue(255);
                IntPtr priors = new IntPtr();
                MCvDTreeParams param = new MCvDTreeParams();
                param.maxDepth = 8;
                param.minSampleCount = 10;
                param.regressionAccuracy = 0;
                param.useSurrogates = true;
                param.maxCategories = 15;
                param.cvFolds = 2;
                param.use1seRule = true;
                param.truncatePrunedTree = true;
                param.priors = priors;
                bool success = dtree.Train(
                    data,
                    Emgu.CV.ML.MlEnum.DATA_LAYOUT_TYPE.ROW_SAMPLE,
                    response,
                    null,
                    null,
                    varType,
                    null,
                    param);
                if (!success) return;
            }
            else
            {
                SVMParams param = new SVMParams();
                param.SVMType = SVM_TYPE.C_SVC;
                param.TermCrit = new MCvTermCriteria(1000);
                param.C = 4;
                svm.Train(data, response, null, null, param);
            }
            watch.Stop();
            txtLearnTime.Text = "" + watch.ElapsedMilliseconds;
            
            double trainDataCorrectRatio = 0;
            double testDataCorrectRatio = 0;
            Matrix<float> output = new Matrix<float>(1,featureCount);
            for (int i = 0; i < data.Rows; i++)
            {
                using (Matrix<float> sample = data.GetRow(i))
                {
                    double r;
                    if (chkDtree.Checked)
                    {
                        r = dtree.Predict(sample, null, true).value;
                    }
                    else
                    {
                        r=svm.Predict(sample);
                    }
                    r = Math.Abs(Math.Round(r) - response[i, 0]);
                    if (r < 1.0e-6)
                    {
                        if (i >=0 && i<3*trainCount/16)
                            trainDataCorrectRatio++;
                        else if (i >= 3*trainCount / 16 && i < trainCount / 4)
                            testDataCorrectRatio++;
                        else if (i >= trainCount / 4 && i < 7 * trainCount / 16)
                            trainDataCorrectRatio++;
                        else if (i >= 7 * trainCount / 16 && i < trainCount / 2)
                            testDataCorrectRatio++;
                        else if (i >= trainCount / 2 && i < 11 * trainCount / 16)
                            trainDataCorrectRatio++;
                        else if (i >= 11 * trainCount / 16 && i < 3*trainCount / 4)
                            testDataCorrectRatio++;
                        else if (i >= 3*trainCount / 4 && i < 15 * trainCount / 16)
                            trainDataCorrectRatio++;
                        else if (i >= 15 * trainCount / 16 && i <  trainCount )
                            testDataCorrectRatio++;

                    }
                }
            }
            trainDataCorrectRatio /= (trainCount*.75);
            testDataCorrectRatio /= (trainCount*.25);
            txtTestRate.Text =(Math.Round(testDataCorrectRatio,4) * 100).ToString();
            txtTrainRate.Text = (Math.Round(trainDataCorrectRatio, 4) * 100).ToString();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            DialogResult res = file.ShowDialog();
            if (res == DialogResult.OK)
            {
                pictureBox1.ImageLocation = file.FileName;
                img = new Image<Bgr, byte>(file.FileName);
                watch = Stopwatch.StartNew();
                double result=0;
                if (chkDtree.Checked)
                    result = dtree.Predict(getSampleData(img), null, true).value;
                else
                {
                    result = svm.Predict(getSampleData(img));
                }
                watch.Stop();
                txtTestTime.Text = "" + watch.ElapsedMilliseconds;
                lblType.Text =((char)Math.Round(result)).ToString().ToUpper();
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
