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
            public string id;
            public string description;
            public string slice_id;
            public string grain_key;
            public string source;
            public string encoding;
            public string payload;
            public long payload_len;
            public string localfilename;

            public void clear()
            {
                id = "";
                description = "";
                slice_id = "";
                grain_key = "";
                source = "";
                encoding = "";
                payload_len = 0;
            }
        }


        public class slice
        {
            public string slice_id;
            public string slice_type;
            public string name;
            public string slicemetadata;
            public string hash; // md5sum of grain uuid concatenated
            public List<grain> grains;

            public void clear()
            {
                slice_id = "";
                slice_type = "";
                name = "";
                slicemetadata = "";
                if (grains != null) { grains.Clear(); }
            }
        }


        public class grainsEnvelope
        {
            public List<grain> grains;
        }

        public class grainsEnvelopeCompact
        {
            public List<string> grainuuids;
        }




        public class grainsResponse
        {
            public string message;
        }

        public class slicesResponse
        {
            public string message;
        }

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
            List<grain> grainsList = new List<grain>();
            grainsEnvelope responseData = new grainsEnvelope();
            grainsEnvelopeCompact responseDataCompact = new grainsEnvelopeCompact();
            grain myGrain = new grain();

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
                            responseData = serializer.Deserialize<grainsEnvelope>(responseString);
                        }
                        else
                        {
                            myGrain = serializer.Deserialize<grain>(responseString);
                            responseData.grains.Add(myGrain);
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

            for (int i = 0; i <= responseData.grains.Count() - 1; i++)
            {// prefix the soure with the grainid to make it unique for local file storage
                responseData.grains[i].localfilename = responseData.grains[i].id + " - " + responseData.grains[i].source;
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

                id = g.id,
                slice_id = g.slice_id,
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
                        historyRecords.Add("POST grain " + g.id + ".  Server responded: " + serverGrainsResponse.message);
                    }
                    catch (Exception ex)
                    {
                        historyRecords.Add("POST grain " + g.id + ". Local error parsing server JSON response from server: " + ex.Message);
                    }
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("POST grain " + g.id + " error. Server HTTP response:" + response.ReasonPhrase);
                }

            }
            catch (Exception ex)
            {
                historyRecords.Add("POST grain " + g.id + ". Local error - " + ex.Message);
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
                    slices = serializer.Deserialize<List<slice>>(responseString);

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
                id = s.slice_id,
                name = s.name,
                slice_type = s.slice_type,
                metadata = s.slicemetadata
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
                        historyRecords.Add("POST slice " + s.slice_id + ".  Server responded: " + serverSlicesResponse.message);
                    }
                    catch (Exception ex)
                    {
                        historyRecords.Add("POST slice " + s.slice_id + ". Local error parsing server JSON response from server: " + ex.Message);
                    }
                }
                else
                {// something other than 200 (success) code back from the other end 
                    historyRecords.Add("POST slice " + s.slice_id + " error. Server HTTP response:" + response.ReasonPhrase);
                }

            }
            catch (Exception ex)
            {
                historyRecords.Add("POST slice " + s.slice_id + ". Local error - " + ex.Message);
            }

            return returnValue;
        }



        public void addMissingLocalSlices(List<slice> slices)
        {
            // add slices to the local list from given list if they do not already exist
            foreach (slice s in slices)
            {
                if(!localSliceExists(s.slice_id))
                {
                    addLocalSlice(s);
                    historyRecords.Add("Added slice " + s.slice_id + " (" + s.name + ") to local pool");
                }
            }
        }

        public int dropRogueLocalSlices(List<slice> remoteSlices)
        {
            // drop slices (and all their grain connections) if they do not appear in the given list
            // this is for syncing the local as secondary
            int returnVal = 0;

            List<string> sliceidsToDrop = new List<string>();
            List<string> remoteSliceids = new List<string>();
            foreach (slice s in remoteSlices){remoteSliceids.Add(s.slice_id);}

            List<slice> localSlices = getLocalSlices();
            foreach (slice s in localSlices)
            {
                if (!remoteSliceids.Contains(s.slice_id))
                {
                    historyRecords.Add("Removing local slice " + s.slice_id + " (" + s.name + ") and all of its grain connections from local pool");
                    dropLocalSlice(s.slice_id);
                }
            }

            return returnVal;
        }





        public bool writeFilegrainToFile(sandpiperClient.grain filegrain, string cacheDir)
        {
            
            if (filegrain.encoding == "z64")
            {
                
                byte[] payloadBytes = unz64(filegrain.payload);
                File.WriteAllBytes(cacheDir + @"\" + filegrain.localfilename, payloadBytes);
            }

            if (filegrain.encoding == "b64")
            {// full-range 8bit binary data (not compressed) is to be expected
                byte[] payloadBytes = Convert.FromBase64String(filegrain.payload);
                File.WriteAllBytes(cacheDir + @"\" + filegrain.localfilename, payloadBytes);
            }

            if (filegrain.encoding == "raw")
            {// probably a text file 
                File.WriteAllBytes(cacheDir + @"\" + filegrain.localfilename, Encoding.UTF8.GetBytes(filegrain.payload));
            }

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
                    if(grainsB[i].id == gA.id)
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
                        if (remoteSlice.slice_id == localSlice.slice_id)
                        {// found a sliceid match

                            foundRemoteSlice = true;

                            if (remoteSlice.hash.ToLower() != localSlice.hash.ToLower())
                            {
                                hashMatch = false;
                            }
                            break;
                        }
                    }

                    if (!foundRemoteSlice)
                    {// no remote slice was found. put it on the "add" list  
                        slice newSlice = new slice();
                        newSlice.slice_id = localSlice.slice_id; newSlice.slice_type = localSlice.slice_type; newSlice.name = localSlice.name; newSlice.slicemetadata = localSlice.slicemetadata; newSlice.hash = localSlice.hash;
                        slicesToAdd.Add(newSlice);
                    }

                    if (!hashMatch)
                    {// no remote slice was found, or it was found with non-matching hash. We need to push this slice into the remote secondary
                        slice newSlice = new slice();
                        newSlice.slice_id = localSlice.slice_id; newSlice.slice_type = localSlice.slice_type; newSlice.name = localSlice.name; newSlice.slicemetadata = localSlice.slicemetadata; newSlice.hash = localSlice.hash;
                        slicesToUpdate.Add(newSlice);
                    }
                }

                // build a droplist of remote slices (ones that are in the remote and not local)

                foreach (slice remoteSlice in remoteSlices)
                {
                    foundLocalSlice = false;
                    foreach (slice localSlice in localSlices)
                    {
                        if (remoteSlice.slice_id == localSlice.slice_id)
                        {// found a sliceid match
                            foundLocalSlice = true;
                            break;
                        }
                    }

                    if (!foundLocalSlice)
                    {
                        slice newSlice = new slice();
                        newSlice.slice_id = remoteSlice.slice_id; newSlice.slice_type = remoteSlice.slice_type; newSlice.name = remoteSlice.name; newSlice.slicemetadata = remoteSlice.slicemetadata; newSlice.hash = remoteSlice.hash;
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

                        if (remoteSlice.slice_id == localSlice.slice_id)
                        {// found a sliceid match

                            foundLocalSlice = true;

                            if (remoteSlice.hash.ToLower() != localSlice.hash.ToLower())
                            {
                                hashMatch = false;
                            }
                            break;
                        }
                    }

                    if (!foundLocalSlice)
                    {// no local slice was found
                        slice newSlice = new slice();
                        newSlice.slice_id = remoteSlice.slice_id; newSlice.slice_type = remoteSlice.slice_type; newSlice.name = remoteSlice.name; newSlice.slicemetadata = remoteSlice.slicemetadata; newSlice.hash = remoteSlice.hash;
                        slicesToAdd.Add(newSlice);
                    }

                    if (!hashMatch)
                    {// slice found with non-matching hash. We need to get the remote (primary) slice
                        slice newSlice = new slice();
                        newSlice.slice_id = remoteSlice.slice_id; newSlice.slice_type = remoteSlice.slice_type; newSlice.name = remoteSlice.name; newSlice.slicemetadata = remoteSlice.slicemetadata; newSlice.hash = remoteSlice.hash;
                        slicesToUpdate.Add(newSlice);
                    }
                }

                //determine a droplist of local slices (ones that are local but not in remote list)
                foreach (slice localSlice in localSlices)
                {
                    foundRemoteSlice = false;
                    foreach (slice remoteSlice in remoteSlices)
                    {
                        if (remoteSlice.slice_id == localSlice.slice_id)
                        {// found a sliceid match
                            foundRemoteSlice = true;
                            break;
                        }
                    }

                    if (!foundRemoteSlice)
                    {
                        slice newSlice = new slice();
                        newSlice.slice_id = localSlice.slice_id; newSlice.slice_type = localSlice.slice_type; newSlice.name = localSlice.name; newSlice.slicemetadata = localSlice.slicemetadata; newSlice.hash = localSlice.hash;
                        slicesToDrop.Add(newSlice);
                    }
                }
            }
        }


        public bool SQLiteInit(string cacheDir)
        {
            bool returnVal = false;
            sqlite_conn.ConnectionString = "Data Source=" + cacheDir + @"\sandpiper.db; Version = 3; New = True; Compress = True;";

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_conn.Open();

                    sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS slice (sliceid VARCHAR(64) PRIMARY KEY, slicetype VARCHAR(64), name VARCHAR(255), slicemetadata TEXT, remotehash VARCHAR(64), localhash VARCHAR(64))";
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


        public bool localSliceExists(string sliceid)
        {
            bool returnVal = false;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT sliceid FROM slice where sliceid='" + sliceid + "'";

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

        public string localSliceHash(string sliceid)
        {
            string returnVal = "";
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT grainhash FROM slice where sliceid='" + sliceid + "'";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        if (sqlite_datareader.Read())
                        {
                            returnVal = sqlite_datareader.GetString(0);
                        }
                    }
                }
            }
            return returnVal;
        }


        public bool localGrainSliceRecordExists(string grainid, string sliceid)
        {
            bool returnVal = false;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT id FROM slice_grain where sliceid='" + sliceid + "' and grainid='"+grainid+"'";

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

        public bool localGrainRecordExists(string grainid)
        {
            bool returnVal = false;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT id FROM grain where grainid='" + grainid + "'";

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




        public slice getLocalSlice(string sliceid)
        {
            slice s = null;

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT sliceid, slicetype, name, slicemetadata, remotehash, localhash FROM slice where sliceid='" + sliceid + "'";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        s = new slice();
                        if(sqlite_datareader.Read())
                        {
                            s.slice_id = sqlite_datareader.GetString(0);
                            s.slice_type = sqlite_datareader.GetString(1);
                            s.name = sqlite_datareader.GetString(2);
                            s.slicemetadata= sqlite_datareader.GetString(3);
                            s.hash = sqlite_datareader.GetString(4);
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
                    sqlite_cmd.CommandText = "SELECT sliceid, slicetype, name, slicemetadata, localhash FROM slice order by name";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        while (sqlite_datareader.Read())
                        {
                            slice s = new slice();
                            s.slice_id = sqlite_datareader.GetString(0);
                            s.slice_type = sqlite_datareader.GetString(1);
                            s.name = sqlite_datareader.GetString(2);
                            s.slicemetadata = sqlite_datareader.GetString(3);
                            s.hash = sqlite_datareader.GetString(4);
                            returnVal.Add(s);
                        }
                    }
                }
            }
            return returnVal;
        }






        public List<grain> getGrainsInLocalSlice(string sliceid, bool with_payload)
        {
            List<grain> grainslist = new List<grain>();

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    if (with_payload)
                    {
                        sqlite_cmd.CommandText = "SELECT grain.grainid, slice_grain.sliceid, source, payload_len, grain_key, description, encoding, payload FROM grain,slice_grain where grain.grainid=slice_grain.grainid and slice_grain.sliceid='" + sliceid + "'";
                    }
                    else
                    {// leave out payload
                        sqlite_cmd.CommandText = "SELECT grain.grainid, slice_grain.sliceid, source, payload_len, grain_key, description, encoding FROM grain,slice_grain where grain.grainid=slice_grain.grainid and slice_grain.sliceid='" + sliceid + "'";
                    }
                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {

                        while (sqlite_datareader.Read())
                        {
                            grain g = new grain();
                            g.id = sqlite_datareader.GetString(0);
                            g.slice_id = sqlite_datareader.GetString(1);
                            g.source = sqlite_datareader.GetString(2);
                            g.payload_len = sqlite_datareader.GetInt32(3);
                            g.grain_key = sqlite_datareader.GetString(4);
                            g.description = sqlite_datareader.GetString(5);
                            g.encoding = sqlite_datareader.GetString(6);
                            g.payload = "";
                            if (with_payload)
                            {
                                g.payload = sqlite_datareader.GetString(7);
                            }
                            grainslist.Add(g);
                        }
                    }
                }
            }
            return grainslist;
        }

        public int countGrainsInLocalSlice(string sliceid)
        {
            int returnVal = 0;
            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT count(id) as graincount FROM slice_grain where sliceid='" + sliceid + "'";
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
                    if (localSliceExists(s.slice_id))
                    {// slice exists - update it

                        sqlite_cmd.CommandText = "UPDATE slice set slicetype='" + s.slice_type + "', name='" + s.name + "', slicemetadata='" + s.slicemetadata + "', remotehash='" + s.hash + "' where sliceid='"+s.slice_id+"';";
                        historyRecords.Add("addLocalSlice(" + s.slice_id + ") - slice record already exists. Updating existing record.");
                        logActivity("", s.slice_id, "", "slice record updated (" + s.name + ")");
                    }
                    else
                    {// slice does not exist - insert it

                        sqlite_cmd.CommandText = "INSERT INTO slice (sliceid, slicetype, name, slicemetadata, remotehash, localhash) VALUES ('" + s.slice_id + "','" + s.slice_type + "','" + s.name + "','" + s.slicemetadata + "','" + s.hash + "','');";
                        logActivity("", s.slice_id, "", "slice record added ("+s.name+")");

                    }

                    sqlite_cmd.ExecuteNonQuery();
                    returnVal = true;
                }
                catch (Exception ex)
                {
                    historyRecords.Add("addLocalSlice(" + s.slice_id + ") - failed inserting slice record: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
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
                    if(!localGrainRecordExists(g.id))
                    {
                        sqlite_cmd.CommandText = "INSERT INTO grain (grainid, source, payload_len, grain_key, description, encoding, payload) VALUES ('" + g.id + "', '" + g.source + "'," + g.payload_len.ToString() + ",'" + g.grain_key + "','" + g.description + "','" + g.encoding + "','" + g.payload + "');";
                        sqlite_cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        historyRecords.Add("addLocalGrain(() - grain " + g.id + " already exists. Skipping grain insert");
                    }

                    if (!localGrainSliceRecordExists(g.id, g.slice_id))
                    {
                        sqlite_cmd.CommandText = "INSERT INTO slice_grain (sliceid, grainid) VALUES ('" + g.slice_id + "', '" + g.id + "');";
                        sqlite_cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        historyRecords.Add("addLocalGrain(() - slice/grain (" + g.slice_id + " / " + g.id + ") record already exists. Skipping slice_grain insert");
                    }
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
        public bool dropLocalSlice(string sliceid)
        {
            bool returnVal = false;

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "DELETE FROM slice where sliceid='" + sliceid + "'";
                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_cmd.CommandText = "DELETE FROM slice_grain where sliceid='" + sliceid + "'";
                    sqlite_cmd.ExecuteNonQuery();

                    returnVal = true;
                }
                catch (Exception ex)
                {
                    historyRecords.Add("dropLocalSlice("+sliceid+") - failed deleting slice or slice_grain records: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }

            return returnVal;
        }

        public void refreshAllLocalSliceHashes()
        {
            List<sandpiperClient.slice> localSlices = getLocalSlices();
            foreach (sandpiperClient.slice s in localSlices)
            {
                refreshLocalSliceHash(s.slice_id);
            }
        }


        public bool refreshLocalSliceHash(string sliceid)
        {
            bool returnVal = false;
            List<string> grainids = new List<string>();

            if (SQliteDatabaseInitialized)
            {
                using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
                {
                    sqlite_cmd.CommandText = "SELECT grainid FROM slice_grain where slice_grain.sliceid='" + sliceid + "' order by grainid";

                    using (SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader())
                    {
                        while (sqlite_datareader.Read())
                        {
                            grainids.Add(sqlite_datareader.GetString(0));
                        }
                    }
                }
            }

            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "UPDATE slice set localhash='" + md5(string.Join("", grainids)) + "' where sliceid='" + sliceid + "';";
                    sqlite_cmd.ExecuteNonQuery();
                    returnVal = true;
                }
                catch (Exception ex)
                {
                    historyRecords.Add("refreshLocalSliceHash("+sliceid+") - failed updating slice record: " + ex.Message + "(" + sqlite_cmd.CommandText + ")");
                }
            }
            return returnVal;
        }


        public void logActivity(string grainid, string sliceid, string planid, string description)
        {
            using (SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand())
            {
                try
                {
                    sqlite_cmd.CommandText = "INSERT INTO activity(description, planid, sliceid, grainid, timestamp) VALUES ('" + description + "','" + planid + "', '" + sliceid + "','" + grainid + "', DATETIME('now'));";
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

        }



    }
}
