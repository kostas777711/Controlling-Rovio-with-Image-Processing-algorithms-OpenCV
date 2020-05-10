using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;


namespace rovio_1
{

    public partial class Form1 : Form
    {

                    /********************************/
                    /*        Public Variables      */
                    /********************************/


        /*************************************************\
        *                                                 *
        *    Image processing Choices                     *
        *                                                 * 
        *       0- Default/Normal                         *
        *       1- Edge Detection                         *
        *       2- Color Detection                        *
        *       3- Segmenting                             *
        *       4- Pink ball commanding (Rovio 2)         *
        *       5- Color Tracking                         *   
        *       6- Pink ball commanding (Rovio 1)         *
        *       7- Circle Detection (Rovio 1)             *
        *       8- PLAY WITH THE BALL                     *
        *                                                 *
        \**************************************************/

        int processingChoice = 0;

        //Rovio 1 URL
        static string rovio1URL = "http://192.168.2.14";
        //Rovio 2 URL
        static string rovio2URL = "http://192.168.2.16";




        //UI, declare a delegate:
        public delegate void MyDelegateMethod(string someParam);
        public delegate void MyDelegateMethod1();
        public delegate void MyDelegateMethod2();
        public delegate void MyDelegateMethod3(int battery);
        public delegate void MyDelegateMethod4(int battery2);

        

        /*   Create rovio object for rovio 1 for the first time     */
        RovioLib.RovioController rovio1 = new RovioLib.RovioController("username", "password", rovio1URL);

        /*   Create rovio object for rovio 2  for the first time        */
        RovioLib.RovioController rovio2 = new RovioLib.RovioController("username", "password", rovio2URL);
       

        //play Flag
        int playFlag = 0;

        /*   Initialisation of the Wander Thread        */
        Thread wanderThread = new Thread(new ThreadStart(wanderMethod));
        Thread wanderThread1 = new Thread(new ThreadStart(wanderMethod1));
        Thread wanderThread2 = new Thread(new ThreadStart(wanderMethod2));


        /*   Variables for Battery Status  Rovio 1 & 2      */
        public int battery, charging, battery1, charging1;

        /*   Stop/Play flag for wandering Rovio 1& 2       */
        bool wanderFlag = false;
        bool wanderFlag1 = false;
        bool wanderFlag2 = false;

        /*   Integers to hold first character of desired strings from GetReport rovio 1 & 2     */
        int  firstCharacterCharg, firstCharacterBat1, firstCharacterCharg1;


        /*   Strings from GetReport for rovio 1 & 2    */
        string str, str1, str_bat, str_charg, str_bat1, str_charg1;


        /*   Initialisation of rovio's speed   (1-fastest,10-slowest)    */
        public static int speed=1;

        
        /*     Positioning Coordinates   */
        int pos1x, pos1y, pos2x, pos2y;

        /*  String Variables for the wander methods    */
        public static string str_ir1, str_ir2;
        public static int flag, firstCharacter1, firstCharacter2;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            /*   Thread for taking images from Rovio 1      */
            Thread takeImagesThread1 = new Thread(new ThreadStart(takeImages1));
            takeImagesThread1.IsBackground = true;   //for the thread to close with the application
            takeImagesThread1.Start();
            //takeImagesThread1.Priority = ThreadPriority.Highest;

            /*   Thread for taking images from Rovio 2      */
            Thread takeImagesThread = new Thread(new ThreadStart(takeImages2));
            takeImagesThread.IsBackground = true;    //for the thread to close with the application
            takeImagesThread.Start();
            //takeImagesThread.Priority = ThreadPriority.Highest;

            /*   Enable Timer1 for displaying Battery Status       */
            //timer1.Enabled = true; 

            /*   Positioning Thread    */
            Thread positioningThread = new Thread(new ThreadStart(positioning_AND_battery_Monitoring));
            positioningThread.IsBackground = true;
            positioningThread.Start();
            //positioningThread.Priority = ThreadPriority.Lowest;

        }

        /*   Button for Displaying the main getReport  Rovio 1     */
        private void button40_Click(object sender, EventArgs e)
        {
            str = rovio1.GetReport();
            MessageBox.Show(str);
        }

        /*   Button for Displaying the main getReport  Rovio 2     */
        private void button1_Click(object sender, EventArgs e)
        {
            str1=rovio2.GetReport();
            MessageBox.Show(str1);
        }

