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
            if (dir.Exists)
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
            }
            return responseModel;
        }
    }
}
