using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

/*
 * to-do list 
 *      include the grain filename on the slice selector popup when a new grain is discovered
 *      thwart the human's ability to drop in grains if the node is a secondary
 *      
 */




namespace SandpiperInspector
{
    public partial class Form1 : Form
    {
        sandpiperClient sandpiper = new sandpiperClient();

        private TabPage hiddenJWTtab = new TabPage();
        private TabPage hiddenRemoteContentTab = new TabPage();
        private TabPage hiddenLocalContentTab = new TabPage();

        private bool localFilesIndexInProgress;
        private bool localContentTreeIsUpToDate;
        private bool useTimerBasedCachedHousekeeping;
        private bool handlingFwatcherChange;


        FileSystemWatcher fwatcher = new FileSystemWatcher();
        


        public Form1()
        {
            InitializeComponent();
            fwatcher.SynchronizingObject = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sandpiper.defaultPlandocumentSchema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:vc=\"http://www.w3.org/2007/XMLSchema-versioning\" vc:minVersion=\"1.1\" elementFormDefault=\"qualified\">\r\n\t<!-- Basic types -->\r\n\t<xs:simpleType name=\"uuid\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:length value=\"36\" fixed=\"true\"/>\r\n\t\t\t<xs:pattern value=\"[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<xs:simpleType name=\"String_Medium\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:maxLength value=\"255\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<xs:simpleType name=\"String_Short\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:maxLength value=\"40\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<xs:simpleType name=\"Email\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:maxLength value=\"255\"/>\r\n\t\t\t<xs:pattern value=\"[^\\s]+@[^\\s]+\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<xs:simpleType name=\"FieldName\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:minLength value=\"1\"/>\r\n\t\t\t<xs:maxLength value=\"63\"/>\r\n\t\t\t<xs:pattern value=\"[A-Za-z][A-Za-z0-9_\\-]+\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<xs:simpleType name=\"FieldValue\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:minLength value=\"1\"/>\r\n\t\t\t<xs:maxLength value=\"255\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<xs:simpleType name=\"Levels\">\r\n\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t<xs:enumeration value=\"1-1\"/>\r\n\t\t\t<xs:enumeration value=\"1-2\"/>\r\n\t\t\t<xs:enumeration value=\"2\"/>\r\n\t\t\t<xs:enumeration value=\"3\"/>\r\n\t\t</xs:restriction>\r\n\t</xs:simpleType>\r\n\t<!-- Attribute templates used in multiple places -->\r\n\t<xs:attributeGroup name=\"Model\">\r\n\t\t<xs:attribute name=\"uuid\" type=\"uuid\" use=\"required\"/>\r\n\t</xs:attributeGroup>\r\n\t<xs:attributeGroup name=\"Description_Main\">\r\n\t\t<xs:attribute name=\"description\" type=\"String_Medium\" use=\"required\"/>\r\n\t</xs:attributeGroup>\r\n\t<xs:attributeGroup name=\"Description_Optional\">\r\n\t\t<xs:attribute name=\"description\" type=\"String_Medium\" use=\"optional\"/>\r\n\t</xs:attributeGroup>\r\n\t<!-- Element templates used in multiple places -->\r\n\t<xs:complexType name=\"LinkGroup\">\r\n\t\t<xs:sequence>\r\n\t\t\t<xs:element name=\"UniqueLink\" minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t<xs:attribute name=\"keyfield\" type=\"FieldName\" use=\"required\"/>\r\n\t\t\t\t\t<xs:attribute name=\"keyvalue\" type=\"FieldValue\" use=\"required\"/>\r\n\t\t\t\t\t<xs:attributeGroup ref=\"Description_Optional\"/>\r\n\t\t\t\t</xs:complexType>\r\n\t\t\t</xs:element>\r\n\t\t\t<xs:element name=\"MultiLink\" minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t<xs:element name=\"MultLinkEntry\" minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t\t\t\t<xs:attribute name=\"keyvalue\" type=\"FieldValue\" use=\"required\"/>\r\n\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Description_Optional\"/>\r\n\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t<xs:attribute name=\"keyfield\" type=\"FieldName\" use=\"required\"/>\r\n\t\t\t\t</xs:complexType>\r\n\t\t\t</xs:element>\r\n\t\t</xs:sequence>\r\n\t</xs:complexType>\r\n\t<xs:complexType name=\"Instance\">\r\n\t\t<xs:sequence>\r\n\t\t\t<xs:element name=\"Software\" minOccurs=\"1\" maxOccurs=\"1\">\r\n\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t<xs:attributeGroup ref=\"Description_Main\"/>\r\n\t\t\t\t\t<xs:attribute name=\"version\" type=\"String_Short\" use=\"required\"/>\r\n\t\t\t\t</xs:complexType>\r\n\t\t\t</xs:element>\r\n\t\t\t<xs:element name=\"Capability\" minOccurs=\"1\" maxOccurs=\"1\">\r\n\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t<!-- If a server is available, it is listed here -->\r\n\t\t\t\t\t\t<xs:element name=\"Response\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t<xs:attribute name=\"uri\" type=\"xs:string\" use=\"required\"/>\r\n\t\t\t\t\t\t\t\t<xs:attribute name=\"role\">\r\n\t\t\t\t\t\t\t\t\t<xs:simpleType>\r\n\t\t\t\t\t\t\t\t\t\t<xs:restriction base=\"xs:string\">\r\n\t\t\t\t\t\t\t\t\t\t\t<xs:enumeration value=\"Synchronization\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t<xs:enumeration value=\"Authentication\"/>\r\n\t\t\t\t\t\t\t\t\t\t</xs:restriction>\r\n\t\t\t\t\t\t\t\t\t</xs:simpleType>\r\n\t\t\t\t\t\t\t\t</xs:attribute>\r\n\t\t\t\t\t\t\t\t<xs:attribute name=\"description\" type=\"String_Medium\" use=\"optional\"/>\r\n\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t<xs:attribute name=\"level\" type=\"Levels\"/>\r\n\t\t\t\t</xs:complexType>\r\n\t\t\t</xs:element>\r\n\t\t</xs:sequence>\r\n\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t</xs:complexType>\r\n\t<!-- Main schema -->\r\n\t<xs:element name=\"Plan\">\r\n\t\t<xs:complexType>\r\n\t\t\t<xs:sequence>\r\n\t\t\t\t<xs:element name=\"Primary\" minOccurs=\"1\" maxOccurs=\"1\">\r\n\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t<xs:element name=\"Instance\" type=\"Instance\" minOccurs=\"1\" maxOccurs=\"1\"/>\r\n\t\t\t\t\t\t\t<xs:element name=\"Controller\" minOccurs=\"1\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Admin\">\r\n\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:attribute name=\"contact\" type=\"String_Medium\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:attribute name=\"email\" type=\"Email\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Description_Main\"/>\r\n\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t<xs:element name=\"Links\" type=\"LinkGroup\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t<xs:unique name=\"PrimaryInstanceLinkUniqueKeyField\">\r\n\t\t\t\t\t\t\t\t\t<xs:selector xpath=\"MultiLink|UniqueLink\"/>\r\n\t\t\t\t\t\t\t\t\t<xs:field xpath=\"@keyfield\"/>\r\n\t\t\t\t\t\t\t\t</xs:unique>\r\n\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t<xs:element name=\"Pools\" maxOccurs=\"1\" minOccurs=\"0\">\r\n\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Pool\" minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Links\" type=\"LinkGroup\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:unique name=\"PrimaryPoolLinkUniqueKeyField\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:selector xpath=\"MultiLink|UniqueLink\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:field xpath=\"@keyfield\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:unique>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Slices\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Slice\" minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Links\" type=\"LinkGroup\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:unique name=\"SliceLinkUniqueKeyField\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:selector xpath=\"MultiLink|UniqueLink\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:field xpath=\"@keyfield\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:unique>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Description_Main\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Description_Main\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t</xs:element>\r\n\t\t\t\t<xs:element name=\"Communal\" minOccurs=\"1\" maxOccurs=\"1\">\r\n\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t<xs:element name=\"Subscriptions\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t<xs:element name=\"Subscription\" minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<!-- Not part of Sandpiper 1.0 - future use -->\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:element name=\"DeliveryProfiles\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:element name=\"DeliveryProfile\" minOccurs=\"1\" maxOccurs=\"unbounded\">\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t\t<xs:attribute name=\"sliceuuid\" type=\"uuid\"/>\r\n\t\t\t\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t</xs:element>\r\n\t\t\t\t<xs:element name=\"Secondary\" minOccurs=\"1\" maxOccurs=\"1\">\r\n\t\t\t\t\t<xs:complexType>\r\n\t\t\t\t\t\t<xs:sequence>\r\n\t\t\t\t\t\t\t<xs:element name=\"Instance\" type=\"Instance\" minOccurs=\"1\" maxOccurs=\"1\"/>\r\n\t\t\t\t\t\t\t<xs:element name=\"Links\" type=\"LinkGroup\" minOccurs=\"0\" maxOccurs=\"1\">\r\n\t\t\t\t\t\t\t\t<xs:unique name=\"SecondaryInstanceLinkUniqueKeyField\">\r\n\t\t\t\t\t\t\t\t\t<xs:selector xpath=\"MultiLink|UniqueLink\"/>\r\n\t\t\t\t\t\t\t\t\t<xs:field xpath=\"@keyfield\"/>\r\n\t\t\t\t\t\t\t\t</xs:unique>\r\n\t\t\t\t\t\t\t</xs:element>\r\n\t\t\t\t\t\t</xs:sequence>\r\n\t\t\t\t\t\t<xs:attributeGroup ref=\"Model\"/>\r\n\t\t\t\t\t</xs:complexType>\r\n\t\t\t\t</xs:element>\r\n\t\t\t</xs:sequence>\r\n\t\t\t<xs:attribute name=\"uuid\" type=\"uuid\"/>\r\n\t\t</xs:complexType>\r\n\t</xs:element>\r\n</xs:schema>";

            useTimerBasedCachedHousekeeping = false;

            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("SandpiperInspector");
            key = key.OpenSubKey("SandpiperInspector", true);

            if (key.GetValue("username") != null) { textBoxUsername.Text = key.GetValue("username").ToString(); }
            if (key.GetValue("password") != null) { textBoxPassword.Text = key.GetValue("password").ToString(); }
            if (key.GetValue("plandocument") != null) { textBoxPlandocument.Text = key.GetValue("plandocument").ToString(); }

            if (key.GetValue("plandocumentschema") != null)
            {// registry entry already exists for plandoc schema - use it to fill the UI text box
                sandpiper.plandocumentSchema = key.GetValue("plandocumentschema").ToString();
                textBoxPlanSchema.Text = sandpiper.plandocumentSchema;
            }
            else
            {// registry key not set - write the default and put it in the UI text box 
                key.SetValue("plandocumentschema", sandpiper.defaultPlandocumentSchema);
                sandpiper.plandocumentSchema = sandpiper.defaultPlandocumentSchema;
                textBoxPlanSchema.Text = sandpiper.defaultPlandocumentSchema;
            }


            if (key.GetValue("role") != null)
            {// registry key exists for role
                if (Convert.ToInt32(key.GetValue("role")) == 0)
                {// registry key states that i'm primary
                    radioButtonRolePrimary.Checked = true;
                }
                else
                {// registry key states that i'm secondary
                    radioButtonRoleSecondary.Checked = true;
                }
            }
            else
            {
                radioButtonRoleSecondary.Checked = true;
            }





            if (key.GetValue("baseurl") != null) { textBoxServerBaseURL.Text = key.GetValue("baseurl").ToString(); }
            lblLocalCacheDir.Text = "";
            if (key.GetValue("cacheDirectoryPath") != null && Directory.Exists(key.GetValue("cacheDirectoryPath").ToString())) { lblLocalCacheDir.Text = key.GetValue("cacheDirectoryPath").ToString(); }



            sandpiper.activeSession = false;
            sandpiper.interactionState = (int)sandpiperClient.interactionStates.IDLE;
            pictureBoxStatus.BackColor = Color.Gray;


            // hiddenHistoryTab = tabControl1.TabPages[1];
            hiddenJWTtab = tabControl1.TabPages[3];
            hiddenRemoteContentTab = tabControl1.TabPages[4];

            tabControl1.TabPages.RemoveByKey("tabPageJWT"); // hide the JWT tab
            tabControl1.TabPages.RemoveByKey("tabPageRemoteContent"); // hide the subscribedSlices tab



            tabControl1.SelectedIndex = 1;  // show the history tab
            resizeControls();


            if (validatePlandocumentForUI())
            {// plan document validates - set the radio buttons and nodeid according to the plan
                updateRoleDependantUIelements();
            }


            if (lblLocalCacheDir.Text == "")
            {
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show("Cache directory must be set. Go to the settings tab.", "Cache directory not set", buttons);
            }
            else
            {
                if (Directory.Exists(lblLocalCacheDir.Text))
                {
                    readCacheIndex(lblLocalCacheDir.Text);
                    indexLocalFiles(lblLocalCacheDir.Text);
                    updateLocalContentTree();


                    fwatcher.Path = lblLocalCacheDir.Text;
                    fwatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
                    fwatcher.EnableRaisingEvents = true;
                    fwatcher.Created += new FileSystemEventHandler(cacheFolderChange);
                    fwatcher.Deleted += new FileSystemEventHandler(cacheFolderChange);
                }
            }



        }






