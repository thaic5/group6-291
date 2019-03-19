﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace group6_291
{
    public partial class AdminMaster : Form
    {
        public AdminMaster()
        {

            InitializeComponent();

        }
        private void AdminMaster_Load(object sender, EventArgs e)
        {
            if (Form1.Account == 0)
            {
                TabControl.TabPages.Remove(UserAccTab);
                TabControl.TabPages.Remove(PatientRecTab);
                TabControl.TabPages.Remove(WardTab);
            }
            addUsername.Leave += new EventHandler(addUsername_Leave);
            addPassword.Leave += new EventHandler(addPassword_Leave);
            populateAccountList();
            addWardNameBox.Leave += new EventHandler(addWardNameBox_Leave);
            AddWardCapacityBox.Leave += new EventHandler(addWardCapacityBox_Leave);
            wardListBox.SelectedIndexChanged += new EventHandler(wardListBox_SelectedIndexChanged);
            populateWardList();
            populateDoctorList();


        }

        //Admin Account Functions

        //Purpose: Populate the account list box with all the registered users
        private void populateDoctorList()
        {
            DoctorErrorLabel.Text = "";
            DoctorUpdateError.Text = "";
            //Open connection and create a dataset from the query
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter("select (firstName + lastName) AS Name, * from [Doctor]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            adapter.Fill(ds);
            ds.Tables[0].DefaultView.Sort = "firstName asc";
            DoctorListBox.DataSource = ds.Tables[0];
            DoctorListBox.DisplayMember = "Name";

            DataTable Department = new DataTable("Department");
            SqlDataAdapter adap = new SqlDataAdapter("Select * from [Department]", conn);
            adap.Fill(Department);
            foreach (DataRow items in Department.Rows)
            {
                DoctorDeptBox.Items.Add(items[0].ToString());
                DoctorUpdDeptBox.Items.Add(items[0].ToString());
            }
            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            string departmentName = DoctorList["departmentName"].ToString();
            DoctorUpdDeptBox.SelectedIndex = DoctorUpdDeptBox.FindStringExact(departmentName);
            conn.Close();
        }


        //Purpose: Populate the account list box with all the registered users
        private void populateAccountList()
        {
            //Open connection and create a dataset from the query
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter("select username, password, isAdmin from [User]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            adapter.Fill(ds);
            ds.Tables[0].DefaultView.Sort = "username asc";
            accountListBox.DataSource = ds.Tables[0];
            accountListBox.DisplayMember = "username";
            conn.Close();
        }

        //Purpose: Change the Username textbox's error label if it is invalid after user leaves the texbox
        private void addUsername_Leave(object sender, EventArgs e)
        {
            bool validUsername = usernameIsValid();
        }

        //Purpose: Change the Password textbox's error label if it is invalid after user leaves the texbox
        private void addPassword_Leave(object sender, EventArgs e)
        {
            bool validPassword = passwordIsValid();

        }

        //Purpose: Uncheck the receptionist checkbox if the admin one was checked
        private void adminCheckBox_CheckChanged(object sender, EventArgs e)
        {
            if (adminCheckbox.Checked == true)
            {
                recepCheckbox.Checked = false;
                checkboxInfo.Text = "";
            }
        }

        //Purpose: Uncheck the admin checkbox if the receptionist one was checked
        private void recepCheckBox_CheckChanged(object sender, EventArgs e)
        {
            if (recepCheckbox.Checked == true)
            {
                adminCheckbox.Checked = false;
                checkboxInfo.Text = "";
            }
        }

        //Purpose: Chceck that the username texbox has a valid username as its input
        private bool usernameIsValid()
        {
            string inputedUser = addUsername.Text;
            //Check username against username constraints
            if (!Regex.IsMatch(inputedUser, @"^[a-zA-Z0-9]+$"))
            {
                usernameInfo.Text = "*Invalid username characters";
                usernameInfo.ForeColor = Color.Red;
                return false;
            }
            else if (inputedUser.Length < 4 || inputedUser.Length > 16)
            {
                usernameInfo.Text = "*Invalid username length";
                usernameInfo.ForeColor = Color.Red;
                return false;
            }
            //Check to see if the username already exists
            else
            {
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand checkUsername = new SqlCommand("select count(*) from [User] where username = @user", conn);
                checkUsername.Parameters.AddWithValue("@user", inputedUser);
                int userExist = (int)checkUsername.ExecuteScalar();
                //Username already exists
                if (userExist > 0)
                {
                    usernameInfo.Text = "*Username already exists";
                    usernameInfo.ForeColor = Color.Red;
                    return false;
                }
                //Username does not already exist
                else
                {
                    usernameInfo.Text = "*Username is available";
                    usernameInfo.ForeColor = Color.Green;
                    return true;
                }
            }
        }

        //Purpose: Chceck that the password texbox has a valid password as its input
        private bool passwordIsValid()
        {
            string inputedPassword = addPassword.Text;
            //Check password against password constraints
            if (inputedPassword.Length < 4 || inputedPassword.Length > 16)
            {
                passwordInfo.Text = "*Invalid password length";
                passwordInfo.ForeColor = Color.Red;
                return false;
            }

            else if (!Regex.IsMatch(inputedPassword, @"^[a-zA-Z0-9]+$"))
            {
                passwordInfo.Text = "*Invalid password characters";
                passwordInfo.ForeColor = Color.Red;
                return false;
            }
            //Password is valid
            else
            {
                passwordInfo.Text = "";
                return true;
            }
        }

        //Purpose: Reset all reporting and input fields
        private void resetAddUserFields()
        {
            addUsername.Text = "";
            addPassword.Text = "";
            usernameInfo.Text = "";
            passwordInfo.Text = "";
            checkboxInfo.Text = "";
            adminCheckbox.Checked = false;
            recepCheckbox.Checked = false;
        }

        //Purpose: Add a new user to the User table in the database when the Add Account button is clicked
        private void addAccountButton_Click(object sender, EventArgs e)
        {
            //Make sure a role is checked
            if (adminCheckbox.Checked == false && recepCheckbox.Checked == false)
            {
                checkboxInfo.Text = "*Please select a role";
                checkboxInfo.ForeColor = Color.Red;
            }
            //Make sure username and password are valid
            else if (!usernameIsValid() || !passwordIsValid())
            {
                requestInfo.Text = "*Invalid add user request. Please fix errors!";
                requestInfo.ForeColor = Color.Red;
            }
            //All criteria is met, add user to databse
            else
            {
                //Get role
                bool isAdmin;
                if (adminCheckbox.Checked)
                    isAdmin = true;
                else
                    isAdmin = false;
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand addUser = new SqlCommand("insert into [User] (username, password, isAdmin) values (@username, @password, @isAdmin)", conn);
                addUser.Parameters.AddWithValue("@username", addUsername.Text);
                addUser.Parameters.AddWithValue("@password", addPassword.Text);
                addUser.Parameters.AddWithValue("@isAdmin", isAdmin);
                addUser.ExecuteNonQuery();
                //Update status and reset fields
                requestInfo.Text = "User added successfully";
                requestInfo.ForeColor = Color.Green;
                resetAddUserFields();
                conn.Close();
            }
            populateAccountList();

        }

        //Purpose: Reset all reporting fields INCLUDING the add user response when reset button is clicked
        private void resetUserButton_Click(object sender, EventArgs e)
        {
            resetAddUserFields();
            requestInfo.Text = "";
        }

        private void deleteAccountButton_Click(object sender, EventArgs e)
        {
            DataRowView accViewItem = accountListBox.SelectedItem as DataRowView;
            string username = accViewItem["username"].ToString();

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand deleteUser = new SqlCommand("delete from [User] where username = @username", conn);
            deleteUser.Parameters.AddWithValue("@username", username);
            deleteUser.ExecuteNonQuery();
            conn.Close();
            //Update status and reset fields
            populateAccountList();
            requestInfo.Text = "";
        }

        //Purpose: Repopulate the account list box when the refresh button is clicked
        private void refreshAccountList_Click(object sender, EventArgs e)
        {
            populateAccountList();
        }

        private void accountListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRowView drvItem = accountListBox.SelectedItem as DataRowView;
            string user = drvItem["username"].ToString();
            string admin = drvItem["isAdmin"].ToString();
            string password = drvItem["password"].ToString();

            if (admin.Equals("True"))
            {

                UpdateRecpCheckBox.Checked = false;
                UpdateAdminCheckBox.Checked = true;
            }
            else
            {
                UpdateAdminCheckBox.Checked = false;
                UpdateRecpCheckBox.Checked = true;
            }
            UpdatePassLabelText.Text = password;
            AccountUpdateLabel.Text = user;
        }

        //Purpose: Update selected account on update button click
        private void UpdateAccountButton_Click(object sender, EventArgs e)
        {
            //Get selected itme values
            DataRowView drvItem = accountListBox.SelectedItem as DataRowView;
            string user = drvItem["username"].ToString();
            string pass = drvItem["password"].ToString();

            //Make sure a role is checked
            if (UpdateRecpCheckBox.Checked == false && UpdateAdminCheckBox.Checked == false)
            {
                UpdateCheckLabel.Text = "*Please select a role";
                UpdateCheckLabel.ForeColor = Color.Red;
                return;
            }

            if (UpdateUserText.TextLength.Equals(0) || UpdatePassText.TextLength.Equals(0))
            {
                UpdateCheckLabel.Text = "Cannot have empty fields";
                UpdateCheckLabel.ForeColor = Color.Red;
            }
            else
            {
                
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                var sql = "UPDATE [User] SET username = @username, password = @password, isAdmin= @isAdmin where username=@userID";// repeat for all variables
                SqlCommand UpdateUser = new SqlCommand(sql, conn);
                UpdateUser.Parameters.AddWithValue("@username", UpdateUserText.Text);
                UpdateUser.Parameters.AddWithValue("@password", UpdatePassText.Text);
                if (UpdateRecpCheckBox.Checked)
                {
                    UpdateUser.Parameters.AddWithValue("@isAdmin", false);
                }
                else
                {
                    UpdateUser.Parameters.AddWithValue("@isAdmin", true);
                }
                UpdateUser.Parameters.AddWithValue("@userID", user);
                UpdateUser.ExecuteNonQuery();
                //Update status and reset fields
                UpdateCheckLabel.Text = "User updated successfully";
                UpdateCheckLabel.ForeColor = Color.Green;

                UpdateUserText.Clear();
                UpdatePassText.Clear();
                populateAccountList();
            }
        }

        //Purpose: Change checkbox value based on reception checkbox change 
        private void UpdateRecpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateAdminCheckBox.Checked == true)
            {
                //UpdateRecpCheckBox.Checked = true;
                UpdateAdminCheckBox.Checked = false;

            } else
            {
                UpdateRecpCheckBox.Checked = true;
            }
        }

        //Purpose: Change checkbox value based on admin checkbox change 
        private void UpdateAdminCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateRecpCheckBox.Checked == true)
            {
                //UpdateAdminCheckBox.Checked = true;
                UpdateRecpCheckBox.Checked = false;

            } else
            {
                UpdateAdminCheckBox.Checked = true;
            }

        }

        //Admin Ward Functions

        //Purpose: Populate the ward list box with all the registered wards
        private void populateWardList()
        {
            //Open connection and create a dataset from the query
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter("select * from [Ward]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            adapter.Fill(ds);
            ds.Tables[0].DefaultView.Sort = "wardName asc";
            wardListBox.DataSource = ds.Tables[0];
            wardListBox.DisplayMember = "wardName";
            conn.Close();
        }

        //Purpose: make sure ward name is available and valid after being inputed
        private void addWardNameBox_Leave(object sender, EventArgs e)
        {
            wardIsValid();
        }

        //Purpose: make sure ward capacity is valid after being inputed
        private void addWardCapacityBox_Leave(object sender, EventArgs e)
        {
            capacityIsValid();
        }

        //Purpose: Check if a ward name is available to use
        private bool wardIsValid()
        {
            string inputedWardName = addWardNameBox.Text;
            //Check username against username constraints
            if (!Regex.IsMatch(inputedWardName, @"^[a-zA-Z]+$"))
            {
                addWardInfo.Text = "*Invalid ward name characters";
                addWardInfo.ForeColor = Color.Red;
                return false;
            }
            else if (inputedWardName.Length < 2 || inputedWardName.Length > 32)
            {
                addWardInfo.Text = "*Invalid ward name length";
                addWardInfo.ForeColor = Color.Red;
                return false;
            }
            //Check to see if the username already exists
            else
            {
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand checkUsername = new SqlCommand("select count(*) from [Ward] where wardName = @wardName", conn);
                checkUsername.Parameters.AddWithValue("@wardName", inputedWardName);
                int wardExist = (int)checkUsername.ExecuteScalar();
                //Username already exists
                if (wardExist > 0)
                {
                    addWardInfo.Text = "*Ward already exists";
                    addWardInfo.ForeColor = Color.Red;
                    return false;
                }
                //Username does not already exist
                else
                {
                    addWardInfo.Text = "*Ward name is available";
                    addWardInfo.ForeColor = Color.Green;
                    return true;
                }
            }
        }

        //Purpose: Check if a capacity is valid
        private bool capacityIsValid()
        {
            int capacity;
            if (Int32.TryParse(AddWardCapacityBox.Text, out capacity))
            {
                if (capacity > 0)
                {
                    addWardCapInfo.Text = "";
                    return true;
                }
                else
                {
                    addWardCapInfo.Text = "*Capacity must be greater than 0";
                    addWardCapInfo.ForeColor = Color.Red;
                    return false;
                }
            }
            else
            {
                addWardCapInfo.Text = "*Invalid capacity";
                addWardCapInfo.ForeColor = Color.Red;
                return false;
            }
        }

        //Purpose: Add a ward to the Ward table if all input is valid
        private void addWardButt_Click(object sender, EventArgs e)
        {
            //Make sure ward name and capacity are valid
            if (!wardIsValid() || !capacityIsValid())
            {
                addWardRequestInfo.Text = "*Invalid add ward request. Please fix errors!";
                addWardRequestInfo.ForeColor = Color.Red;
            }
            //All criteria is met, add user to databse
            else
            {
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand addWard = new SqlCommand("insert into [Ward] (wardName, overall_capacity, current_capacity) values (@wardName, @overallCap, @currentCap)", conn);
                addWard.Parameters.AddWithValue("@wardName", addWardNameBox.Text);
                addWard.Parameters.AddWithValue("@overallCap", Int32.Parse(AddWardCapacityBox.Text));
                addWard.Parameters.AddWithValue("@currentCap", 0);
                addWard.ExecuteNonQuery();
                conn.Close();
                //Update status and reset fields
                addWardRequestInfo.Text = "Ward added successfully";
                addWardRequestInfo.ForeColor = Color.Green;
                resetAddWardFields();
            }
        }

        //Purpose: Reset all add ward reporting and input fields
        private void resetAddWardFields()
        {
            addWardNameBox.Text = "";
            AddWardCapacityBox.Text = "";
            addWardInfo.Text = "";
            addWardCapInfo.Text = "";
        }

        //Purpose: Reset all add ward reporting and input fields when reset button is clicked
        private void addWardReset_Click(object sender, EventArgs e)
        {
            resetAddWardFields();
            addWardRequestInfo.Text = "";
        }

        //Purpose: Refresh the ward list on refresh button click
        private void wardListRefresh_Click(object sender, EventArgs e)
        {
            populateWardList();
        }

        //Purpose: Delete the selected ward on delete click
        private void deleteWardButton_Click(object sender, EventArgs e)
        {
            //Get selected ward values
            DataRowView wardViewItem = wardListBox.SelectedItem as DataRowView;
            string wardName = wardViewItem["wardName"].ToString();

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand addWard = new SqlCommand("delete from [Ward] where wardName = @wardName", conn);
            addWard.Parameters.AddWithValue("@wardName", wardName);
            addWard.ExecuteNonQuery();
            conn.Close();
            //Update status and reset fields
            populateWardList();
            wardUpdateReqInfo.Text = "";
            addWardInfo.Text = "";
        }

        //Purpose: Update selected ward with inputed info on update click
        private void WardUpdateButton_Click(object sender, EventArgs e)
        {
            //Get selected ward values
            DataRowView wardViewItem = wardListBox.SelectedItem as DataRowView;
            string wardName = wardViewItem["wardName"].ToString();
            int overallCap = Int32.Parse(wardViewItem["overall_capacity"].ToString());
            int currentCap = Int32.Parse(wardViewItem["current_capacity"].ToString());

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand updateWard = new SqlCommand("update [Ward] set wardName = @newWardName, overall_capacity = @newOverallCap where wardName = @oldWardName", conn);
            updateWard.Parameters.AddWithValue("@oldWardName", wardName);

            //Use new ward name if one is given
            if (updateWardNameBox.Text.Length > 0)
                updateWard.Parameters.AddWithValue("@newWardName", updateWardNameBox.Text);
            else
                updateWard.Parameters.AddWithValue("@newWardName", wardName);

            //Use new capacity if a valid one is given
            int newCapacity;
            if (updateWardCapacityBox.Text.Length > 0)
            {
                updateSelectedWard.Text = "we here";
                if (Int32.TryParse(updateWardCapacityBox.Text, out newCapacity))
                {
                    if (newCapacity > 0 && (currentCap <= newCapacity))
                        updateWard.Parameters.AddWithValue("@newOverallCap", newCapacity);
                    else
                    {
                        wardUpdateReqInfo.Text = "*Invalid new capacity";
                        wardUpdateReqInfo.ForeColor = Color.Red;
                        conn.Close();
                        return;
                    }
                }
                else
                {
                    wardUpdateReqInfo.Text = "*Invalid new capacity number";
                    wardUpdateReqInfo.ForeColor = Color.Red;
                    conn.Close();
                    return;
                }
            }
            else
                updateWard.Parameters.AddWithValue("@newOverallCap", overallCap);

            updateWard.ExecuteNonQuery();
            wardUpdateReqInfo.Text = "Ward successfully update";
            wardUpdateReqInfo.ForeColor = Color.Green;
            resetUpdateWardFields();
            conn.Close();
            populateWardList();
        }

        //Purpose: Reset all update ward reporting and input fields
        private void resetUpdateWardFields()
        {
            updateWardNameBox.Text = "";
            updateWardCapacityBox.Text = "";
        }

        //Purpose: Update ward update tabs information when a new ward is selected
        private void wardListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get selected ward values
            DataRowView wardViewItem = wardListBox.SelectedItem as DataRowView;
            string wardName = wardViewItem["wardName"].ToString();
            string overallCap = wardViewItem["overall_capacity"].ToString();
            string currentCap = wardViewItem["current_capacity"].ToString();
            //Update labels
            updateCurrentName.Text = wardName;
            updateCurrentCap.Text = currentCap;
            updateOverallCap.Text = overallCap;
            //Check if ward is full or not
            if (Int32.Parse(overallCap) == Int32.Parse(currentCap))
                updateCurrentStatus.Text = "Full";
            else
                updateCurrentStatus.Text = "Not Full";
        }


        //Purpose: Reset all add ward reporting and input fields on reset click
        private void resetUpdateWard_Click(object sender, EventArgs e)
        {
            resetUpdateWardFields();
            wardUpdateReqInfo.Text = "";
        }

        private void DoctorListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            string firstName = DoctorList["firstName"].ToString();
            string lastName = DoctorList["lastName"].ToString();
            string departmentName = DoctorList["departmentName"].ToString();
            string specialization = DoctorList["specialization"].ToString();
            string duties = DoctorList["duties"].ToString();

            DoctorUpdFirstName.Text = firstName;
            DoctorUpdLastName.Text = lastName;
            DoctorUpdDeptBox.SelectedIndex = DoctorUpdDeptBox.FindStringExact(departmentName);
            DoctorUpdSpec.Text = specialization;
            DoctorUpdDuty.Text = duties;

           
        }

        private void UpdateDoctorButton_Click(object sender, EventArgs e)
        {

            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            string doctorID = DoctorList["doctorID"].ToString();
            DoctorUpdateError.Text = doctorID;
            string firstName = DoctorList["firstName"].ToString();
            string lastName = DoctorList["lastName"].ToString();
            string departmentName = DoctorList["departmentName"].ToString();
            string specialization = DoctorList["specialization"].ToString();
            string duties = DoctorList["duties"].ToString();
            
            if (DoctorUpdFirstName.TextLength.Equals(0) || DoctorUpdLastName.TextLength.Equals(0)
               || DoctorUpdDeptBox.SelectedIndex == -1 || DoctorUpdSpec.TextLength.Equals(0) ||
               DoctorUpdDuty.TextLength.Equals(0))
            {
                DoctorUpdateError.Text = "Cannot have empty fields";
                DoctorUpdateError.ForeColor = Color.Red;
            }
            else
            {
                //Update database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                var sql = "UPDATE [Doctor] SET firstName=@firstName, lastName=@lastName, departmentName=@department, "
                    + "specialization=@spec, duties =@duty where doctorID=@doctorID";// repeat for all variables

                SqlCommand updateDoctor = new SqlCommand(sql, conn);
                updateDoctor.Parameters.AddWithValue("@firstName", DoctorUpdFirstName.Text);
                updateDoctor.Parameters.AddWithValue("@lastName", DoctorUpdLastName.Text);
                updateDoctor.Parameters.AddWithValue("@department", DoctorUpdDeptBox.SelectedIndex.ToString());
                updateDoctor.Parameters.AddWithValue("@spec", DoctorUpdSpec.Text);
                updateDoctor.Parameters.AddWithValue("@duty", DoctorUpdDuty.Text);
                updateDoctor.Parameters.AddWithValue("@doctorID", Int32.Parse(doctorID));
                updateDoctor.ExecuteNonQuery();
                conn.Close();
                //Update status and reset fields
                DoctorErrorLabel.Text = "Doctor updated successfully";
                DoctorErrorLabel.ForeColor = Color.Green;
                //resetDoctorAddFields();
                populateDoctorList();
            }
        }

        private void AddDoctorButton_Click(object sender, EventArgs e)
        {

            if (DoctorFirstNameText.TextLength.Equals(0) || DoctorLastNameText.TextLength.Equals(0)
                || DoctorDeptBox.SelectedIndex == -1 || DoctorDutyText.TextLength.Equals(0) ||
                DoctorSpecText.TextLength.Equals(0)) {
                DoctorErrorLabel.Text = "Cannot have empty fields";
                DoctorErrorLabel.ForeColor = Color.Red;
            }
            else
            {
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand addDoctor = new SqlCommand("insert into [Doctor] (firstName, lastName, departmentName, specialization, duties)" +
                    "values (@firstName, @lastName, @department, @spec, @duty)", conn);
                addDoctor.Parameters.AddWithValue("@firstName", DoctorFirstNameText.Text);
                addDoctor.Parameters.AddWithValue("@lastName", DoctorLastNameText.Text);
                addDoctor.Parameters.AddWithValue("@department", DoctorDeptBox.SelectedIndex.ToString());
                addDoctor.Parameters.AddWithValue("@spec", DoctorSpecText.Text);
                addDoctor.Parameters.AddWithValue("@duty", DoctorDutyText.Text);
                addDoctor.ExecuteNonQuery();
                conn.Close();
                //Update status and reset fields
                DoctorErrorLabel.Text = "Doctor added successfully";
                DoctorErrorLabel.ForeColor = Color.Green;
                resetDoctorAddFields();
                populateDoctorList();
            }
        }

        private void resetDoctorAddFields()
        {
            DoctorFirstNameText.Text = "";
            DoctorLastNameText.Text = "";
            DoctorDeptBox.SelectedIndex.Equals(0);
            DoctorDutyText.Text = "";
            DoctorSpecText.Text = "";
        }

        private void UpdateDoctorReset_Click(object sender, EventArgs e)
        {
            DoctorUpdFirstName.Text = "";
            DoctorUpdLastName.Text = "";
            DoctorUpdDeptBox.SelectedIndex.Equals(0);
            DoctorUpdSpec.Text = "";
            DoctorUpdDuty.Text = "";
        }

        private void DoctorListRefresh_Click(object sender, EventArgs e)
        {
            populateDoctorList();
        }

        private void DeleteDoctorButton_Click(object sender, EventArgs e)
        {
            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            int doctorID = Int32.Parse(DoctorList["doctorID"].ToString());

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand addWard = new SqlCommand("delete from [Doctor] where doctorID=@doctorID", conn);
            addWard.Parameters.AddWithValue("@doctorID", doctorID);
            addWard.ExecuteNonQuery();
            conn.Close();
            //Update status and reset fields
            populateDoctorList();
        }
    }
}