using System;
using System.Collections.Generic;
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
        public List<slice> localSlices = new List<slice>();

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
        public List<grain> localGrainsCache = new List<grain>();
        public List<slice> slicesToUpdate = new List<slice>();
        public List<slice> slicesToAdd = new List<slice>();
        public List<slice> slicesToDrop = new List<slice>();
        public List<grain> grainsToTransfer = new List<grain>();
        public List<grain> grainsToDrop = new List<grain>();




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



        public async Task<bool> sendHeartbeat(int value)
        {


            bool returnVal = false;
            try
            {
                string source = "SandpiperInspector";
                string reportdate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
                string encodedpayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("[{ \"t\":\"" + reportdate + "\",\"n\":\"heartbeat\",\"v\":"+value.ToString()+"}]"));
                string signingsecret = "sandpiper";
                string signature = md5(source + encodedpayload + signingsecret);

                var requestData = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://aps.dev/FNKDPR/report.php?f=" + source + "&p=" + encodedpayload + "&s=" + signature.ToLower() )
                };

                HttpResponseMessage response = await client.SendAsync(requestData);

                if (response.IsSuccessStatusCode)
                {
                    returnVal=true; //historyRecords.Add("successful callHome(): " + serverGrainsResponse.message);
                }

                response.Dispose();
            }
            catch (Exception ex)
            {
                returnVal = false; //  historyRecords.Add("callHome() error - " + ex.Message);
            }
            return returnVal;
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




        public string reUUIDslice(string slice_id)
        {
            string new_id = Guid.NewGuid().ToString("D"); // just in case we need one
            string returnValue = "";

            for (int i = 0; i <= localGrainsCache.Count() - 1; i++)
            {
                if (localGrainsCache[i].slice_id == slice_id)
                {
                    localGrainsCache[i].slice_id = new_id;
                    returnValue = new_id;
                }
            }

            for (int i = 0; i <= localSlices.Count() - 1; i++)
            {
                if (localSlices[i].slice_id == slice_id)
                {
                    localSlices[i].slice_id = new_id;
                    returnValue = new_id;
                }
            }

            return returnValue;
        }


        public bool deleteLocalSice(slice s, string cacheDir)
        {
            // remove a slice and all of its grains from the local cache directory and update the index files

            bool deletedSlice = false;
            bool deletedGrain = false;
            //readCacheIndex(cacheDir);

            int sliceElementToRemove = -1;
            int grainElementToRemove;

            int i = 0;
            foreach (slice localSlice in localSlices)
            {
                if (localSlice.slice_id == s.slice_id) { sliceElementToRemove = i; break; }
                i++;
            }

            if (sliceElementToRemove >= 0)
            {// slice given was found in slicelist 

                localSlices.RemoveAt(sliceElementToRemove);
                historyRecords.Add("Deleted slice: " + s.name);

                deletedGrain = true;
                while (deletedGrain)
                {
                    deletedGrain = false;

                    i = 0; grainElementToRemove = -1;
                    foreach (grain localGrain in localGrainsCache)
                    {// look for grains claiming the given slice and delete them
                        if (localGrain.slice_id == s.slice_id) { grainElementToRemove = i; break; }
                        i++;
                    }

                    if (grainElementToRemove >= 0)
                    {
                        if (File.Exists(cacheDir + @"\" + localGrainsCache[grainElementToRemove].source))
                        {
                            historyRecords.Add("Removing grain " + localGrainsCache[grainElementToRemove].id + " (" + localGrainsCache[grainElementToRemove].source + ") from local cache");
                            File.Delete(cacheDir + @"\" + localGrainsCache[grainElementToRemove].source);
                            localGrainsCache.RemoveAt(grainElementToRemove);
                        }
                        deletedGrain = true;
                    }
                }

                writeFullCacheIndex(cacheDir);
            }
            return (deletedGrain || deletedSlice);
        }



        public bool deleteLocalGrain(grain g, string cacheDir)
        {
            bool returnValue = false;
            //readCacheIndex(cacheDir);

            int elementToRemove = -1;
            int i = 0;
            foreach (grain localGrain in localGrainsCache)
            {
                if (localGrain.id == g.id) { elementToRemove = i; break; }
                i++;
            }

            if (elementToRemove >= 0)
            {
                if (File.Exists(cacheDir + @"\" + localGrainsCache[elementToRemove].source))
                {
                    historyRecords.Add("Removing grain " + localGrainsCache[elementToRemove].id + " (" + localGrainsCache[elementToRemove].source + ") from local cache");
                    File.Delete(cacheDir + @"\" + localGrainsCache[elementToRemove].source);
                    localGrainsCache.RemoveAt(elementToRemove);
                }

                writeFullCacheIndex(cacheDir);
            }
            return returnValue;
        }


        public void addMissingLocalSlices(List<slice> slices, string cacheDir)
        {
            // add slices to the local list from given list if they do not already exist
            bool existingSlice;
            for (int i = 0; i <= slices.Count() - 1; i++)
            {
                existingSlice = false;
                foreach (sandpiperClient.slice s in localSlices)
                {
                    if (s.slice_id == slices[i].slice_id) 
                    {// slice exists already in local cache
                        existingSlice = true; break; 
                    }
                }
                if (!existingSlice) { localSlices.Add(slices[i]); }
            }
            writeFullCacheIndex(cacheDir);
        }

        public bool dropRogueLocalSlices(List<slice> slices, string cacheDir)
        {
            // drop slices (and all their grains) if they do not appear in the given list
            // this is for syncing the local as secondary
            
            bool existingSlice;
            bool removedSlice;
            bool removedGrain;
            bool updateCache=false;
            List<string> sliceidsToDrop = new List<string>();

            removedSlice = true;
            while (removedSlice)
            {
                removedSlice = false;

                for (int i = 0; i <= localSlices.Count() - 1; i++)
                {
                    existingSlice = false;

                    foreach (slice s in slices)
                    {
                        if (localSlices[i].slice_id == s.slice_id)
                        {// local slice list contains this given slice - no need to drop the local one
                            existingSlice = true; break;
                        }
                    }

                    if (!existingSlice)
                    {// current slice (s) was not found in the local cache list. add the relative element number to the drop list
                        sliceidsToDrop.Add(localSlices[i].slice_id);
                        removedSlice = true;
                        historyRecords.Add("Removing slice " + localSlices[i].slice_id + " (" + localSlices[i].name + ") from local cache");
                        localSlices.RemoveAt(i);
                        updateCache = true;
                        break;
                    }
                }
            }

            removedGrain = true;
            while (removedGrain)
            {
                removedGrain = false;

                for (int i = 0; i <= localGrainsCache.Count() - 1; i++)
                {
                    if (sliceidsToDrop.Contains(localGrainsCache[i].slice_id))
                    {// this grain should be dropped because is is part of a slice that is being dropped

                        if (File.Exists(cacheDir + @"\" + localGrainsCache[i].source))
                        {
                            historyRecords.Add("Removing grain " + localGrainsCache[i].id + " (" + localGrainsCache[i].source + ") from local cache");
                            File.Delete(cacheDir + @"\" + localGrainsCache[i].source);
                            localGrainsCache.RemoveAt(i);
                            removedGrain = true;
                            break;
                        }
                    }
                }
            }
            
            if (updateCache) { writeFullCacheIndex(cacheDir); }
            return removedSlice;
        }



        public bool writeFullCacheIndex(string cacheDir)
        {
            // rebuild (from scratch) the local grains list and slice list files from what is memory resident (localGrainsCache and localSlices


            // grain index file format: tab-delimited list of: grainid,sliceid,filename
            // slice index file format: tab-delimited list of: slice_id, slice_type, name, slicemetadata, localgrainidhash, remotehash

            bool grainsSuccess = false;
            bool slicesSuccess = false;

            


            try
            {
                using (StreamWriter file = new StreamWriter(cacheDir + @"\grainlist.txt", false))
                {
                    foreach (grain g in localGrainsCache)
                    {
                        file.WriteLine(g.id + "\t" + g.slice_id + "\t" + g.source);
                    }
                    grainsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                historyRecords.Add("writeCacheIndex() failed writing grains index: " + ex.Message);
            }

            try
            {
                using (StreamWriter file = new StreamWriter(cacheDir + @"\slicelist.txt", false))
                {
                    string hashTemp = "";
                    foreach (slice s in localSlices)
                    {
                        // calc the hash of grainids claiming this slice
                        hashTemp = "";
                        foreach (grain g in localGrainsCache)
                        {
                            if (g.slice_id == s.slice_id) { hashTemp += g.id; }
                        }

                        file.WriteLine(s.slice_id + "\t" + s.slice_type + "\t" + s.name + "\t" + s.slicemetadata + "\t" + md5(hashTemp) + "\t" + s.hash);
                    }
                    slicesSuccess = true;
                }
            }
            catch (Exception ex)
            {
                historyRecords.Add("writeCacheIndex() failed writing slices list: " + ex.Message);
            }

            return (grainsSuccess & slicesSuccess);
        }


        public bool writeFilegrainToFile(sandpiperClient.grain filegrain, string cacheDir)
        {
            for (int i =0; i<=localGrainsCache.Count()-1; i++)
            {// find the existing grain in the cachelist and remove it if it exists
                if (localGrainsCache[i].id == filegrain.id)
                {
                    localGrainsCache.RemoveAt(i);
                    break;
                }
            }

            localGrainsCache.Add(filegrain); // att this grain to the cachelist
            
            if (filegrain.encoding == "z64")
            {
                //ccc
                byte[] payloadBytes = unz64(filegrain.payload);
                File.WriteAllBytes(cacheDir + @"\" + filegrain.source, payloadBytes);
            }

            if (filegrain.encoding == "b64")
            {// full-range 8bit binary data (not compressed) is to be expected
                byte[] payloadBytes = Convert.FromBase64String(filegrain.payload);
                File.WriteAllBytes(cacheDir + @"\" + filegrain.source, payloadBytes);
                //ccc
            }

            if (filegrain.encoding == "raw")
            {// probably a text file 
                File.WriteAllBytes(cacheDir + @"\" + filegrain.source, Encoding.UTF8.GetBytes(filegrain.payload));
            }

            writeFullCacheIndex(cacheDir);

            return true;
        }


        public List<grain> grainsInLocalSlice(string slice_id)
        {
            List<sandpiperClient.grain> tempGrainsList = new List<sandpiperClient.grain>();

            foreach (grain g in localGrainsCache)
            {
                if (slice_id == g.slice_id)
                {
                    grain ng = new grain();
                    ng.description = g.description; ng.encoding = g.encoding; ng.grain_key = g.grain_key; ng.id = g.id; ng.payload = g.payload; ng.slice_id = g.slice_id; ng.source = g.source; ng.payload_len = g.payload_len;
                    tempGrainsList.Add(ng);
                }
            }
            return tempGrainsList;
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

                            if (remoteSlice.hash != localSlice.hash)
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

                            if (remoteSlice.hash != localSlice.hash)
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


       

    }
}