        private async void btnAuthenticate_Click(object sender, EventArgs e)
        {

            lblStatus.Text = "Authenticating...";
            sandpiper.interactionState = (int)sandpiperClient.interactionStates.AUTHENTICATING;
            pictureBoxStatus.BackColor = Color.Blue;
            sandpiper.responseTime = 0;

            textBoxServerBaseURL.Enabled = false;
            groupBoxRole.Enabled = false;
            textBoxPassword.Enabled = false;
            textBoxUsername.Enabled = false;
            textBoxPlandocument.Enabled = false;

            bool loginSuccess = await sandpiper.loginAsync(textBoxServerBaseURL.Text + "/login", textBoxUsername.Text, textBoxPassword.Text, textBoxPlandocument.Text);

            if (loginSuccess)
            {
                treeViewRemoteContent.Nodes.Clear();
            }
            else
            {// failed login 
                textBoxServerBaseURL.Enabled = true;
                groupBoxRole.Enabled = true;
                textBoxPassword.Enabled = true;
                textBoxUsername.Enabled = true;
                textBoxPlandocument.Enabled = true;
            }
        }


        // timer-based (1 second interval) local files watcher  
        private void timerLocalFilesIndexer_Tick(object sender, EventArgs e)
        {
            if (useTimerBasedCachedHousekeeping)
            {
                timerLocalFilesIndexer.Enabled = false;

                readCacheIndex(lblLocalCacheDir.Text);

                indexLocalFiles(lblLocalCacheDir.Text);
                if (!localContentTreeIsUpToDate)
                {// local file was added to the index - we need to flag the local content tree for refresh
                    updateLocalContentTree();
                }

                timerLocalFilesIndexer.Enabled = true;
            }
        }






