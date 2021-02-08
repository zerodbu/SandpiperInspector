namespace SandpiperInspector
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnAuthenticate = new System.Windows.Forms.Button();
            this.textBoxServerBaseURL = new System.Windows.Forms.TextBox();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSettings = new System.Windows.Forms.TabPage();
            this.checkBoxTranscript = new System.Windows.Forms.CheckBox();
            this.lblResetSchema = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxPlanSchema = new System.Windows.Forms.TextBox();
            this.checkBoxAutotest = new System.Windows.Forms.CheckBox();
            this.lblLocalCacheDir = new System.Windows.Forms.Label();
            this.buttonSelectCacheDir = new System.Windows.Forms.Button();
            this.tabPageHistory = new System.Windows.Forms.TabPage();
            this.textBoxHistory = new System.Windows.Forms.TextBox();
            this.tabPageLocalConntent = new System.Windows.Forms.TabPage();
            this.buttonDeleteLocalSlice = new System.Windows.Forms.Button();
            this.buttonEditLocalSlice = new System.Windows.Forms.Button();
            this.buttonNewLocalSlice = new System.Windows.Forms.Button();
            this.treeViewLocalContent = new System.Windows.Forms.TreeView();
            this.tabPageTranscript = new System.Windows.Forms.TabPage();
            this.textBoxTranscript = new System.Windows.Forms.TextBox();
            this.tabPageRemoteContent = new System.Windows.Forms.TabPage();
            this.buttonNewRemoteSlice = new System.Windows.Forms.Button();
            this.buttonUploadGrain = new System.Windows.Forms.Button();
            this.buttonGetSubscribedSlices = new System.Windows.Forms.Button();
            this.treeViewRemoteContent = new System.Windows.Forms.TreeView();
            this.buttonDownloadGrain = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.textBoxPlandocument = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            this.timerHousekeeping = new System.Windows.Forms.Timer(this.components);
            this.buttonValidatePlan = new System.Windows.Forms.Button();
            this.groupBoxRole = new System.Windows.Forms.GroupBox();
            this.radioButtonRoleSecondary = new System.Windows.Forms.RadioButton();
            this.radioButtonRolePrimary = new System.Windows.Forms.RadioButton();
            this.lblNodeID = new System.Windows.Forms.Label();
            this.textBoxNodeID = new System.Windows.Forms.TextBox();
            this.buttonSync = new System.Windows.Forms.Button();
            this.timerLocalFilesIndexer = new System.Windows.Forms.Timer(this.components);
            this.timerTransscriptRefresh = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPageSettings.SuspendLayout();
            this.tabPageHistory.SuspendLayout();
            this.tabPageLocalConntent.SuspendLayout();
            this.tabPageTranscript.SuspendLayout();
            this.tabPageRemoteContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            this.groupBoxRole.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnAuthenticate
            // 
            this.btnAuthenticate.Location = new System.Drawing.Point(441, 140);
            this.btnAuthenticate.Name = "btnAuthenticate";
            this.btnAuthenticate.Size = new System.Drawing.Size(105, 35);
            this.btnAuthenticate.TabIndex = 0;
            this.btnAuthenticate.Text = "Authenticate";
            this.btnAuthenticate.UseVisualStyleBackColor = true;
            this.btnAuthenticate.Click += new System.EventHandler(this.btnAuthenticate_Click);
            // 
            // textBoxServerBaseURL
            // 
            this.textBoxServerBaseURL.Location = new System.Drawing.Point(136, 12);
            this.textBoxServerBaseURL.Name = "textBoxServerBaseURL";
            this.textBoxServerBaseURL.Size = new System.Drawing.Size(383, 22);
            this.textBoxServerBaseURL.TabIndex = 1;
            this.textBoxServerBaseURL.Leave += new System.EventHandler(this.textBoxServerBaseURL_Leave);
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(136, 40);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(156, 22);
            this.textBoxUsername.TabIndex = 2;
            this.textBoxUsername.Leave += new System.EventHandler(this.textBoxUsername_Leave);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(390, 40);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(156, 22);
            this.textBoxPassword.TabIndex = 3;
            this.textBoxPassword.Leave += new System.EventHandler(this.textBoxPassword_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Server Base UrL";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(315, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Password";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageSettings);
            this.tabControl1.Controls.Add(this.tabPageHistory);
            this.tabControl1.Controls.Add(this.tabPageLocalConntent);
            this.tabControl1.Controls.Add(this.tabPageTranscript);
            this.tabControl1.Controls.Add(this.tabPageRemoteContent);
            this.tabControl1.Location = new System.Drawing.Point(12, 209);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(534, 230);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPageSettings
            // 
            this.tabPageSettings.Controls.Add(this.checkBoxTranscript);
            this.tabPageSettings.Controls.Add(this.lblResetSchema);
            this.tabPageSettings.Controls.Add(this.label5);
            this.tabPageSettings.Controls.Add(this.textBoxPlanSchema);
            this.tabPageSettings.Controls.Add(this.checkBoxAutotest);
            this.tabPageSettings.Controls.Add(this.lblLocalCacheDir);
            this.tabPageSettings.Controls.Add(this.buttonSelectCacheDir);
            this.tabPageSettings.Location = new System.Drawing.Point(4, 25);
            this.tabPageSettings.Name = "tabPageSettings";
            this.tabPageSettings.Size = new System.Drawing.Size(526, 201);
            this.tabPageSettings.TabIndex = 3;
            this.tabPageSettings.Text = "Settings";
            this.tabPageSettings.UseVisualStyleBackColor = true;
            // 
            // checkBoxTranscript
            // 
            this.checkBoxTranscript.AutoSize = true;
            this.checkBoxTranscript.Location = new System.Drawing.Point(289, 46);
            this.checkBoxTranscript.Name = "checkBoxTranscript";
            this.checkBoxTranscript.Size = new System.Drawing.Size(144, 21);
            this.checkBoxTranscript.TabIndex = 18;
            this.checkBoxTranscript.Text = "Display Transcript";
            this.checkBoxTranscript.UseVisualStyleBackColor = true;
            this.checkBoxTranscript.CheckedChanged += new System.EventHandler(this.checkBoxTranscript_CheckedChanged);
            // 
            // lblResetSchema
            // 
            this.lblResetSchema.AutoSize = true;
            this.lblResetSchema.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblResetSchema.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblResetSchema.Location = new System.Drawing.Point(208, 78);
            this.lblResetSchema.Name = "lblResetSchema";
            this.lblResetSchema.Size = new System.Drawing.Size(115, 17);
            this.lblResetSchema.TabIndex = 17;
            this.lblResetSchema.Text = "Reset To Default";
            this.lblResetSchema.Click += new System.EventHandler(this.lblResetSchema_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(185, 17);
            this.label5.TabIndex = 16;
            this.label5.Text = "Plandocument Schema XSD";
            // 
            // textBoxPlanSchema
            // 
            this.textBoxPlanSchema.Location = new System.Drawing.Point(13, 98);
            this.textBoxPlanSchema.Multiline = true;
            this.textBoxPlanSchema.Name = "textBoxPlanSchema";
            this.textBoxPlanSchema.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxPlanSchema.Size = new System.Drawing.Size(500, 22);
            this.textBoxPlanSchema.TabIndex = 15;
            this.textBoxPlanSchema.WordWrap = false;
            this.textBoxPlanSchema.Leave += new System.EventHandler(this.textBoxPlanSchema_Leave);
            // 
            // checkBoxAutotest
            // 
            this.checkBoxAutotest.AutoSize = true;
            this.checkBoxAutotest.Checked = true;
            this.checkBoxAutotest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutotest.Location = new System.Drawing.Point(13, 46);
            this.checkBoxAutotest.Name = "checkBoxAutotest";
            this.checkBoxAutotest.Size = new System.Drawing.Size(250, 21);
            this.checkBoxAutotest.TabIndex = 14;
            this.checkBoxAutotest.Text = "Get available content list after auth";
            this.checkBoxAutotest.UseVisualStyleBackColor = true;
            // 
            // lblLocalCacheDir
            // 
            this.lblLocalCacheDir.AutoSize = true;
            this.lblLocalCacheDir.Location = new System.Drawing.Point(129, 18);
            this.lblLocalCacheDir.Name = "lblLocalCacheDir";
            this.lblLocalCacheDir.Size = new System.Drawing.Size(46, 17);
            this.lblLocalCacheDir.TabIndex = 1;
            this.lblLocalCacheDir.Text = "label5";
            // 
            // buttonSelectCacheDir
            // 
            this.buttonSelectCacheDir.Location = new System.Drawing.Point(13, 13);
            this.buttonSelectCacheDir.Name = "buttonSelectCacheDir";
            this.buttonSelectCacheDir.Size = new System.Drawing.Size(110, 27);
            this.buttonSelectCacheDir.TabIndex = 0;
            this.buttonSelectCacheDir.Text = "Local Cache";
            this.buttonSelectCacheDir.UseVisualStyleBackColor = true;
            this.buttonSelectCacheDir.Click += new System.EventHandler(this.buttonSelectCacheDir_Click);
            // 
            // tabPageHistory
            // 
            this.tabPageHistory.Controls.Add(this.textBoxHistory);
            this.tabPageHistory.Location = new System.Drawing.Point(4, 25);
            this.tabPageHistory.Name = "tabPageHistory";
            this.tabPageHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHistory.Size = new System.Drawing.Size(526, 201);
            this.tabPageHistory.TabIndex = 0;
            this.tabPageHistory.Text = "History";
            this.tabPageHistory.UseVisualStyleBackColor = true;
            // 
            // textBoxHistory
            // 
            this.textBoxHistory.Location = new System.Drawing.Point(6, 6);
            this.textBoxHistory.Multiline = true;
            this.textBoxHistory.Name = "textBoxHistory";
            this.textBoxHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxHistory.Size = new System.Drawing.Size(514, 186);
            this.textBoxHistory.TabIndex = 0;
            this.textBoxHistory.WordWrap = false;
            // 
            // tabPageLocalConntent
            // 
            this.tabPageLocalConntent.Controls.Add(this.buttonDeleteLocalSlice);
            this.tabPageLocalConntent.Controls.Add(this.buttonEditLocalSlice);
            this.tabPageLocalConntent.Controls.Add(this.buttonNewLocalSlice);
            this.tabPageLocalConntent.Controls.Add(this.treeViewLocalContent);
            this.tabPageLocalConntent.Location = new System.Drawing.Point(4, 25);
            this.tabPageLocalConntent.Name = "tabPageLocalConntent";
            this.tabPageLocalConntent.Size = new System.Drawing.Size(526, 201);
            this.tabPageLocalConntent.TabIndex = 4;
            this.tabPageLocalConntent.Text = "Local Content";
            this.tabPageLocalConntent.UseVisualStyleBackColor = true;
            // 
            // buttonDeleteLocalSlice
            // 
            this.buttonDeleteLocalSlice.Enabled = false;
            this.buttonDeleteLocalSlice.Location = new System.Drawing.Point(175, 168);
            this.buttonDeleteLocalSlice.Name = "buttonDeleteLocalSlice";
            this.buttonDeleteLocalSlice.Size = new System.Drawing.Size(101, 30);
            this.buttonDeleteLocalSlice.TabIndex = 3;
            this.buttonDeleteLocalSlice.Text = "Delete Slice";
            this.buttonDeleteLocalSlice.UseVisualStyleBackColor = true;
            this.buttonDeleteLocalSlice.Click += new System.EventHandler(this.buttonDeleteLocalSlice_Click);
            // 
            // buttonEditLocalSlice
            // 
            this.buttonEditLocalSlice.Enabled = false;
            this.buttonEditLocalSlice.Location = new System.Drawing.Point(425, 168);
            this.buttonEditLocalSlice.Name = "buttonEditLocalSlice";
            this.buttonEditLocalSlice.Size = new System.Drawing.Size(98, 30);
            this.buttonEditLocalSlice.TabIndex = 2;
            this.buttonEditLocalSlice.Text = "Edit Slice";
            this.buttonEditLocalSlice.UseVisualStyleBackColor = true;
            this.buttonEditLocalSlice.Click += new System.EventHandler(this.buttonEditLocalSlice_Click);
            // 
            // buttonNewLocalSlice
            // 
            this.buttonNewLocalSlice.Location = new System.Drawing.Point(302, 168);
            this.buttonNewLocalSlice.Name = "buttonNewLocalSlice";
            this.buttonNewLocalSlice.Size = new System.Drawing.Size(94, 30);
            this.buttonNewLocalSlice.TabIndex = 1;
            this.buttonNewLocalSlice.Text = "New Slice";
            this.buttonNewLocalSlice.UseVisualStyleBackColor = true;
            this.buttonNewLocalSlice.Click += new System.EventHandler(this.buttonNewLocalSlice_Click);
            // 
            // treeViewLocalContent
            // 
            this.treeViewLocalContent.Location = new System.Drawing.Point(3, 3);
            this.treeViewLocalContent.Name = "treeViewLocalContent";
            this.treeViewLocalContent.Size = new System.Drawing.Size(520, 159);
            this.treeViewLocalContent.TabIndex = 0;
            this.treeViewLocalContent.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewLocalContent_AfterSelect);
            this.treeViewLocalContent.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeViewLocalContent_KeyUp);
            // 
            // tabPageTranscript
            // 
            this.tabPageTranscript.Controls.Add(this.textBoxTranscript);
            this.tabPageTranscript.Location = new System.Drawing.Point(4, 25);
            this.tabPageTranscript.Name = "tabPageTranscript";
            this.tabPageTranscript.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTranscript.Size = new System.Drawing.Size(526, 201);
            this.tabPageTranscript.TabIndex = 1;
            this.tabPageTranscript.Text = "Transcript";
            this.tabPageTranscript.UseVisualStyleBackColor = true;
            // 
            // textBoxTranscript
            // 
            this.textBoxTranscript.Location = new System.Drawing.Point(6, 6);
            this.textBoxTranscript.Multiline = true;
            this.textBoxTranscript.Name = "textBoxTranscript";
            this.textBoxTranscript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxTranscript.Size = new System.Drawing.Size(514, 189);
            this.textBoxTranscript.TabIndex = 0;
            this.textBoxTranscript.WordWrap = false;
            // 
            // tabPageRemoteContent
            // 
            this.tabPageRemoteContent.Controls.Add(this.buttonNewRemoteSlice);
            this.tabPageRemoteContent.Controls.Add(this.buttonUploadGrain);
            this.tabPageRemoteContent.Controls.Add(this.buttonGetSubscribedSlices);
            this.tabPageRemoteContent.Controls.Add(this.treeViewRemoteContent);
            this.tabPageRemoteContent.Controls.Add(this.buttonDownloadGrain);
            this.tabPageRemoteContent.Location = new System.Drawing.Point(4, 25);
            this.tabPageRemoteContent.Name = "tabPageRemoteContent";
            this.tabPageRemoteContent.Size = new System.Drawing.Size(526, 201);
            this.tabPageRemoteContent.TabIndex = 2;
            this.tabPageRemoteContent.Text = "Remote Content";
            this.tabPageRemoteContent.UseVisualStyleBackColor = true;
            // 
            // buttonNewRemoteSlice
            // 
            this.buttonNewRemoteSlice.Location = new System.Drawing.Point(23, 170);
            this.buttonNewRemoteSlice.Name = "buttonNewRemoteSlice";
            this.buttonNewRemoteSlice.Size = new System.Drawing.Size(91, 28);
            this.buttonNewRemoteSlice.TabIndex = 6;
            this.buttonNewRemoteSlice.Text = "New Slice";
            this.buttonNewRemoteSlice.UseVisualStyleBackColor = true;
            this.buttonNewRemoteSlice.Click += new System.EventHandler(this.buttonNewSlice_Click);
            // 
            // buttonUploadGrain
            // 
            this.buttonUploadGrain.Location = new System.Drawing.Point(343, 170);
            this.buttonUploadGrain.Name = "buttonUploadGrain";
            this.buttonUploadGrain.Size = new System.Drawing.Size(76, 28);
            this.buttonUploadGrain.TabIndex = 4;
            this.buttonUploadGrain.Text = "Upload";
            this.buttonUploadGrain.UseVisualStyleBackColor = true;
            this.buttonUploadGrain.Click += new System.EventHandler(this.buttonUploadGrain_Click);
            // 
            // buttonGetSubscribedSlices
            // 
            this.buttonGetSubscribedSlices.Location = new System.Drawing.Point(267, 170);
            this.buttonGetSubscribedSlices.Name = "buttonGetSubscribedSlices";
            this.buttonGetSubscribedSlices.Size = new System.Drawing.Size(70, 28);
            this.buttonGetSubscribedSlices.TabIndex = 3;
            this.buttonGetSubscribedSlices.Text = "Refresh";
            this.buttonGetSubscribedSlices.UseVisualStyleBackColor = true;
            this.buttonGetSubscribedSlices.Click += new System.EventHandler(this.buttonGetSubscribedSlices_Click);
            // 
            // treeViewRemoteContent
            // 
            this.treeViewRemoteContent.Location = new System.Drawing.Point(3, 3);
            this.treeViewRemoteContent.Name = "treeViewRemoteContent";
            this.treeViewRemoteContent.Size = new System.Drawing.Size(520, 161);
            this.treeViewRemoteContent.TabIndex = 2;
            this.treeViewRemoteContent.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewSubscriptions_AfterSelect);
            this.treeViewRemoteContent.Click += new System.EventHandler(this.treeViewSubscriptions_Click);
            this.treeViewRemoteContent.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeViewRemoteContent_KeyUp);
            // 
            // buttonDownloadGrain
            // 
            this.buttonDownloadGrain.Location = new System.Drawing.Point(425, 170);
            this.buttonDownloadGrain.Name = "buttonDownloadGrain";
            this.buttonDownloadGrain.Size = new System.Drawing.Size(98, 28);
            this.buttonDownloadGrain.TabIndex = 1;
            this.buttonDownloadGrain.Text = "Download";
            this.buttonDownloadGrain.UseVisualStyleBackColor = true;
            this.buttonDownloadGrain.Click += new System.EventHandler(this.buttonDownloadGrain_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 178);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(20, 17);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "...";
            // 
            // textBoxPlandocument
            // 
            this.textBoxPlandocument.Location = new System.Drawing.Point(136, 68);
            this.textBoxPlandocument.Multiline = true;
            this.textBoxPlandocument.Name = "textBoxPlandocument";
            this.textBoxPlandocument.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxPlandocument.Size = new System.Drawing.Size(410, 66);
            this.textBoxPlandocument.TabIndex = 10;
            this.textBoxPlandocument.WordWrap = false;
            this.textBoxPlandocument.Leave += new System.EventHandler(this.textBoxPlandocument_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "Plan Document";
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.BackColor = System.Drawing.Color.Lime;
            this.pictureBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxStatus.Location = new System.Drawing.Point(525, 12);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(21, 22);
            this.pictureBoxStatus.TabIndex = 12;
            this.pictureBoxStatus.TabStop = false;
            // 
            // timerHousekeeping
            // 
            this.timerHousekeeping.Enabled = true;
            this.timerHousekeeping.Interval = 10;
            this.timerHousekeeping.Tick += new System.EventHandler(this.timerHousekeeping_Tick);
            // 
            // buttonValidatePlan
            // 
            this.buttonValidatePlan.Location = new System.Drawing.Point(325, 140);
            this.buttonValidatePlan.Name = "buttonValidatePlan";
            this.buttonValidatePlan.Size = new System.Drawing.Size(110, 35);
            this.buttonValidatePlan.TabIndex = 13;
            this.buttonValidatePlan.Text = "Validate Plan";
            this.buttonValidatePlan.UseVisualStyleBackColor = true;
            this.buttonValidatePlan.Click += new System.EventHandler(this.buttonValidatePlan_Click);
            // 
            // groupBoxRole
            // 
            this.groupBoxRole.Controls.Add(this.radioButtonRoleSecondary);
            this.groupBoxRole.Controls.Add(this.radioButtonRolePrimary);
            this.groupBoxRole.Location = new System.Drawing.Point(9, 102);
            this.groupBoxRole.Name = "groupBoxRole";
            this.groupBoxRole.Size = new System.Drawing.Size(121, 73);
            this.groupBoxRole.TabIndex = 15;
            this.groupBoxRole.TabStop = false;
            this.groupBoxRole.Text = "Local Role";
            // 
            // radioButtonRoleSecondary
            // 
            this.radioButtonRoleSecondary.AutoSize = true;
            this.radioButtonRoleSecondary.Location = new System.Drawing.Point(5, 48);
            this.radioButtonRoleSecondary.Name = "radioButtonRoleSecondary";
            this.radioButtonRoleSecondary.Size = new System.Drawing.Size(97, 21);
            this.radioButtonRoleSecondary.TabIndex = 1;
            this.radioButtonRoleSecondary.TabStop = true;
            this.radioButtonRoleSecondary.Text = "Secondary";
            this.radioButtonRoleSecondary.UseVisualStyleBackColor = true;
            this.radioButtonRoleSecondary.CheckedChanged += new System.EventHandler(this.radioButtonRoleSecondary_CheckedChanged);
            // 
            // radioButtonRolePrimary
            // 
            this.radioButtonRolePrimary.AutoSize = true;
            this.radioButtonRolePrimary.Location = new System.Drawing.Point(6, 21);
            this.radioButtonRolePrimary.Name = "radioButtonRolePrimary";
            this.radioButtonRolePrimary.Size = new System.Drawing.Size(72, 21);
            this.radioButtonRolePrimary.TabIndex = 0;
            this.radioButtonRolePrimary.TabStop = true;
            this.radioButtonRolePrimary.Text = "Pimary";
            this.radioButtonRolePrimary.UseVisualStyleBackColor = true;
            this.radioButtonRolePrimary.CheckedChanged += new System.EventHandler(this.radioButtonRolePrimary_CheckedChanged);
            // 
            // lblNodeID
            // 
            this.lblNodeID.AutoSize = true;
            this.lblNodeID.Location = new System.Drawing.Point(136, 137);
            this.lblNodeID.Name = "lblNodeID";
            this.lblNodeID.Size = new System.Drawing.Size(97, 17);
            this.lblNodeID.TabIndex = 16;
            this.lblNodeID.Text = "Local Node ID";
            // 
            // textBoxNodeID
            // 
            this.textBoxNodeID.Enabled = false;
            this.textBoxNodeID.Location = new System.Drawing.Point(136, 153);
            this.textBoxNodeID.Name = "textBoxNodeID";
            this.textBoxNodeID.Size = new System.Drawing.Size(299, 22);
            this.textBoxNodeID.TabIndex = 17;
            this.textBoxNodeID.Leave += new System.EventHandler(this.textBoxNodeID_Leave);
            // 
            // buttonSync
            // 
            this.buttonSync.Enabled = false;
            this.buttonSync.Location = new System.Drawing.Point(441, 181);
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.Size = new System.Drawing.Size(105, 33);
            this.buttonSync.TabIndex = 18;
            this.buttonSync.Text = "Sync";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // timerLocalFilesIndexer
            // 
            this.timerLocalFilesIndexer.Enabled = true;
            this.timerLocalFilesIndexer.Interval = 1000;
            this.timerLocalFilesIndexer.Tick += new System.EventHandler(this.timerLocalFilesIndexer_Tick);
            // 
            // timerTransscriptRefresh
            // 
            this.timerTransscriptRefresh.Enabled = true;
            this.timerTransscriptRefresh.Interval = 300;
            this.timerTransscriptRefresh.Tick += new System.EventHandler(this.timerTransscriptRefresh_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(703, 676);
            this.Controls.Add(this.buttonSync);
            this.Controls.Add(this.textBoxNodeID);
            this.Controls.Add(this.lblNodeID);
            this.Controls.Add(this.groupBoxRole);
            this.Controls.Add(this.buttonValidatePlan);
            this.Controls.Add(this.pictureBoxStatus);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxPlandocument);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.textBoxServerBaseURL);
            this.Controls.Add(this.btnAuthenticate);
            this.MinimumSize = new System.Drawing.Size(700, 550);
            this.Name = "Form1";
            this.Text = "Sandpiper Inspector";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.tabControl1.ResumeLayout(false);
            this.tabPageSettings.ResumeLayout(false);
            this.tabPageSettings.PerformLayout();
            this.tabPageHistory.ResumeLayout(false);
            this.tabPageHistory.PerformLayout();
            this.tabPageLocalConntent.ResumeLayout(false);
            this.tabPageTranscript.ResumeLayout(false);
            this.tabPageTranscript.PerformLayout();
            this.tabPageRemoteContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            this.groupBoxRole.ResumeLayout(false);
            this.groupBoxRole.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAuthenticate;
        private System.Windows.Forms.TextBox textBoxServerBaseURL;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageHistory;
        private System.Windows.Forms.TabPage tabPageTranscript;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox textBoxHistory;
        private System.Windows.Forms.TextBox textBoxTranscript;
        private System.Windows.Forms.TextBox textBoxPlandocument;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabPageRemoteContent;
        private System.Windows.Forms.TreeView treeViewRemoteContent;
        private System.Windows.Forms.Button buttonDownloadGrain;
        private System.Windows.Forms.Button buttonGetSubscribedSlices;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
        private System.Windows.Forms.Timer timerHousekeeping;
        private System.Windows.Forms.TabPage tabPageSettings;
        private System.Windows.Forms.Label lblLocalCacheDir;
        private System.Windows.Forms.Button buttonSelectCacheDir;
        private System.Windows.Forms.CheckBox checkBoxAutotest;
        private System.Windows.Forms.Button buttonUploadGrain;
        private System.Windows.Forms.Button buttonValidatePlan;
        private System.Windows.Forms.TextBox textBoxPlanSchema;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBoxRole;
        private System.Windows.Forms.RadioButton radioButtonRoleSecondary;
        private System.Windows.Forms.RadioButton radioButtonRolePrimary;
        private System.Windows.Forms.Label lblNodeID;
        private System.Windows.Forms.TextBox textBoxNodeID;
        private System.Windows.Forms.Label lblResetSchema;
        private System.Windows.Forms.TabPage tabPageLocalConntent;
        private System.Windows.Forms.TreeView treeViewLocalContent;
        private System.Windows.Forms.Button buttonNewLocalSlice;
        private System.Windows.Forms.Button buttonSync;
        private System.Windows.Forms.Button buttonNewRemoteSlice;
        private System.Windows.Forms.Button buttonEditLocalSlice;
        private System.Windows.Forms.Button buttonDeleteLocalSlice;
        private System.Windows.Forms.Timer timerLocalFilesIndexer;
        private System.Windows.Forms.Timer timerTransscriptRefresh;
        private System.Windows.Forms.CheckBox checkBoxTranscript;
    }
}

