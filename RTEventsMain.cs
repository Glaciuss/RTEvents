
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace RTEvents
{
    public partial class RTEventsMain : Form
    {
        public RTEventsMain()
        {
            InitializeComponent();
        }

        //Create Standalone SDK class dynamicly.
        public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();

        #region Communication
        private bool bIsConnected = false;//the boolean value identifies whether the device is connected
        private int iMachineNumber = 1;//the serial number of the device.After connecting the device ,this value will be changed.

        //If your device supports the TCP/IP communications, you can refer to this.
        //when you are using the tcp/ip communication,you can distinguish different devices by their IP address.
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (txtIP.Text.Trim() == "" || txtPort.Text.Trim() == "")
            {
                MessageBox.Show("IP and Port cannot be null", "Error");
                return;
            }
            int idwErrorCode = 0;

            Cursor = Cursors.WaitCursor;
            if (btnConnect.Text == "DisConnect")
            {
                axCZKEM1.Disconnect();

                //this.axCZKEM1.OnVerify -= new zkemkeeper._IZKEMEvents_OnVerifyEventHandler(axCZKEM1_OnVerify);
                this.axCZKEM1.OnAttTransactionEx -= new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);


                bIsConnected = false;
                btnConnect.Text = "Connect";
                lblState.Text = "Current State:DisConnected";
                Cursor = Cursors.Default;
                return;
            }

            bIsConnected = axCZKEM1.Connect_Net(txtIP.Text, Convert.ToInt32(txtPort.Text));
            if (bIsConnected == true)
            {
                btnConnect.Text = "DisConnect";
                btnConnect.Refresh();
                lblState.Text = "Current State:Connected";
                iMachineNumber = 1;//In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.
                if (axCZKEM1.RegEvent(iMachineNumber, 65535))//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                {
                    //this.axCZKEM1.OnVerify += new zkemkeeper._IZKEMEvents_OnVerifyEventHandler(axCZKEM1_OnVerify);
                    this.axCZKEM1.OnAttTransactionEx += new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);

                }
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                MessageBox.Show("Unable to connect the device,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        //If your device supports the SerialPort communications, you can refer to this.

        #endregion

        #region RealTime Events

        //private void axCZKEM1_OnVerify(int iUserID)
        //{
        //    lbRTShow.Items.Add("...UserName:" + iUserID);
        //}

        //If your fingerprint(or your card) passes the verification,this event will be triggered
        private void axCZKEM1_OnAttTransactionEx(string sEnrollNumber, int iIsInValid, int iAttState, int iVerifyMethod, int iYear, int iMonth, int iDay, int iHour, int iMinute, int iSecond, int iWorkCode)
        {
            lbRTShow.Items.Add("RTEvent OnAttTrasactionEx Has been Triggered,Verified OK");
            lbRTShow.Items.Add("...UserID:" + sEnrollNumber);
            lbRTShow.Items.Add("...isInvalid:" + iIsInValid.ToString());
            lbRTShow.Items.Add("...attState:" + iAttState.ToString());
            lbRTShow.Items.Add("...VerifyMethod:" + iVerifyMethod.ToString());
            lbRTShow.Items.Add("...Workcode:" + iWorkCode.ToString());//the difference between the event OnAttTransaction and OnAttTransactionEx
            string TimeStamp = iYear.ToString() + "-" + iMonth.ToString() + "-" + iDay.ToString() + " " + iHour.ToString() + ":" + iMinute.ToString() + ":" + iSecond.ToString();
            lbRTShow.Items.Add("...Time:" + TimeStamp);
            outtable(sEnrollNumber, iVerifyMethod.ToString(), TimeStamp);
        }


        private void outtable(string UserID, string Mod, string TimeStamp)
        {
            ////begin upload
            string connetionString;
            SqlConnection cnn;
            SqlCommand cmd;
            connetionString = @"Data Source=192.168.88.141;Initial Catalog=CarService;User ID=sa;Password=sa0816812178";
            cnn = new SqlConnection(connetionString);

            cnn.Open();
            
            String query = "INSERT INTO dbo.Log_ZKTeco (EmployeeNumber,EmployeeName,VerifyMethod,TimeStamp) ";
            query += "VALUES (@EmployeeNumber,@EmployeeName, @VerifyMethod,@TimeStamp)";
            SqlCommand uploadFace = new SqlCommand(query, cnn);

            uploadFace.Parameters.AddWithValue("@EmployeeNumber", UserID);//col 1 in SQL (dbo.EmpFace)
            uploadFace.Parameters.AddWithValue("@EmployeeName", Mod);//col 2 in SQL (dbo.EmpFace)
            uploadFace.Parameters.AddWithValue("VerifyMethod", Mod);//col 3 in SQL (dbo.EmpFace)
            uploadFace.Parameters.AddWithValue("TimeStamp", TimeStamp);//col 4 in SQL (dbo.EmpFace)
            uploadFace.ExecuteNonQuery();

            //Update Name SQL
            String updateName = "update Log_ZKTeco";
            updateName += " set Log_ZKTeco.EmployeeName = EmpIndex.EmployeeName";
            updateName += " from Log_ZKTeco inner join EmpIndex on Log_ZKTeco.EmployeeNumber = EmpIndex.EmployeeNumber";
            SqlCommand uploadName = new SqlCommand(updateName, cnn);
            uploadName.ExecuteNonQuery();

            cnn.Close();
            
            //Table Show
            ListViewItem list = new ListViewItem();
            list.Text = UserID;
            list.SubItems.Add("unknow");
            list.SubItems.Add(Mod);
            list.SubItems.Add(TimeStamp);
            lvRT.Items.Add(list);
        }



        #endregion

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void lbRTShow_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
} 