        private async void timerHousekeeping_Tick(object sender, EventArgs e)
        {

            sandpiper.tenMilisecondCounter++;


            switch (sandpiper.interactionState)
            {
                case (int)sandpiperClient.interactionStates.IDLE:


                    break;

                case (int)sandpiperClient.interactionStates.AUTHENTICATING:

                    sandpiper.responseTime++;
                    lblStatus.Text = "Authenticating (Elapsed Time: " + (10 * sandpiper.responseTime).ToString() + "mS)";
                    break;


                case (int)sandpiperClient.interactionStates.AUTHFAILED_UPDATING_UI:

                    sandpiper.activeSession = false;
                    pictureBoxStatus.BackColor = Color.Red;
                    lblStatus.Text = "";
                    tabControl1.TabPages.RemoveByKey("tabPageJWT");
                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.AUTHFAILED;
                    break;

                case (int)sandpiperClient.interactionStates.AUTHFAILED:


                    if (sandpiper.tenMilisecondCounter > 200)
                    {
                        pictureBoxStatus.BackColor = Color.Gray;
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.IDLE;
                    }

                    break;

                case (int)sandpiperClient.interactionStates.AUTHENTICATED_UPDATING_UI:

                    sandpiper.activeSession = true;
                    pictureBoxStatus.BackColor = Color.Lime;
                    lblStatus.Text = "";
                    textBoxJWT.Text = sandpiper.sessionJTW.niceFormat;

                    if (!tabControl1.TabPages.ContainsKey("tabPageJWT"))
                    {
                        tabControl1.TabPages.Add(hiddenJWTtab);
                    }

                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.AUTHENTICATED;

                    break;


                case (int)sandpiperClient.interactionStates.AUTHENTICATED:

                    buttonSync.Enabled = true;

                    if (checkBoxAutotest.Checked)
                    {
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.GETTINGSLICES;
                    }
                    else
                    {
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.IDLE;
                    }

                    break;


                case (int)sandpiperClient.interactionStates.GETTINGSLICES:

                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.GETTINGSLICES_AWAITING;
                    sandpiper.historyRecords.Add("Getting list of available slices using /v1/slices API route");
                    sandpiper.awaitingServerResponse = true;
                    sandpiper.responseTime = 0;
                    sandpiper.availableSlices = await sandpiper.getSlicesAsync(textBoxServerBaseURL.Text + "/v1/slices", sandpiper.sessionJTW);
                    sandpiper.awaitingServerResponse = false;
                    sandpiper.historyRecords.Add("    Received list of " + sandpiper.availableSlices.Count() + " slices (" + (10 * sandpiper.responseTime).ToString() + " mS response time)");
                    break;


                case (int)sandpiperClient.interactionStates.GETTINGSLICES_AWAITING:

                    if (sandpiper.awaitingServerResponse)
                    {// awaiting return of getSlicesAsync call
                        sandpiper.responseTime++;
                        lblStatus.Text = "Getting slice list (Elapsed Time: " + (10 * sandpiper.responseTime).ToString() + "mS)";
                    }
                    else
                    { // previous call to get slices is no longer awaiting - advance state and make next call

                        sandpiper.historyRecords.Add("Gettting list of available grains List using /grains API route");
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.GETTINGGRAINS_AWAITING;
                        sandpiper.awaitingServerResponse = true;
                        sandpiper.responseTime = 0;
                        sandpiper.availableGrains = await sandpiper.getGrainsAsync(textBoxServerBaseURL.Text + "/v1/grains", sandpiper.sessionJTW);
                        sandpiper.awaitingServerResponse = false;
                        sandpiper.historyRecords.Add("    Received list of " + sandpiper.availableGrains.Count() + " grains without payloads (" + (10 * sandpiper.responseTime).ToString() + " mS response time)");
                    }
                    break;


                case (int)sandpiperClient.interactionStates.GETTINGGRAINS_AWAITING:

                    if (sandpiper.awaitingServerResponse)
                    {// do nothing - maybe add timeout checking here
                        sandpiper.responseTime++;
                        lblStatus.Text = "Getting grain list (Elapsed Time: " + (10 * sandpiper.responseTime).ToString() + "mS)";
                    }
                    else
                    {
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.IDLE;
                        unlockUIelemets();
                        lblStatus.Text = "";
                        //sandpiper.historyRecords.Add("Updating available Content tree view");
                        updateRemoteContentTree(sandpiper.availableGrains, sandpiper.availableSlices);
                    }

                    break;

                case (int)sandpiperClient.interactionStates.DOWNLOADINGGRAIN:

                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.DOWNLOADINGGRAIN_AWAITING;
                                        
                    List<sandpiperClient.grain> grains = new List<sandpiperClient.grain>();
                    grains = await sandpiper.getGrainsAsync(textBoxServerBaseURL.Text + "/v1/grains/" + sandpiper.grainsToTransfer.First().id + "?payload=yes", sandpiper.sessionJTW);
                    if (grains.Count() == 1 && grains[0].id == sandpiper.grainsToTransfer.First().id)
                    {
                        sandpiper.writeFilegrainToFile(grains[0], lblLocalCacheDir.Text);

                        sandpiper.historyRecords.Add("Retrieved grain " + grains[0].id + " to local file " + grains[0].source + " (" + grains[0].payload_len.ToString() + " bytes in " + (10*sandpiper.responseTime).ToString() + "mS)");
                        sandpiper.grainsToTransfer.RemoveAt(0);
                        if (sandpiper.grainsToTransfer.Count() > 0)
                        {// the shopping list contains more grains
                            sandpiper.responseTime = 0;
                            sandpiper.interactionState = (int)sandpiperClient.interactionStates.DOWNLOADINGGRAIN;
                        }
                        else
                        {// we have downloaded all grains in the "get" list 
                            sandpiper.awaitingServerResponse = false;
                            lblStatus.Text = "";
                            sandpiper.interactionState = (int)sandpiperClient.interactionStates.IDLE;
                            unlockUIelemets();
                            updateLocalContentTree();
                        }
                    }
                    else
                    {
                        sandpiper.historyRecords.Add("server response to a specific grain id request was not the single specific grains requested");
                    }
                    sandpiper.awaitingServerResponse = false;

                    break;

                case (int)sandpiperClient.interactionStates.DOWNLOADINGGRAIN_AWAITING:

                    sandpiper.responseTime++;
                    lblStatus.Text = "Downloading grain (Elapsed Time: " + (10 * sandpiper.responseTime).ToString() + "mS)  "+ sandpiper.grainsToTransfer.Count().ToString() + " grains remaining";
                    break;



                case (int)sandpiperClient.interactionStates.UPLOADINGGRAIN:

                    if (sandpiper.grainsToTransfer.Count() > 0)
                    {//ccc
                        byte[] fileBytes = File.ReadAllBytes(lblLocalCacheDir.Text + "\\" + sandpiper.grainsToTransfer[0].source);
                        sandpiper.selectedGrain.id = sandpiper.grainsToTransfer[0].id;
                        sandpiper.selectedGrain.encoding = "z64";
                        sandpiper.selectedGrain.grain_key = "Level-1";
                        sandpiper.selectedGrain.source = Path.GetFileName(sandpiper.grainsToTransfer[0].source);
                        sandpiper.selectedGrain.slice_id = sandpiper.grainsToTransfer[0].slice_id;
                        sandpiper.selectedGrain.description = "";
                        sandpiper.responseTime = 0;
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.UPLOADINGGRAIN_AWAITING;
                        await sandpiper.postGrainAsync(textBoxServerBaseURL.Text + "/v1/grains", sandpiper.sessionJTW, sandpiper.selectedGrain, sandpiper.z64(fileBytes));
                        sandpiper.grainsToTransfer.RemoveAt(0);
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.UPLOADINGGRAIN;
                    }
                    else
                    { // no more grains to upload
                        lblStatus.Text = "";
                        sandpiper.interactionState = (int)sandpiperClient.interactionStates.IDLE;
                        unlockUIelemets();
                    }

                    break;


                case (int)sandpiperClient.interactionStates.UPLOADINGGRAIN_AWAITING:

                    sandpiper.responseTime++;
                    lblStatus.Text = "Uploading grain " + sandpiper.selectedGrain.source + " (Elapsed Time: " + (10 * sandpiper.responseTime).ToString() + "mS)";
                    break;


                default: break;

            }


            // update the history display
            if (sandpiper.historyRecords.Count > sandpiper.historyRecordCountTemp)
            {
                int historyViewLength = textBoxHistory.Height / 14;
                List<string> stringsList = new List<string>();

                for (int i = Math.Max(0, sandpiper.historyRecords.Count - historyViewLength); i < sandpiper.historyRecords.Count; ++i)
                {
                    stringsList.Add(sandpiper.historyRecords[i]);
                }
                textBoxHistory.Text = string.Join("\r\n", stringsList);
                sandpiper.historyRecordCountTemp = sandpiper.historyRecords.Count();
            }



        }

