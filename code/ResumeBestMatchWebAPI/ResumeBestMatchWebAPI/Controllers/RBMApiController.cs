using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using ResumeBestMatchWebAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace ResumeBestMatchWebAPI.Controllers
{
    
    public class RBMApiController : ApiController
    {
        public StringBuilder text;
        public List<results> resList;
        public results res;
        public ApiResponseModel responseModel;

        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=anonymustorageaccount;AccountKey=KhVZIFdo89NJ2iMK6vIXQYl5nEqBw+RI10a/mxARUygiN1JlDVP4mRrzpt5mlpnELZ9Q5PmHkfHZ+AStNaWJcQ==;EndpointSuffix=core.windows.net";
        private static string containerName = "resumetestdata";
        public RBMApiController()
        {
            text = new StringBuilder();
            resList = new List<results>();
            responseModel = new ApiResponseModel();
            res = new results();
        }
        [HttpPost]
        public HttpResponseMessage Get([FromBody] ApiRequestModel reqModel)
        {
            string context = reqModel.context;
            string category = reqModel.category;
            decimal threshold = reqModel.threshold;
            int noOfMatches = reqModel.noOfMatches;
            string inputPath=reqModel.inputPath;
            if (context == null || category == null || threshold == 0 || noOfMatches == 0 || inputPath == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "All the fileds are manditory : context, category, threshold, noOfMatches and inputPath");
            }
            else {
                try
                {
                    responseModel = GetTextFromPDF(inputPath, category, context, noOfMatches);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
                return Request.CreateResponse(HttpStatusCode.OK, responseModel);
            }
        }
        public ApiResponseModel GetTextFromPDF(string filePath, string category, string context, int noOfMatches)
        {
            int count = 0;
            int responseFileMatchCount = 1;
            DirectoryInfo dir = new DirectoryInfo(filePath);
            //
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            var results = new Dictionary<string, string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                Console.WriteLine($"Reading blob: {blobItem.Name}");
                
                if (blobItem.Name.Contains(".pdf"))
                {
                    string content = await new Program().ReadPdfFromBlob(connectionString, containerName, blobItem.Name);
                    Console.WriteLine(content);
                    results.Add(blobItem.Name, content);
                }
                else if (blobItem.Name.Contains(".docx"))
                {
                    string content = new Program().ReadDocFromBlob(connectionString, containerName, blobItem.Name);
                    Console.WriteLine(content);
                    results.Add(blobItem.Name, content);
                }
                else
                {
                    if (await blobClient.ExistsAsync())
                    {
                        var response = await blobClient.DownloadAsync();
                        try
                        {
                            using (var streamReader = new StreamReader(response.Value.Content))
                            {

                                string content = await streamReader.ReadToEndAsync();

                                
                                results.Add(blobItem.Name, content);
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message.ToString());
                        }
                    }
                }
            }
            foreach (var filedata in results.values)
            {
                if (category == "resume")
                {
                    count = Regex.Matches(filedata(item).ToString(), context).Count;
                    if (count > 0)
                    {
                        if (responseFileMatchCount <= noOfMatches)
                        {
                            res = new results
                            {
                                id = j,
                                score = "0.5",
                                path = file.Name
                            };
                            resList.Add(res);
                            responseFileMatchCount++;
                        }
                        j++;
                    }
                    count = 0;
                }
                else if (category == "JD")
                {
                    //code for .docx file read
                        responseModel = new ApiResponseModel
                        {
                            status = "No Code",
                            count = count,
                        };
                }
                else
                {
                    //code for txt file read
                    responseModel = new ApiResponseModel
                    {
                        status = "No Code",
                        count = count,
                    };
                }
                
            }
            responseModel = new ApiResponseModel
                {
                    status = "success",
                    count = responseFileMatchCount,
                    results = resList
                };
            //Console.WriteLine(results);
            //
            /* if (dir.Exists)
            {
                 IEnumerable<FileInfo> filesList = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                IEnumerable<FileInfo> fileQuery = from file in filesList
                                                  where file.Extension == ((category == "resume") ? ".pdf" : (category == "JD" ? ".docx" : ".txt"))
                                                  orderby file.Name.ToString()
                                                  select file;
                int j = 1;
                foreach (FileInfo file in fileQuery)
                {
                    string fullFileNameTxt = System.IO.Path.Combine(file.DirectoryName, file.FullName);
                    if (category == "resume")
                    {
                        using (PdfReader reader = new PdfReader(fullFileNameTxt))
                        {
                            reader.RemoveAnnotations();
                            reader.RemoveUnusedObjects();
                            if (reader.NumberOfPages >= 1)
                            {
                                for (int i = 1; i <= reader.NumberOfPages; i++)
                                {
                                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                                }
                                count = Regex.Matches(text.ToString(), context).Count;
                                text.Clear(); 
                            }
                            if (count > 0)
                            {
                                if (responseFileMatchCount <= noOfMatches)
                                {
                                    res = new results
                                    {
                                        id = j,
                                        score = "0.5",
                                        path = file.Name
                                    };
                                    resList.Add(res);
                                    responseFileMatchCount++;
                                }
                                j++;
                            }
                            count = 0;

                        }
                    }
                    else if (category == "JD")
                    {
                        //code for .docx file read
                        responseModel = new ApiResponseModel
                        {
                            status = "No Code",
                            count = count,
                        };
                    }
                    else
                    {
                        //code for txt file read
                        responseModel = new ApiResponseModel
                        {
                            status = "No Code",
                            count = count,
                        };
                    }

                } 
                responseModel = new ApiResponseModel
                {
                    status = "success",
                    count = responseFileMatchCount,
                    results = resList
                };
            } */
            return responseModel;
        }
        public async Task<string> ReadPdfFromBlob(string connectionString, string containerName, string blobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobClient blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);

            StringBuilder text = new StringBuilder();

            using (var memoryStream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                PdfReader pdfReader = new PdfReader(memoryStream);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    text.Append(currentPageText);
                }

                pdfReader.Close();
            }

            return text.ToString();
        }

        public string ReadDocFromBlob(string connectionString, string containerName, string blobName)
        {
            
            // Create a BlobServiceClient to interact with the blob service
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Download the blob to a stream
            using (var memoryStream = new MemoryStream())
            {
                blobClient.DownloadTo(memoryStream);

                // Use OpenXml to read the .docx file from the stream
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(memoryStream, false))
                {
                    // Read the document text
                    

                    // Print the text
                    return wordDoc.MainDocumentPart.Document.Body.InnerText;

                }
            }

        }
    }
}
