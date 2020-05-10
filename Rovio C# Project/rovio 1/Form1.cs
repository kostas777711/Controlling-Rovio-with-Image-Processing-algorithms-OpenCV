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
using RovioLib;
 


namespace rovio_1
{

    public partial class Form1 : Form
    {

                    /********************************/
                    /*        Public Variables      */
                    /********************************/


        /***********************************************************************\
        *                                                                       *
        *    Image processing Choices                                           *
        *                                                                       * 
        *       0- Default/Normal                                               *
        *       1- Edge Detection                                               *
        *       2- Color Detection                                              *
        *       3- Segmenting                                                   *
        *       4- Pink ball commanding (Rovio 2)                               *
        *       5- Color Tracking                                               *   
        *       6- Pink ball commanding (Rovio 1)                               *
        *       7- Circle Detection (Rovio 1)                                   *
        *       8- PLAY WITH THE BALL                                           *
        *       9- Yellow Ball commanding (Rovio 1)                             *
        *      10- Yellow Ball commanding (Rovio 2)                             *
        *      11- Mission 1  (pink-> ball->yellow->pink->) for Rovio 1         *
        *      12- New Ball commanding (Rovio 1) 
        * 
        \************************************************************************/

        int processingChoice = 0;

        //Rovio 1 URL
        static string rovio1URL = "http://192.168.2.11";
        //Rovio 2 URL
        static string rovio2URL = "http://192.168.2.12";
        //Rovio 3 URL
        static string rovio3URL = "http://192.168.2.14";
        //Rovio 4 URL
        static string rovio4URL = "http://192.168.2.15";
        //Rovio 5 URL
        static string rovio5URL = "http://192.168.2.16";

        

        //UI, declare a delegate:  "display delegates"
        public delegate void MyDelegateMethod(string coordinates);
        public delegate void MyDelegateMethod1();
        public delegate void MyDelegateMethod2();
        public delegate void MyDelegateMethod3(int battery);
        public delegate void MyDelegateMethod4(int battery2);
        public delegate void MyDelegateMethod5(decimal distance);
        public delegate void MyDelegateMethod6();
        public delegate void MyDelegateMethod7(string circlePosition);
        public delegate void MyDelegateMethod8(string color);

        //for circle detection public variables and flags
        Hsv hsv = new Hsv();
        Point centre = new Point();
        bool button20Clicked = false;
        bool button45Clicked = false;

        //set HSV trackbars from automatic color range adjustment
        int hue_min = 0;
        int sat_min = 0;
        int val_min = 0;
        int hue_max = 255;
        int sat_max = 255;
        int val_max = 255;


        //parameters for circle detection
        //number of circles
        int N = 1;
        int thresholdCircle=138;
        int param1 = 11, param2 = 11, param3 = 0, param4 = 0;
        int threshold1 = 1, threshold2 = 3, apertureSize = 5;
        //hough parameters
        int accResolution=2,minDist=100,cannyThreshold=5,accThreshold=50,minRadius=0,maxRadius=100;


        //edge detection parameters
        int edparam1 = 70, edparam2 = 60;

        //segmentation parameters
        int segments = 5;

        
        

        //ir wander 1 thread start/pause
        int lockWander1=0;

        //ir wander 2 thread start/pause
        int lockWander2 = 0;

        //ir wander 1&2 thread start/pause
        int lockWander = 0;

        //enable flab tab2 
        public bool enableTab2 = false; 
        

        /*   Create rovio object for rovio 1 for the first time     */
        RovioController rovio1 = new RovioController("username", "password", rovio1URL);
        /*   Create rovio object for rovio 2  for the first time        */
        RovioController rovio2 = new RovioController("username", "password", rovio2URL);
        /*   Create rovio object for rovio 3  for the first time        */
        RovioController rovio3 = new RovioController("username", "password", rovio3URL);
        /*   Create rovio object for rovio 4  for the first time        */
        RovioController rovio4 = new RovioController("username", "password", rovio4URL);
        /*   Create rovio object for rovio 5  for the first time        */
        RovioController rovio5 = new RovioController("username", "password", rovio5URL);
       
        //mission 1 flag
        int mission1Flag = 0;

        //go home flag
        int goHomeFlag = 0;

        //play Flag
        int playFlag = 0;

        /*   Initialisation of the Wander Thread        */
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
        string  str_bat, str_charg, str_bat1, str_charg1;


        /*   Initialisation of rovio's speed   (1-fastest,10-slowest)    */
        public static int speed = 1;

        
        /*     Positioning Coordinates   */
        public static int pos1x, pos1y, pos2x, pos2y;
        public static decimal pos1theta, pos2theta;

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
            Thread takeImagesThread2 = new Thread(new ThreadStart(takeImages2));
            takeImagesThread2.IsBackground = true;    //for the thread to close with the application
            takeImagesThread2.Start();
            //takeImagesThread.Priority = ThreadPriority.Highest;

            
            /*   Positioning Thread    */
            Thread positioning_and_collisionAvoidanceThread = new Thread(new ThreadStart(positioning_and_collisionAvoidance));
            positioning_and_collisionAvoidanceThread.IsBackground = true;
            positioning_and_collisionAvoidanceThread.Start();
            //positioningThread.Priority = ThreadPriority.Lowest;

            /*   Battery Monitoring Thread    */
            Thread batteryMonitoringThread = new Thread(new ThreadStart(batteryMonitoring));
            batteryMonitoringThread.IsBackground = true;
            batteryMonitoringThread.Start();
            
            

            
        }