        private void textBoxUsername_Leave(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("SandpiperInspector");
            key = key.OpenSubKey("SandpiperInspector", true);
            key.SetValue("username", textBoxUsername.Text);
        }

        private void textBoxPassword_Leave(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("SandpiperInspector");
            key = key.OpenSubKey("SandpiperInspector", true);
            key.SetValue("password", textBoxPassword.Text);
        }

        private void textBoxPlandocument_Leave(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("SandpiperInspector");
            key = key.OpenSubKey("SandpiperInspector", true);
            key.SetValue("plandocument", textBoxPlandocument.Text);
        }

        private void textBoxServerBaseURL_Leave(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("SandpiperInspector");
            key = key.OpenSubKey("SandpiperInspector", true);
            key.SetValue("baseurl", textBoxServerBaseURL.Text);
        }

        private void textBoxNodeID_Leave(object sender, EventArgs e)
        {
        }


        private void textBoxPlanSchema_Leave(object sender, EventArgs e)
        {
            consumePlandocumentSchema();
        }

        private bool consumePlandocumentSchema()
        {
            if (sandpiper.validPlandocumentSchema(textBoxPlanSchema.Text))
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key.CreateSubKey("SandpiperInspector");
                key = key.OpenSubKey("SandpiperInspector", true);
                key.SetValue("plandocumentschema", textBoxPlanSchema.Text);
                textBoxPlanSchema.BackColor = Color.White;
                sandpiper.plandocumentSchema = textBoxPlanSchema.Text;
                return true;
            }
            else
            {// something about the plandoc schema was no good
                textBoxPlanSchema.BackColor = Color.OrangeRed;
                return false;
            }
        }



        private void radioButtonRolePrimary_CheckedChanged(object sender, EventArgs e)
        {
            updateRoleDependantUIelements();
        }

        private void updateRoleDependantUIelements()
        {
            sandpiper.getNodeIDsFromPlan(textBoxPlandocument.Text);

            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key.CreateSubKey("SandpiperInspector");
            key = key.OpenSubKey("SandpiperInspector", true);

            if (radioButtonRolePrimary.Checked == true)
            {// role changed to primary
                key.SetValue("role", 0);
                sandpiper.myRole = 0;
                textBoxNodeID.Text = sandpiper.primaryNodeID;
            }
            else
            {// role changed to secondary 
                key.SetValue("role", 1);
                sandpiper.myRole = 1;
                textBoxNodeID.Text = sandpiper.secondaryNodeID;
            }


        }


        private void radioButtonRoleSecondary_CheckedChanged(object sender, EventArgs e)
        {

        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            resizeControls();
        }

        private void resizeControls()
        {
            pictureBoxStatus.Left = this.Width - 40;
            textBoxServerBaseURL.Width = this.Width - 150;
            textBoxPlandocument.Width = this.Width - 126;
            textBoxPassword.Width = this.Width - 316;
            btnAuthenticate.Left = this.Width - 102;
            buttonSync.Left = this.Width - 102;

            textBoxPlandocument.Height = this.Height / 4;


            lblNodeID.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 3;
            textBoxNodeID.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 16;

            buttonValidatePlan.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 10;
            buttonValidatePlan.Left = this.Width - 190;
            btnAuthenticate.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 10;
            buttonSync.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 40;

            tabControl1.Width = this.Width - 30;

            tabControl1.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 70;

            tabControl1.Height = this.Height - tabControl1.Top - 48;

            lblStatus.Top = textBoxPlandocument.Top + textBoxPlandocument.Height + 50;


            textBoxHistory.Width = tabControl1.Width - 18;
            textBoxHistory.Height = tabControl1.Height - 32;
            textBoxJWT.Width = tabControl1.Width - 18;
            textBoxJWT.Height = tabControl1.Height - 32;



            treeViewLocalContent.Width = tabControl1.Width - 18;
            treeViewLocalContent.Height = tabControl1.Height - 65;

            treeViewRemoteContent.Width = tabControl1.Width - 18;
            treeViewRemoteContent.Height = tabControl1.Height - 65;




            buttonNewRemoteSlice.Left = tabControl1.Width - 385;
            buttonNewRemoteSlice.Top = treeViewRemoteContent.Bottom + 8;
            buttonGetSubscribedSlices.Left = tabControl1.Width - 210;
            buttonGetSubscribedSlices.Top = treeViewRemoteContent.Bottom + 8;
            buttonUploadGrain.Left = tabControl1.Width - 154;
            buttonUploadGrain.Top = treeViewRemoteContent.Bottom + 8;
            buttonDownloadGrain.Left = tabControl1.Width - 90;
            buttonDownloadGrain.Top = treeViewRemoteContent.Bottom + 8;



            buttonDeleteLocalSlice.Left = tabControl1.Width - 250;
            buttonDeleteLocalSlice.Top = treeViewRemoteContent.Bottom + 8;


            buttonNewLocalSlice.Left = tabControl1.Width - 170;
            buttonNewLocalSlice.Top = treeViewRemoteContent.Bottom + 8;

            buttonEditLocalSlice.Left = tabControl1.Width - 90;
            buttonEditLocalSlice.Top = treeViewRemoteContent.Bottom + 8;




            textBoxPlanSchema.Height = tabControl1.Height - 114;
            textBoxPlanSchema.Width = tabControl1.Width - 31;
        }

        private void treeViewSubscriptions_Click(object sender, EventArgs e)
        {
        }

        private void treeViewSubscriptions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonDownloadGrain.Enabled = false;
            buttonUploadGrain.Enabled = false;
            sandpiper.selectedGrain.clear();
            sandpiper.selectedSlice.clear();

            if (e.Node.Name.Contains("grain_"))
            {
                string[] chunks = e.Node.Name.Split('_');
                if (sandpiper.looksLikeAUUID(chunks[1]))
                {
                    sandpiper.selectedGrain.id = chunks[1];
                    buttonDownloadGrain.Enabled = true;
                }
            }