        /*   selecting speed of Rovio       */
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Maximum = 10;
            numericUpDown1.Minimum = 1;
            speed = (int) numericUpDown1.Value;
        }


                    /////////
                    /////////    
                    /////////
        //////////////////////////////////////
        //                                  //
        //         Manual Control Rovio 1    //
        //                                  //  
        //////////////////////////////////////   

        private void button31_Click_1(object sender, EventArgs e)  //stop
        {
            rovio1.ManualDrive(0, speed);
        }

        private void button38_Click_1(object sender, EventArgs e)  //forward
        {
            rovio1.ManualDrive(1, speed);
        }

        private void button37_Click_1(object sender, EventArgs e)  //backwards
        {
            rovio1.ManualDrive(2, speed);
        }

        private void button36_Click_1(object sender, EventArgs e)    //right
        {
            rovio1.ManualDrive(4, speed);
        }

        private void button39_Click_1(object sender, EventArgs e)  //left
        {
            rovio1.ManualDrive(3, speed);
        }

        private void button35_Click_1(object sender, EventArgs e)  //forward right
        {
            rovio1.ManualDrive(8, speed);
        }

        private void button34_Click_1(object sender, EventArgs e)  //forward left
        {
            rovio1.ManualDrive(7, speed);
        }

        private void button32_Click_1(object sender, EventArgs e)   //backwards right
        {
            rovio1.ManualDrive(10, speed);
        }

        private void button33_Click_1(object sender, EventArgs e)   //backwards left
        {
            rovio1.ManualDrive(9, speed);
        }

        private void button30_Click_1(object sender, EventArgs e)  //head Up
        {
            rovio1.ManualDrive(11, speed);
        }

        private void button29_Click_1(object sender, EventArgs e)   //head down
        {
            rovio1.ManualDrive(12, speed);
        }

        private void button28_Click_1(object sender, EventArgs e)  //head middle
        {
            rovio1.ManualDrive(13, speed);
        }

        private void button27_Click_1(object sender, EventArgs e)  //rotate right
        {
            rovio1.ManualDrive(18, speed);
        }

        private void button26_Click_1(object sender, EventArgs e)    //rotate left
        {
            rovio1.ManualDrive(17, speed);
        }


                      /////////
                      /////////    
                      /////////
        //////////////////////////////////////
        //                                  //
        //         Manual Control Rovio 2    //
        //                                  //  
        //////////////////////////////////////   

        private void button10_Click(object sender, EventArgs e)  //stop
        {
            rovio2.ManualDrive(0, speed);
        }

        private void button2_Click(object sender, EventArgs e)  //forward
        {
                rovio2.ManualDrive(1, speed);
        }

        private void button5_Click(object sender, EventArgs e)  //backwards
        {
                rovio2.ManualDrive(2, speed);
        }

        private void button4_Click(object sender, EventArgs e)    //right
        {
                rovio2.ManualDrive(4, speed);
        }

        private void button3_Click(object sender, EventArgs e)  //left
        {
                rovio2.ManualDrive(3, speed);
        }

        private void button6_Click(object sender, EventArgs e)  //forward right
        {
            rovio2.ManualDrive(8, speed);
        }

        private void button7_Click(object sender, EventArgs e)  //forward left
        {
            rovio2.ManualDrive(7, speed);
        }

        private void button9_Click(object sender, EventArgs e)   //backwards right
        {
            rovio2.ManualDrive(10, speed);
        }

        private void button8_Click(object sender, EventArgs e)   //backwards left
        {
            rovio2.ManualDrive(9, speed);
        }

        private void button11_Click(object sender, EventArgs e)  //head Up
        {
            rovio2.ManualDrive(11, speed);
        }

        private void button12_Click(object sender, EventArgs e)   //head down
        {  
            rovio2.ManualDrive(12, speed);
        }

        private void button13_Click(object sender, EventArgs e)  //head middle
        {
            rovio2.ManualDrive(13, speed);
        }

        private void button14_Click(object sender, EventArgs e)  //rotate right
        {
            rovio2.ManualDrive(18, speed);
        }

        private void button15_Click(object sender, EventArgs e)    //rotate left
        {
            rovio2.ManualDrive(17, speed);
        }

                /////////
                /////////    
                /////////
        //////////////////////////////////
        //           (Rovio 1)          //
        //  IR-based Wandering Method   //
        //                              //  
        //              &               //
        //                              //
        //         Button_click         //
        //                              //
        //////////////////////////////////   

        private void button42_Click(object sender, EventArgs e)
        {
            if (wanderFlag1 == false)
            {
                wanderFlag1 = true;
                //change image to pause
                button42.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector
                rovio1.ActivateIRDetector();
                wanderThread1.Start();
                
            }
            else if (wanderFlag1 == true)
            {
                wanderFlag1 = false;
                button42.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\play.jpg");
                rovio1.DeactivateIRDetector();
                wanderThread1.Suspend();
            }
        }


                    /////////
                    /////////    
                    /////////
        //////////////////////////////////
        //           (Rovio 2)          //
        //  IR-based Wandering Method   //
        //                              //  
        //              &               //
        //                              //
        //         Button_click         //
        //                              //
        //////////////////////////////////   


        //button click method wander Rovio 2
        private void button19_Click(object sender, EventArgs e) 
        {

            if (wanderFlag2 == false)
            {
                wanderFlag2 = true;
                //change image to pause
                button19.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector
                rovio2.ActivateIRDetector();
                wanderThread2.Start();
                
            }
            else if (wanderFlag2 == true)
            {
                wanderFlag2 = false;
                button19.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\play.jpg");
                rovio2.DeactivateIRDetector();
                wanderThread2.Suspend();
            }

            
        }

                    /////////
                    /////////    
                    /////////
        //////////////////////////////////
        //    (Rovio 1 & 2 Together)    //
        //  IR-based Wandering Method   //
        //                              //  
        //              &               //
        //                              //
        //         Button_click         //
        //                              //
        //////////////////////////////////  
        private void button45_Click(object sender, EventArgs e)
        {

            if (wanderFlag == false)
            {
                wanderFlag = true;
                //change image to pause
                button45.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector
                rovio1.ActivateIRDetector();
                rovio2.ActivateIRDetector();
                wanderThread.Start();

            }
            else if (wanderFlag == true)
            {
                wanderFlag = false;
                button45.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\play.jpg");
                rovio1.DeactivateIRDetector();
                rovio2.DeactivateIRDetector();
                wanderThread.Suspend();
            }

        }

        //roaming together
        public static void wanderMethod()
        {
            RovioLib.RovioController rovio1 = new RovioLib.RovioController("username", "password", rovio1URL);
            RovioLib.RovioController rovio2 = new RovioLib.RovioController("username", "password", rovio2URL);
            
            while (true)
            {
                //get Reports in these two strings
                str_ir1 = rovio1.GetReport();
                str_ir2 = rovio2.GetReport();


                //get the x and y coordinates of the two rovios
                //get Rovio 1 x coordinate

                int first = str_ir1.IndexOf("x=");
                string tempString = str_ir1.Substring(first + 2, 6);
                int last = tempString.IndexOf("|");
                string str;
                str = tempString.Substring(0, last);
                int pos1x = Convert.ToInt32(str);

                //get Rovio 1 y coordinate

                first = str_ir1.IndexOf("y=");
                tempString = str_ir1.Substring(first + 2, 6);
                last = tempString.IndexOf("|");
                str = tempString.Substring(0, last);
                int pos1y = Convert.ToInt32(str);

                //get Rovio 2 x coordinate

                first = str_ir2.IndexOf("x=");
                tempString = str_ir2.Substring(first + 2, 6);
                last = tempString.IndexOf("|");
                str = tempString.Substring(0, last);
                int pos2x = Convert.ToInt32(str);

                //get Rovio 2 y coordinate

                first = str_ir2.IndexOf("y=");
                tempString = str_ir2.Substring(first + 2, 6);
                last = tempString.IndexOf("|");
                str = tempString.Substring(0, last);
                int pos2y = Convert.ToInt32(str);

                //so Rovio1(pos1x,pos1y) AND Rovio2(pos2x,pos2y)
                


                //check the flags of the IR of Rovio 1
                if (str_ir1 == "5")
                {
                    rovio1.ManualDrive(1, speed);  //forward

                }
                else
                {
                    rovio1.ManualDrive(17, speed);  //rotate left 
                    rovio1.ManualDrive(0, speed); //stop
                }

                //check the flags of the IR of Rovio 2
                if (str_ir2 == "5")
                {
                    rovio2.ManualDrive(1, speed);  //forward

                }
                else
                {
                    rovio2.ManualDrive(17, speed);  //rotate left 
                    rovio2.ManualDrive(0, speed); //stop
                }
            }

        }

        //wander method for rovio 1
        public static void wanderMethod1()
        {
            RovioLib.RovioController rovio1 = new RovioLib.RovioController("username", "password", rovio1URL);
            while (true)
            {
                str_ir1 = rovio1.GetReport();
                firstCharacter1 = str_ir1.IndexOf("flags");
                str_ir1 = str_ir1.Substring(firstCharacter1 + 9, 1);

                if (str_ir1 == "5")
                {
                    rovio1.ManualDrive(1, speed);  //forward

                }
                else
                {
                    rovio1.ManualDrive(17, speed);  //rotate left 
                    rovio1.ManualDrive(0, speed); //stop
                }
            }

        }

        //wander method for rovio 2
        public static void wanderMethod2()
        {
            RovioLib.RovioController rovio2 = new RovioLib.RovioController("username", "password", rovio2URL);
            while (true)
            {
                str_ir2 = rovio2.GetReport();
                firstCharacter2 = str_ir2.IndexOf("flags");
                str_ir2 = str_ir2.Substring(firstCharacter2 + 9, 1);

                if (str_ir2 == "5")
                {
                    rovio2.ManualDrive(1, speed);  //forward

                }
                else
                {
                    rovio2.ManualDrive(17, speed);  //rotate left 
                    rovio2.ManualDrive(0, speed); //stop
                }
            }

        }



        private void button20_Click(object sender, EventArgs e)
        {
            MessageBox.Show(rovio2.GetMCUReport());
        }


        /***************************/
        /*******              ******/
        /*      test button        */
        /******               ******/
        /***************************/

        private void button21_Click(object sender, EventArgs e)  
        {

            //get Reports in these two strings
            str_ir1 = rovio1.GetReport();
            str_ir2 = rovio2.GetReport();


            //get the x and y coordinates of the two rovios
            //get Rovio 1 x coordinate

            int first = str_ir1.IndexOf("x=");
            string tempString = str_ir1.Substring(first + 2, 6);
            int last = tempString.IndexOf("|");
            string str;
            str = tempString.Substring(0, last);
            int pos1x = Convert.ToInt32(str);

            //get Rovio 1 y coordinate

            first = str_ir1.IndexOf("y=");
            tempString = str_ir1.Substring(first + 2, 6);
            last = tempString.IndexOf("|");
            str = tempString.Substring(0, last);
            int pos1y = Convert.ToInt32(str);

            //get Rovio 2 x coordinate

            first = str_ir2.IndexOf("x=");
            tempString = str_ir2.Substring(first + 2, 6);
            last = tempString.IndexOf("|");
            str = tempString.Substring(0, last);
            int pos2x = Convert.ToInt32(str);

            //get Rovio 2 y coordinate

            first = str_ir2.IndexOf("y=");
            tempString = str_ir2.Substring(first + 2, 6);
            last = tempString.IndexOf("|");
            str = tempString.Substring(0, last);
            int pos2y = Convert.ToInt32(str);

            
            MessageBox.Show("Rovio1 (" + Convert.ToString(pos1x) + "," + Convert.ToString(pos1y)+")"+"\n"+
                            "Rovio2 (" + Convert.ToString(pos2x) + "," + Convert.ToString(pos2y) + ")");
            
        }

                /////////
                /////////    
                /////////
        //////////////////////////////////
        //                              //
        //        Positioning           //
        //                              //  
        ////////////////////////////////// 
        public void positioning_AND_battery_Monitoring()
        {
            while (true)
            {
                string strRovio1 = rovio1.GetReport();
                string strRovio2 = rovio2.GetReport();


                //get the x and y coordinates of the two rovios

                //get Rovio 1 x coordinate

                int first = strRovio1.IndexOf("x=");
                string tempString = strRovio1.Substring(first + 2, 8);
                int last = tempString.IndexOf("|");
                string str1x;
                str1x = tempString.Substring(0, last);
                pos1x = Convert.ToInt32(str);

                //get Rovio 1 y coordinate

                first = strRovio1.IndexOf("y=");
                tempString = strRovio1.Substring(first + 2, 8);
                last = tempString.IndexOf("|");
                string str1y;
                str1y = tempString.Substring(0, last);
                pos1y = Convert.ToInt32(str);

                //get Rovio 2 x coordinate

                first = strRovio2.IndexOf("x=");
                tempString = strRovio2.Substring(first + 2, 8);
                last = tempString.IndexOf("|");
                string str2x;
                str2x = tempString.Substring(0, last);
                pos2x = Convert.ToInt32(str);

                //get Rovio 2 y coordinate

                first = strRovio2.IndexOf("y=");
                tempString = strRovio2.Substring(first + 2, 8);
                last = tempString.IndexOf("|");
                string str2y;
                str2y = tempString.Substring(0, last);
                pos2y = Convert.ToInt32(str);

                displayPosRovio1("(" + str1x + "," + str1y + ")");
                displayPosRovio2("(" + str2x + "," + str2y + ")");


                //1
                str_charg = strRovio2;
                str_charg1 = strRovio1;

                //2
                int firstCharacterBat = str_charg.IndexOf("battery");
                str_bat = str_charg.Substring(firstCharacterBat + 8, 3);
                battery = Convert.ToInt32(str_bat);

                firstCharacterBat1 = str_charg1.IndexOf("battery");
                str_bat1 = str_charg1.Substring(firstCharacterBat1 + 8, 3);
                battery1 = Convert.ToInt32(str_bat1);

                //3
                firstCharacterCharg = str_charg.IndexOf("charging");
                str_charg = str_charg.Substring(firstCharacterCharg + 9, 2);

                firstCharacterCharg1 = str_charg1.IndexOf("charging");
                str_charg1 = str_charg1.Substring(firstCharacterCharg1 + 9, 2);

                //4
                if (str_charg == "80")
                {
                    rollProgressBar2();
                }
                else
                {
                    displayBar2(battery);
                }

                if (str_charg1 == "80")
                {
                    rollProgressBar1();
                }
                else
                {
                    displayBar1(battery1);
                }


            }
        }
                


                    /////////
                    /////////    
                    /////////
        //////////////////////////////////
        //                              //
        //take images method for Rovio 1//
        //                              //  
        //////////////////////////////////  


        public void takeImages1()
        {

            while (true)
            {
                //positioning
                //positioning();
                //create stopwatch start in the beginning of the thread execution
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //string sourceURL = "http://192.168.2.15//Jpeg/CamImg.jpg";
                string sourceURL = rovio1URL + "//Jpeg/CamImg.jpg";
                //pictureBox1.Load(sourceURL);
                byte[] buffer = new byte[100000];
                int read, total = 0;
                // create HTTP request


                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
                // get response


                WebResponse resp = req.GetResponse();
                // get response stream

                Stream stream = resp.GetResponseStream();
                // read data from stream

                while ((read = stream.Read(buffer, total, 1000)) != 0)
                {
                    total += read;
                }
                // get bitmap

                Bitmap bmp = (Bitmap)Bitmap.FromStream(new MemoryStream(buffer, 0, total));

                Image<Bgr, Byte> frame = new Image<Bgr, Byte>((Bitmap)bmp);
                Image<Bgr, Byte> frameOutput = frame;
                Image<Gray, Byte> frameOutputGray = frame.Convert<Gray, Byte>();

                
                if (processingChoice == 1)
                {

                    frameOutputGray = EdgeDetection(frame);
                    imageBox1.Image = displayDelayInGray(frameOutputGray,sw);

                }
                else if (processingChoice == 2)
                {
                    frameOutput = PinkTracking(frame);
                    imageBox1.Image = displayDelayInColor(frameOutput, sw);
                }
                else if (processingChoice == 3)
                {
                    frameOutput = Segmenting(frame);
                    imageBox1.Image = displayDelayInColor(frameOutput, sw);
                }
                else if (processingChoice == 6 || processingChoice == 8 )  
                {
                    frameOutput = PinkBallCommanding1(frame);
                    imageBox1.Image = displayDelayInColor(frameOutput, sw);
                }
                else if (processingChoice == 5)
                {
                    //color range initialisation 
                    MCvScalar hsv_min = new MCvScalar(0, 0, 0);
                    MCvScalar hsv_max = new MCvScalar(0, 0, 0);

                    if (checkBox1.Checked)
                    {
                        //range for blue color 
                        hsv_min = new MCvScalar(110, 50, 110);
                        hsv_max = new MCvScalar(124, 180, 200);
                    }
                    else if (checkBox2.Checked)
                    {
                        //range for purple color 
                        hsv_min = new MCvScalar(125, 50, 110);
                        hsv_max = new MCvScalar(150, 180, 200);
                    }
                    else if (checkBox3.Checked)
                    {
                        //range for yellow color 
                        hsv_min = new MCvScalar(31, 50, 180);
                        hsv_max = new MCvScalar(40, 200, 200);
                    }
                    else if (checkBox4.Checked)
                    {
                        //range for ? color 
                        hsv_min = new MCvScalar(60, 50, 110);
                        hsv_max = new MCvScalar(70, 180, 200);
                    }
                    else
                    {
                        //black color
                        hsv_min = new MCvScalar(0, 0, 0);
                        hsv_max = new MCvScalar(0, 0, 0);
                    }

                    frameOutputGray = colorTracking(frame, hsv_min, hsv_max);
                    imageBox1.Image = displayDelayInGray(frameOutputGray, sw);
                }
                else  //Normal
                {
                    //display Image in normal mode
                    imageBox1.Image = displayDelayInColor(frameOutput, sw);
                }

                //end stopwatch -calculate duration of each frame display
                sw.Stop();
            }
        }



                    /////////
                    /////////    
                    /////////
        //////////////////////////////////
        //                              //
        //take images method for Rovio 2//
        //                              //  
        //////////////////////////////////     

        public void takeImages2()
        {
            
            while (true)
            {
                //create stopwatch start in the beginning of the thread execution
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //string sourceURL = "http://192.168.2.11//Jpeg/CamImg.jpg";
                string sourceURL = rovio2URL + "//Jpeg/CamImg.jpg";
                //pictureBox1.Load(sourceURL);
                byte[] buffer = new byte[100000];
                int read, total = 0;

                // create HTTP request
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);

                // get response
                WebResponse resp = req.GetResponse();

                // get response stream
                Stream stream = resp.GetResponseStream();

                // read data from stream
                while ((read = stream.Read(buffer, total, 1000)) != 0)
                {
                    total += read;
                }
                // get bitmap

                Bitmap bmp = (Bitmap)Bitmap.FromStream(new MemoryStream(buffer, 0, total));

                Image<Bgr, Byte> frame = new Image<Bgr, Byte>((Bitmap)bmp);
                Image<Bgr, Byte> frameOutput = frame;
                Image<Gray, Byte> frameOutputGray = frame.Convert<Gray, Byte>();

                if (processingChoice == 1)
                {
                    frameOutputGray = EdgeDetection(frame);
                    captureImageBox.Image = displayDelayInGray(frameOutputGray, sw); ;
                }
                else if (processingChoice == 2)
                {
                    frameOutput = PinkTracking(frame);
                    captureImageBox.Image = displayDelayInColor(frameOutput, sw);
                }
                else if (processingChoice == 3)
                {
                    frameOutput = Segmenting(frame);
                    captureImageBox.Image = displayDelayInColor(frameOutput, sw);
                }
                else if (processingChoice == 4)   //originally 4
                {
                    frameOutput = PinkBallCommanding2(frame);
                    captureImageBox.Image = displayDelayInColor(frameOutput, sw);
                }
                else if (processingChoice == 5)
                {
                    //color range initialisation 
                    MCvScalar hsv_min = new MCvScalar(0, 0, 0);
                    MCvScalar hsv_max = new MCvScalar(0, 0, 0);

                    if (checkBox1.Checked)
                    {
                        //range for blue color 
                        hsv_min = new MCvScalar(110, 50, 110);
                        hsv_max = new MCvScalar(124, 180, 200);
                    }
                    else if (checkBox2.Checked)
                    {
                        //range for purple color 
                        hsv_min = new MCvScalar(125, 50, 110);
                        hsv_max = new MCvScalar(150, 180, 200);
                    }
                    else if (checkBox3.Checked)
                    {
                        //range for yellow color 
                        hsv_min = new MCvScalar(31, 50, 180);
                        hsv_max = new MCvScalar(40, 200, 200);
                    }
                    else if (checkBox4.Checked)
                    {
                        //range for ? color 
                        hsv_min = new MCvScalar(60, 50, 110);
                        hsv_max = new MCvScalar(70, 180, 200);
                    }
                    else
                    {
                        //black color
                        hsv_min = new MCvScalar(0, 0, 0);
                        hsv_max = new MCvScalar(0, 0, 0);
                    }

                    frameOutputGray = colorTracking(frame, hsv_min, hsv_max);

                    captureImageBox.Image = displayDelayInGray(frameOutputGray, sw);
                }
                else  //Normal
                {
                    //display Image in normal mode
                    captureImageBox.Image = displayDelayInColor(frameOutput, sw);
                }

                //end stopwatch -calculate duration of each frame display
                sw.Stop();


            }


        }



        /* Go home Button and command  - Rovio 2   */
        private void button22_Click(object sender, EventArgs e)
        {
            rovio2.GoHome();
        }

        /*    form closing    */
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wanderThread2.IsAlive == true)
            {
                wanderThread2.Abort();
                
            }
            if (wanderThread1.IsAlive == true)
            {
                wanderThread2.Abort();

            }
            
            
            
            
        }



        ///////////////////////////////////////////////////////////////
        //                                                           //
        //          Image Processing Routines using OpenCV           //
        //                                                           //  
        ///////////////////////////////////////////////////////////////  

                    /*************************************/
                    /*          Edge Detection           */
                    /*************************************/


       public Image<Gray,Byte> EdgeDetection(Image<Bgr,Byte> image)
       {
           Image<Gray, Byte> grayFrame = image.Convert<Gray, Byte>();
           Image<Gray, Byte> smallGrayFrame = grayFrame.PyrDown();
           Image<Gray, Byte> smoothedGrayFrame = smallGrayFrame.PyrUp();
           Image<Gray, Byte> cannyFrame = smoothedGrayFrame.Canny(new Gray(100), new Gray(60));
           return cannyFrame;
       }


                        /*************************************/
                        /*         Tracking Pink Color       */
                        /*************************************/


       public Image<Bgr, Byte> PinkTracking(Image<Bgr, Byte> image)
       {
           
           

           MCvMoments moments = new MCvMoments();
               
           MCvScalar hsv_min=new MCvScalar(0,50,170);
           MCvScalar hsv_max=new MCvScalar(10,180,256);
           MCvScalar hsv_min2=new MCvScalar(170,50,170);
           MCvScalar hsv_max2=new MCvScalar(256,180,256);
           
          
           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           CvInvoke.cvOr(thresholded, thresholded2, thresholded,IntPtr.Zero);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvThreshold(thresholded, thresholded, 12, 256, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
           int iterations = 5;
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);

           CvInvoke.cvMoments(thresholded, ref moments, 1);

           double moment10 = CvInvoke.cvGetSpatialMoment(ref moments, 1, 0);
           double moment01 = CvInvoke.cvGetSpatialMoment(ref moments, 0, 1);
           double areaPink = CvInvoke.cvGetCentralMoment(ref moments, 0, 0);

           int posX = 0;
           int posY = 0;

           //exception handling
           try
           {
               posX = Convert.ToInt32(moment10 / areaPink);
               posY = Convert.ToInt32(moment01 / areaPink);
           }
           catch
           {
               //do nothing
           }
           Point center=new Point(posX,posY);
           
           MCvScalar colorCenter=new MCvScalar(204,102,255);
           MCvFont font = new MCvFont();
           MCvScalar colorFont=new MCvScalar(0,0,255);
           

           if (posX > 0 && posY > 0)
           {

               CvInvoke.cvCircle(image, center, 10, colorCenter, -1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
               CvInvoke.cvInitFont(ref font, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
               string textCenter = "(" + Convert.ToString(center.X) + "," + Convert.ToString(center.Y) + ")";
               CvInvoke.cvPutText(image, textCenter, center, ref font, colorFont);
           }

           return image;
       }


                        /*************************************/
                        /*          Segmenting Image         */
                        /*************************************/

       public Image<Bgr, Byte> Segmenting(Image<Bgr, Byte> image)
       {



           MCvMoments moments = new MCvMoments();

           MCvScalar hsv_min = new MCvScalar(0, 50, 170);
           MCvScalar hsv_max = new MCvScalar(10, 180, 256);
           MCvScalar hsv_min2 = new MCvScalar(170, 50, 170);
           MCvScalar hsv_max2 = new MCvScalar(256, 180, 256);


           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           CvInvoke.cvOr(thresholded, thresholded2, thresholded, IntPtr.Zero);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvThreshold(thresholded, thresholded, 12, 256, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
           int iterations = 5;
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           

           CvInvoke.cvMoments(thresholded, ref moments, 1);

           double moment10 = CvInvoke.cvGetSpatialMoment(ref moments, 1, 0);
           double moment01 = CvInvoke.cvGetSpatialMoment(ref moments, 0, 1);
           double areaPink = CvInvoke.cvGetCentralMoment(ref moments, 0, 0);

           int posX = 0;
           int posY = 0;

           //exception handling
           try
           {
               posX = Convert.ToInt32(moment10 / areaPink);
               posY = Convert.ToInt32(moment01 / areaPink);
           }
           catch
           {
               //do nothing
           }
           Point center = new Point(posX, posY);
           MCvScalar colorCenter = new MCvScalar(204, 102, 255);
           MCvFont font = new MCvFont();
           MCvScalar colorFont = new MCvScalar(0, 0, 255);


           if (posX > 0 && posY > 0)
           {

               CvInvoke.cvCircle(image, center, 10, colorCenter, -1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
               CvInvoke.cvInitFont(ref font, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
               string textCenter = "(" + Convert.ToString(center.X) + "," + Convert.ToString(center.Y) + ")";
               CvInvoke.cvPutText(image, textCenter, center, ref font, colorFont);
           }

           //1st segment
           MCvScalar colorRect = new MCvScalar(0, 255, 255);
           Point p1 = new Point(image.Width / 5, image.Height / 5);
           Point p2 = new Point(image.Width / 5, image.Height / 5);

           for (int i = 0; i <= 5; i++)
           {
               for (int j = 0; j <= 5; j++)
               {
                   //create points
                   p1 = new Point(i * image.Width / 5, j*image.Height / 5);
                   p2 = new Point((i + 1) * image.Width / 5, (j+1)*image.Height / 5);

                   //display points
                   MCvFont font1 = new MCvFont();
                   CvInvoke.cvInitFont(ref font1, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX,0.5f,0.5f, 0,1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
                   string textRectPointsP1 = "(" + Convert.ToString(p1.X) + "," + Convert.ToString(p1.Y) + ")";
                   string textRectPointsP2 = "(" + Convert.ToString(p2.X) + "," + Convert.ToString(p2.Y) + ")";
                   CvInvoke.cvPutText(image, textRectPointsP1, p1, ref font1, colorRect);
                   CvInvoke.cvPutText(image, textRectPointsP2, p2, ref font1, colorRect);

                   //display rectangles
                   CvInvoke.cvRectangle(image, p1, p2, colorRect, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
               }
           }
           return image;
       }

       /********************************************/
       /*          Pink Ball Commanding  Rovio 1   */
       /********************************************/


       public Image<Bgr, Byte> PinkBallCommanding1(Image<Bgr, Byte> image)
       {

           MCvMoments moments = new MCvMoments();

           MCvScalar hsv_min = new MCvScalar(0, 50, 170);
           MCvScalar hsv_max = new MCvScalar(10, 180, 256);
           MCvScalar hsv_min2 = new MCvScalar(170, 50, 170);
           MCvScalar hsv_max2 = new MCvScalar(256, 180, 256);


           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           CvInvoke.cvOr(thresholded, thresholded2, thresholded, IntPtr.Zero);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvThreshold(thresholded, thresholded, 12, 256, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
           int iterations = 5;
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);


           CvInvoke.cvMoments(thresholded, ref moments, 1);

           double moment10 = CvInvoke.cvGetSpatialMoment(ref moments, 1, 0);
           double moment01 = CvInvoke.cvGetSpatialMoment(ref moments, 0, 1);
           double areaPink = CvInvoke.cvGetCentralMoment(ref moments, 0, 0);

           int posX = 0;
           int posY = 0;

           //exception handling
           try
           {
               posX = Convert.ToInt32(moment10 / areaPink);
               posY = Convert.ToInt32(moment01 / areaPink);
           }
           catch
           {
               //do nothing
           }
           Point center = new Point(posX, posY);
           MCvScalar colorCenter = new MCvScalar(0, 0, 255);
           MCvFont font = new MCvFont();
           MCvScalar colorFont = new MCvScalar(255, 0, 0);


           if (posX > 0 && posY > 0)
           {
               // draw circle around the center of the detected pink blob
               CvInvoke.cvCircle(image, center, 10, colorCenter, -1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
               CvInvoke.cvInitFont(ref font, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
               string textCenter = "(" + Convert.ToString(center.X) + "," + Convert.ToString(center.Y) + ")";
               CvInvoke.cvPutText(image, textCenter, center, ref font, colorFont);
           }


           //Action message point and message color
           Point pointActionMessage = new Point(0, 30);
           MCvScalar colorMessage = new MCvScalar(0, 255, 255);
           string textStop = "Action:Stop";
           string textForwardRight = "Action:Forward Right";
           string textForwardLeft = "Action:Forward Left";
           string textForward = "Action:Forward";
           string textRotateLeft = "Action:Rotate Left";


           //searching and centralizing algorithm 6
           if (center.Y > 440)
           {
               if (processingChoice == 8)   //play with the ball
               {
                   //give permission to rovio 2
                   processingChoice = 4;
                   playFlag = 1;
               }
               //stop Rovio 2
               rovio1.ManualDrive(0, speed); //stop
               CvInvoke.cvPutText(image, textStop, pointActionMessage, ref font, colorMessage);

           }
           else if (center.Y > 96)
               if (center.X > 512)
               {
                   rovio1.ManualDrive(8, speed);  //forward right
                   CvInvoke.cvPutText(image, textForwardRight, pointActionMessage, ref font, colorMessage);
               }
               else if (center.X > 348)
               {
               
                   rovio1.ManualDrive(1, speed);  //forward

                   CvInvoke.cvPutText(image, textForward , pointActionMessage, ref font, colorMessage);
               }
               else if (center.X > 256)
               {
                   rovio1.ManualDrive(1, speed);  //forward

                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
               }
               else if (center.X > 128)
               {
                   rovio1.ManualDrive(1, speed);  //forward
                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
               }
               else
               {
                   rovio1.ManualDrive(7, speed);  //forward left
                   CvInvoke.cvPutText(image, textForwardLeft, pointActionMessage, ref font, colorMessage);
               }
           else
           {
               rovio1.ManualDrive(17, speed); //rotate left
               CvInvoke.cvPutText(image, textRotateLeft, pointActionMessage, ref font, colorMessage);
               rovio1.ManualDrive(0, speed); //stop
               CvInvoke.cvPutText(image, textStop, pointActionMessage, ref font, colorMessage);
           }



           return image;
       }

                            /********************************************/
                            /*          Pink Ball Commanding  Rovio 2   */
                            /********************************************/


       public Image<Bgr, Byte> PinkBallCommanding2(Image<Bgr, Byte> image)
       {

           MCvMoments moments = new MCvMoments();

           MCvScalar hsv_min = new MCvScalar(0, 50, 170);
           MCvScalar hsv_max = new MCvScalar(10, 180, 256);
           MCvScalar hsv_min2 = new MCvScalar(170, 50, 170);
           MCvScalar hsv_max2 = new MCvScalar(256, 180, 256);


           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           CvInvoke.cvOr(thresholded, thresholded2, thresholded, IntPtr.Zero);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvThreshold(thresholded, thresholded, 12, 256, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
           int iterations = 5;
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);


           CvInvoke.cvMoments(thresholded, ref moments, 1);

           double moment10 = CvInvoke.cvGetSpatialMoment(ref moments, 1, 0);
           double moment01 = CvInvoke.cvGetSpatialMoment(ref moments, 0, 1);
           double areaPink = CvInvoke.cvGetCentralMoment(ref moments, 0, 0);

           int posX = 0;
           int posY = 0;

           //exception handling
           try
           {
               posX = Convert.ToInt32(moment10 / areaPink);
               posY = Convert.ToInt32(moment01 / areaPink);
           }
           catch
           {
               //do nothing
           }
           Point center = new Point(posX, posY);
           MCvScalar colorCenter = new MCvScalar(0, 0, 255);
           MCvFont font = new MCvFont();
           MCvScalar colorFont = new MCvScalar(255, 0, 0);


           if (posX > 0 && posY > 0)
           {
               // draw circle around the center of the detected pink blob
               CvInvoke.cvCircle(image, center, 10, colorCenter, -1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
               CvInvoke.cvInitFont(ref font, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX_SMALL, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
               string textCenter = "(" + Convert.ToString(center.X) + "," + Convert.ToString(center.Y) + ")";
               CvInvoke.cvPutText(image, textCenter, center, ref font, colorFont);
           }
           

           //Action message point and message color
           Point pointActionMessage=new Point(0,30);
           MCvScalar colorMessage = new MCvScalar(0, 255, 255);
           string textStop = "Action:Stop";
           string textForwardRight = "Action:Forward Right";
           string textForwardLeft = "Action:Forward Left";
           string textForward = "Action:Forward";
           string textRotateLeft = "Action:Rotate Left";

           //searching and centralizing algorithm 6
           if (center.Y > 440)
           {
               //stop Rovio 2
               rovio2.ManualDrive(0, speed); //stop
               CvInvoke.cvPutText(image, textStop,pointActionMessage, ref font, colorMessage);
               if (playFlag == 1)
               {
                   processingChoice = 8;
               }
               
           }
           else if (center.Y > 96)
               if (center.X > 512)
               {
                   rovio2.ManualDrive(8, speed);  //forward right
                   CvInvoke.cvPutText(image, textForwardRight, pointActionMessage, ref font, colorMessage);
               }
               else if (center.X > 348)
               {
                   rovio2.ManualDrive(1, speed);  //forward

                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
               }
               else if (center.X > 256)
               {
                   rovio2.ManualDrive(1, speed);  //forward
 
                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
               }
               else if (center.X > 128)
               {

                   rovio2.ManualDrive(1, speed);  //forward

                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
               }
               else
               {
                   rovio2.ManualDrive(7, speed); //forward left

                   CvInvoke.cvPutText(image, textForwardLeft, pointActionMessage, ref font, colorMessage);
               }
           else
           {
               rovio2.ManualDrive(17, speed); //rotate left
               CvInvoke.cvPutText(image, textRotateLeft, pointActionMessage, ref font, colorMessage);
               rovio2.ManualDrive(0, speed); //stop
               CvInvoke.cvPutText(image, textStop, pointActionMessage, ref font, colorMessage);
           }



           return image;
       }

                            /*************************************/
                            /*          Color Tracking           */
                            /*************************************/


       public Image<Gray, Byte> colorTracking(Image<Bgr, Byte> image,MCvScalar hsv_min,MCvScalar hsv_max)
       {
           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);

           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvSmooth(thresholded, thresholded, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_BLUR, 9, 9, 0, 0);
           CvInvoke.cvThreshold(thresholded, thresholded, 12, 256, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
           int iterations = 5;
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvDilate(thresholded, thresholded, IntPtr.Zero, iterations);
           CvInvoke.cvErode(thresholded, thresholded, IntPtr.Zero, iterations);
           return thresholded;
       }




       private void button23_Click(object sender, EventArgs e)  //edge detection button
       {
           if (processingChoice == 1)
           {
               processingChoice = 0;
           }
           else
           {
               processingChoice = 1;
           }
       }

       private void button24_Click(object sender, EventArgs e)   //color detection
       {
           if (processingChoice == 2)
           {
               processingChoice = 0;
           }
           else
           {
               processingChoice = 2;
           }
       }

       private void button25_Click(object sender, EventArgs e)   //Normal Jpeg Display button
       {
           
               processingChoice = 0;
           
       }

       private void button16_Click_1(object sender, EventArgs e)
       {
           if (processingChoice == 3)
           {
               processingChoice = 0;
           }
           else
           {
               processingChoice = 3;
           }
       }

       private void button18_Click(object sender, EventArgs e)  //rovio 2 find the ball/commanding
       {
           if (processingChoice == 4)
           {
               processingChoice = 0;
           }
           else
           {
               processingChoice = 4;
           }


       }
       private void button41_Click(object sender, EventArgs e)
       {
           if (processingChoice == 6)
           {
               processingChoice = 0;
           }
           else
           {
               processingChoice = 6;
           }

       }

       private void button17_Click(object sender, EventArgs e)
       {
           if (processingChoice == 5)
           {
               processingChoice = 0;
           }
           else
           {
               processingChoice = 5;
           }
       }

       
       


        //pink ball commanding Rovio 1 & 2

       private void button43_Click(object sender, EventArgs e)
       {
           if (processingChoice == 8)
           {
               processingChoice = 0;
           }
           else
           {

               processingChoice = 8;
           }
       }

        //circle detection Rovio 1

       private void button44_Click(object sender, EventArgs e)
       {
           if (processingChoice == 7)
           {
               processingChoice = 0;
           }
           else
           {

               processingChoice = 7;
           }
       }


        /********************************      
         *                              *
         *     Method to diplay         *
         *     Frame Delay in           *
         *     Gray Images              *
         *                              *
         ********************************/

       public Image<Gray, Byte> displayDelayInGray(Image<Gray, Byte> grayImage,Stopwatch sw1)
       {
           //Frame delay display
           Point pointFrameDelayText = new Point(350, 20);
           MCvFont fontFrameDelayText = new MCvFont();
           CvInvoke.cvInitFont(ref fontFrameDelayText, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
           MCvScalar colorFrameDelayText = new MCvScalar(255, 255, 255);
           string strFrameDelay = "Frame Delay:" + sw1.Elapsed.Milliseconds + "ms";
           CvInvoke.cvPutText(grayImage, strFrameDelay, pointFrameDelayText, ref fontFrameDelayText, colorFrameDelayText);

           return grayImage;
       }


       /********************************      
        *                              *
        *     Method to diplay         *
        *     Frame Delay in           *
        *     Gray Images              *
        *                              *
        ********************************/


       public Image<Bgr, Byte> displayDelayInColor(Image<Bgr, Byte> colorImage, Stopwatch sw1)
       {
           //Frame delay display
           Point pointFrameDelayText = new Point(350, 20);
           MCvFont fontFrameDelayText = new MCvFont();
           CvInvoke.cvInitFont(ref fontFrameDelayText, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
           MCvScalar colorFrameDelayText = new MCvScalar(0, 0, 255);
           string strFrameDelay = "Frame Delay:" + sw1.Elapsed.Milliseconds + "ms";
           CvInvoke.cvPutText(colorImage, strFrameDelay, pointFrameDelayText, ref fontFrameDelayText, colorFrameDelayText);

           return colorImage;
       }

       private void button46_Click(object sender, EventArgs e) //submit rovio 1
       {

           if (checkBox5.Checked)  //rovio 1
           {
               rovio1URL = "http://192.168.2.11";
           }
           else if(checkBox6.Checked)   //rovio 2
           {
               rovio1URL = "http://192.168.2.12";
           }
           else if(checkBox7.Checked)   //rovio 3
           {
               rovio1URL = "http://192.168.2.14";
           }
           else if(checkBox8.Checked)   //rovio 4
           {
               rovio1URL = "http://192.168.2.15";
           }
           else if (checkBox9.Checked)  //rovio 5
           {
               rovio1URL = "http://192.168.2.16";
           }
           /*   Create rovio object for rovio 1  AGAIN    */
           rovio1 = new RovioLib.RovioController("username", "password", rovio1URL);

           
       }

       private void button47_Click(object sender, EventArgs e) //submit rovio 2
       {
           if (checkBox5.Checked)  //rovio 1
           {
               rovio2URL = "http://192.168.2.11";
           }
           else if (checkBox6.Checked)   //rovio 2
           {
               rovio2URL = "http://192.168.2.12";
           }
           else if (checkBox7.Checked)   //rovio 3
           {
               rovio2URL = "http://192.168.2.14";
           }
           else if (checkBox8.Checked)   //rovio 4
           {
               rovio2URL = "http://192.168.2.15";
           }
           else if (checkBox9.Checked)  //rovio 5
           {
               rovio2URL = "http://192.168.2.16";
           }

           /*   Create rovio object for rovio 2  AGAIN   */
           rovio2 = new RovioLib.RovioController("username", "password", rovio2URL);
       

       }


        //display Rovios Coordinates from another thread - UI delegated calling

       public void displayPosRovio1(string someParam)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod theDelegateMethod = new MyDelegateMethod(this.displayPosRovio1);
               this.Invoke(theDelegateMethod, new object[] { someParam });
           }
           else
           {
               this.label8.Text = someParam;
           }
       }

       public void displayPosRovio2(string someParam)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod theDelegateMethod1 = new MyDelegateMethod(this.displayPosRovio2);
               this.Invoke(theDelegateMethod1, new object[] { someParam });
           }
           else
           {
               this.label9.Text = someParam;
           }
       }

       //bars for battery monitoring
       //roll progress bar 2

       public void rollProgressBar2()
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod2 theDelegateMethod2 = new MyDelegateMethod2(this.rollProgressBar2);
               this.Invoke(theDelegateMethod2, new object[] {  });
           }
           else
           {

           if (this.progressBar2.Value < 100)
               this.progressBar2.Value += 10;
           else
               this.progressBar2.Value = 0;
       
           }
       }
       //roll progress bar 1
       public void rollProgressBar1()
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod1 theDelegateMethod3 = new MyDelegateMethod1(this.rollProgressBar1);
               this.Invoke(theDelegateMethod3, new object[] { });
           }
           else
           {

               if (this.progressBar1.Value < 100)
                   this.progressBar1.Value += 10;
               else
                   this.progressBar1.Value = 0;

           }
       }

        //display battery for Rovio 1


       public void displayBar1(int battery)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod3 theDelegateMethod4 = new MyDelegateMethod3(this.displayBar1);
               this.Invoke(theDelegateMethod4, new object[] { battery });
           }
           else
           {
               if (battery >= 125)
                   this.progressBar1.Value = 100;
               else if (battery >= 123)
                   this.progressBar1.Value = 90;
               else if (battery >= 120)
                   this.progressBar1.Value = 80;
               else if (battery >= 117)
                   this.progressBar1.Value = 70;
               else if (battery >= 114)
                   this.progressBar1.Value = 60;
               else if (battery >= 111)
                   this.progressBar1.Value = 50;
               else if (battery >= 109)
                   this.progressBar1.Value = 40;
               else if (battery >= 107)
                   this.progressBar1.Value = 30;
               else
                   this.rovio1.GoHome();
           }
       }

       public void displayBar2(int battery2)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod4 theDelegateMethod5 = new MyDelegateMethod4(this.displayBar2);
               this.Invoke(theDelegateMethod5, new object[] { battery2 });
           }
           else
           {
               if (battery2 >= 125)
                   this.progressBar2.Value = 100;
               else if (battery2 >= 123)
                   this.progressBar2.Value = 90;
               else if (battery2 >= 120)
                   this.progressBar2.Value = 80;
               else if (battery2 >= 117)
                   this.progressBar2.Value = 70;
               else if (battery2 >= 114)
                   this.progressBar2.Value = 60;
               else if (battery2 >= 111)
                   this.progressBar2.Value = 50;
               else if (battery2 >= 109)
                   this.progressBar2.Value = 40;
               else if (battery2 >= 107)
                   this.progressBar2.Value = 30;
               else
                   this.rovio1.GoHome();
           }
       }

       
           
      

       
    }
}