        /*   Button for Displaying the main getReport  Rovio 1     */
        private void button40_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show(rovio1.GetReport());
        }

        /*   Button for Displaying the main getReport  Rovio 2     */
        private void button1_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show(rovio2.GetReport());
        }

        /*   selecting speed of Rovio       */
        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            trackBar10.SetRange(1, 10);
            speed = trackBar10.Value;
        }


        //edge detection first parameter
        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            trackBar1.SetRange(0, 500);
            label17.Text = "Thresh = " + Convert.ToInt32(trackBar1.Value);
            edparam1 = trackBar1.Value ;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            trackBar2.SetRange(0, 500);
            label18.Text = "ThreshLinking = " + Convert.ToInt32(trackBar2.Value);
            edparam2 = trackBar2.Value;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            trackBar3.SetRange(2, 10);
            label20.Text = "Segments = " + Convert.ToInt32(trackBar3.Value * trackBar3.Value);
            segments = trackBar3.Value;
        }


        //min hsv color
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            trackBar4.SetRange(0, 256);
            label23.Text = "H_MIN = " + Convert.ToInt32(trackBar4.Value);
            hue_min = trackBar4.Value;
        }
        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            trackBar5.SetRange(0, 256);
            label25.Text = "S_MIN = " + Convert.ToInt32(trackBar5.Value);
            sat_min = trackBar5.Value;
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            trackBar6.SetRange(0, 256);
            label27.Text = "V_MIN = " + Convert.ToInt32(trackBar6.Value);
            val_min = trackBar6.Value;
        }

        //max hsv color
        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            trackBar9.SetRange(0, 256);
            label24.Text = "H_MAX = " + Convert.ToInt32(trackBar9.Value);
            hue_max = trackBar9.Value;
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            trackBar8.SetRange(0, 256);
            label26.Text = "S_MAX = " + Convert.ToInt32(trackBar8.Value);
            sat_max = trackBar8.Value;
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            trackBar7.SetRange(0, 256);
            label28.Text = "V_MAX = " + Convert.ToInt32(trackBar7.Value);
            val_max = trackBar7.Value;
        }


        //no of circles detected
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            N = (int)numericUpDown1.Value;
        }

        //threshold in edge detection
        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            trackBar11.SetRange(0, 256);
            thresholdCircle = trackBar11.Value;
            label11.Text = "Threshold = " + Convert.ToInt32(trackBar11.Value);
        }

        //smothing gaussian parameters

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)  //paramet 1
        {
            if(numericUpDown2.Value%2==1)
                param1 = (int)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)   //paramet 2
        {
            if (numericUpDown3.Value % 2 == 1)
                param2 = (int)numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)   //paramet 3
        {
            param3 = (int)numericUpDown4.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e) //paramet 2
        {
            param4 = (int)numericUpDown5.Value;
        }

        //canny in circles detection
        private void numericUpDown8_ValueChanged(object sender, EventArgs e)  //threshold 1
        {
            threshold1 = (int)numericUpDown8.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)   //threshold 2
        {
            threshold2 = (int)numericUpDown7.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)   //aperture size
        {
            if (numericUpDown6.Value % 2 == 1)
                apertureSize = (int)numericUpDown6.Value;
        }


        //hough parameters
        private void numericUpDown9_ValueChanged(object sender, EventArgs e)  //acc resolution
        {
            accResolution = (int)numericUpDown9.Value;
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)  //min distance
        {
            minDist =(int)numericUpDown10.Value;
        }

        private void label42_Click(object sender, EventArgs e)  //canny threshold
        {
            cannyThreshold = (int)numericUpDown11.Value;
        }

        private void label43_Click(object sender, EventArgs e)  //accum threshold
        {
            accThreshold = (int)numericUpDown12.Value;
        }

        private void label44_Click(object sender, EventArgs e)   //min radius
        {
            minRadius = (int)numericUpDown13.Value;
        }

        private void label45_Click(object sender, EventArgs e)   //max radius
        {
            maxRadius = (int)numericUpDown14.Value;
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

            if (wanderFlag1 == false && lockWander1==0)
            {
                wanderFlag1 = true;
                //change image to pause
                button42.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector
                rovio1.ActivateIRDetector();
                wanderThread1.Start();
                lockWander1 = 1;
                
            }
            else if (wanderFlag1 == false && lockWander1 == 1 )
            {
                wanderFlag1 = true;
                //change image to pause
                button42.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector
                //rovio1.ActivateIRDetector();
                wanderThread1.Resume();
                lockWander1 = 1;
            }
            else if (wanderFlag1 == true)
            {
                wanderFlag1 = false;
                button42.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\play.jpg");
                //rovio1.DeactivateIRDetector();
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

            if (wanderFlag2 == false && lockWander2==0)
            {
                wanderFlag2 = true;
                //change image to pause
                button19.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector
                rovio2.ActivateIRDetector();
                wanderThread2.Start();
                lockWander2 = 1;
            }
            else if (wanderFlag2 == false && lockWander2 == 1)
            {
                wanderFlag2 = true;
                //change image to pause
                button19.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\pause.jpg");
                //activate IR detector

                wanderThread2.Resume();
                lockWander2 = 1;
            }
            else if (wanderFlag2 == true)
            {
                wanderFlag2 = false;
                button19.Image = Image.FromFile(@"C:\Documents and Settings\kat091\Desktop\Rovio C# Project\Rovio C# Project\rovio 1\images\play.jpg");
                //rovio2.DeactivateIRDetector();
                wanderThread2.Suspend();
            }

            
        }




        //wander method for rovio 1 for thread
        public static void wander1()
        {
            RovioController rovio1 = new RovioController("username", "password", rovio1URL);

            while (true)
            {
                str_ir1 = rovio1.GetReport();
                firstCharacter1 = str_ir1.IndexOf("flags");
                str_ir1 = str_ir1.Substring(firstCharacter1 + 9, 1);

                if (str_ir1 != "5")
                {
                    rovio1.ManualDrive(17, speed);  //rotate left 
                    rovio1.ManualDrive(0, speed); //stop

                }
                

                
            }

        }


        //wander method for rovio 2 for thread
        public static void wander2()
        {
            RovioController rovio2 = new RovioController("username", "password", rovio2URL);

            while (true)
            {
                str_ir2 = rovio2.GetReport();
                firstCharacter2 = str_ir2.IndexOf("flags");
                str_ir2 = str_ir2.Substring(firstCharacter2 + 9, 1);

                if (str_ir2 != "5")
                {
                    rovio2.ManualDrive(17, speed);  //rotate left 
                    rovio2.ManualDrive(0, speed); //stop

                }
                
            }

        }


        //wander method for rovio 1
        public static void wanderMethod1()
        {
            RovioController rovio1 = new RovioController("username", "password", rovio1URL);

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
            RovioController rovio2 = new RovioController("username", "password", rovio2URL);
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



        


       

                     /////////
                    /////////    
                     /////////
        //////////////////////////////////
        //                              //
        //        Positioning           //
        //            AND               //
        //       collisionAvoidance     //
        //                              //
        ////////////////////////////////// 
        public void positioning_and_collisionAvoidance()
        {
            decimal previousDistance=0;

            while (true)
            {
                //create the two rovio objects -- IMPORTANCE OF CREATING THE NEW OBJECTS IN TEH NEW THREAD
                RovioController rovio1 = new RovioController("username", "password", rovio1URL);
                RovioController rovio2 = new RovioController("username", "password", rovio2URL);
                //rovio 1 get report
                string strRovio1 = rovio1.GetReport();
                //rovio 2 get report
                string strRovio2 = rovio2.GetReport();

                string str1x = "0";
                string str1y = "0";
                string str1theta = "0";
                string str2x = "0";
                string str2y = "0";
                string str2theta = "0";
                int last = 0;

                //averaging the positions- initialisation
                int avepos1x = 0;
                int avepos1y = 0;
                decimal avepos1theta = 0;
                int avepos2x = 0;
                int avepos2y = 0;
                decimal avepos2theta = 0;

                //averaging the positions- initialisation
                int sumpos1x = 0;
                int sumpos1y = 0;
                decimal sumpos1theta = 0;
                int sumpos2x = 0;
                int sumpos2y = 0;
                decimal sumpos2theta = 0;

                
                //number of samples taken
                int N=10;

                for(int i=1;i<=N;i++)
                {
                    
                    //rovio 1 get report
                    strRovio1 = rovio1.GetReport();
                    //rovio 2 get report
                    strRovio2 = rovio2.GetReport();

                    //get the x and y coordinates of the two rovios

                    //get Rovio 1 x coordinate

                    int first = strRovio1.IndexOf("x=");
                    string tempString = strRovio1.Substring(first + 2, 8);
                    last = tempString.IndexOf("|");
                    str1x = tempString.Substring(0, last);
                    pos1x = Convert.ToInt32(str1x);
                    sumpos1x += pos1x;

                    //get Rovio 1 y coordinate

                    first = strRovio1.IndexOf("y=");
                    tempString = strRovio1.Substring(first + 2, 8);
                    last = tempString.IndexOf("|");
                    str1y = tempString.Substring(0, last);
                    pos1y = Convert.ToInt32(str1y);
                    sumpos1y += pos1y;

                    //get Rovio 1 theta coordinate
                    first = strRovio1.IndexOf("theta=");
                    tempString = strRovio1.Substring(first + 6, 8);
                    last = tempString.IndexOf("|");
                    str1theta = tempString.Substring(0, last);
                    pos1theta = Convert.ToDecimal(str1theta);
                    sumpos1theta += pos1theta;

                    //get Rovio 2 x coordinate
                    first = strRovio2.IndexOf("x=");
                    tempString = strRovio2.Substring(first + 2, 8);
                    last = tempString.IndexOf("|");
                    str2x = tempString.Substring(0, last);
                    pos2x = Convert.ToInt32(str2x);
                    sumpos2x += pos2x;

                    //get Rovio 2 y coordinate

                    first = strRovio2.IndexOf("y=");
                    tempString = strRovio2.Substring(first + 2, 8);
                    last = tempString.IndexOf("|");
                    str2y = tempString.Substring(0, last);
                    pos2y = Convert.ToInt32(str2y);
                    sumpos2y += pos2y;


                    //get Rovio 2 theta coordinate
                    first = strRovio2.IndexOf("theta=");
                    tempString = strRovio2.Substring(first + 6, 8);
                    last = tempString.IndexOf("|");
                    str2theta = tempString.Substring(0, last);
                    pos2theta = Convert.ToDecimal(str2theta);
                    sumpos2theta += pos2theta;

                    
                }
                
                //calculating teh average values
                avepos1x = sumpos1x / N;
                avepos1y = sumpos1y / N;
                avepos1theta = sumpos1theta / N;

                avepos2x = sumpos2x / N;
                avepos2y = sumpos2y / N;
                avepos2theta = (decimal)sumpos2theta / N;


                displayPosRovio1("(" + Convert.ToString(avepos1x) + "," + Convert.ToString(avepos1y) + ") " + "theta :" + Convert.ToString(TruncateFunction(pos1theta,2)));
                displayPosRovio2("(" + Convert.ToString(avepos2x) + "," + Convert.ToString(avepos2y) + ") " + "theta :" + Convert.ToString(TruncateFunction(pos2theta, 2)));

                int xd = avepos2x - avepos1x;
                int yd = avepos2y - avepos1y;

                decimal distance = (decimal) Math.Sqrt(xd * xd + yd * yd);

                //display distance
                displayDistance(TruncateFunction(distance,2));

                //if (distance < 1500)
                //{
                //    if (distance < previousDistance)
                //    {
                //        rovio1.ManualDrive(3, speed);
                //        rovio2.ManualDrive(3, speed);
                //    }
                //    else
                //    {
                //        rovio2.ManualDrive(4, speed);
                //        rovio1.ManualDrive(4, speed);
                //    }
                //}
                previousDistance = distance;

            }
        }



        //battery monitoring method
        public void batteryMonitoring()
        {
            while (true)
            {

                //create the two rovio objects -- IMPORTANCE OF CREATING THE NEW OBJECTS IN TEH NEW THREAD
                RovioController rovio1 = new RovioController("username", "password", rovio1URL);
                RovioController rovio2 = new RovioController("username", "password", rovio2URL);
                //rovio 1 get report
                string strRovio1 = rovio1.GetReport();
                //rovio 2 get report
                string strRovio2 = rovio2.GetReport();


                int last;

                //battery monitoring
                //1
                str_charg = strRovio2;
                str_charg1 = strRovio1;

                //2
                int firstCharacterBat = str_charg.IndexOf("battery");
                str_bat = str_charg.Substring(firstCharacterBat + 8, 6);
                last = str_bat.IndexOf("|");
                battery = Convert.ToInt32(str_bat.Substring(0, last));

                firstCharacterBat1 = str_charg1.IndexOf("battery");
                str_bat1 = str_charg1.Substring(firstCharacterBat1 + 8, 6);
                last = str_bat1.IndexOf("|");
                battery1 = Convert.ToInt32(str_bat1.Substring(0, last));

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
                
                //create stopwatch start in the beginning of the thread execution
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //url string command initialisation
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
                    imageBox1.Image = displayExecutionTimeInGray(frameOutputGray,sw);

                }
                else if (processingChoice == 2)
                {
                    frameOutput = PinkTracking(frame);
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else if (processingChoice == 3)
                {
                    frameOutput = Segmenting(frame);
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else if (processingChoice == 6 || processingChoice == 8 || processingChoice==11)  
                {
                    frameOutput = PinkBallCommanding1(frame);
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
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
                        hsv_min = new MCvScalar(30, 101, 102);
                        hsv_max = new MCvScalar(53, 174, 239);
                    }
                    else if (checkBox4.Checked)
                    {
                        //range for ? color 
                        hsv_min = new MCvScalar(hue_min, sat_min, val_min);
                        hsv_max = new MCvScalar(hue_max, sat_max, val_max);
                    }
                    else
                    {
                        //black color
                        hsv_min = new MCvScalar(0, 0, 0);
                        hsv_max = new MCvScalar(0, 0, 0);
                    }

                    frameOutputGray = colorTracking(frame, hsv_min, hsv_max);
                    imageBox1.Image = displayExecutionTimeInGray(frameOutputGray, sw);
                }
                else if (processingChoice == 9)
                {
                    frameOutput = YellowBallCommanding1(frame);
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else if (processingChoice == 7)
                {
                    frameOutput = CircleAndColorDetection(frame);
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else if (processingChoice == 12)
                {
                    frameOutput = NewBallCommanding1(frame);
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else  //Normal
                {
                    //display Image in normal mode
                    imageBox1.Image = displayExecutionTimeInColor(frameOutput, sw);
                    
                }
                //standard for tab 2
                if (enableTab2 == true)
                {
                    imageBox2.Image = displayExecutionTimeInColor(frame, sw);
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

                //url string command initialisation
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
                    captureImageBox.Image = displayExecutionTimeInGray(frameOutputGray, sw); ;
                }
                else if (processingChoice == 2)
                {
                    frameOutput = PinkTracking(frame);
                    captureImageBox.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else if (processingChoice == 3)
                {
                    frameOutput = Segmenting(frame);
                    captureImageBox.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else if (processingChoice == 4)   //originally 4
                {
                    frameOutput = PinkBallCommanding2(frame);
                    captureImageBox.Image = displayExecutionTimeInColor(frameOutput, sw);
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
                        hsv_min = new MCvScalar(30, 101, 102);
                        hsv_max = new MCvScalar(53, 174, 239);
                    }
                    else if (checkBox4.Checked)
                    {
                        //range for ? color 
                        hsv_min = new MCvScalar(hue_min, sat_min, val_min);
                        hsv_max = new MCvScalar(hue_max, sat_max, val_max);
                    }
                    else
                    {
                        //black color
                        hsv_min = new MCvScalar(0, 0, 0);
                        hsv_max = new MCvScalar(0, 0, 0);
                    }

                    frameOutputGray = colorTracking(frame, hsv_min, hsv_max);

                    captureImageBox.Image = displayExecutionTimeInGray(frameOutputGray, sw);
                }
                else if (processingChoice == 10)
                {
                    frameOutput = YellowBallCommanding2(frame);
                    captureImageBox.Image = displayExecutionTimeInColor(frameOutput, sw);
                }
                else  //Normal
                {
                    //display Image in normal mode
                    captureImageBox.Image = displayExecutionTimeInColor(frameOutput, sw);
                    
                }

                //tab 2 display
                if (enableTab2 == true)
                {
                    imageBox3.Image = displayExecutionTimeInColor(frame, sw);
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
        //take images method for Rovio 3//
        //                              //  
        //////////////////////////////////  


        public void takeImages3()
        {

            while (true)
            {

                //create stopwatch start in the beginning of the thread execution
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //url string command initialisation
                string sourceURL = rovio3URL + "//Jpeg/CamImg.jpg";
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


                
                 //display Image in normal mode
                 
                imageBox4.Image = displayExecutionTimeInColor(frameOutput, sw);
                

                //end stopwatch -calculate duration of each frame display
                sw.Stop();
            }
        }


        /////////
        /////////    
        /////////
        //////////////////////////////////
        //                              //
        //take images method for Rovio 4//
        //                              //  
        //////////////////////////////////  


        public void takeImages4()
        {

            while (true)
            {

                //create stopwatch start in the beginning of the thread execution
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //url string command initialisation
                string sourceURL = rovio4URL + "//Jpeg/CamImg.jpg";
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



                //display Image in normal mode

                imageBox5.Image = displayExecutionTimeInColor(frameOutput, sw);


                //end stopwatch -calculate duration of each frame display
                sw.Stop();
            }
        }


        /////////
        /////////    
        /////////
        //////////////////////////////////
        //                              //
        //take images method for Rovio 5//
        //                              //  
        //////////////////////////////////  


        public void takeImages5()
        {

            while (true)
            {

                //create stopwatch start in the beginning of the thread execution
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //url string command initialisation
                string sourceURL = rovio5URL + "//Jpeg/CamImg.jpg";
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



                //display Image in normal mode

                imageBox6.Image = displayExecutionTimeInColor(frameOutput, sw);


                //end stopwatch -calculate duration of each frame display
                sw.Stop();
            }
        }


        /* Go home Button and command  - Rovio 1   */
        private void button48_Click(object sender, EventArgs e)
        {
            rovio1.GoHomeAndDock();
        }

        
        private void button22_Click(object sender, EventArgs e)
        {
            rovio2.GoHomeAndDock();
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
           //Image<Gray, Byte> cannyFrame = smoothedGrayFrame.Canny(new Gray(100), new Gray(60));   //100,60
           Image<Gray, Byte> cannyFrame = smoothedGrayFrame.Canny(new Gray(edparam1), new Gray(edparam2));
           return cannyFrame;
       }


                        /*************************************/
                        /*         Tracking Pink Color       */
                        /*************************************/


       public Image<Bgr, Byte> PinkTracking(Image<Bgr, Byte> image)
       {
           
           

           MCvMoments moments = new MCvMoments();
               
           //MCvScalar hsv_min=new MCvScalar(0,50,170);
           //MCvScalar hsv_max=new MCvScalar(10,180,256);
           //MCvScalar hsv_min2=new MCvScalar(170,50,170);
           //MCvScalar hsv_max2=new MCvScalar(256,180,256);

           MCvScalar hsv_min = new MCvScalar(172, 147, 185);
           MCvScalar hsv_max = new MCvScalar(176, 211, 256);

           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           //CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           //CvInvoke.cvOr(thresholded, thresholded2, thresholded,IntPtr.Zero);
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

           for (int i = 0; i <= segments; i++)
           {
               for (int j = 0; j <= segments; j++)
               {
                   //create points
                   p1 = new Point(i * image.Width / segments, j * image.Height / segments);
                   p2 = new Point((i + 1) * image.Width / segments, (j + 1) * image.Height / segments);

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

           //MCvScalar hsv_min = new MCvScalar(0, 50, 170);
           //MCvScalar hsv_max = new MCvScalar(10, 180, 256);
           //MCvScalar hsv_min2 = new MCvScalar(170, 50, 170);
           //MCvScalar hsv_max2 = new MCvScalar(256, 180, 256);

           MCvScalar hsv_min = new MCvScalar(172, 147, 185);
           MCvScalar hsv_max = new MCvScalar(176, 211, 256);

           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           //CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           //CvInvoke.cvOr(thresholded, thresholded2, thresholded, IntPtr.Zero);
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
           if (center.Y > 440) //440 originally
           {
               if (goHomeFlag == 1)
               {
                   rovio1.GoHome();
                   processingChoice = 0;
                   goHomeFlag = 0;
               }
               if (processingChoice == 11)
               {
                   processingChoice = 9;//yellow ball
                   mission1Flag = 1;
               }
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

           //MCvScalar hsv_min = new MCvScalar(0, 50, 170);
           //MCvScalar hsv_max = new MCvScalar(10, 180, 256);
           //MCvScalar hsv_min2 = new MCvScalar(170, 50, 170);
           //MCvScalar hsv_max2 = new MCvScalar(256, 180, 256);

           MCvScalar hsv_min = new MCvScalar(172, 147, 185);
           MCvScalar hsv_max = new MCvScalar(176, 211, 256);


           Image<Gray, Byte> thresholded = image.Convert<Gray, Byte>();
           Image<Gray, Byte> thresholded2 = image.Convert<Gray, Byte>();
           Image<Hsv, Byte> hsv_image = image.Convert<Hsv, Byte>();

           CvInvoke.cvCvtColor(image, hsv_image, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2HSV);
           CvInvoke.cvInRangeS(hsv_image, hsv_min, hsv_max, thresholded);
           //CvInvoke.cvInRangeS(hsv_image, hsv_min2, hsv_max2, thresholded2);
           //CvInvoke.cvOr(thresholded, thresholded2, thresholded, IntPtr.Zero);
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
           if (center.Y > 440) //originally 440
           {
               
               
               if (playFlag == 1)
               {
                   processingChoice = 8;
               }

               //stop Rovio 2
               rovio2.ManualDrive(0, speed); //stop
               CvInvoke.cvPutText(image, textStop, pointActionMessage, ref font, colorMessage);
               
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


       /**********************************************/
       /*          Yellow Ball Commanding  Rovio 1   */
       /**********************************************/


       public Image<Bgr, Byte> YellowBallCommanding1(Image<Bgr, Byte> image)
       {

           MCvMoments moments = new MCvMoments();

           MCvScalar hsv_min = new MCvScalar(30, 101, 102);
           MCvScalar hsv_max = new MCvScalar(53, 174, 239);

           

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
           if (center.Y > 400) //440 originally
           {
               if (mission1Flag == 1)
               {
                   processingChoice = 11;  //again pink
                   mission1Flag = 0;  
                   goHomeFlag = 1;
               }
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

                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
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

       /**********************************************/
       /*          Yellow Ball Commanding  Rovio 2   */
       /**********************************************/


       public Image<Bgr, Byte> YellowBallCommanding2(Image<Bgr, Byte> image)
       {

           MCvMoments moments = new MCvMoments();

           MCvScalar hsv_min = new MCvScalar(30, 101, 102);
           MCvScalar hsv_max = new MCvScalar(53, 174, 239);



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
           if (center.Y > 400) //440 originally
           {
               if (processingChoice == 8)   //play with the ball
               {
                   //give permission to rovio 2
                   processingChoice = 4;
                   playFlag = 1;
               }
               //stop Rovio 2
               rovio2.ManualDrive(0, speed); //stop
               CvInvoke.cvPutText(image, textStop, pointActionMessage, ref font, colorMessage);



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
                   rovio2.ManualDrive(7, speed);  //forward left
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



       /**********************************************/
       /*          New Ball Commanding  Rovio 1   */
       /**********************************************/


       public Image<Bgr, Byte> NewBallCommanding1(Image<Bgr, Byte> image)
       {

           MCvMoments moments = new MCvMoments();

           MCvScalar hsv_min = new MCvScalar(hue_min, sat_min, val_min);
           MCvScalar hsv_max = new MCvScalar(hue_max, sat_max, val_max);



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
           if (center.Y > 400) //440 originally
           {
               if (mission1Flag == 1)
               {
                   processingChoice = 11;  //again pink
                   mission1Flag = 0;
                   goHomeFlag = 1;
               }
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

                   CvInvoke.cvPutText(image, textForward, pointActionMessage, ref font, colorMessage);
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

                            /*************************************************/
                            /*          Circle and color   detection         */
                            /*************************************************/

       public Image<Bgr, Byte> CircleAndColorDetection(Image<Bgr, Byte> image)
       {
           
           MCvScalar centreColor = new MCvScalar(0, 0, 0);  //black centre
           MCvScalar circumColorI = new MCvScalar(255, 255, 255);   //purple internal circumfere
           MCvScalar circumColorE = new MCvScalar(255, 0, 0);   // white external circumf

           IntPtr cstorage = CvInvoke.cvCreateMemStorage(0);
           Image<Gray, Byte> gray = image.Convert<Gray, Byte>();
           Image<Gray, Byte> edge = image.Convert<Gray, Byte>();
           CvInvoke.cvThreshold(gray, gray, thresholdCircle, 256, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
           CvInvoke.cvSmooth(gray, gray, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_GAUSSIAN,param1, param2,(double)param3,(double)param4);
           CvInvoke.cvCanny(gray, edge, (double)threshold1, (double)threshold1,apertureSize);
           IntPtr circles = CvInvoke.cvHoughCircles(gray, cstorage, Emgu.CV.CvEnum.HOUGH_TYPE.CV_HOUGH_GRADIENT,(double)accResolution,(double)minDist,(double)cannyThreshold,(double)accThreshold,minRadius,maxRadius); //2, 100, 5, 50, 0, 100

           

           for (int i = 0; i < N; i++)   //filter N circles
           {
               unsafe 
               {
                   try
                   {
                       float* p = (float*)CvInvoke.cvGetSeqElem(circles, i);
                       centre = new Point((int)p[0], (int)p[1]);
                       
                       //CvInvoke.cvCircle(image, centre, 1, centreColor, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
                       CvInvoke.cvCircle(image, centre, (int)p[2], circumColorI, 3, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
                       CvInvoke.cvCircle(image, centre, (int)p[2] + 3, circumColorE, 3, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, 0);
                       string s = "Circle "+(i+1)+", Centre (" + Convert.ToString(Convert.ToInt32(p[0])) + "," + Convert.ToString(Convert.ToInt32(p[1])) + "), Radius: " + Convert.ToString(Convert.ToInt32(p[2]));
                       displayCirclePosition(s);
                   }
                   catch
                   { 
                       //do nothing
                   }
               }

               //button clicked 
               if (button20Clicked == true)    //take sample
               {
                   
                   Bgr bgr = image[centre];  //(0,0)
                   centre.X += 10;
                   Bgr bgr1 = image[centre]; //(10,0)
                   centre.Y += 10;
                   Bgr bgr2 = image[centre];  //(10,10)
                   centre.X -= 10;
                   Bgr bgr3 = image[centre];  //(0,10)
                   centre.X -= 10;
                   Bgr bgr4 = image[centre];  //(-10,10)
                   centre.Y -= 10;
                   Bgr bgr5 = image[centre];  //(-10,0)
                   centre.Y -= 10;
                   Bgr bgr6 = image[centre];  //(-10,-10)
                   centre.X += 10;
                   Bgr bgr7 = image[centre];  //(0,-10)
                   centre.X += 10;
                   Bgr bgr8 = image[centre];  //(10,-10)

                   Bgr bgrAve = averageBGR(bgr, bgr1, bgr2, bgr3, bgr4, bgr5, bgr6, bgr7, bgr8);


                   Rgb rgb = new Rgb();
                   rgb.Red = bgrAve.Red;
                   rgb.Green = bgrAve.Green;
                   rgb.Blue = bgrAve.Blue;
                   string bgrString = "BGR Average: Centre : R[" + (int)rgb.Red + "] G[" + (int)rgb.Green + "] B[" + (int)rgb.Blue + "]";
                   displayColor(bgrString);
                   hsv = RGB_to_HSV(rgb);
                   string hsvString = "HSV: Centre : H[" + (int)hsv.Hue + "] S[" + (int)hsv.Satuation + "] V[" + (int)hsv.Value + "]";
                   displayColor(hsvString);
                   
                   button20Clicked = false;
               }

               //button clicked
               if (button45Clicked == true)
               {
                   int hue_half_range=60;
                   int sat_half_range=15;
                   int val_half_range=15;

                   hue_min = (int)hsv.Hue - hue_half_range;
                   if (hue_min < 0)
                       hue_min = 0;


                   sat_min = (int)hsv.Satuation - sat_half_range;
                   if (sat_min < 0)
                       sat_min = 0;

                   val_min = (int)hsv.Value - val_half_range;
                   if (val_min < 0)
                       val_min = 0;




                   hue_max = (int)hsv.Hue + hue_half_range;
                   if (hue_max > 255)
                       hue_max = 255;

                   sat_max = (int)hsv.Satuation + sat_half_range;
                   if (sat_max > 255)
                       sat_max = 255;

                   val_max = (int)hsv.Value + val_half_range;
                   if (val_max > 255)
                       val_max = 255;

                   
                   button45Clicked = false;
               }
           }
           


         

               return image;
       }



       private void button23_Click(object sender, EventArgs e)  //edge detection button
       {

           if (processingChoice == 1)
           {
               processingChoice = 0;
           }
           else
           {
               trackBar1.Value = edparam1;
               label17.Text = "Thresh = " + Convert.ToInt32(trackBar1.Value);
               trackBar2.Value = edparam2;
               label18.Text = "ThreshLinking = " + Convert.ToInt32(trackBar2.Value);

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
               trackBar3.Value = segments;
               label20.Text = "Segments = " + Convert.ToInt32(trackBar3.Value * trackBar3.Value);

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
               playFlag = 0; //stop game flag
           }
           else
           {

               processingChoice = 8;
           }
       }

        //new ball commanding button rovio 1
       private void button21_Click(object sender, EventArgs e)
       {
           if (processingChoice == 12)
           {
               processingChoice = 0;
               playFlag = 0; //stop game flag
           }
           else
           {

               processingChoice = 12;
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
               
               numericUpDown1.Value = N;
               label11.Text = "threshold =" + Convert.ToString(thresholdCircle);
               numericUpDown2.Value = param1;
               numericUpDown3.Value = param2;
               numericUpDown4.Value = param3;
               numericUpDown5.Value = param4;
               numericUpDown6.Value = apertureSize;
               numericUpDown7.Value = threshold2;
               numericUpDown8.Value = threshold1;

               
               numericUpDown9.Value = accResolution;
               numericUpDown10.Value = minDist;
               numericUpDown11.Value = cannyThreshold;
               numericUpDown12.Value = accThreshold;
               numericUpDown13.Value = minRadius;
               numericUpDown14.Value = maxRadius;
           }
       }

       private void button49_Click(object sender, EventArgs e)  //find yellow ball rovio 1
       {
           if (processingChoice == 9)
           {
               processingChoice = 0;
           }
           else
           {

               processingChoice = 9;
           }
       }

       private void button50_Click(object sender, EventArgs e)  //find yellow ball rovio 2
       {
           if (processingChoice == 10)
           {
               processingChoice = 0;
           }
           else
           {

               processingChoice = 10;
           }
       }

       private void button51_Click(object sender, EventArgs e)   //mission 1
       {
           if (processingChoice == 11)
           {
               processingChoice = 0;
           }
           else
           {

               processingChoice = 11;
           }
       }


       /********************************      
        *                              *
        *     Method to diplay         *
        *     Execution Time in           *
        *     Gray Images              *
        *                              *
        ********************************/

       public Image<Gray, Byte> displayExecutionTimeInGray(Image<Gray, Byte> grayImage,Stopwatch sw1)
       {
           //Frame delay display
           Point pointFrameDelayText = new Point(350, 20);
           MCvFont fontFrameDelayText = new MCvFont();
           CvInvoke.cvInitFont(ref fontFrameDelayText, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 1, 1, 0, 1, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
           MCvScalar colorFrameDelayText = new MCvScalar(255, 255, 255);
           string strFrameDelay = "Execution Time:" + sw1.Elapsed.Milliseconds + "ms";
           CvInvoke.cvPutText(grayImage, strFrameDelay, pointFrameDelayText, ref fontFrameDelayText, colorFrameDelayText);

           return grayImage;
       }


       /********************************      
        *                              *
        *     Method to diplay         *
        *     Execution Time in           *
        *     Gray Images              *
        *                              *
        ********************************/


       public Image<Bgr, Byte> displayExecutionTimeInColor(Image<Bgr, Byte> colorImage, Stopwatch sw1)
       {
           //Frame delay display
           Point pointFrameDelayText = new Point(350, 20);
           MCvFont fontFrameDelayText = new MCvFont();
           CvInvoke.cvInitFont(ref fontFrameDelayText, Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 1.3, 1.3, 0, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED);
           MCvScalar colorFrameDelayText = new MCvScalar(0, 0, 255);
           string strFrameDelay = "Execution Time:" + sw1.Elapsed.Milliseconds + "ms";
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
           rovio1 = new RovioController("username", "password", rovio1URL);

           
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
           rovio2 = new RovioController("username", "password", rovio2URL);
       

       }


        //display Rovios Coordinates from another thread - UI delegated calling

       public void displayPosRovio1(string coordinates)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod theDelegateMethod = new MyDelegateMethod(this.displayPosRovio1);
               this.Invoke(theDelegateMethod, new object[] { coordinates });
           }
           else
           {
               this.label8.Text = coordinates;
           }
       }


        //DisplayNameAttribute circle position in CircleF detection
       public void displayCirclePosition(string circlePosition)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod7 theDelegateMethod = new MyDelegateMethod7(this.displayCirclePosition);
               this.Invoke(theDelegateMethod, new object[] { circlePosition });
           }
           else
           {
               
               this.richTextBox1.AppendText(circlePosition+"\n");
               this.richTextBox1.ScrollToCaret();
           }
       }

       public void displayColor(string color)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod8 theDelegateMethod = new MyDelegateMethod8(this.displayColor);
               this.Invoke(theDelegateMethod, new object[] { color });
           }
           else
           {
               this.richTextBox2.AppendText(color + "\n");
               this.richTextBox2.ScrollToCaret();
           }
       }

       public void displayPosRovio2(string coordinates)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod theDelegateMethod1 = new MyDelegateMethod(this.displayPosRovio2);
               this.Invoke(theDelegateMethod1, new object[] { coordinates });
           }
           else
           {
               this.label9.Text = coordinates;
           }
       }

        //display distance between two rovios
       public void displayDistance(decimal distance)
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod5 theDelegateMethod = new MyDelegateMethod5(this.displayDistance);
               this.Invoke(theDelegateMethod, new object[] { distance });
           }
           else
           {
               this.label10.Text = "Distance : "+Convert.ToString(distance);
               
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
                   this.progressBar1.Value = 30;
                   //this.rovio1.GoHome();
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
                   this.progressBar2.Value = 30;
                   //this.rovio1.GoHome();
           }
       }

        //display ball position based on the positioning system
       public void displayBallPosition()
       {
           if (this.InvokeRequired)
           {
               MyDelegateMethod6 theDelegateMethod = new MyDelegateMethod6(this.displayBallPosition);
               this.Invoke(theDelegateMethod, new object[] { });
           }
           else
           {
               //i deleted this label
               //this.label11.Text = "Ball Position: (" + Convert.ToString(pos1x)+","+Convert.ToString(pos1y)+")" ;

           }
       }




        //truncate function for displaying decimal digits , theta and distance (2 decimal digits)
       public decimal TruncateFunction(decimal number, int digits)
       {
           decimal stepper = (decimal)(Math.Pow(10.0, (double)digits));
           int temp = (int)(stepper * number);
           return (decimal)temp / stepper;
       }



       //Tab 2 manual controls for all the Rovios

       //rovio 1 manual

       private void button55_Click(object sender, EventArgs e) //forward
       {
           rovio1.ManualDrive(1, speed);
       }

       private void button59_Click(object sender, EventArgs e)  //down
       {
           rovio1.ManualDrive(2, speed);
       }

       private void button57_Click(object sender, EventArgs e)  //left
       {
           rovio1.ManualDrive(3, speed);
       }

       private void button52_Click(object sender, EventArgs e)  //right
       {
           rovio1.ManualDrive(4, speed);
       }

       private void button54_Click(object sender, EventArgs e)  //forward right
       {
           rovio1.ManualDrive(8, speed);
       }

       private void button56_Click(object sender, EventArgs e)  //forward left
       {
           rovio1.ManualDrive(7, speed);
       }

       private void button58_Click(object sender, EventArgs e) //back left
       {
           rovio1.ManualDrive(9, speed);
       }

       private void button53_Click(object sender, EventArgs e)   //back right
       {
           rovio1.ManualDrive(10, speed);
       }

       private void button60_Click(object sender, EventArgs e)    //stop
       {
           rovio1.ManualDrive(0, speed);
       }

       private void button98_Click(object sender, EventArgs e)    //rotate right
       {
           rovio1.ManualDrive(18, speed);
       }

       private void button97_Click(object sender, EventArgs e)    //rotate left
       {
           rovio1.ManualDrive(17, speed);
       }

       //rovio 2manual

       private void button75_Click(object sender, EventArgs e)  //forward
       {
           rovio2.ManualDrive(1, speed);
       }

       private void button71_Click(object sender, EventArgs e)  //back
       {
           rovio2.ManualDrive(2, speed);
       }

       private void button77_Click(object sender, EventArgs e)  //right
       {
           rovio2.ManualDrive(4, speed);
       }

       private void button73_Click(object sender, EventArgs e)  //left
       {
           rovio2.ManualDrive(3, speed);
       }

       private void button76_Click(object sender, EventArgs e)   //forward right
       {
           rovio2.ManualDrive(8, speed);
       }

       private void button74_Click(object sender, EventArgs e)   //forward left
       {
           rovio2.ManualDrive(7, speed);
       }

       private void button72_Click(object sender, EventArgs e)   //back left
       {
           rovio2.ManualDrive(9, speed);
       }

       private void button78_Click(object sender, EventArgs e)   //back right
       {
           rovio2.ManualDrive(10, speed);
       }

       private void button70_Click(object sender, EventArgs e)   //stop
       {
           rovio2.ManualDrive(0, speed);
       }

       private void button153_Click(object sender, EventArgs e)  //rotate right
       {
           rovio2.ManualDrive(18, speed);
       }

       private void button152_Click(object sender, EventArgs e) //rotate left
       {
           rovio2.ManualDrive(17, speed);
       }


        //rovio 3 manual
       private void button66_Click(object sender, EventArgs e)  //forawrd
       {
           rovio3.ManualDrive(1, speed);
       }

       private void button62_Click(object sender, EventArgs e)  //back
       {
           rovio3.ManualDrive(2, speed);
       }

       private void button64_Click(object sender, EventArgs e)  //left
       {
           rovio3.ManualDrive(3, speed);
       }

       private void button68_Click(object sender, EventArgs e)   //right
       {
           rovio3.ManualDrive(4, speed);
       }

       private void button67_Click(object sender, EventArgs e)   //forward right
       {
           rovio3.ManualDrive(8, speed);
       }

       private void button65_Click(object sender, EventArgs e)   //forwrad left
       {
           rovio3.ManualDrive(7, speed);
       }

       private void button63_Click(object sender, EventArgs e)   //back left
       {
           rovio3.ManualDrive(9, speed);
       }
         
       private void button69_Click(object sender, EventArgs e)   //back right
       {
           rovio3.ManualDrive(10, speed);
       }

       private void button147_Click(object sender, EventArgs e)   //rotate right
       {
           rovio3.ManualDrive(18, speed);
       }

       private void button146_Click(object sender, EventArgs e)  //rotate left
       {
           rovio3.ManualDrive(17, speed);
       }

       private void button61_Click(object sender, EventArgs e)  //stop
       {
           rovio3.ManualDrive(0, speed);
       }



        //rovio 4 manual

       private void button84_Click(object sender, EventArgs e)   //forward
       {
           rovio4.ManualDrive(1, speed);
       }

       private void button80_Click(object sender, EventArgs e)   //back
       {
           rovio4.ManualDrive(2, speed);
       }

       private void button82_Click(object sender, EventArgs e)   //left
       {
           rovio4.ManualDrive(3, speed);
       }

       private void button86_Click(object sender, EventArgs e)    //right
       {
           rovio4.ManualDrive(4, speed);
       }

       private void button85_Click(object sender, EventArgs e)   //forward right
       {
           rovio4.ManualDrive(8, speed);
       }

       private void button83_Click(object sender, EventArgs e)   //forward left
       {
           rovio4.ManualDrive(7, speed);
       }

       private void button81_Click(object sender, EventArgs e)   //back left
       {
           rovio4.ManualDrive(9, speed);
       }

       private void button87_Click(object sender, EventArgs e)   //back right
       {
           rovio4.ManualDrive(10, speed);
       }

       private void button151_Click(object sender, EventArgs e)   //rotate right
       {
           rovio4.ManualDrive(18, speed);
       }

       private void button150_Click(object sender, EventArgs e)   //rotate left
       {
           rovio4.ManualDrive(17, speed);
       }

       private void button79_Click(object sender, EventArgs e)   //stop
       {
           rovio4.ManualDrive(0, speed);
       }


        //rovio 5 manual

       private void button93_Click(object sender, EventArgs e)  //forawrd
       {
           rovio5.ManualDrive(1, speed);
       }

       private void button89_Click(object sender, EventArgs e)  //back
       {
           rovio5.ManualDrive(2, speed);
       }

       private void button91_Click(object sender, EventArgs e)  //left
       {
           rovio5.ManualDrive(3, speed);
       }

       private void button95_Click(object sender, EventArgs e)  //rihgt
       {
           rovio5.ManualDrive(4, speed);
       }

       private void button94_Click(object sender, EventArgs e)  //forward right
       {
           rovio5.ManualDrive(8, speed);
       }

       private void button92_Click(object sender, EventArgs e)  //forward left
       {
           rovio5.ManualDrive(7, speed);
       }

       private void button90_Click(object sender, EventArgs e)  //back left
       {
           rovio5.ManualDrive(9, speed);
       }

       private void button96_Click(object sender, EventArgs e)   //back right
       {
           rovio5.ManualDrive(10, speed);
       }

       private void button149_Click(object sender, EventArgs e)  //rotate right
       {
           rovio5.ManualDrive(18, speed);
       }

       private void button148_Click(object sender, EventArgs e)   //rotate left
       {
           rovio5.ManualDrive(17, speed);
       }

       private void button88_Click(object sender, EventArgs e)  //stop
       {
           rovio5.ManualDrive(0, speed);
       }

       private void button154_Click(object sender, EventArgs e) //enable threads for tab 2
       {
           enableTab2 = true; 

           /*   Thread for taking images from Rovio 3      */
           Thread takeImagesThread3 = new Thread(new ThreadStart(takeImages3));
           takeImagesThread3.IsBackground = true;    //for the thread to close with the application
           takeImagesThread3.Start();

           /*   Thread for taking images from Rovio 4      */
           Thread takeImagesThread4 = new Thread(new ThreadStart(takeImages4));
           takeImagesThread4.IsBackground = true;    //for the thread to close with the application
           takeImagesThread4.Start();

           /*   Thread for taking images from Rovio 5      */
           Thread takeImagesThread5 = new Thread(new ThreadStart(takeImages5));
           takeImagesThread5.IsBackground = true;    //for the thread to close with the application
           takeImagesThread5.Start();
       }

       private void button20_Click(object sender, EventArgs e)
       {
           button20Clicked = true;
           
       }

        //rgb to hsv method
       public static Hsv RGB_to_HSV(Rgb rgb)
       {
           int rgb_max = (int)Math.Max(rgb.Red, Math.Max(rgb.Green, rgb.Blue));
           int rgb_min = (int)Math.Min(rgb.Red, Math.Min(rgb.Green, rgb.Blue));
           Hsv hsv = new Hsv();
           hsv.Value = rgb_max;
           if (hsv.Value == 0)
           {
               hsv.Hue = hsv.Satuation = 0;
               return hsv;
           }
           hsv.Satuation = 255 * (rgb_max - rgb_min) / hsv.Value;
           if (hsv.Satuation == 0)
           {
               hsv.Hue = 0;
               return hsv;
           }
           /* Compute hue */
           if (rgb_max == rgb.Red)
           {
               hsv.Hue = 0 + 43 * (rgb.Green - rgb.Blue) / (rgb_max - rgb_min);
           }
           else if (rgb_max == rgb.Green)
           {
               hsv.Hue = 85 + 43 * (rgb.Blue - rgb.Red) / (rgb_max - rgb_min);
           }
           else /* rgb_max == rgb.b */
           {
               hsv.Hue = 171 + 43 * (rgb.Red - rgb.Green) / (rgb_max - rgb_min);
           }
           return hsv;

       }

       private void button45_Click(object sender, EventArgs e)
       {
           button45Clicked = true;

           trackBar4.Value = hue_min;
           trackBar5.Value = sat_min;
           trackBar6.Value = val_min;

           trackBar9.Value = hue_max;
           trackBar8.Value = sat_max;
           trackBar7.Value = val_max;

       }


       public static Bgr averageBGR(Bgr bgr, Bgr bgr1, Bgr bgr2, Bgr bgr3, Bgr bgr4, Bgr bgr5, Bgr bgr6, Bgr bgr7, Bgr bgr8)
       {
           Bgr aveBgr = new Bgr();

           aveBgr.Blue = (bgr.Blue + bgr1.Blue + bgr2.Blue + bgr3.Blue + bgr4.Blue + bgr5.Blue + bgr6.Blue + bgr7.Blue + bgr8.Blue) / 9;
           aveBgr.Green = (bgr.Green + bgr1.Green + bgr2.Green + bgr3.Green + bgr4.Green + bgr5.Green + bgr6.Green + bgr7.Green + bgr8.Green) / 9;
           aveBgr.Red = (bgr.Red + bgr1.Red + bgr2.Red + bgr3.Red + bgr4.Red + bgr5.Red + bgr6.Red + bgr7.Red + bgr8.Red) / 9;
           return aveBgr;
       }

       

       
      

      

        

       

       
    }
}