            if (e.Node.Name.Contains("slice_"))
            {
                string[] chunks = e.Node.Name.Split('_');
                if (sandpiper.looksLikeAUUID(chunks[1]))
                {
                    sandpiper.selectedSlice.slice_id = chunks[1];
                    buttonUploadGrain.Enabled = true;
                }
            }
        }

        private void buttonGetSubscribedSlices_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Getting content list...";
            sandpiper.responseTime = 0;
            treeViewRemoteContent.Nodes.Clear();
            if (sandpiper.activeSession)
            {
                sandpiper.interactionState = (int)sandpiperClient.interactionStates.GETTINGSLICES;
            }
        }

        private void buttonSelectCacheDir_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key.CreateSubKey("SandpiperInspector");
                key = key.OpenSubKey("SandpiperInspector", true);
                if (key.GetValue("cacheDirectoryPath") != null) { fbd.SelectedPath = key.GetValue("cacheDirectoryPath").ToString(); }
                DialogResult dialogResult = fbd.ShowDialog();
                if (dialogResult == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    key.SetValue("cacheDirectoryPath", fbd.SelectedPath);
                    lblLocalCacheDir.Text = fbd.SelectedPath;
                }
            }

        }


        private void updateLocalContentTree()
        {
            treeViewLocalContent.Nodes.Clear();

            Dictionary<string, int> grainCounts = new Dictionary<string, int>();
            foreach (sandpiperClient.grain g in sandpiper.localGrainsCache)
            {
                if (grainCounts.ContainsKey(g.slice_id))
                {
                    grainCounts[g.slice_id]++;
                }
                else
                {
                    grainCounts.Add(g.slice_id, 1);
                }
            }


            foreach (sandpiperClient.slice s in sandpiper.localSlices)
            {
                treeViewLocalContent.Nodes.Add("slice_" + s.slice_id, s.name);
                treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes.Add("slice_id", "Slice ID: " + s.slice_id);
                treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes.Add("slice_type", "Type: " + s.slice_type);
                treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes.Add("slice_metadata", "Metadata: " + s.slicemetadata);

                int grainCount; grainCounts.TryGetValue(s.slice_id, out grainCount);

                treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes.Add("grains", "Grains (" + grainCount.ToString() + ")");


                foreach (sandpiperClient.grain g in sandpiper.localGrainsCache)
                {
                    if (g.slice_id != s.slice_id) { continue; }

                    treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes.Add("grain_" + g.id, g.source);
                    treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("id", "ID: " + g.id);
                    treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("encoding", "Encoding: " + g.encoding);
                    treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("grain_key", "Grain Key: " + g.grain_key);
                    treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("source", "Source: " + g.source);
                    treeViewLocalContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("payload_len", "Payload Length: " + g.payload_len.ToString());
                }
            }

            localContentTreeIsUpToDate = true;
            buttonDeleteLocalSlice.Enabled = false;
            buttonEditLocalSlice.Enabled = false;

        }

        private void updateRemoteContentTree(List<sandpiperClient.grain> grains, List<sandpiperClient.slice> slices)
        {
          //  sandpiper.readCacheIndex(lblLocalCacheDir.Text); // read in the local cache grains list
            //localGrainsCache now holds the list of grains already present
            sandpiper.grainsToTransfer.Clear();
            sandpiper.grainsToDrop.Clear();


            if (sandpiper.myRole == 0)
            {// i am primary

                //build a list of local grains that I need to upload (because they are missing from the remote system)
                for (int i = 0; i <= sandpiper.localGrainsCache.Count() - 1; i++)
                {
                    bool found = false;
                    foreach (sandpiperClient.grain remoteGrain in grains)
                    {
                        if (sandpiper.localGrainsCache[i].id == remoteGrain.id)
                        {
                            found = true; break;
                        }
                    }
                    if (!found) { sandpiper.grainsToTransfer.Add(sandpiper.localGrainsCache[i]); }
                }

                if (sandpiper.grainsToTransfer.Count() == 0)
                {
                    sandpiper.historyRecords.Add("No grains need to be uploaded to remote secondary");
                }
                else
                {
                    sandpiper.historyRecords.Add(sandpiper.grainsToTransfer.Count().ToString() + " grains need to be uploaded to remote secondary");
                }

                // build a list of remote grains to drop (because they do not exist in local cache)
                for (int i = 0; i <= grains.Count() - 1; i++)
                {
                    bool found = false;
                    foreach (sandpiperClient.grain localGrain in sandpiper.localGrainsCache)
                    {
                        if (grains[i].id == localGrain.id)
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        sandpiper.grainsToDrop.Add(grains[i]);
                    }
                }

                if (sandpiper.grainsToDrop.Count() == 0)
                {
                    sandpiper.historyRecords.Add("No grains need to be dropped from remote secondary");
                }
                else
                {
                    sandpiper.historyRecords.Add(sandpiper.grainsToDrop.Count().ToString() + " grains need to be dropped from remote secondary");
                }

            }
            else
            { // i am secondary

                //build a list of grains that i need to download
                for (int i = 0; i <= grains.Count() - 1; i++)
                {
                    bool found = false;
                    foreach (sandpiperClient.grain localGrain in sandpiper.localGrainsCache)
                    {
                        if (grains[i].id == localGrain.id)
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        sandpiper.grainsToTransfer.Add(grains[i]);
                    }
                }


                // build a list of local grains to drop
                for (int i = 0; i <= sandpiper.localGrainsCache.Count() - 1; i++)
                {
                    bool found = false;
                    foreach (sandpiperClient.grain remoteGrain in grains)
                    {
                        if (sandpiper.localGrainsCache[i].id == remoteGrain.id)
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        sandpiper.grainsToDrop.Add(sandpiper.localGrainsCache[i]);
                    }
                }
            }

            // render the remote data into the content tree in any case (primary or secondary)
            // organize remote grains into slices to populate tree
            for (int i = 0; i <= slices.Count() - 1; i++)
            {
                slices[i].grains = new List<sandpiperClient.grain>();
                bool grainsConsumed = true;
                while (grainsConsumed)
                {
                    grainsConsumed = false;

                    for (int j = 0; j <= grains.Count() - 1; j++)
                    {
                        if (slices[i].slice_id == grains[j].slice_id)
                        {
                            slices[i].grains.Add(grains[j]);
                            grains.RemoveAt(j);
                            grainsConsumed = true;
                            break;
                        }
                    }
                }
                // no grains were consumed into slices on this last attempt
            }

            if (!tabControl1.TabPages.ContainsKey("tabPageRemoteContent")) { tabControl1.TabPages.Add(hiddenRemoteContentTab); }
            foreach (sandpiperClient.slice s in slices)
            {
                treeViewRemoteContent.Nodes.Add("slice_" + s.slice_id, s.name);
                treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes.Add("slice_id", "Slice ID: " + s.slice_id);
                treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes.Add("slice_type", "Type: " + s.slice_type);
                treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes.Add("slice_metadata", "Metadata: " + s.slicemetadata);
                treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes.Add("grains", "Grains (" + s.grains.Count.ToString() + ")");

                foreach (sandpiperClient.grain g in s.grains)
                {
                    treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes.Add("grain_" + g.id, g.source);
                    treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("id", "ID: " + g.id);
                    treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("encoding", "Encoding: " + g.encoding);
                    treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("grain_key", "Grain Key: " + g.grain_key);
                    treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("source", "Source: " + g.source);
                    treeViewRemoteContent.Nodes["slice_" + s.slice_id].Nodes["grains"].Nodes["grain_" + g.id].Nodes.Add("payload_len", "Payload Length: " + g.payload_len.ToString());
                }
            }

            // if any grains remain, they are orphaned (referencing a non-existent sliceid)
            if (grains.Count > 0)
            {
                treeViewRemoteContent.Nodes.Add("orphans", "Orphan Grains");
                treeViewRemoteContent.Nodes["orphans"].BackColor = Color.OrangeRed;
                foreach (sandpiperClient.grain g in grains)
                {
                    treeViewRemoteContent.Nodes["orphans"].Nodes.Add("grain_" + g.id, g.id);
                    treeViewRemoteContent.Nodes["orphans"].Nodes["grain_" + g.id].Nodes.Add("sliceid", "Sliceid: " + g.slice_id);
                    treeViewRemoteContent.Nodes["orphans"].Nodes["grain_" + g.id].Nodes.Add("encoding", "Encoding: " + g.encoding);
                    treeViewRemoteContent.Nodes["orphans"].Nodes["grain_" + g.id].Nodes.Add("grain_key", "Grain Key: " + g.grain_key);
                    treeViewRemoteContent.Nodes["orphans"].Nodes["grain_" + g.id].Nodes.Add("source", "Source: " + g.source);
                    treeViewRemoteContent.Nodes["orphans"].Nodes["grain_" + g.id].Nodes.Add("payload_len", "Pyload Length: " + g.payload_len.ToString());
                    sandpiper.historyRecords.Add("Orphan grain:" + g.id);
                }
            }
        }


        private void buttonDownloadGrain_Click(object sender, EventArgs e)
        {
            if (sandpiper.looksLikeAUUID(sandpiper.selectedGrain.id))
            {
                sandpiper.grainsToTransfer.Add(sandpiper.selectedGrain);
                sandpiper.awaitingServerResponse = true;
                sandpiper.responseTime = 0;
                sandpiper.interactionState = (int)sandpiperClient.interactionStates.DOWNLOADINGGRAIN;
            }
        }

        private async void buttonUploadGrain_Click(object sender, EventArgs e)
        {

            using (var openFileDialog = new OpenFileDialog())
            {

                DialogResult openFileResult = openFileDialog.ShowDialog();

                if (openFileResult.ToString() == "OK")
                {
                    byte[] fileBytes = File.ReadAllBytes(openFileDialog.FileName);

                    sandpiper.selectedGrain.id = Guid.NewGuid().ToString("D");
                    sandpiper.selectedGrain.encoding = "z64";
                    sandpiper.selectedGrain.source = Path.GetFileName(openFileDialog.FileName);
                    sandpiper.selectedGrain.slice_id = sandpiper.selectedSlice.slice_id;
                    sandpiper.selectedGrain.description = "";

                    sandpiper.awaitingServerResponse = true;
                    sandpiper.responseTime = 0;
                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.UPLOADINGGRAIN;

                    await sandpiper.postGrainAsync(textBoxServerBaseURL.Text + "/v1/grains", sandpiper.sessionJTW, sandpiper.selectedGrain, sandpiper.z64(fileBytes));

                    sandpiper.awaitingServerResponse = false;
                }
            }
        }

        private async void treeViewRemoteContent_KeyUp(object sender, KeyEventArgs e)
        {
            bool deleteResult = true;

            if (e.KeyCode == Keys.Delete)
            {
                if (sandpiper.looksLikeAUUID(sandpiper.selectedGrain.id))
                {
                    deleteResult = await sandpiper.deleteGrainAsync(textBoxServerBaseURL.Text + "/v1/grains/" + sandpiper.selectedGrain.id, sandpiper.sessionJTW);
                    if (deleteResult)
                    {
                        treeViewRemoteContent.SelectedNode.Remove();
                    }
                }
            }
        }



        private void buttonValidatePlan_Click(object sender, EventArgs e)
        {
            validatePlandocumentForUI();
        }

        private bool validatePlandocumentForUI()
        {
            if (sandpiper.validPlandocument(textBoxPlandocument.Text))
            {
                textBoxPlandocument.BackColor = Color.White;
                sandpiper.getNodeIDsFromPlan(textBoxPlandocument.Text);
                return true;
            }
            else
            {
                textBoxPlandocument.BackColor = Color.OrangeRed;
                return false;
            }
        }



        private void buttonUpdateLocal_Click(object sender, EventArgs e)
        {
        }

        private async void buttonNewSlice_Click(object sender, EventArgs e)
        {
            //https://stackoverflow.com/questions/1665533/communicate-between-two-windows-forms-in-c-sharp

            using (FormNewSlice f = new FormNewSlice())
            {
                f.createButtonClick += (mysender, mye) =>
                {

                    sandpiper.selectedSlice.name = f.sliceDescription;
                    sandpiper.selectedSlice.slice_type = f.sliceType;
                    sandpiper.selectedSlice.slicemetadata = f.sliceMetadata;
                    sandpiper.selectedSlice.slice_id = Guid.NewGuid().ToString("D");
                };
                f.ShowDialog(this);

                if (f.DialogResult == DialogResult.OK)
                {

                    bool success = await sandpiper.postSliceAsync(textBoxServerBaseURL.Text + "/v1/slices", sandpiper.sessionJTW, sandpiper.selectedSlice);
                    if (success)
                    {
                        sandpiper.historyRecords.Add("creating new slice (" + sandpiper.selectedSlice.slice_id + ") named:" + sandpiper.selectedSlice.name);
                    }
                    else
                    {
                        sandpiper.historyRecords.Add("failed to create new slice");
                    }
                }

            }

        }

        private void lblResetSchema_Click(object sender, EventArgs e)
        {
            string message = "Do you want to revert back to the default plandocument schema XSD?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, "Reset schema to default", buttons);
            if (result == DialogResult.Yes)
            {// reload schema text box, and update registry
                textBoxPlanSchema.Text = sandpiper.defaultPlandocumentSchema;
                consumePlandocumentSchema();
            }
        }

        private void buttonNewLocalSlice_Click(object sender, EventArgs e)
        {
            using (FormNewSlice f = new FormNewSlice())
            {
                f.createButtonClick += (mysender, mye) =>
                {

                    sandpiper.selectedSlice.name = f.sliceDescription;
                    sandpiper.selectedSlice.slice_type = f.sliceType;
                    sandpiper.selectedSlice.slicemetadata = f.sliceMetadata;
                    sandpiper.selectedSlice.slice_id = Guid.NewGuid().ToString("D");
                };
                f.ShowDialog(this);

                if (f.DialogResult == DialogResult.OK)
                {
                    sandpiperClient.slice newSlice = new sandpiperClient.slice();
                    newSlice.slice_id = sandpiper.selectedSlice.slice_id;
                    newSlice.slice_type = sandpiper.selectedSlice.slice_type;
                    newSlice.name = sandpiper.selectedSlice.name;
                    newSlice.slicemetadata = sandpiper.selectedSlice.slicemetadata;
                    sandpiper.localSlices.Add(newSlice);
                    sandpiper.writeFullCacheIndex(lblLocalCacheDir.Text);
                    sandpiper.historyRecords.Add("New local slice (" + newSlice.slice_id + ") created");
                    updateLocalContentTree();
                }
            }
        }

        private async void buttonSync_Click(object sender, EventArgs e)
        {
            //lockUIelemets();
//            sandpiper.readCacheIndex(lblLocalCacheDir.Text);  // read in (and verify file existance) for good measure - in case something has been deleted or addded on the local cache directory in the past few moments by an outside person or processs
//            indexLocalFiles(lblLocalCacheDir.Text);



            if (sandpiper.myRole == 0)
            {// my local client is primary in the the plan
             // every file in the loacal cache directory needs to be in agreement with the remote - even if local cache index is not aware of the files


                if (sandpiper.grainsToTransfer.Count() > 0)
                {
                    sandpiper.historyRecords.Add("Uploading " + sandpiper.grainsToTransfer.Count().ToString() + " grains");
                    sandpiper.awaitingServerResponse = true;
                    sandpiper.responseTime = 0;
                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.UPLOADINGGRAIN;
                }
                else
                {
                    sandpiper.historyRecords.Add("Remote pool contains all grains in the local cache. Nothing to upload");
                }



                if (sandpiper.grainsToDrop.Count() > 0)
                {
                    sandpiper.awaitingServerResponse = true;

                    foreach (sandpiperClient.grain g in sandpiper.grainsToDrop)
                    {
                        sandpiper.historyRecords.Add("Deleting grain ("+ g.id + ") from remote pool");
                        await sandpiper.deleteGrainAsync(textBoxServerBaseURL.Text + "/v1/grains/" + g.id, sandpiper.sessionJTW);
                    }

                    sandpiper.awaitingServerResponse = false;
                }
                else
                {
                    sandpiper.historyRecords.Add("Remote pool contains no extraneous grains. Nothing to drop");
                }







            }
            else
            {// my local client is secondary in the the plan


                // drop any rogue slices that exist locally (drop grains that claim them also)
                sandpiper.dropRogueLocalSlices(sandpiper.availableSlices, lblLocalCacheDir.Text);

                // add slices from the server's list that are not present locally
                sandpiper.addMissingLocalSlices(sandpiper.availableSlices, lblLocalCacheDir.Text);





                if (sandpiper.grainsToTransfer.Count() > 0)
                {
                    sandpiper.historyRecords.Add("Downloading " + sandpiper.grainsToTransfer.Count().ToString() + " grains");
                    sandpiper.awaitingServerResponse = true;
                    sandpiper.responseTime = 0;
                    sandpiper.interactionState = (int)sandpiperClient.interactionStates.DOWNLOADINGGRAIN;
                }
                else
                {
                    sandpiper.historyRecords.Add("Local grains list contains all grains reported by server. Nothing to download");
                }

                if (sandpiper.grainsToDrop.Count() > 0)
                {
                    foreach (sandpiperClient.grain g in sandpiper.grainsToDrop)
                    {
                        sandpiper.deleteLocalGrain(g, lblLocalCacheDir.Text);
                    }
                    sandpiper.grainsToDrop.Clear();
                }
                else
                {
                    sandpiper.historyRecords.Add("All local grains exist in list reported by the server. Nothing to delete from the local cache");
                }

                updateLocalContentTree();
            }

        }


        private void lockUIelemets()
        {
            buttonSync.Enabled = false;
            btnAuthenticate.Enabled = false;
            buttonValidatePlan.Enabled = false;
            buttonNewLocalSlice.Enabled = false;
            buttonNewRemoteSlice.Enabled = false;
            buttonDownloadGrain.Enabled = false;
            buttonUploadGrain.Enabled = false;
            buttonGetSubscribedSlices.Enabled = false;
        }

        private void unlockUIelemets()
        {
            buttonSync.Enabled = true;
            btnAuthenticate.Enabled = true;
            buttonValidatePlan.Enabled = true;
            buttonNewLocalSlice.Enabled = true;
            buttonNewRemoteSlice.Enabled = true;
            buttonDownloadGrain.Enabled = true;
            buttonUploadGrain.Enabled = true;
            buttonGetSubscribedSlices.Enabled = true;
        }

        private void treeViewLocalContent_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonEditLocalSlice.Enabled = false;

            if (e.Node.Name.Contains("slice_"))
            {
                string[] chunks = e.Node.Name.Split('_');
                if (sandpiper.looksLikeAUUID(chunks[1]))
                {
                    sandpiper.selectedSlice.slice_id = chunks[1];

                    foreach (sandpiperClient.slice s in sandpiper.localSlices)
                    {
                        if (s.slice_id == sandpiper.selectedSlice.slice_id)
                        {
                            sandpiper.selectedSlice.name = s.name;
                            sandpiper.selectedSlice.slice_type = s.slice_type;
                            sandpiper.selectedSlice.slicemetadata = s.slicemetadata;
                            break;
                        }
                    }
                    buttonEditLocalSlice.Enabled = true;
                    buttonDeleteLocalSlice.Enabled = true;
                }
            }

            if (e.Node.Name.Contains("grain_"))
            {
                string[] chunks = e.Node.Name.Split('_');
                if (sandpiper.looksLikeAUUID(chunks[1]))
                {
                    sandpiper.selectedGrain.id = chunks[1];

                    foreach (sandpiperClient.grain g in sandpiper.localGrainsCache)
                    {
                        if (g.slice_id == sandpiper.selectedGrain.id)
                        {
                            sandpiper.selectedGrain.description = g.description;
                            sandpiper.selectedGrain.encoding = g.encoding;
                            sandpiper.selectedGrain.grain_key = g.grain_key;
                            sandpiper.selectedGrain.payload_len = g.payload_len;
                            sandpiper.selectedGrain.source = g.source;
                            sandpiper.selectedGrain.slice_id = g.slice_id;
                            break;
                        }
                    }
                }
            }








        }

        private void buttonEditLocalSlice_Click(object sender, EventArgs e)
        {
            using (FormNewSlice f = new FormNewSlice())
            {
                bool updateIndex = false;

                f.titleText = "Edit local slice " + sandpiper.selectedSlice.slice_id;
                f.goButttonText = "Save";

                f.sliceDescription = sandpiper.selectedSlice.name;
                f.sliceType = sandpiper.selectedSlice.slice_type;
                f.sliceMetadata = sandpiper.selectedSlice.slicemetadata;

                f.createButtonClick += (mysender, mye) =>
                {

                    sandpiper.selectedSlice.name = f.sliceDescription;
                    sandpiper.selectedSlice.slice_type = f.sliceType;
                    sandpiper.selectedSlice.slicemetadata = f.sliceMetadata;
                };
                f.ShowDialog(this);

                if (f.DialogResult == DialogResult.OK)
                {

                    for (int i = 0; i <= sandpiper.localSlices.Count() - 1; i++)
                    {
                        if (sandpiper.localSlices[i].slice_id == sandpiper.selectedSlice.slice_id)
                        {
                            sandpiper.localSlices[i].name = sandpiper.selectedSlice.name;
                            sandpiper.localSlices[i].slice_type = sandpiper.selectedSlice.slice_type;
                            sandpiper.localSlices[i].slicemetadata = sandpiper.selectedSlice.slicemetadata;
                            updateIndex = true;
                            break;
                        }
                    }

                    if (updateIndex)
                    {
                        sandpiper.reUUIDslice("00000000-0000-0000-0000-000000000000"); // just in case there exists an auto-generated NULL-UUID slice
                        sandpiper.writeFullCacheIndex(lblLocalCacheDir.Text);
                        sandpiper.historyRecords.Add("Local slice (" + sandpiper.selectedSlice.slice_id + ") edited");
                        updateLocalContentTree();
                    }


                }

            }

        }

        private void treeViewLocalContent_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void buttonDeleteLocalSlice_Click(object sender, EventArgs e)
        {
            string message = "You are about to delete local slice ("+sandpiper.selectedSlice.name+") and all of its grains. Are you sure?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, "Delete Slice", buttons);

            if (result == DialogResult.Yes)
            {
                sandpiper.deleteLocalSice(sandpiper.selectedSlice, lblLocalCacheDir.Text);
                updateLocalContentTree();
            }


            /*

            // see if any grains claim this slice

            int grainCount = 0;
            foreach (sandpiperClient.grain g in sandpiper.localGrainsCache)
            {
                if (sandpiper.selectedSlice.slice_id == g.slice_id) { grainCount++; }            
            }

            if(grainCount == 0)
            {
                string message = "You are about to delete empty slice (named: "+sandpiper.selectedSlice.name+") from the local pool. No grains will be deleted. Are you sure?";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, "Delete Empty Slice", buttons);
                if (result == DialogResult.Yes)
                {
                    for (int i = 0; i <= sandpiper.localSlices.Count() - 1; i++)
                    {
                        if (sandpiper.localSlices[i].slice_id == sandpiper.selectedSlice.slice_id)
                        {
                            sandpiper.localSlices.RemoveAt(i);
                            break;
                        }                   
                    }

                    sandpiper.writeFullCacheIndex(lblLocalCacheDir.Text);
                    updateLocalContentTree();
                }
            }
            else
            {
                string message = "Slices must be empty (no grains) before you delete them. Manually delete the "+grainCount.ToString()+" grainfiles in this slice from the cache directory.";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, "Delete Slice", buttons);
            }

        */


        }

        public string getUserSliceSelection(string grainDescription, string lastUserSelectedSliceID)
        {
            string returnValue = "";
            //show slice slector form if 

            FormSelectSlice f = new FormSelectSlice();


                f.headline = grainDescription;
                int i = 0;

                foreach (sandpiperClient.slice s in sandpiper.localSlices)
                {
                    
                    f.listItemString = s.slice_id + "\t" + s.name;
                    if (s.slice_id == lastUserSelectedSliceID)
                    {
                        f.sliceSelectedIndex = i;
                    }

                    i++;
                }

                f.ShowDialog(this);
                sandpiper.selectedSlice.name = f.selectedSliceText;
                string[] chunks = f.selectedSliceText.Split('\t');
                if (sandpiper.looksLikeAUUID(chunks[0]))
                {
                    returnValue = chunks[0];
                }

            f.Dispose();

            return returnValue;
        }



        public bool indexLocalFiles(string cacheDir)
        {// roll through all files found in the local cache directory and add them to the index if they are not already there.
            // return ture if a file was added to the cacheindex

//            localFilesIndexInProgress = true;

            bool updateIndex = false;
            Dictionary<string, string> indexedFilenames = new Dictionary<string, string>();

            foreach (sandpiperClient.grain localGrain in sandpiper.localGrainsCache)
            {
                if (!indexedFilenames.ContainsKey(localGrain.source))
                {

                    indexedFilenames.Add(localGrain.source, localGrain.id);
                }
            }

            //indexedFilenames now contains filename-keyed list of what the cache index has

            try
            {
                DirectoryInfo d = new DirectoryInfo(cacheDir);
                string lastUserSelectedSliceID = "";

                FileInfo[] Files = d.GetFiles("*.*");
                foreach (FileInfo file in Files)
                {
                    if (file.Name == "grainlist.txt" || file.Name == "slicelist.txt") { continue; }
                    if (!indexedFilenames.ContainsKey(file.Name))
                    { // our cacheindex is not aware of this file

                        sandpiperClient.grain newGrain = new sandpiperClient.grain();
                        newGrain.id = Guid.NewGuid().ToString("D");
                        newGrain.source = file.Name;
                        //newGrain.slice_id = "00000000-0000-0000-0000-000000000000";
                        newGrain.slice_id = getUserSliceSelection("New grain ("+ file.Name + ") must be assigned to a slice", lastUserSelectedSliceID);
                        lastUserSelectedSliceID = newGrain.slice_id;
                        sandpiper.localGrainsCache.Add(newGrain);
                        sandpiper.writeFullCacheIndex(cacheDir);
                        sandpiper.historyRecords.Add("Local file (" + file.Name + ") is not in the cache index. Adding it with new grain ID " + newGrain.id + " in slice " + newGrain.slice_id);
                        updateIndex = true;
                        localContentTreeIsUpToDate = false;
                    }
                }
            }
            catch (Exception ex)
            {
                sandpiper.historyRecords.Add("Error in indexLocalFiles(" + cacheDir + "):" + ex.Message);
            }

//            localFilesIndexInProgress = false;

            return updateIndex;
        }


        public bool readCacheIndex(string cacheDir)
        {
            // verify all the grains in the index (that they actually exist) and remove records if their file is not found
            // return true if a change was made to the index based on a non-existant local file
            // file record format is tab-delimited list of: grainid,sliceid,filename 

            // read in the slices list
            Dictionary<string, string> sliceidKeyedNames = new Dictionary<string, string>();

            sandpiper.localSlices.Clear();
            string localCacheFilePath = cacheDir + @"\slicelist.txt";
            if (File.Exists(localCacheFilePath))
            {
                string[] lines = File.ReadAllLines(localCacheFilePath);

                foreach (string line in lines)
                {
                    string[] fields = line.Split('\t');
                    if (fields.Count() == 4 && sandpiper.looksLikeAUUID(fields[0]))
                    {
                        sandpiperClient.slice s = new sandpiperClient.slice();
                        s.slice_id = fields[0];
                        s.slice_type = fields[1];
                        s.name = fields[2];
                        s.slicemetadata = fields[3];
                        sandpiper.localSlices.Add(s);
                        if (!sliceidKeyedNames.ContainsKey(s.slice_id)) { sliceidKeyedNames.Add(s.slice_id, s.name); }
                    }
                }
            }

            bool refreshCache = false;
            sandpiper.localGrainsCache.Clear();
            localCacheFilePath = cacheDir + @"\grainlist.txt";
            if (File.Exists(localCacheFilePath))
            {
                string[] lines = File.ReadAllLines(localCacheFilePath);

                foreach (string line in lines)
                {
                    string[] fields = line.Split('\t');
                    if (fields.Count() == 3 && sandpiper.looksLikeAUUID(fields[0]) && sandpiper.looksLikeAUUID(fields[1]))
                    {
                        if (File.Exists(cacheDir + @"\" + fields[2]))
                        {// looks like a legit record - pull it into the 
                            sandpiperClient.grain g = new sandpiperClient.grain();
                            g.id = fields[0];
                            g.slice_id = fields[1];
                            g.source = fields[2];
                            g.payload_len = new System.IO.FileInfo(cacheDir + @"\" + fields[2]).Length;
                            sandpiper.localGrainsCache.Add(g);

                            if (!sliceidKeyedNames.ContainsKey(g.slice_id))
                            {
                                sandpiper.historyRecords.Add("Local grain cache index referes to a slice (" + g.slice_id + ") that is not found in the local slicelist. Adding a place-holder slice.");
                                sandpiperClient.slice newSlice = new sandpiperClient.slice();
                                newSlice.slice_id = g.slice_id;
                                newSlice.name = "unknown";
                                newSlice.slice_type = "unknown";
                                sandpiper.localSlices.Add(newSlice);
                                sliceidKeyedNames.Add(g.slice_id, "unknown");
                                refreshCache = true;
                                localContentTreeIsUpToDate = false;
                            }
                        }
                        else
                        {// index referes to a non-existant file
                            refreshCache = true;
                            sandpiper.historyRecords.Add("Local grain cache index referes to a file (" + fields[2] + ") that does not exist. Entry removed from index file.");
                            localContentTreeIsUpToDate = false;
                        }
                    }
                }


                if (refreshCache && sandpiper.localGrainsCache.Count() > 0)
                {
                    sandpiper.writeFullCacheIndex(cacheDir);
                }
            }

            return refreshCache;
        }



        private void cacheFolderChange(object sender, FileSystemEventArgs e)
        {
            if (handlingFwatcherChange) { return; }

            handlingFwatcherChange = true;

            readCacheIndex(lblLocalCacheDir.Text);

            indexLocalFiles(lblLocalCacheDir.Text);
            if (!localContentTreeIsUpToDate)
            {// local file was added to the index - we need to flag the local content tree for refresh
                updateLocalContentTree();
            }
            handlingFwatcherChange = false;
        }



    }

}
