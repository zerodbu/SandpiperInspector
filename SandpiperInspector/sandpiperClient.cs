using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SandpiperInspector
{
    class sandpiperClient
    {

        public class JWT
        {
            public string token;
            public DateTime expiration;
            public List<string> claims;
            public string niceFormat;
        }


        public int myRole; // 0=primary, 1=secondary
        public string primaryNodeID;
        public string secondaryNodeID;

        public static HttpClient client = new HttpClient();
        public JWT sessionJTW = new JWT();
        public List<grain> availableGrains = new List<grain>();
        public grain selectedGrain = new grain();
        public List<slice> remoteSlices = new List<slice>();
        public slice selectedSlice = new slice();

        public bool activeSession;
        public bool awaitingServerResponse;
        public bool recordTranscript;
        public int responseTime;
        public List<string> historyRecords = new List<string>();
        public List<string> transcriptRecords = new List<string>();

        public int interactionState;
        public int tenMilisecondCounter;
        public int historyRecordCountTemp;
        public string plandocumentSchema;   // the active one
        public string defaultPlandocumentSchema; // for use if the user "resets to default"
        public Dictionary<string, string> localSliceHashes = new Dictionary<string, string>();
        public List<slice> slicesToUpdate = new List<slice>();
        public List<slice> slicesToAdd = new List<slice>();
        public List<slice> slicesToDrop = new List<slice>();
        public List<grain> grainsToTransfer = new List<grain>();
        public List<grain> grainsToDrop = new List<grain>();

        private SQLiteConnection sqlite_conn = new SQLiteConnection();
        public bool SQliteDatabaseInitialized = false;


        public enum interactionStates : int
        {
            IDLE = 0,
            AUTHENTICATING = 1,
            AUTHFAILED_UPDATING_UI = 2,
            AUTHFAILED = 3,
            AUTHENTICATED_UPDATING_UI = 4,
            AUTHENTICATED = 5,

            REMOTE_SEC_GET_SLICELIST = 6,
            REMOTE_SEC_GET_SLICELIST_AWAITING = 7,
            REMOTE_SEC_GET_GRAINLIST = 8,
            REMOTE_SEC_GET_GRAINLIST_AWAITING = 9,
            REMOTE_SEC_DROP_SLICES = 10,
            REMOTE_SEC_DROP_SLICES_AWAITING = 11,
            REMOTE_SEC_DROP_GRAINS = 12,
            REMOTE_SEC_DROP_GRAINS_AWAITING = 13,
            REMOTE_SEC_CREATE_SLICES = 14,
            REMOTE_SEC_CREATE_SLICES_AWAITING = 15,
            REMOTE_SEC_UPLOADING_GRAINS = 16,
            REMOTE_SEC_UPLOADING_GRAINS_AWAITING = 17,

            REMOTE_PRI_GET_SLICELIST = 18,
            REMOTE_PRI_GET_SLICELIST_AWAITING = 19,
            REMOTE_PRI_GET_GRAINLIST = 20,
            REMOTE_PRI_GET_GRAINLIST_AWAITING = 21,
            REMOTE_PRI_GET_GRAINS = 22,
            REMOTE_PRI_GET_GRAINS_AWAITING = 23
        }


        public class loginResponse
        {
            public string token;
            public DateTime expires;
            public string planschemaerrors;
            public string message;
        }

        public class grain
        {
            public string grain_uuid;
            public string grain_key;
            public string grain_reference;
            public string slice_uuid;
            public string description;
            public string source;
            public string encoding;
            public string payload;
            public long payload_len;
            public string localfile_name;

            public void clear()
            {
                grain_uuid = "";
                description = "";
                slice_uuid = "";
                grain_key = "";
                source = "";
                encoding = "";
                payload_len = 0;
            }
        }


        public class slice
        {
            public string slice_uuid;
            public string pool_uuid;
            public string slice_description;
            public string slice_type;
            public string file_name;
            public string slice_meta_data;
            public Int32 slice_order;
            public string slice_grainlist_hash; // md5sum of grain UUIDs concatenated
            public List<grain> grains;

            public void clear()
            {
                slice_uuid = "";
                slice_type = "";
                file_name = "";
                slice_meta_data = "";
                if (grains != null) { grains.Clear(); }
            }
        }


        public class grainsEnvelope
        {
            public List<grain> grains;
        }

        public class grainsEnvelopeCompact
        {
            public List<string> grain_uuids;
        }




        public class grainsResponse
        {
            public string message;
        }

        public class slicesResponse
        {
            public string message;
        }


        //---  temporary naming bridge ------------
        public class altSlice
        {
            public string slice_id;
            public string slice_type;
            public string name;
            public string slice_meta_data;
            public string hash;
        }

        public class altGrain
        {
            public string id;
            public string description;
            public string slice_id;
            public string grain_key;
            public string source;
            public string encoding;
            public string payload;
            public Int32 payload_len;
        }

        public class altGrainsEnvelope
        {
            public List<altGrain> grains;
        }

        public class altGrainsEnvelopeCompact
        {
            public List<string> grain_uuids;
        }





        //--------------------------------------------

        public async Task<bool> loginAsync(string path, string username, string password, string plandocument)
        {
            sessionJTW.token = "";
            bool returnValue = false;

            string plandocumentEncoded = Base64Encode(plandocument);

            string json = new JavaScriptSerializer().Serialize(new
            {
                username = username,
                password = password,
                plandocument = plandocumentEncoded
            });

            if (recordTranscript) { transcriptRecords.Add(FormatJson(json)); }


            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(path, content);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (recordTranscript){transcriptRecords.Add(FormatJson(responseString));}

                    try
                    {
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        loginResponse r = new loginResponse();
                        r = serializer.Deserialize<loginResponse>(responseString);

                        sessionJTW.token = r.token;

                        if (r.token != null)
                        {
                            string[] chunks = sessionJTW.token.Split('.');
                            if (chunks.Count() == 3)
                            {
                                sessionJTW.niceFormat = "Valid JWT Token receied:" + Environment.NewLine + Environment.NewLine;
                                sessionJTW.niceFormat += "---- Header ----" + Environment.NewLine + Base64Decode(chunks[0].PadRight(chunks[0].Length + (4 - chunks[0].Length % 4) % 4, '=')) + Environment.NewLine + Environment.NewLine;
                                sessionJTW.niceFormat += "---- Payload ----" + Environment.NewLine + Base64Decode(chunks[1].PadRight(chunks[1].Length + (4 - chunks[1].Length % 4) % 4, '=')) + Environment.NewLine + Environment.NewLine;
                                sessionJTW.niceFormat += "---- Signature ----" + Environment.NewLine + chunks[2] + Environment.NewLine;
                                historyRecords.Add("Authenticated and received a JWT (" + (10 * responseTime).ToString() + "mS response time)");
                                historyRecords.Add("    Response message: " + r.message);
                                interactionState = (int)interactionStates.AUTHENTICATED_UPDATING_UI;
                                returnValue = true;
                            }
                            else
                            {// token did not contain three parts

                                sessionJTW.niceFormat = "Parse error - Received this invalid data: " + Environment.NewLine + Environment.NewLine + sessionJTW.token;
                                sessionJTW.niceFormat += "Authentication error - Received this invalid JWT data: " + Environment.NewLine;
                                interactionState = (int)interactionStates.AUTHFAILED_UPDATING_UI;
                            }
                        }
                        else
                        {// token was empty 
                            sessionJTW.niceFormat = "Parse error - did not receive a JWT" + Environment.NewLine + Environment.NewLine + sessionJTW.token;
                            historyRecords.Add("Authentication error - Did not receive a JWT");
                            interactionState = (int)interactionStates.AUTHFAILED_UPDATING_UI;
                        }
                    }
                    catch (Exception ex)
                    {
                        interactionState = (int)interactionStates.AUTHFAILED_UPDATING_UI;
                        historyRecords.Add("Authentication error parsing server JSON response:" + ex.Message);
                    }
                }
                else
                {// something other than 200 (success) code back from the other end 
                    interactionState = (int)interactionStates.AUTHFAILED_UPDATING_UI;
                    historyRecords.Add("Authentication error - HTTP response: " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                interactionState = (int)interactionStates.AUTHFAILED_UPDATING_UI;
                historyRecords.Add("Authentication error - " + ex.Message);
            }

            return returnValue;
        }


        public async Task<List<grain>> getGrainsAsync(string path, JWT jwt)
        {

            List<altGrain> altGrainsList = new List<altGrain>();
            grainsEnvelope responseData = new grainsEnvelope();

            altGrainsEnvelope altResponseData = new altGrainsEnvelope();
            altGrain myAltGrain = new altGrain();

            altResponseData.grains = altGrainsList;

            List<grain> grainsList = new List<grain>();
            responseData.grains = grainsList;


            try
            {

                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(path)
                };

                requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", jwt.token));


                HttpResponseMessage response = await client.SendAsync(requestData);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;

                    // grains response can be wrapped in "grains"

                    if (recordTranscript){transcriptRecords.Add(FormatJson(responseString));}

                    try
                    {
                        if (responseString.Substring(0, 13).Contains("\"grains\""))
                        {// json scructure includes a wrapper evelope 
                            altResponseData = serializer.Deserialize<altGrainsEnvelope>(responseString);
                        }
                        else
                        {
                            myAltGrain = serializer.Deserialize<altGrain>(responseString);
                            altResponseData.grains.Add(myAltGrain);
                        }
                    }
                    catch (Exception ex)
                    {// error parsing json 
                        historyRecords.Add("Grains error pasring JSON response from server - " + ex.Message);
                    }
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("Grains error - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                historyRecords.Add("Grains error - " + ex.Message);
            }


            foreach (altGrain ag in altResponseData.grains)
            {
                grain g = new grain();
                g.description = ag.description;
                g.encoding = ag.encoding;
                g.grain_key = ag.grain_key;
                g.localfile_name = "";
                g.payload = ag.payload;
                g.payload_len = ag.payload_len;
                g.slice_uuid = ag.slice_id;
                g.source = ag.source;
                g.grain_reference = ag.source;
                g.grain_uuid = ag.id;
                responseData.grains.Add(g);
            }


            return responseData.grains;
        }


        public async Task<bool> postGrainAsync(string path, JWT jwt, grain g, string payloadString)
        {
            bool returnValue = false;

            grainsResponse serverGrainsResponse = new grainsResponse();


            JavaScriptSerializer bodySerializer = new JavaScriptSerializer();
            bodySerializer.MaxJsonLength = Int32.MaxValue;
            string bodyJSON = bodySerializer.Serialize(new
            {

                grain_uuid = g.grain_uuid,
                slice_uuid = g.slice_uuid,
                name = g.description,
                source = g.source,
                grain_key = "level-1",
                encoding = g.encoding,
                payload = payloadString
            });

            if (recordTranscript){transcriptRecords.Add(FormatJson(bodyJSON));}

            try
            {
                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(path)
                };

                requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", jwt.token));
                requestData.Content = new StringContent(bodyJSON, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(requestData);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (recordTranscript) { transcriptRecords.Add(FormatJson(responseString)); }
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;

                    try
                    {
                        serverGrainsResponse = serializer.Deserialize<grainsResponse>(responseString);
                        historyRecords.Add("POST grain " + g.grain_uuid + ".  Server responded: " + serverGrainsResponse.message);
                    }
                    catch (Exception ex)
                    {
                        historyRecords.Add("POST grain " + g.grain_uuid + ". Local error parsing server JSON response from server: " + ex.Message);
                    }
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("POST grain " + g.grain_uuid + " error. Server HTTP response:" + response.ReasonPhrase);
                }

            }
            catch (Exception ex)
            {
                historyRecords.Add("POST grain " + g.grain_uuid + ". Local error - " + ex.Message);
            }

            return returnValue;
        }


        public async Task<bool> deleteGrainAsync(string path, JWT jwt)
        {
            grainsResponse serverGrainsResponse = new grainsResponse();
            try
            {
                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(path)
                };

                requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", jwt.token));

                HttpResponseMessage response = await client.SendAsync(requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (recordTranscript) { transcriptRecords.Add(FormatJson(responseString)); }

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serverGrainsResponse = serializer.Deserialize<grainsResponse>(responseString);
                    historyRecords.Add("Delete grain response message - " + serverGrainsResponse.message);
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("Grain delete error - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                historyRecords.Add("Grain delete error - " + ex.Message);
            }
            return true;
        }


        public async Task<List<slice>> getSlicesAsync(string path, JWT jwt)
        {
            List<altSlice> altSlices = new List<altSlice>();
            List<slice> slices = new List<slice>();

            try
            {

                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(path)
                };

                requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", jwt.token));


                HttpResponseMessage response = await client.SendAsync(requestData);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (recordTranscript) { transcriptRecords.Add(FormatJson(responseString)); }
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    altSlices = serializer.Deserialize<List<altSlice>>(responseString);

                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("Slices error - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                historyRecords.Add("Slices error - " + ex.Message);
            }

            //--- temporary hack to account for the naming difference between old and new slice naming convention-------

            foreach (altSlice alts in altSlices)
            {
                slice s = new slice();
                s.slice_uuid = alts.slice_id;
                s.slice_description = alts.name;
                s.slice_type = alts.slice_type;
                s.slice_meta_data = alts.slice_meta_data;
                s.slice_grainlist_hash = alts.hash;
                slices.Add(s);            
            }
            //----------------------------


            return slices;
        }


        public async Task<bool> deleteSliceAsync(string path, JWT jwt)
        {
            slicesResponse serverSlicesResponse = new slicesResponse();
            try
            {
                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(path)
                };

                requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", jwt.token));

                HttpResponseMessage response = await client.SendAsync(requestData);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (recordTranscript) { transcriptRecords.Add(FormatJson(responseString)); }

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serverSlicesResponse = serializer.Deserialize<slicesResponse>(responseString);
                    historyRecords.Add("Delete slice response message - " + serverSlicesResponse.message);
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("Slice delete error - " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                historyRecords.Add("Slice delete error - " + ex.Message);
            }
            return true;
        }


        public string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public async Task<bool> postSliceAsync(string path, JWT jwt, slice s)
        {
            bool returnValue = false;

            slicesResponse serverSlicesResponse = new slicesResponse();


            JavaScriptSerializer bodySerializer = new JavaScriptSerializer();
            bodySerializer.MaxJsonLength = Int32.MaxValue;
            string bodyJSON = bodySerializer.Serialize(new
            {
                slice_uuid = s.slice_uuid,
                description = s.slice_description,
                slice_type = s.slice_type,
                metadata = s.slice_meta_data
            });

            if (recordTranscript) { transcriptRecords.Add(FormatJson(bodyJSON)); }


            try
            {
                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(path)
                };

                requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", jwt.token));
                requestData.Content = new StringContent(bodyJSON, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(requestData);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (recordTranscript) { transcriptRecords.Add(FormatJson(responseString)); }
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;

                    try
                    {
                        serverSlicesResponse = serializer.Deserialize<slicesResponse>(responseString);
                        historyRecords.Add("POST slice " + s.slice_uuid + ".  Server responded: " + serverSlicesResponse.message);
                    }
                    catch (Exception ex)
                    {
                        historyRecords.Add("POST slice " + s.slice_uuid + ". Local error parsing server JSON response from server: " + ex.Message);
                    }
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("POST slice " + s.slice_uuid + " error. Server HTTP response:" + response.ReasonPhrase);
                }

            }
            catch (Exception ex)
            {
                historyRecords.Add("POST slice " + s.slice_uuid + ". Local error - " + ex.Message);
            }

            return returnValue;
        }



        public void addMissingLocalSlices(List<slice> slices)
        {
            // add slices to the local list from given list if they do not already exist
            foreach (slice s in slices)
            {
                if(!localSliceExists(s.slice_uuid))
                {
                    addLocalSlice(s);
                    historyRecords.Add("Added slice " + s.slice_uuid + " (" + s.slice_description + ") to local pool");
                    logActivity("", s.slice_uuid, "", "slice (" + s.slice_description + ") added to local pool");
                }
            }
        }

        public int dropRogueLocalSlices(List<slice> remoteSlices)
        {
            // drop slices (and all their grain connections) if they do not appear in the given list
            // this is for syncing the local as secondary
            int returnVal = 0;

            List<string> remoteSliceids = new List<string>();
            foreach (slice s in remoteSlices){remoteSliceids.Add(s.slice_uuid);}

            List<slice> localSlices = getLocalSlices();
            foreach (slice s in localSlices)
            {
                if (!remoteSliceids.Contains(s.slice_uuid))
                {
                    historyRecords.Add("Removing local slice " + s.slice_uuid + " (" + s.slice_description + ") and all of its grain connections from local pool");
                    dropLocalSlice(s.slice_uuid);
                    logActivity("", s.slice_uuid, "", "slice (" + s.slice_description + ") removed from local pool");
                }
            }

            return returnVal;
        }





        public bool writeFilegrainToFile(sandpiperClient.grain filegrain, string cacheDir)
        {
            
            if (filegrain.encoding == "z64")
            {
                
                byte[] payloadBytes = unz64(filegrain.payload);
                File.WriteAllBytes(cacheDir + @"\" + filegrain.localfile_name, payloadBytes);
            }

            if (filegrain.encoding == "b64")
            {// full-range 8bit binary data (not compressed) is to be expected
                byte[] payloadBytes = Convert.FromBase64String(filegrain.payload);
                File.WriteAllBytes(cacheDir + @"\" + filegrain.localfile_name, payloadBytes);
            }

            if (filegrain.encoding == "raw")
            {// probably a text file 
                File.WriteAllBytes(cacheDir + @"\" + filegrain.localfile_name, Encoding.UTF8.GetBytes(filegrain.payload));
            }

            logActivity(filegrain.grain_uuid, filegrain.slice_uuid, "", "grain (" + filegrain.description + ") exported to local file ("+ filegrain.localfile_name + ")");

            return true;
        }


        // compare two gainlists (A and B) and return a new list of the grains in B that are not present in A
        public List<grain> differentialGrainsList(List<grain> grainsA, List<grain> grainsB)
        {
            List<grain> diffs = new List<grain>();
            bool found;

            for (int i = 0; i <= grainsB.Count() - 1; i++)
            {
                found = false;
                foreach (grain gA in grainsA)
                {
                    if(grainsB[i].grain_uuid== gA.grain_uuid)
                    {// this grain from the "B" list was found in the "A" list - quit looking
                        found = true;
                        break;
                    }
                }

                if (!found)
                {// this grain from the "B" list was not found in the "A" list - add it to the results
                    diffs.Add(grainsB[i]);
                }


            }
            return diffs;
        }





        public static string md5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }



        public string Base64ForUrlDecode(string str)
        {
            byte[] decbuff = HttpServerUtility.UrlTokenDecode(str);
            return Encoding.UTF8.GetString(decbuff);
        }


        public bool looksLikeAUUID(string input)
        {
            string[] chunks = input.Split('-');
            if (chunks.Count() == 5 && chunks[0].Length == 8 && chunks[1].Length == 4 && chunks[2].Length == 4 && chunks[3].Length == 4 && chunks[4].Length == 12)
            {
                return true;
            }
            return false;
        }



        public byte[] unz64(string input)
        {
            byte[] compressed = Convert.FromBase64String(input);
            byte[] decompressed = Decompress(compressed);
            return decompressed;
        }

        public byte[] Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }




        public string z64(byte[] raw)
        {
            byte[] compressed = Compress(raw);
            return Convert.ToBase64String(compressed);
        }


        public byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }






        public bool getNodeIDsFromPlan(string planxml)
        {
            this.primaryNodeID = "";
            this.secondaryNodeID = "";

            try
            {
                XDocument xDoc = XDocument.Parse(planxml);
                XElement planElement;
                planElement = xDoc.Element("Plan");
                XElement primaryElement = planElement.Element("Primary");
                this.primaryNodeID = primaryElement.Attribute("uuid").Value;
                XElement secondaryElement = planElement.Element("Secondary");
                this.secondaryNodeID = secondaryElement.Attribute("uuid").Value;

            }
            catch(Exception ex)
            {
                historyRecords.Add("xml parsing error trying to extract node IDs:"+ex.Message);
                return false;
            }

            return true;
        }




        public bool validPlandocument(string planxml)
        {
            List<String> xmlValidationErrors = new List<string>();
            XDocument xmlDoc = null;
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(plandocumentSchema)));

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (o, args) => { xmlValidationErrors.Add(args.Message); };
            settings.Schemas.Add(schemas);

            try
            {
                XmlReader reader = XmlReader.Create(new StringReader(planxml), settings);
                while (reader.Read()) ;
            }
            catch (Exception ex) { xmlValidationErrors.Add(ex.Message); }

            if (xmlValidationErrors.Count() > 0)
            {
                historyRecords.Add(string.Join("; ", xmlValidationErrors));
                return false;
            }
            else
            {
                historyRecords.Add("Plan XML validates against schema");
                return true;
            }
        }



        public bool validPlandocumentSchema(string planschema)
        {
            List<String> xmlValidationErrors = new List<string>();
            XDocument xmlDoc = null;
            XmlSchemaSet schemas = new XmlSchemaSet();

            try
            {
                schemas.Add("", XmlReader.Create(new StringReader(planschema)));
            }
            catch (Exception ex) { xmlValidationErrors.Add(ex.Message); }

            if (xmlValidationErrors.Count() > 0)
            {
                historyRecords.Add(string.Join("; ", xmlValidationErrors));
                return false;
            }
            else
            {
                historyRecords.Add("Plan XSD is valid");
                return true;
            }

        }


        public static string FormatJson(string json, string indent = "  ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in json ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }


        //slicesToTransfer and slicesToDrop lists will be populated based on current localSlices and remoteSlices lists and my current role (primary vs secondary)
        public void updateSliceHitlists()
        {
            slicesToAdd.Clear();
            slicesToUpdate.Clear();
            slicesToDrop.Clear();
            bool hashMatch = true;
            bool foundRemoteSlice = false;
            bool foundLocalSlice = false;
            //ddd

            List<slice> localSlices = getLocalSlices();

            if(myRole == 0)
            {// local client is primary

                foreach (slice localSlice in localSlices)
                {
                    foundRemoteSlice = false;
                    hashMatch = true;

                    foreach (slice remoteSlice in remoteSlices)
                    {
                        if (remoteSlice.slice_uuid == localSlice.slice_uuid)
                        {// found a sliceid match

                            foundRemoteSlice = true;

                            if (remoteSlice.slice_grainlist_hash.ToLower() != localSlice.slice_grainlist_hash.ToLower())
                            {
                                hashMatch = false;
                            }
                            break;
                        }
                    }

                    if (!foundRemoteSlice)
                    {// no remote slice was found. put it on the "add" list  
                        slice newSlice = new slice();
                        newSlice.slice_uuid = localSlice.slice_uuid;
                        newSlice.pool_uuid = localSlice.pool_uuid;
                        newSlice.slice_type = localSlice.slice_type;
                        newSlice.file_name = localSlice.file_name;
                        newSlice.slice_description = localSlice.slice_description; 
                        newSlice.slice_meta_data = localSlice.slice_meta_data; 
                        newSlice.slice_grainlist_hash = localSlice.slice_grainlist_hash;
                        slicesToAdd.Add(newSlice);
                    }

                    if (!hashMatch)
                    {// no remote slice was found, or it was found with non-matching hash. We need to push this slice into the remote secondary
                        slice newSlice = new slice();
                        newSlice.slice_uuid = localSlice.slice_uuid;
                        newSlice.slice_type = localSlice.slice_type;
                        newSlice.pool_uuid = localSlice.pool_uuid;
                        newSlice.file_name = localSlice.file_name;
                        newSlice.slice_description = localSlice.slice_description;
                        newSlice.slice_meta_data = localSlice.slice_meta_data;
                        newSlice.slice_grainlist_hash = localSlice.slice_grainlist_hash;
                        slicesToUpdate.Add(newSlice);
                    }
                }

                // build a droplist of remote slices (ones that are in the remote and not local)

                foreach (slice remoteSlice in remoteSlices)
                {
                    foundLocalSlice = false;
                    foreach (slice localSlice in localSlices)
                    {
                        if (remoteSlice.slice_uuid == localSlice.slice_uuid)
                        {// found a sliceid match
                            foundLocalSlice = true;
                            break;
                        }
                    }

                    if (!foundLocalSlice)
                    {
                        slice newSlice = new slice();
                        newSlice.slice_uuid = remoteSlice.slice_uuid;
                        newSlice.pool_uuid = remoteSlice.pool_uuid;
                        newSlice.slice_description = remoteSlice.slice_description;
                        newSlice.slice_type = remoteSlice.slice_type;
                        newSlice.file_name = remoteSlice.file_name;
                        newSlice.slice_meta_data = remoteSlice.slice_meta_data; 
                        newSlice.slice_grainlist_hash = remoteSlice.slice_grainlist_hash;
                        slicesToDrop.Add(newSlice);
                    }
                }
            }
            else
            {// local client is secondary 

                foreach (slice remoteSlice in remoteSlices)
                {
                    foundLocalSlice = false;
                    hashMatch = true;

                    foreach (slice localSlice in localSlices)
                    {

                        if (remoteSlice.slice_uuid == localSlice.slice_uuid)
                        {// found a sliceid match

                            foundLocalSlice = true;

                            if (remoteSlice.slice_grainlist_hash.ToLower() != localSlice.slice_grainlist_hash.ToLower())
                            {
                                hashMatch = false;
                            }
                            break;
                        }
                    }

                    if (!foundLocalSlice)
                    {// no local slice was found
                        slice newSlice = new slice();
                        newSlice.slice_uuid = remoteSlice.slice_uuid;
                        newSlice.pool_uuid = remoteSlice.pool_uuid;
                        newSlice.slice_type = remoteSlice.slice_type; 
                        newSlice.file_name = remoteSlice.file_name;
                        newSlice.slice_description = remoteSlice.slice_description;
                        newSlice.slice_meta_data = remoteSlice.slice_meta_data;
                        newSlice.slice_grainlist_hash = remoteSlice.slice_grainlist_hash;
                        slicesToAdd.Add(newSlice);
                    }

                    if (!hashMatch)
                    {// slice found with non-matching hash. We need to get the remote (primary) slice
                        slice newSlice = new slice();
                        newSlice.slice_uuid = remoteSlice.slice_uuid;
                        newSlice.pool_uuid = remoteSlice.pool_uuid;
                        newSlice.slice_type = remoteSlice.slice_type;
                        newSlice.slice_description = remoteSlice.slice_description;
                        newSlice.file_name = remoteSlice.file_name;
                        newSlice.slice_meta_data = remoteSlice.slice_meta_data; 
                        newSlice.slice_grainlist_hash = remoteSlice.slice_grainlist_hash;
                        slicesToUpdate.Add(newSlice);
                    }
                }

                //determine a droplist of local slices (ones that are local but not in remote list)
                foreach (slice localSlice in localSlices)
                {
                    foundRemoteSlice = false;
                    foreach (slice remoteSlice in remoteSlices)
                    {
                        if (remoteSlice.slice_uuid == localSlice.slice_uuid)
                        {// found a sliceid match
                            foundRemoteSlice = true;
                            break;
                        }
                    }

                    if (!foundRemoteSlice)
                    {
                        slice newSlice = new slice();
                        newSlice.slice_uuid = localSlice.slice_uuid;
                        newSlice.pool_uuid = localSlice.pool_uuid;
                        newSlice.slice_type = localSlice.slice_type;
                        newSlice.slice_description = localSlice.slice_description;
                        newSlice.file_name = localSlice.file_name; 
                        newSlice.slice_meta_data = localSlice.slice_meta_data; 
                        newSlice.slice_grainlist_hash = localSlice.slice_grainlist_hash;
                        slicesToDrop.Add(newSlice);
                    }
                }
            }
        }

        /*
                public bool SQLiteInit(string cacheDir)
                {
                    bool returnVal = false;
                    sqlite_conn.ConnectionString = "Data Source=" + cacheDir + @"\sandpiper.db; Version = 3; New = True; Compress = True;";

                    using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                    {
                        try
                        {
                            sqlite_conn.Open();

                            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS slices (sliceid VARCHAR(64) PRIMARY KEY, slice_type VARCHAR(64), name VARCHAR(255), slice_meta_data TEXT, remotehash VARCHAR(64), localhash VARCHAR(64))";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_sliceid ON slice (sliceid);";
                            sqlite_cmd.ExecuteNonQuery();


                            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS grain (id INTEGER PRIMARY KEY AUTOINCREMENT, grainid VARCHAR(64), source VARCHAR(255), payload_len INTEGER, grain_key VARCHAR(32), description VARCHAR(255), encoding VARCHAR(32), payload TEXT)";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_grainid ON grain (grainid);";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_grain_key ON grain (grain_key);";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_source ON grain (source);";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_payload_len ON grain (payload_len);";
                            sqlite_cmd.ExecuteNonQuery();



                            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS slice_grain (id INTEGER PRIMARY KEY AUTOINCREMENT, sliceid VARCHAR(64), grainid VARCHAR(64))";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_sliceid ON slice_grain (sliceid);";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_grainid ON slice_grain (grainid);";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_sliceid_grainid ON slice_grain (sliceid,grainid);";
                            sqlite_cmd.ExecuteNonQuery();




                            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS activity (id INTEGER PRIMARY KEY AUTOINCREMENT, description varchar(255), planid VARCHAR(64), sliceid VARCHAR(64), grainid VARCHAR(64), timestamp DATETIME)";
                            sqlite_cmd.ExecuteNonQuery();

                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_planid ON activity (planid);";
                            sqlite_cmd.ExecuteNonQuery();

                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_sliceid ON activity (sliceid);";
                            sqlite_cmd.ExecuteNonQuery();

                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_grainid ON activity (grainid);";
                            sqlite_cmd.ExecuteNonQuery();

                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_timestamp ON activity (timestamp);";
                            sqlite_cmd.ExecuteNonQuery();

                            sqlite_cmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_description ON activity (description);";
                            sqlite_cmd.ExecuteNonQuery();




                            // create indexes on activity table here


                            sqlite_cmd.CommandText = "PRAGMA journal_mode = WAL;";
                            sqlite_cmd.ExecuteNonQuery();
                            sqlite_cmd.CommandText = "PRAGMA synchronous = NORMAL;";
                            sqlite_cmd.ExecuteNonQuery();


                            SQliteDatabaseInitialized = true;
                            returnVal = true;
                        }
                        catch (Exception ex)
                        {
                            historyRecords.Add("failed opening SQLite database: " + ex.Message);
                        }
                    }
                    return returnVal;
                }

                */


        public bool SQLiteInit(string cacheDir, List<string> SQLcommandStringList)
        {
            bool returnVal = false;
            sqlite_conn.ConnectionString = "Data Source=" + cacheDir + @"\sandpiper.db; Version = 3; New = True; Compress = True;";

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                string lastCommand = "";
                try
                {
                    sqlite_conn.Open();

                    foreach (string SQLcommandString in SQLcommandStringList)
                    {

                        lastCommand = SQLcommandString;
                        sqlite_cmd.CommandText = SQLcommandString;
                        sqlite_cmd.ExecuteNonQuery();

                        sqlite_cmd.CommandText = "PRAGMA journal_mode = WAL;";
                        sqlite_cmd.ExecuteNonQuery();
                        sqlite_cmd.CommandText = "PRAGMA synchronous = NORMAL;";
                        sqlite_cmd.ExecuteNonQuery();

                        SQliteDatabaseInitialized = true;
                        returnVal = true;
                    }
                }
                catch (Exception ex)
                {
                    historyRecords.Add("failed initializing SQLite database: " + ex.Message + "\r\n    "+ lastCommand);
                }
            }
            return returnVal;
        }

        public bool SQLiteOpen(string cacheDir)
        {
            bool returnVal = false;
            sqlite_conn.ConnectionString = "Data Source=" + cacheDir + @"\sandpiper.db; Version = 3; New = False; Compress = True;";

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_conn.Open();
                    sqlite_cmd.CommandText = "PRAGMA journal_mode = WAL;";
                    sqlite_cmd.ExecuteNonQuery();
                    sqlite_cmd.CommandText = "PRAGMA synchronous = NORMAL;";
                    sqlite_cmd.ExecuteNonQuery();

                    SQliteDatabaseInitialized = true;
                    returnVal = true;
                }
                catch (Exception ex)
                {
                    historyRecords.Add("failed open SQLite database ("+ cacheDir + @"\sandpiper.db): " + ex.Message);
                }
            }
            return returnVal;
        }


        public bool localSliceExists(string slice_uuid)
        {
            bool returnVal = false;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT slice_description FROM slices where slice_uuid='" + slice_uuid + "'";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        if (sqlite_datareader.Read())
                        {
                            returnVal = true;
                        }
                    }
                }
            }
            return returnVal;
        }


        public bool localSliceGrainsRecordExists(string grain_uuid, string slice_uuid)
        {
            bool returnVal = false;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT slice_grain_id FROM slice_grains where slice_uuid='" + slice_uuid + "' and grain_uuid='"+grain_uuid+"'";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        if (sqlite_datareader.Read())
                        {
                            returnVal = true;
                        }
                    }
                }
            }
            return returnVal;
        }

        public bool localGrainRecordExists(string grain_uuid)
        {
            bool returnVal = false;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT grain_uuid FROM grains where grain_uuid='" + grain_uuid + "'";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        if (sqlite_datareader.Read())
                        {
                            returnVal = true;
                        }
                    }
                }
            }
            return returnVal;
        }




        public slice getLocalSlice(string slice_uuid)
        {
            slice s = null;

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT slice_uuid, pool_uuid, slice_description, slice_type, file_name, '' as slice_meta_data FROM slices where slice_uuid='" + slice_uuid + "'";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        s = new slice();
                        if(sqlite_datareader.Read())
                        {
                            s.slice_uuid = sqlite_datareader.GetString(0);
                            s.pool_uuid= sqlite_datareader.GetString(1);
                            s.slice_description = sqlite_datareader.GetString(2);
                            s.slice_type = sqlite_datareader.GetString(3);
                            s.file_name = sqlite_datareader.GetString(4);
                            s.slice_meta_data= sqlite_datareader.GetString(5);
                        }
                    }
                }
            }
            return s;
        }

        public List<slice> getLocalSlices()
        {
            List<slice> returnVal = new List<slice>();

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT slice_uuid, pool_uuid, slice_description, slice_type, file_name, '' as slice_meta_data FROM slices order by slice_order";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        while (sqlite_datareader.Read())
                        {
                            slice s = new slice();
                            s.slice_uuid = sqlite_datareader.GetString(0);
                            s.pool_uuid = sqlite_datareader.GetString(1);
                            s.slice_description = sqlite_datareader.GetString(2);
                            s.slice_type = sqlite_datareader.GetString(3);
                            s.file_name = sqlite_datareader.GetString(4);
                            s.slice_meta_data = sqlite_datareader.GetString(5);
                            s.slice_grainlist_hash = refreshLocalSliceHash(s.slice_uuid);
                            returnVal.Add(s);
                        }
                    }
                }




            }
            return returnVal;
        }






        public List<grain> getGrainsInLocalSlice(string slice_uuid, bool with_payload)
        {
            List<grain> grainslist = new List<grain>();

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    if (with_payload)
                    {
                        sqlite_cmd.CommandText = "SELECT" +
                            " grains.grain_uuid," +
                            " slices.slice_uuid," +
                            " grains.grain_key, " +
                            " grains.grain_reference," +
                            " grain_payloads.encoding," +
                            " length(payload) as payload_len," +
                            " grain_payloads.payload" +
                            " FROM slices,slice_grains,grains,grain_payloads where" +
                            " slices.slice_uuid=slice_grains.slice_uuid and " +
                            " slice_grains.grain_uuid=grains.grain_uuid and" +
                            " grains.grain_uuid=grain_payloads.grain_uuid and slices.slice_uuid='" + slice_uuid + "'";
                    }
                    else
                    {// leave out payload
                        sqlite_cmd.CommandText =

                            "SELECT" +
                            " grains.grain_uuid," +
                            " slices.slice_uuid," +
                            " grains.grain_key, " +
                            " grains.grain_reference," +
                            " grain_payloads.encoding," +
                            " length(payload) as payload_Len"+
                            " FROM slices,slice_grains,grains,grain_payloads where" +
                            " slices.slice_uuid=slice_grains.slice_uuid and " +
                            " slice_grains.grain_uuid=grains.grain_uuid and" +
                            " grains.grain_uuid=grain_payloads.grain_uuid and slices.slice_uuid='" + slice_uuid + "'";
                    }
                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {

                        while (sqlite_datareader.Read())
                        {
                            grain g = new grain();
                            g.grain_uuid = sqlite_datareader.GetString(0);
                            g.slice_uuid = sqlite_datareader.GetString(1);
                            g.grain_key= sqlite_datareader.GetString(2);
                            g.grain_reference= sqlite_datareader.GetString(3);
                            g.encoding = sqlite_datareader.GetString(4);
                            g.payload = "";
                            g.payload_len = sqlite_datareader.GetInt32(5);
                            if (with_payload)
                            {
                                g.payload = sqlite_datareader.GetString(6);
                            }
                            grainslist.Add(g);
                        }
                    }
                }
            }
            return grainslist;
        }






        public grain getLocalGrain(string grain_uuid, bool with_payload)
        {
            grain g = new grain();

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    if (with_payload)
                    {
                        sqlite_cmd.CommandText = "SELECT" +
                            " grains.grain_uuid," +
                            " grains.grain_key, " +
                            " grains.grain_reference," +
                            " grain_payloads.encoding," +
                            " length(payload) as payload_len," +
                            " grain_payloads.payload" +
                            " FROM grains, grain_payloads where" +
                            " grains.grain_uuid=grain_payloads.grain_uuid and grains.grain_uuid='" + grain_uuid + "'";
                    }
                    else
                    {// leave out payload
                        sqlite_cmd.CommandText = "SELECT" +
                            " grains.grain_uuid," +
                            " grains.grain_key, " +
                            " grains.grain_reference," +
                            " grain_payloads.encoding," +
                            " length(payload) as payload_len" +
                            " FROM grains, grain_payloads where" +
                            " grains.grain_uuid=grain_payloads.grain_uuid and grains.grain_uuid='" + grain_uuid + "'";
                    }
                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {

                        while (sqlite_datareader.Read())
                        {
                            g.grain_uuid= sqlite_datareader.GetString(0);
                            g.grain_key = sqlite_datareader.GetString(1);
                            g.grain_reference = sqlite_datareader.GetString(2);
                            g.encoding = sqlite_datareader.GetString(3);
                            g.payload_len = sqlite_datareader.GetInt32(4);
                            g.payload = "";
                            if (with_payload)
                            {
                                g.payload = sqlite_datareader.GetString(5);
                            }
                        }
                    }
                }
            }
            return g;
        }











        public int countGrainsInLocalSlice(string slice_uuid)
        {
            int returnVal = 0;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT count(slice_grain_id) as graincount FROM slice_grains where slice_uuid='" + slice_uuid + "'";
                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        while (sqlite_datareader.Read())
                        {
                            returnVal = sqlite_datareader.GetInt32(0);
                        }
                    }
                }
            }
            return returnVal;
        }


        public bool addLocalSlice(slice s)
        {
            bool returnVal = false;
            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    if (localSliceExists(s.slice_uuid))
                    {// slice exists - update it

                        sqlite_cmd.CommandText = "UPDATE slices set slice_type='" + s.slice_type + "', slice_description='" + s.slice_description + "' where slice_uuid='"+s.slice_uuid+"';";
                        historyRecords.Add("addLocalSlice(" + s.slice_uuid + ") - slice record already exists. Updating existing record.");
                        logActivity("", s.slice_uuid, "", "slice record updated (" + s.slice_description + ")");
                    }
                    else
                    {// slice does not exist - insert it

                        sqlite_cmd.CommandText = "INSERT INTO slices (slice_uuid, pool_uuid, slice_description, slice_type, file_name, slice_order, created_on) VALUES ('" + s.slice_uuid + "','" + s.pool_uuid+ "','" + s.slice_description + "','" + s.slice_type + "','" + s.file_name + "'," + s.slice_order.ToString() + ",DATE('now'));";
                        logActivity("", s.slice_uuid, "", "slice record added ("+s.slice_description+")");

                    }

                    sqlite_cmd.ExecuteNonQuery();
                    returnVal = true;
                }
                catch (Exception ex)
                {
                    historyRecords.Add("addLocalSlice(" + s.slice_uuid + ") - failed inserting slice record: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }
            return returnVal;
        }


        public bool addLocalGrain(grain g)
        {
            // writes grain, slice and slice_grain, and actifity records
            // if sliceid exists, it is not updated

            //**** need to check for existance first ******


            bool returnVal = false;

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    if(!localGrainRecordExists(g.grain_uuid))
                    {
                        sqlite_cmd.CommandText = "INSERT INTO grains (grain_uuid, grain_key, grain_reference, created_on) VALUES ('" + g.grain_uuid + "','" + g.grain_key + "','" + g.grain_reference  + "',DATE('now'));";
                        sqlite_cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        historyRecords.Add("addLocalGrain(() - grain " + g.grain_uuid + " already exists. Skipping grain insert");
                    }


                    if (!localSliceGrainsRecordExists(g.grain_uuid, g.slice_uuid))
                    {
                        sqlite_cmd.CommandText = "INSERT INTO slice_grains (slice_uuid, grain_uuid,grain_order) VALUES ('" + g.slice_uuid + "','" + g.grain_uuid + "',0);";
                        sqlite_cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        historyRecords.Add("addLocalGrain(() - slice/grain (" + g.slice_uuid + " / " + g.grain_uuid + ") record already exists. Skipping slice_grain insert");
                    }

                    // delete the paylod rec (in case it exists)
                    sqlite_cmd.CommandText = "DELETE FROM grain_payloads where grain_uuid='" + g.grain_uuid + "';";
                    sqlite_cmd.ExecuteNonQuery();

                    // we need to re-work this into a bound-paramter insert because the payload can contain unsafe characters
                    sqlite_cmd.CommandText = "INSERT INTO grain_payloads (grain_uuid, encoding, payload) VALUES ('" + g.grain_uuid + "','" + g.encoding+ "','" + g.payload + "');";
                    sqlite_cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    historyRecords.Add("addLocalGrain(() - failed inserting  grain or slice_grain record: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }

            return returnVal;
        }

        /** delete slice (by its uuid) and all connections to grains (slice_grain records). 
         * does not delete the grain records
         * 
         */
        public bool dropLocalSlice(string slice_uuid)
        {
            bool returnVal = false;

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "DELETE FROM slices where slice_uuid='" + slice_uuid + "'";
                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_cmd.CommandText = "DELETE FROM slice_grains where slice_uuid='" + slice_uuid + "'";
                    sqlite_cmd.ExecuteNonQuery();

                    returnVal = true;
                }
                catch (Exception ex)
                {
                    historyRecords.Add("dropLocalSlice("+slice_uuid+") - failed deleting slice or slice_grain records: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }

            return returnVal;
        }

        public void refreshAllLocalSliceHashes()
        {
            List<sandpiperClient.slice> localSlices = getLocalSlices();
            foreach (sandpiperClient.slice s in localSlices)
            {
                refreshLocalSliceHash(s.slice_uuid);
            }
        }


        public string refreshLocalSliceHash(string slice_uuid)
        {
            string hash = "";
            List<string> grainids = new List<string>();

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT grain_uuid FROM slice_grains where slice_uuid='" + slice_uuid + "' order by grain_uuid";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        while (sqlite_datareader.Read())
                        {
                            grainids.Add(sqlite_datareader.GetString(0));
                        }
                    }
                }

                hash = md5(string.Join("", grainids));

                if (localSliceHashes.ContainsKey(slice_uuid))
                {// entry already exists for this slice - update it
                    localSliceHashes[slice_uuid] = hash;
                }
                else
                {// slice does not exist in the dictionary - add it
                    localSliceHashes.Add(slice_uuid, hash);
                }
            }

            return hash;

        }


        public void logActivity(string grainid, string sliceid, string planid, string description)
        {
            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "INSERT INTO activity(activity_description, plan_uuid, slice_uuid, grain_uuid, activity_timestamp) VALUES ('" + description + "','" + planid + "', '" + sliceid + "','" + grainid + "', DATETIME('now'));";
                    sqlite_cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    historyRecords.Add("logActivity() - failed inserting SQLite activity record: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }
        }



        //validate database integrity
        // delete any slice_grain record that does not have a valid sliceid
        // delete any slice_grain record that does not have a valid grainid
        public void cleanupDatabase()
        {

            /*
            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "DELETE from slice_grain where slice_grain.id in (select slice_grain.id from slice_grain LEFT JOIN slice on slice_grain.sliceid=slice.sliceid where slice.sliceid is null);";
                    sqlite_cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    historyRecords.Add("cleanupDatabase() - failed deleting orphan slice_grain records (joining slice): " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "DELETE from slice_grain where slice_grain.id in (select slice_grain.id from slice_grain LEFT JOIN grain on slice_grain.grainid = grain.grainid where grain.id is null);";
                    sqlite_cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    historyRecords.Add("cleanupDatabase() - failed deleting orphan slice_grain records (joining grain): " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "DELETE from grain where grain.grainid in (select grain.grainid from grain LEFT JOIN slice_grain on grain.grainid = slice_grain.grainid where slice_grain.id is null);";
                    sqlite_cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    historyRecords.Add("cleanupDatabase() - failed deleting orphan grains: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }

            */

        }



    }
}
